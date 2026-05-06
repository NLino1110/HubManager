using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;
using DMOrders.Controls.Tools;
using DMSA.Models.Odoo.Security;
using DMSA.Models.Security;
using DMSA.Sync.Core;
using DMSA.Sync.Core.Database.Sqlite;
using DMSA.Sync.Core.Update;
using DMSA.Sync.Core.Update.Cloud;

namespace DMOrders.Services.Update
{
    public class LaunchManager
    {
        private static bool _isRunning = false;

        private ServerPuller serverPuller { get; set; }

        public LaunchManager()
        {
            serverPuller = new ServerPuller();
        }

        private async Task<bool> ServerOnlineStatus_Odoo()
        {
            ApiChecker apiChecker = new ApiChecker(App.Session.odooConnection.Host + "/connect/checkonline");
            return await apiChecker.IsApiAvailable();
        }

        public async Task Execute()
        {
            if (_isRunning)
                return;

            _isRunning = true;

            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

            try
            {
                await Toast.Make("Iniciando actualización...", ToastDuration.Short, 14)
                    .Show(cancellationTokenSource.Token);

                Pipeline pipeline = new Pipeline();

                //////bool packageReady = await pipeline.ExistAttachRecord();

                //////if (!packageReady)
                //////{
                //////    var packFound = await pipeline.NewestZipPack();

                //////    if (packFound != null)
                //////    {
                //////        await SqliteDbBase<object>.CloseDatabaseAsync();

                //////        if (await pipeline.DownloadSqliteZip(true))
                //////        {
                //////            await pipeline.InsertAttachRecord(packFound);
                //////        }
                //////        else
                //////        {
                //////            await Toast.Make("Error al descargar/descomprimir archivo.", ToastDuration.Long)
                //////                .Show();
                //////        }

                //////        await Toast.Make("Actualización rápida terminada", ToastDuration.Short)
                //////            .Show();

                //////        var databaseUserAccess = new UserAccessDb(App.Session.odooConnection.DbNameSqlite);
                //////        await databaseUserAccess.FixMissingCurrentUser();
                //////    }
                //////}

                bool success = await LaunchOnlineUpdate();

                if (success)
                {
                    await Toast.Make("Actualización terminada", ToastDuration.Short, 14)
                        .Show();
                }
                else
                {
                    await Toast.Make("Actualización incompleta", ToastDuration.Long, 14)
                        .Show();
                }
            }
            catch (Exception ex)
            {
                await Toast.Make("Error general: " + ex.Message, ToastDuration.Long)
                    .Show();
            }
            finally
            {
                _isRunning = false;
            }
        }

        private async Task<bool> LaunchOnlineUpdate()
        {
            var userDb = new UserAccessDb(App.Session.odooConnection.DbNameSqlite);
            var hubUser = new ApiManager.HubUser(App.Session);

            var response = await hubUser.ValidaSincronizacionAsync(App.Session.CurrentUser, DateTime.Now);

            if (!response.success)
                return false;

            var user = await userDb.GetItemAsync(App.Session.CurrentUserFront.uid);

            if (user == null)
                return false;

            bool patchRequireUpdate = Preferences.Get("patch_require_update", false);

            if (user.log_fec_sincro.Date == response.data[0].datetime.Date && !patchRequireUpdate)
                return false;

            bool success = false;

            try
            {
                success = await ExecutePipeline();

                if (success)
                {
                    await SaveSyncDate(user, response.data[0].datetime);
                }

                ///await HandleUploadPipeline(App.Session.odooConnection.DbNameSqlite);

                return success;
            }
            catch
            {
                return false;
            }
        }

        private async Task UpdateProgressState(int current, int total, string title)
        {
            if (total <= 0) total = 1;

            current = Math.Min(current, total - 1);

            int displayPage = current + 1;
            double percent = (double)displayPage / total;

            //await MainThread.InvokeOnMainThreadAsync(() =>
            //{
                await UITools.SetNotifyLoadingPopup($"{title} - Página {displayPage} de {total}");                
            //});
        }

        private async Task<bool> ExecutePipeline()
        {
            await SafeExecute(() => serverPuller.PullPromotions(async (current, total) => { await UpdateProgressState(current, total, "Promociones"); }), "Promotions");
            await SafeExecute(() => serverPuller.ProductMarca(async (current, total) => { await UpdateProgressState(current, total, "Marcas"); }), "ProductMarca");
            await SafeExecute(() => serverPuller.OnlineSyncCategoria(async (current, total) => { await UpdateProgressState(current, total, "Cat. Prod."); }), "Categoria");
            await SafeExecute(() => serverPuller.OnlineSyncSubcategoria(async (current, total) => { await UpdateProgressState(current, total, "Subcat. Prod"); }), "Subcategoria");
            await SafeExecute(() => serverPuller.OnlineSyncProductLinea(async (current, total) => { await UpdateProgressState(current, total, "Linea Prod."); }), "ProductLinea");
            await SafeExecute(() => serverPuller.OnlineSyncProductGrupoTipo(async (current, total) => { await UpdateProgressState(current, total, "Tipo Prod."); }), "GrupoTipo");
            await SafeExecute(() => serverPuller.OnlineCalificacionCrediticia(async (current, total) => { await UpdateProgressState(current, total, "Calif."); }), "CalificacionCrediticia");
            await SafeExecute(() => serverPuller.OnlineSyncResPartnerFull(async (current, total) => { await UpdateProgressState(current, total, "Clientes"); }), "ResPartner");

            await SafeExecute(() => serverPuller.OnlineSyncProductPricelist(async (current, total) => { await UpdateProgressState(current, total, "L.Precios"); }), "Pricelist");
            await SafeExecute(() => serverPuller.OnlineSyncProductPricelistItem(async (current, total) => { await UpdateProgressState(current, total, "L.Pr.Items"); }), "PricelistItem");
            await SafeExecute(() => serverPuller.OnlineAccountTaxes(async (current, total) => { await UpdateProgressState(current, total, "Taxes"); }), "Taxes");

            await ExecuteCriticalStockBlock();

            await SafeExecute(() => serverPuller.OnlineSyncProductProductNoImage(async (current, total) => { await UpdateProgressState(current, total, "Prod."); }), "Product");
            await SafeExecute(() => serverPuller.SyncSaleOrders(), "SaleOrders");

            return true;
        }

        private async Task ExecuteCriticalStockBlock()
        {
            await serverPuller.OnlineSyncStockWarehouse(false);
            //await serverPuller.OnlineSyncStockLocation();
            //await serverPuller.OnlineSyncStockQuant();
            await serverPuller.UomUom(true);
            await serverPuller.OnlineSyncWmsStockQuant(async (current, total) => { await UpdateProgressState(current, total, "Stock WMS"); });
            await serverPuller.UpdateWmsStockQuant(async (current, total) => { await UpdateProgressState(current, total, "WmsStockQuant"); });
        }

        private async Task SafeExecute(Func<Task> action, string name)
        {
            try
            {
                await action();
            }
            catch (Exception ex)
            {
                await Toast.Make($"Error en {name}: {ex.Message}", ToastDuration.Long)
                    .Show();
            }
        }

        private async Task SaveSyncDate(user_access user, DateTime serverDate)
        {
            var userDb = new UserAccessDb(App.Session.odooConnection.DbNameSqlite);

            if (user.log_fec_acceso.Date != serverDate.Date)
            {
                user.log_fec_acceso = serverDate;
                App.Session.CurrentUserFront.log_fec_acceso = user.log_fec_acceso;
            }

            user.log_fec_sincro = serverDate;
            App.Session.CurrentUserFront.log_fec_sincro = user.log_fec_sincro;

            await userDb.UpdateAsync(user);
        }

        private async Task HandleUploadPipeline(string DbName)
        {
            Pipeline pipeline = new Pipeline();

            bool requiredNewUpload = await pipeline.RequiredNewUploadCustom(DbName);

            if (requiredNewUpload)
            {
                (var attachData, bool successUpload) = await pipeline.UploadSqliteZip();

                if (successUpload)
                {
                    if (!await pipeline.ExistAttachRecord())
                        await pipeline.InsertAttachRecord(attachData);
                }
            }
        }
    }
}