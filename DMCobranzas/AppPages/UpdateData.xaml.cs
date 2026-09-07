using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;
using DMCobranzas.Controls;
using DMCobranzas.Settings.helpers;
using DMSA.Models.Odoo.Accounting;
using DMSA.Models.Security;
using DMSA.Sync.Core.Controls;
using DMSA.Sync.Core.Database.Sqlite;
using DMSA.Sync.Core.Database.Sqlite.Payments;
using DMSA.Sync.Core.Update;
using DMSA.Sync.Core.Update.Cloud;
using RestSharp;
using System.Diagnostics;

namespace DMCobranzas.AppPages;

public partial class UpdateData : ContentPage
{
    ServerPuller serverPuller { get; set; }

    private bool useExternalNetworkForCache = false;

    public UpdateData()
    {
        InitializeComponent();
        lblUpdated.Text = "Ult. Actualización: " + App.Session.CurrentUserFront.log_fec_sincro.ToString("dd/MM/yyyy HH:mm:ss");        
        serverPuller = new DMSA.Sync.Core.Update.ServerPuller();
        ConfigureInvoiceDateRangeUi();
    }

    /// <summary>
    /// Filtro Desde/Hasta por invoice_date. REVERTIR: EnableInvoiceDateRangeSync = true en ServerPuller.
    /// </summary>
    private void ConfigureInvoiceDateRangeUi()
    {
        bool enabled = ServerPuller.EnableInvoiceDateRangeSync;
        brInvoiceDateInfo.IsVisible = enabled;
        grInvoiceDatePickers.IsVisible = enabled;

        if (enabled)
            InitializeInvoiceDatePickers();
    }

    private void InitializeInvoiceDatePickers()
    {
        ApplyDefaultInvoiceDateFrom();
    }

    private void ApplyDefaultInvoiceDateFrom()
    {
        var today = DateTime.Today;
        dpInvoiceDateTo.Date = today;
        dpInvoiceDateTo.MaximumDate = today;

        if (App.Session?.odooConnection == null)
        {
            dpInvoiceDateFrom.Date = today.AddMonths(-5);
            return;
        }

        var database = new AccountMoveDb(App.Session.odooConnection.DbNameSqlite);
        bool firstSyncOfDay = database.IsFirstAccountMoveSyncOfDay();

        // 1ra del día: periodo amplio (5 meses). Siguientes: solo lo del día (parcial), editable.
        dpInvoiceDateFrom.Date = firstSyncOfDay
            ? today.AddMonths(-5)
            : today;
    }

    private static DateTime GetPickerDate(DatePicker picker)
    {
        return (picker.Date ?? DateTime.Today).Date;
    }

    private bool TryGetInvoiceSyncDateRange(out DateTime dateFrom, out DateTime dateTo)
    {
        dateFrom = GetPickerDate(dpInvoiceDateFrom);
        dateTo = GetPickerDate(dpInvoiceDateTo);
        return true;
    }

    private async Task<bool> ValidateInvoiceSyncDateRangeAsync()
    {
        if (!chkGroup1.IsChecked && !chkGroup2.IsChecked)
            return true;

        var dateFrom = GetPickerDate(dpInvoiceDateFrom);
        var dateTo = GetPickerDate(dpInvoiceDateTo);

        if (dateFrom > dateTo)
        {
            await DisplayAlertAsync(
                "Fechas invalidas",
                "La fecha Desde no puede ser mayor que la fecha Hasta.",
                "Aceptar");
            return false;
        }

        return true;
    }

    private async Task<bool> ServerOnlineStatus_Odoo()
    {
        ApiChecker apiChecker = new ApiChecker(App.Session.odooConnection.Host + "/connect/checkonline");
        bool isOnline = await apiChecker.IsApiAvailable();

        return isOnline;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        if (ServerPuller.EnableInvoiceDateRangeSync)
            ApplyDefaultInvoiceDateFrom();

        IDispatcherTimer timer;

        timer = Dispatcher.CreateTimer();
        timer.Interval = TimeSpan.FromMilliseconds(500);
        timer.IsRepeating = false;
        timer.Tick += async (s, e) =>
        {
            //if (await SuggestCacheMode())
            //{
            //    //chkUpdateBig.IsChecked = true;
            //    //chkUpdateFacDet.IsChecked = true;
            //    chkCacheMode.IsChecked = true;
            //}

            if(await ServerOnlineStatus_Odoo())
            {
                BoxViewServerStatusOdoo.Color = Colors.LawnGreen;
            }
            else
            {
                await Toast.Make("Servidor Odoo no disponible", ToastDuration.Short, 14).Show();
                lblServerStatusOdoo.Text = "Servidor Odoo (x)";
            }

            //if (await ServerOnlineStatus_Resources())
            //{
            //    BoxViewServerStatusResBuilder.Color = Colors.LawnGreen;
            //}
            //else
            //{
            //    await Toast.Make("Servidor de recursos no disponible", ToastDuration.Short, 14).Show();
            //    lblServerStatusResources.Text = "Servidor Recursos (x)";
            //}

            timer.Stop();
        };
        timer.Start();
    }

    void OnNetworkCheckedChanged(object sender, CheckedChangedEventArgs e)
    {
        RadioButton rdbItem = (RadioButton)sender;

        if (rdbItem.Value.ToString() == "RedExterna" && !e.Value)
        {
            useExternalNetworkForCache = false;
        }

        if (rdbItem.Value.ToString() == "RedExterna" && e.Value)
        {
            useExternalNetworkForCache = true;
        }

        Debug.WriteLine(rdbItem.Value + " " + useExternalNetworkForCache.ToString());

        // Perform required operation
    }

    private async Task DownloadResource(ProgressBarAnimationBehaviorPage obj,
        IToast toast,
        ToastDuration duration,
        double fontSize,
        CancellationTokenSource cancellationTokenSource,
        string DeviceStorage,
        string ZipFileName)
    {
        string FinalDirectory = Path.Combine(DeviceStorage, ZipFileName);

        if (!Directory.Exists(Path.GetDirectoryName(FinalDirectory)))
        {
            Directory.CreateDirectory(Path.GetDirectoryName(FinalDirectory));
        }

        //_rootUrl = App.Session.odooConnection.HostDump;

        if (File.Exists(Path.Combine(DeviceStorage, ZipFileName)))
            File.Delete(Path.Combine(DeviceStorage, ZipFileName));

        //_rootUrl += App.Session.odooConnection.DumpService;

        //TODO: Deprecated
        //if (useExternalNetworkForCache)
        //{
        //    _rootUrl += App.Session.CacheFilesUrlExternal;
        //}
        //else
        //{

        //}
                
        obj.SetSubTitle("Descargando " + ZipFileName);
        obj.SetPercentProgress(0.10);

        //TODO: ByPass SSL
        RestClientOptions restClientOptions = new RestClientOptions();
        restClientOptions.RemoteCertificateValidationCallback += (sender, certificate, chain, sslPolicyErrors) => true;
        //restClientOptions.BaseUrl = new Uri($"{_rootUrl}");
        //restClientOptions.MaxTimeout = 30000;
        restClientOptions.Timeout = TimeSpan.FromMilliseconds(30000);

        RestClient restClient = new RestClient(restClientOptions);

        //var responseFile = restClient.Execute(new RestRequest(ZipFileName, Method.Get), new CancellationToken()
        //{

        //});

        var responseFile = await restClient.ExecuteAsync(new RestRequest(ZipFileName, Method.Get));

        //var fileBytes = restClient.DownloadData(new RestRequest(ZipFileName, Method.Get));
        var fileBytes = responseFile.RawBytes;

        if (fileBytes == null || fileBytes.Length == 0)
        {
            //await Navigation.PopModalAsync();
            //text = "Actualización terminada...";
            string text = "Error al descargar...no se obtuvieron datos";
            toast = Toast.Make(text, duration, fontSize);
            await toast.Show(cancellationTokenSource.Token);
            return;
        }

        File.WriteAllBytes(Path.Combine(DeviceStorage, ZipFileName), fileBytes);
    }
                               
    private async void DeleteTables(object sender, EventArgs e)
    {
        bool answer = await DisplayAlertAsync("Borrar tablas?",
            "Este proceso eliminará todos los datos actuales",
            "Eliminar",
            "Cancelar");
        //Debug.WriteLine("Answer: " + answer);
        if (!answer)
        {
            return;
        }

        //Eliminar datos de los archivos de actualizacion
        string DeviceStorage = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "tmp");
        
        if (Directory.Exists(DeviceStorage))
        {
            Directory.Delete(DeviceStorage,true);
        }

        AppSettingsDb appSettingsDb = new AppSettingsDb(App.Session.odooConnection.DbNameSqlite);
        await appSettingsDb.TruncateAsync();

        CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

        string text = "Datos eliminados!";
        ToastDuration duration = ToastDuration.Short;
        double fontSize = 14;
        var toast = Toast.Make(text, duration, fontSize);
        await toast.Show(cancellationTokenSource.Token);
    }

    private async void UploadData(object sender, EventArgs e)
    {
        bool answer = await DisplayAlertAsync("Enviar datos al servidor?",
            "Esto realizará la sincronización con el servidor (Odoo).",
            "Sincronizar",
            "Cancelar");

        if (!answer)
        {
            return;
        }

        CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        string text = "Datos enviados!";
        ToastDuration duration = ToastDuration.Short;
        double fontSize = 14;
        var toast = Toast.Make(text, duration, fontSize);

        ProgressBarPage progressBarPage = new ProgressBarPage();

        await Navigation.PushModalAsync(progressBarPage, true);

        progressBarPage.SetTitle("Finalizado...");
        await progressBarPage.DisplayAlertAsync("Actualización", "Actualización terminada", "Aceptar");
        await Navigation.PopModalAsync();

        toast = Toast.Make(text, duration, fontSize);
        await toast.Show(cancellationTokenSource.Token);
    }

    private async void LaunchUpdate(object sender, EventArgs e)
    {        
        bool answer = await DisplayAlertAsync("Actualizar datos de la aplicación?", "Este proceso realiza una sincronización de los datos hacia su dispositivo.", "Actualizar", "Cancelar");
        
        if (!answer)
        {
            return;
        }

        if (ServerPuller.EnableInvoiceDateRangeSync && !await ValidateInvoiceSyncDateRangeAsync())
        {
            return;
        }

        DateTime dtInitialize = DateTime.Now;
        lblUpdatedInfo.Text = "Iniciada: " + dtInitialize;

        CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        ToastDuration duration = ToastDuration.Short;
        double fontSize = 14;

        var toast = Toast.Make("Iniciando actualización...", duration, fontSize);
        await toast.Show(cancellationTokenSource.Token);

        bool retry;
        do
        {
            retry = false;
            var progressBarPage = new ProgressBarPage();
            await Navigation.PushModalAsync(progressBarPage, true);
            progressBarPage.SetTotalPercent(0.10);

            try
            {
                var outcome = await ExecuteUpdateAsync(progressBarPage, dtInitialize, cancellationTokenSource, duration, fontSize);

                if (outcome == UpdateOutcome.Success)
                {
                    await progressBarPage.DisplayAlertAsync("Actualización", "Actualización terminada", "Aceptar");
                    if (ServerPuller.EnableInvoiceDateRangeSync)
                        ApplyDefaultInvoiceDateFrom();
                }
                else
                {
                    string message = outcome == UpdateOutcome.ServerOffline
                        ? "El servidor de datos no está disponible."
                        : "No se pudo completar la actualización.";

                    retry = await progressBarPage.DisplayAlertAsync(
                        "Error de actualización",
                        message,
                        "Reintentar",
                        "Cancelar");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[LaunchUpdate] {ex.Message}\n{ex.StackTrace}");
                try
                {
                    await Toast.Make("Error en actualización: " + ex.Message, duration, fontSize).Show(cancellationTokenSource.Token);
                    retry = await progressBarPage.DisplayAlertAsync(
                        "Error de actualización",
                        ex.Message,
                        "Reintentar",
                        "Cancelar");
                }
                catch (Exception alertEx)
                {
                    Debug.WriteLine($"[LaunchUpdate] Error al mostrar alerta: {alertEx.Message}");
                }
            }
            finally
            {
                try
                {
                    await Navigation.PopModalAsync();
                }
                catch (Exception popEx)
                {
                    Debug.WriteLine($"[LaunchUpdate] Error al cerrar modal: {popEx.Message}");
                }
            }
        } while (retry);
    }

    private enum UpdateOutcome
    {
        Success,
        ServerOffline
    }

    private async Task<UpdateOutcome> ExecuteUpdateAsync(
        ProgressBarPage progressBarPage,
        DateTime dtInitialize,
        CancellationTokenSource cancellationTokenSource,
        ToastDuration duration,
        double fontSize)
    {
        if (await ServerOnlineStatus_Odoo())
        {
            BoxViewServerStatusOdoo.Color = Colors.LawnGreen;
            lblServerStatusOdoo.Text = "Servidor Odoo";
        }
        else
        {
            var toast = Toast.Make("Servidor Odoo no disponible", duration, fontSize);
            await toast.Show(cancellationTokenSource.Token);

            BoxViewServerStatusOdoo.Color = Colors.SaddleBrown;
            lblServerStatusOdoo.Text = "Servidor Odoo (x)";

            return UpdateOutcome.ServerOffline;
        }

        var pipeline = new Pipeline();

        bool packageReady = await pipeline.ExistAttachRecord();
        bool isValidData = await pipeline.IsValidData();

        if (!packageReady && !isValidData)
        {
            var packFound = await pipeline.NewestZipPack();

            if (packFound != null)
            {
                await SqliteDbBase<object>.CloseDatabaseAsync();
                progressBarPage.SetTitle("Iniciando actualización rápida...");
                progressBarPage.SetTotalPercent(0.2);

                if (await pipeline.DownloadSqliteZip(
                    packFound,
                    true,
                    async (current, total) => { await UpdateProgressState(progressBarPage, current, total, "Archivos"); }))
                {
                    await pipeline.InsertAttachRecord(packFound);
                    await pipeline.ResetUserData();
                }
                else
                {
                    await Toast.Make("Hubo un error al descargar/descomprimir archivo.", duration, fontSize).Show();
                }

                await Toast.Make("Actualización rápida terminada", duration, fontSize).Show();

                var databaseUserAccess = new UserAccessDb(App.Session.odooConnection.DbNameSqlite);
                await databaseUserAccess.FixMissingCurrentUser();
            }
        }

        progressBarPage.SetTitle("Actualización en línea...");

        DateTime? invoiceDateFrom = null;
        DateTime? invoiceDateTo = null;
        if (ServerPuller.EnableInvoiceDateRangeSync
            && TryGetInvoiceSyncDateRange(out DateTime from, out DateTime to))
        {
            invoiceDateFrom = from;
            invoiceDateTo = to;
        }

        await LaunchOnlineUpdate(progressBarPage, invoiceDateFrom, invoiceDateTo);

        progressBarPage.SetTotalPercent(1);
        progressBarPage.SetTitle("Finalizado...");

        TimeSpan span = (DateTime.Now - dtInitialize);

        lblUpdatedInfo.Text += ", finalizada: " + DateTime.Now +
            " (" + String.Format("{0} días, {1} horas, {2} minutos, {3} segundos)",
            span.Days, span.Hours, span.Minutes, span.Seconds);

        return UpdateOutcome.Success;
    }

    private async Task LaunchOnlineUpdate(
        ProgressBarPage progressBarPage,
        DateTime? invoiceDateFrom = null,
        DateTime? invoiceDateTo = null)
    {
        try
        {
        CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        string text = "Actualización en linea";
        ToastDuration duration = ToastDuration.Short;
        double fontSize = 14;
        var toast = Toast.Make(text, duration, fontSize);
        await toast.Show(cancellationTokenSource.Token);
                
        if (chkGroup1.IsChecked)
        {
            await UpdateProgressState(progressBarPage, 0, 0, "Actualización " + AccountMoveDocumentDisplay.BulkSyncHeadersProgressLabel);
            await serverPuller.OnlineSyncAccountMove(
                async (current, total) =>
                {
                    await UpdateProgressState(progressBarPage, current, total, AccountMoveDocumentDisplay.BulkSyncHeadersProgressLabel);
                },
                invoiceDateFrom,
                invoiceDateTo);
            progressBarPage.SetTotalPercent(0.80);
        }

        if(chkGroup2.IsChecked)
        {
            await serverPuller.OnlineSyncAccountMoveLine(
                async (current, total) =>
                {
                    await UpdateProgressState(progressBarPage, current, total, AccountMoveDocumentDisplay.BulkSyncDetailsProgressLabel);
                },
                invoiceDateFrom,
                invoiceDateTo);
        }

        if (chkGroup3.IsChecked)
        {
            await serverPuller.OnlineSyncUsers();
            await serverPuller.OnlineSyncProductProductNoImage(async (current, total) => { await UpdateProgressState(progressBarPage, current, total, "Productos"); });            
            await serverPuller.OnlineSyncJournal(async (current, total) => { await UpdateProgressState(progressBarPage, current, total, "Asientos"); });
            await serverPuller.OnlineSyncBank(async (current, total) => { await UpdateProgressState(progressBarPage, current, total, "Bancos"); });
            await serverPuller.OnlineSyncCompany(false);
            await serverPuller.OnlineCreditNotesRelated(async (current, total) => { await UpdateProgressState(progressBarPage, current, total, "Credito Data"); });

            await serverPuller.GetTarjetas(async (current, total) => { await UpdateProgressState(progressBarPage, current, total, "Tarjetas"); });
            await serverPuller.GetTarjetasTipoPago(async (current, total) => { await UpdateProgressState(progressBarPage, current, total, "Tipos de Pago"); });
            await serverPuller.GetTarjetasPlazosBanco(async (current, total) => { await UpdateProgressState(progressBarPage, current, total, "Plazos Banco"); });            

            await serverPuller.GetCities(async (current, total) => { await UpdateProgressState(progressBarPage, current, total, "Ciudades"); });
            await serverPuller.GetFullResCenterLine(true, async (current, total) => { await UpdateProgressState(progressBarPage, current, total, "Centros de Recursos"); });
        }

        if(chkGroup4.IsChecked)
        {
            await serverPuller.GetReceiptReceiptsLine(async (current, total) => { await UpdateProgressState(progressBarPage, current, total, "Recibos Lines"); });
            await serverPuller.OnlineAccountTaxes(async (current, total) => { await UpdateProgressState(progressBarPage, current, total, "Impuestos"); });
            await serverPuller.OnlineSyncPaymentHeader(async (current, total) => { await UpdateProgressState(progressBarPage, current, total, "Cobros"); });
        }

        if (chkGroup5.IsChecked)
        {
            // ANTES: OnlineSyncResPartnerFull (mismo proceso de Órdenes, filtro write_date).
            // DESPUÉS: OnlineSyncResPartnerCobranzasAll
            //   Fase 1: search_read (datos maestros)
            //   Fase 2: web_read (saldos) — revertir con EnableResPartnerCobranzasSaldosWebRead = false
            await serverPuller.OnlineSyncResPartnerCobranzasAll(
                async (current, total) => { await UpdateProgressState(progressBarPage, current, total, "Clientes (datos)"); },
                async (current, total) => { await UpdateProgressState(progressBarPage, current, total, "Clientes (saldos)"); });
        }

        if (chkGroup6.IsChecked)
        {
            await serverPuller.OnlineSyncAccountPaymentDaily(async (current, total) => { await UpdateProgressState(progressBarPage, current, total, "Pagos Diarios"); });
            await serverPuller.DownloadAccountMoveRefund(async (current, total) => { await UpdateProgressState(progressBarPage, current, total, "Reembolsos NC"); });
        }

        progressBarPage.SetTotalPercent(1);
        progressBarPage.SetTitle("Finalizado...");

        ApiManager.HubUser hubUser = new ApiManager.HubUser(App.Session);
        var responseValSync = await hubUser.ValidaSincronizacionAsync(App.Session.CurrentUser, DateTime.Now);
        if (responseValSync.success)
        {
            var responseSync = await hubUser.actualizaFechaSincroNotaCredito(App.Session.CurrentUser, DateTime.Now);

            if (responseSync.success)
            {
                var database = new UserAccessDb(App.Session.odooConnection.DbNameSqlite);
                var foundUser = await database.GetItemAsync(App.Session.CurrentUserFront.uid);
                if (foundUser != null)
                {
                    foundUser.log_fec_acceso = responseSync.current_datetime;
                    foundUser.log_fec_sincro = responseValSync.data[0].datetime;
                    foundUser.log_fec_sincro_nc = responseSync.current_datetime;

                    App.Session.CurrentUserFront.log_fec_sincro = foundUser.log_fec_sincro;
                    App.Session.CurrentUserFront.log_fec_sincro_nc = foundUser.log_fec_sincro_nc;

                    await database.UpdateAsync(foundUser);
                }
            }
        }

        var pipeline = new Pipeline();
        bool requiredNewUpload = await pipeline.RequiredNewUploadCustom(App.Session.odooConnection.DbNameSqlite);
        if (requiredNewUpload)
        {
            //(var attachData, bool successUpload) = await pipeline.UploadSqliteZip(async (current, total) => { await UpdateProgressState(progressBarPage, current, total, "Paquetes (upload)"); });
            bool successUpload = await pipeline.UploadToFileMode2(App.Session.odooConnection.DbNameSqlite, 
                App.Session.odooConnection.DbNameSqlite);

            if (successUpload)
            {
                //if (!await pipeline.ExistAttachRecord())
                //    await pipeline.InsertAttachRecordCustom(attachData);
            }
        }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[LaunchOnlineUpdate] {ex.Message}\n{ex.StackTrace}");
            throw;
        }
    }

    private async void btnUploadPipeline_Clicked(object sender, EventArgs e)
    {
        var pipeline = new Pipeline();
        await pipeline.UploadSqliteZip();
    }

    private async void btnFromPipeline_Clicked(object sender, EventArgs e)
    {
        await SqliteDbBase<object>.CloseDatabaseAsync();
        //Pipeline pipeline = new Pipeline();
        //await pipeline.DownloadSqliteZip(true);
    }

    private async void btnBack_Clicked(object sender, EventArgs e)
    {
        btnBack.IsEnabled = false;
        await Navigation.PopModalAsync();
    }

    private async Task UpdateProgressState(ProgressBarPage progressBarPage, int current, int total, string title)
    {
        if (total <= 0) total = 1;

        current = Math.Min(current, total - 1);

        int displayPage = current + 1;
        double percent = (double)displayPage / total;

        await MainThread.InvokeOnMainThreadAsync(() =>
        {
            progressBarPage.SetSubTitle($"{title} - Página {displayPage} de {total}");
            progressBarPage.SetPercent(percent);
        });
    }
}