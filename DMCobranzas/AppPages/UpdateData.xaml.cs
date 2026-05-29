using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;
using DMCobranzas.Controls;
using DMCobranzas.Settings.helpers;
using DMSA.Models.Security;
using DMSA.Sync.Core.Controls;
using DMSA.Sync.Core.Database.Sqlite;
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

        DateTime dtInitialize = DateTime.Now;
        lblUpdatedInfo.Text = "Iniciada: " + dtInitialize;

        CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

        string fechaActualizaTablet = "2021-01-01 00:00:00";
        AppSession _appSession = App.Session;

        DateTime dateTimeIni = DateTime.Now;

        string text = "Iniciando actualización...";
        ToastDuration duration = ToastDuration.Short;
        double fontSize = 14;
        var toast = Toast.Make(text, duration, fontSize);
        await toast.Show(cancellationTokenSource.Token);
        ProgressBarPage progressBarPage = new ProgressBarPage();        
        await Navigation.PushModalAsync(progressBarPage, true);
        bool launchSalesUpdate = true;
        progressBarPage.SetTotalPercent(0.10);

        if (await ServerOnlineStatus_Odoo())
        {
            BoxViewServerStatusOdoo.Color = Colors.LawnGreen;
            lblServerStatusOdoo.Text = "Servidor Odoo";
        }
        else
        {            
            toast = Toast.Make("Servidor Odoo no disponible", duration, fontSize);
            await toast.Show(cancellationTokenSource.Token);

            BoxViewServerStatusOdoo.Color = Colors.SaddleBrown;
            lblServerStatusOdoo.Text = "Servidor Odoo (x)";

            await progressBarPage.DisplayAlertAsync("Error de actualización", "El servidor de datos no está disponible.", "Aceptar");
            await Navigation.PopModalAsync();           
        }

        Pipeline pipeline = new Pipeline();

        //////bool packageReady = await pipeline.ExistAttachRecord();

        //////if(!packageReady)
        //////{
                        
        //////    var packFound = await pipeline.NewestZipPack();

        //////    if (packFound != null)
        //////    {
        //////        await SqliteDbBase<object>.CloseDatabaseAsync();
        //////        progressBarPage.SetTitle("Iniciando actualización rápida...");
        //////        progressBarPage.SetTotalPercent(0.2);
                
        //////        if(await pipeline.DownloadSqliteZip(true, async (current, total) => { await UpdateProgressState(progressBarPage, current, total, "Archivos"); }))
        //////        {
        //////            await pipeline.InsertAttachRecord(packFound);
        //////        }
        //////        else
        //////        {
        //////            await Toast.Make("Hubo un error al descargar/descomprimir archivo.", duration, fontSize).Show();
        //////        }
                
        //////        await Toast.Make("Actualización rápida terminada", duration, fontSize).Show();
                
        //////        var databaseUserAccess = new UserAccessDb(App.Session.odooConnection.DbNameSqlite);
        //////        await databaseUserAccess.FixMissingCurrentUser();
        //////    }
        //////}

        progressBarPage.SetTitle("Actualización en línea...");

        await LaunchOnlineUpdate(progressBarPage);

        progressBarPage.SetTotalPercent(1);
        progressBarPage.SetTitle("Finalizado...");

        TimeSpan span = (DateTime.Now - dtInitialize);

        lblUpdatedInfo.Text += ", finalizada: " + DateTime.Now +
            " (" + String.Format("{0} días, {1} horas, {2} minutos, {3} segundos)",
            span.Days, span.Hours, span.Minutes, span.Seconds);

        await progressBarPage.DisplayAlertAsync("Actualización", "Actualización terminada", "Aceptar");        
        
        await Navigation.PopModalAsync();
    }

    private async Task LaunchOnlineUpdate(ProgressBarPage progressBarPage)
    {
        CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        string text = "Actualización en linea";
        ToastDuration duration = ToastDuration.Short;
        double fontSize = 14;
        var toast = Toast.Make(text, duration, fontSize);
        await toast.Show(cancellationTokenSource.Token);
                
        if (chkGroup1.IsChecked)
        {
            await UpdateProgressState(progressBarPage, 0, 0, "Actualización Facturas");
            await serverPuller.OnlineSyncAccountMove(async (current, total) => { await UpdateProgressState(progressBarPage, current, total, "Facturas"); });
            await serverPuller.OnlineSyncAccountMoveLine(async (current, total) => { await UpdateProgressState(progressBarPage, current, total, "Det. Facturas"); });
            await serverPuller.OnlineSyncUsers();

            await serverPuller.OnlineSyncProductProductNoImage(async (current, total) => { await UpdateProgressState(progressBarPage, current, total, "Productos"); });
            await serverPuller.OnlineSyncResPartnerFull(async (current, total) => { await UpdateProgressState(progressBarPage, current, total, "Clientes"); });
            await serverPuller.OnlineSyncJournal(async (current, total) => { await UpdateProgressState(progressBarPage, current, total, "Asientos"); });
            await serverPuller.OnlineSyncBank(async (current, total) => { await UpdateProgressState(progressBarPage, current, total, "Bancos"); });
            await serverPuller.OnlineSyncCompany(false);
            await serverPuller.OnlineCreditNotesRelated(async (current, total) => { await UpdateProgressState(progressBarPage, current, total, "Credito Data"); });
            await serverPuller.GetFullResCenterLine(true, async (current, total) => { await UpdateProgressState(progressBarPage, current, total, "Centros de Recursos"); });
            progressBarPage.SetTotalPercent(0.80);
        }

        if(chkGroup2.IsChecked)
        {
            await serverPuller.GetTarjetas(async (current, total) => { await UpdateProgressState(progressBarPage, current, total, "Tarjetas"); });
            await serverPuller.GetTarjetasTipoPago(async (current, total) => { await UpdateProgressState(progressBarPage, current, total, "Tipos de Pago"); });
            await serverPuller.GetTarjetasPlazosBanco(async (current, total) => { await UpdateProgressState(progressBarPage, current, total, "Plazos Banco"); });            
            await serverPuller.OnlineSyncAccountPaymentDaily(async (current, total) => { await UpdateProgressState(progressBarPage, current, total, "Pagos Diarios"); });
            await serverPuller.DownloadAccountMoveRefund(async (current, total) => { await UpdateProgressState(progressBarPage, current, total, "Reembolsos NC"); });
        }

        if (chkGroup3.IsChecked)
        {
            await serverPuller.GetCities(async (current, total) => { await UpdateProgressState(progressBarPage, current, total, "Ciudades"); });
        }

        if(chkGroup4.IsChecked)
        {
            await serverPuller.GetReceiptReceiptsLine(async (current, total) => { await UpdateProgressState(progressBarPage, current, total, "Recibos Lines"); });
            await serverPuller.OnlineAccountTaxes(async (current, total) => { await UpdateProgressState(progressBarPage, current, total, "Impuestos"); });
            await serverPuller.OnlineSyncPaymentHeader(async (current, total) => { await UpdateProgressState(progressBarPage, current, total, "Cobros"); });
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


        Pipeline pipeline = new Pipeline();
        //////bool requiredNewUpload = await pipeline.RequiredNewUploadCustom(App.Session.odooConnection.DbNameSqlite);
        //////if (requiredNewUpload)
        //////{
        //////    (var attachData, bool successUpload) = await pipeline.UploadSqliteZip(async (current, total) => { await UpdateProgressState(progressBarPage, current, total, "Paquetes (upload)"); });

        //////    if (successUpload)
        //////    {
        //////        if(!await pipeline.ExistAttachRecord())
        //////            await pipeline.InsertAttachRecord(attachData);
        //////    }
        //////}
    }

    private async void btnUploadPipeline_Clicked(object sender, EventArgs e)
    {
        Pipeline pipeline = new Pipeline();
        await pipeline.UploadSqliteZip();
    }

    private async void btnFromPipeline_Clicked(object sender, EventArgs e)
    {
        await SqliteDbBase<object>.CloseDatabaseAsync();
        Pipeline pipeline = new Pipeline();
        await pipeline.DownloadSqliteZip(true);
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