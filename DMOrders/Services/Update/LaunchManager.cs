using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;
using DMSA.Models.Odoo.DMApps;
using DMSA.Models.Security;
using DMSA.Sync.Core;
using DMSA.Sync.Core.Database.Sqlite;
using DMSA.Sync.Core.Update;
using DMSA.Sync.Core.Update.Cloud;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMOrders.Services.Update
{
    public class LaunchManager
    {
        DMSA.Sync.Core.Update.ServerPuller serverPuller { get; set; }

        public LaunchManager()
        {
            serverPuller = new DMSA.Sync.Core.Update.ServerPuller();
        }

        private async Task<bool> ServerOnlineStatus_Odoo()
        {
            ApiChecker apiChecker = new ApiChecker(App.Session.odooConnection.Host + "/connect/checkonline");
            bool isOnline = await apiChecker.IsApiAvailable();

            //if (!isOnline)
            //{
            //    BtnTryLogin.IsEnabled = true;
            //    await Toast.Make("Offline o servidor inválido!").Show();
            //    Debug.WriteLine("Offline o servidor inválido!");
            //    return false;
            //}

            return isOnline;
        }

        public async Task Execute()
        {
            DateTime dtInitialize = DateTime.Now;
            
            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

            string fechaActualizaTablet = "2021-01-01 00:00:00";
            AppSession _appSession = App.Session;

            DateTime dateTimeIni = DateTime.Now;

            string text = "Iniciando actualización...";
            ToastDuration duration = ToastDuration.Short;
            double fontSize = 14;
            var toast = Toast.Make(text, duration, fontSize);
            await toast.Show(cancellationTokenSource.Token);
            
            //ProgressBarAnimationBehaviorPage obj = new ProgressBarAnimationBehaviorPage();
            //await Navigation.PushModalAsync(obj, true);
            bool launchSalesUpdate = true;
            //obj.SetTotalPercentProgress(0.10);

            if (await ServerOnlineStatus_Odoo())
            {
                
            }
            else
            {
                toast = Toast.Make("Servidor Odoo no disponible", duration, fontSize);
                await toast.Show(cancellationTokenSource.Token);

                //await obj.DisplayAlert("Error de actualización", "El servidor de datos no está disponible.", "Aceptar");
                //await Navigation.PopModalAsync();
            }

            Pipeline pipeline = new Pipeline();

            bool packageReady = await pipeline.ExistAttachRecord();

            if (!packageReady)
            {
                //await SqliteDbBase<object>.CloseDatabaseAsync();            
                //Pipeline pipeline = new Pipeline();

                var packFound = await pipeline.NewestZipPack();

                if (packFound != null)
                {
                    await SqliteDbBase<object>.CloseDatabaseAsync();
                    //obj.SetTitle("Iniciando actualización rápida...");
                    //obj.SetTotalPercentProgress(0.2);

                    if (await pipeline.DownloadSqliteZip(true))
                    {
                        await pipeline.InsertAttachRecord(packFound);
                    }
                    else
                    {
                        await Toast.Make("Hubo un error al descargar/descomprimir archivo.", duration, fontSize).Show();
                    }

                    await Toast.Make("Actualización rápida terminada", duration, fontSize).Show();
                }
            }

            //obj.SetTitle("Actualización en línea...");

            await LaunchOnlineUpdate();

            //obj.SetTotalPercentProgress(1);
            //obj.SetTitle("Finalizado...");

            TimeSpan span = (DateTime.Now - dtInitialize);
            
            await Toast.Make("Actualización terminada", duration, fontSize).Show();

            //await obj.DisplayAlert("Actualización", "Actualización terminada", "Aceptar");

            //await Navigation.PopModalAsync();
        }

        private async Task LaunchOnlineUpdate()
        {
            var UserDatabase = new UserAccessDb(App.Session.odooConnection.DbNameSqlite);
            ApiManager.HubUser hubUser = new ApiManager.HubUser(App.Session);
            bool ShouldSaveSyncDate = false;
            user_access foundUser = null;

            var responseValSync = await hubUser.ValidaSincronizacionAsync(App.Session.CurrentUser, DateTime.Now);
            if (responseValSync.success)
            {
                foundUser = await UserDatabase.GetItemAsync(App.Session.CurrentUserFront.uid);
                if (foundUser != null)
                {
                    if (foundUser.log_fec_sincro.Date != responseValSync.data[0].datetime.Date)
                    {
                        ShouldSaveSyncDate = true;
                    }
                    else
                    {
                        //No requiere actualización
                        return;
                    }
                }
                else
                {
                    //Chao
                    return;
                }
            }

            await serverPuller.PullPromotions();

            //obj.SetTotalPercentProgress(0.30);

            try
            {
                await serverPuller.ProductMarca();
            }
            catch (Exception ex)
            {
                await Toast.Make("Error durante actualización en línea ProductMarca: " + ex.Message, ToastDuration.Long).Show();
            }

            try
            {
                await serverPuller.OnlineSyncCategoria();
            }
            catch (Exception ex)
            {
                await Toast.Make("Error durante actualización en línea OnlineSyncCategoria: " + ex.Message, ToastDuration.Long).Show();
            }

            try
            {
                await serverPuller.OnlineSyncSubcategoria();
            }
            catch (Exception ex)
            {
                await Toast.Make("Error durante actualización en línea OnlineSyncSubcategoria: " + ex.Message, ToastDuration.Long).Show();
            }

            try
            {
                await serverPuller.OnlineSyncSubcategoria();
            }
            catch (Exception ex)
            {
                await Toast.Make("Error durante actualización en línea OnlineSyncSubcategoria: " + ex.Message, ToastDuration.Long).Show();
            }

            try
            {
                await serverPuller.OnlineSyncProductLinea();
            }
            catch (Exception ex)
            {
                await Toast.Make("Error durante actualización en línea OnlineSyncProductLinea: " + ex.Message, ToastDuration.Long).Show();
            }

            try
            {
                await serverPuller.OnlineSyncProductGrupoTipo();
            }
            catch (Exception ex)
            {
                await Toast.Make("Error durante actualización en línea OnlineSyncProductGrupoTipo: " + ex.Message, ToastDuration.Long).Show();
            }

            try
            {
                await serverPuller.OnlineCalificacionCrediticia();
            }
            catch (Exception ex)
            {
                await Toast.Make("Error durante actualización en línea OnlineCalificacionCrediticia: " + ex.Message, ToastDuration.Long).Show();
            }


            //obj.SetTotalPercentProgress(0.80);
            try
            {
                await serverPuller.OnlineSyncResPartnerFull();
            }
            catch (Exception ex)
            {
                await Toast.Make("Error durante actualización en línea OnlineSyncResPartnerFull: " + ex.Message, ToastDuration.Long).Show();
            }

            try
            {
                await serverPuller.OnlineSyncResPartnerFull();
            }
            catch (Exception ex)
            {
                await Toast.Make("Error durante actualización en línea OnlineSyncResPartnerFull: " + ex.Message, ToastDuration.Long).Show();
            }

            try
            {
                await serverPuller.OnlineSyncProductPricelist();
            }
            catch (Exception ex)
            {
                await Toast.Make("Error durante actualización en línea OnlineSyncProductPricelist: " + ex.Message, ToastDuration.Long).Show();
            }

            try
            {
                await serverPuller.OnlineSyncProductPricelistItem();
            }
            catch (Exception ex)
            {
                await Toast.Make("Error durante actualización en línea OnlineSyncProductPricelistItem: " + ex.Message, ToastDuration.Long).Show();
            }

            try
            {
                await serverPuller.OnlineAccountTaxes();
            }
            catch (Exception ex)
            {
                await Toast.Make("Error durante actualización en línea OnlineAccountTaxes: " + ex.Message, ToastDuration.Long).Show();
            }

            try
            {
                await serverPuller.OnlineSyncStockWarehouse(false);
            }
            catch (Exception ex)
            {
                await Toast.Make("Error durante actualización en línea OnlineSyncStockWarehouse: " + ex.Message, ToastDuration.Long).Show();
            }

            try
            {
                await serverPuller.OnlineSyncStockLocation();
            }
            catch (Exception ex)
            {
                await Toast.Make("Error durante actualización en línea OnlineSyncStockLocation: " + ex.Message, ToastDuration.Long).Show();
            }

            try
            {
                await serverPuller.OnlineSyncStockQuant();
            }
            catch (Exception ex)
            {
                await Toast.Make("Error durante actualización en línea OnlineSyncStockQuant: " + ex.Message, ToastDuration.Long).Show();
            }

            try
            {
                await serverPuller.UomUom(true);
            }
            catch (Exception ex)
            {
                await Toast.Make("Error durante actualización en línea UomUom: " + ex.Message, ToastDuration.Long).Show();
            }

            try
            {
                await serverPuller.OnlineSyncWmsStockQuant();
            }
            catch (Exception ex)
            {
                await Toast.Make("Error durante actualización en línea OnlineSyncWmsStockQuant: " + ex.Message, ToastDuration.Long).Show();
            }

            try
            {
                await serverPuller.UpdateWmsStockQuant();
            }
            catch (Exception ex)
            {
                await Toast.Make("Error durante actualización en línea UpdateWmsStockQuant: " + ex.Message, ToastDuration.Long).Show();
            }

            try
            {
                await serverPuller.OnlineSyncProductProduct();
            }
            catch (Exception ex)
            {
                await Toast.Make("Error durante actualización en línea OnlineSyncProductProduct: " + ex.Message, ToastDuration.Long).Show();
            }

            try
            {
                //Upload
                await serverPuller.SyncSaleOrders();
            }
            catch (Exception ex)
            {
                await Toast.Make("Error durante actualización en línea SyncSaleOrders: " + ex.Message, ToastDuration.Long).Show();
            }

            if (ShouldSaveSyncDate && foundUser != null)
            {
                foundUser.log_fec_sincro = responseValSync.data[0].datetime;
                App.Session.CurrentUserFront.log_fec_sincro = foundUser.log_fec_sincro;                
                await UserDatabase.UpdateAsync(foundUser);
            }

            Pipeline pipeline = new Pipeline();
            bool requiredNewUpload = await pipeline.RequiredNewUpload();
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
