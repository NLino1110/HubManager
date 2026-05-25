using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;
using CommunityToolkit.Maui.Extensions;
using DMOrders.Controls.Alerts;
using DMOrders.Services.Helpers;
using DMOrders.Services.Update;
using DMSA.Models.Security;
using DMSA.Sync.Core.Database.Sqlite;
using DMSA.Sync.Core.Update.Cloud;
namespace DMOrders.Pages.Sys;

public partial class UpdateData : ContentPage
{
    DMSA.Sync.Core.Update.ServerPuller serverPuller { get; set; }

    private string _rootUrl = "";    

    public UpdateData()
    {
        InitializeComponent();
        lblUpdated.Text = "Ult. Actualización: " + App.Session.CurrentUserFront.log_fec_sincro.ToString("dd/MM/yyyy HH:mm:ss");        

        if (!App.Session.odooConnection.IsProduction)
        {
            BtnDeleteTables.IsVisible = true;
        }

        serverPuller = new DMSA.Sync.Core.Update.ServerPuller();
        BindingContext = this;
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

            if(await ServerOnlineStatus_Odoo())
            {
                BoxViewServerStatusOdoo.Color = Colors.LawnGreen;
            }
            else
            {
                await Toast.Make("Servidor Odoo no disponible", ToastDuration.Short, 14).Show();
                lblServerStatusOdoo.Text = "Servidor Odoo (x)";
            }

            timer.Stop();
        };
        timer.Start();
    }

    void OnNetworkCheckedChanged(object sender, CheckedChangedEventArgs e)
    {
        RadioButton rdbItem = (RadioButton)sender;

        if (rdbItem.Value.ToString() == "RedExterna" && !e.Value)
        {
            
        }

        if (rdbItem.Value.ToString() == "RedExterna" && e.Value)
        {
            
        }
    }

    [Obsolete("This method is obsolete. Use DeleteTablesAsync instead.")]
    private async void DeleteTables(object sender, EventArgs e)
    {
        bool answer = await DisplayAlert("Borrar los datos de cache?",
            "Esto permitirá volver a leer los datos de cache en la actualización, esto no afectará la base de datos.",
            "Eliminar",
            "Cancelar");
        
        if (!answer)
        {
            return;
        }

        string DeviceStorage = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "tmp");
        
        if (Directory.Exists(DeviceStorage))
        {
            Directory.Delete(DeviceStorage,true);
        }

        AppSettingsDb appSettingsDb = new AppSettingsDb(App.Session.odooConnection.DbNameSqlite);
        await appSettingsDb.TruncateAsync();

        UserAccessDb database_CobUsuarios = new UserAccessDb(App.Session.odooConnection.DbNameSqlite);
        await database_CobUsuarios.Truncate();

        CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

        string text = "Datos eliminados!";
        ToastDuration duration = ToastDuration.Short;
        double fontSize = 14;
        var toast = Toast.Make(text, duration, fontSize);
        await toast.Show(cancellationTokenSource.Token);
    }

    private async void UploadData(object sender, EventArgs e)
    {
        bool answer = await DisplayAlert("Enviar datos al servidor?",
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

        ProgressBarPage obj = new ProgressBarPage();
        
        await Navigation.PushModalAsync(obj, true);

        obj.SetTitle("Finalizado...");

        await obj.DisplayAlert("Actualización", "Actualización terminada", "Aceptar");

        await Navigation.PopModalAsync();

        toast = Toast.Make(text, duration, fontSize);
        await toast.Show(cancellationTokenSource.Token);
    }

    private async void LaunchUpdate(object sender, EventArgs e)
    {        
        bool answer = await DisplayAlert("Actualizar datos de la aplicación?", "Este proceso realiza una sincronización de los datos hacia su dispositivo.", "Actualizar", "Cancelar");
        
        if (!answer)
        {
            return;
        }

        await AppTools.ClearCacheData();

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

            await progressBarPage.DisplayAlert("Error de actualización", "El servidor de datos no está disponible.", "Aceptar");
            await Navigation.PopModalAsync();           
        }
        
        //////Pipeline pipeline = new Pipeline();

        //////bool packageReady = await pipeline.ExistAttachRecord();

        //////if(!packageReady)
        //////{                        
        //////    var packFound = await pipeline.NewestZipPack();

        //////    if (packFound != null)
        //////    {
        //////        await SqliteDbBase<object>.CloseDatabaseAsync();
        //////        progressBarPage.SetTitle("Iniciando actualización rápida...");
        //////        progressBarPage.SetTotalPercent(0.2);
                
        //////        if(await pipeline.DownloadSqliteZip(true))
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

        await progressBarPage.DisplayAlert("Actualización", "Actualización terminada", "Aceptar");        
        
        await Navigation.PopModalAsync();
    }

    private async void LaunchUpdateCatalog(object sender, EventArgs e)
    {
        var popup = new OptionPopup();

        await this.ShowPopupAsync(popup);

        var result = popup.Result;

        switch (result)
        {
            case "Total":
                await FullCatalogUpdate();
                break;
            case "Parcial":
                await PartialCatalogUpdate();
                break;
            default:
                break;
        }
    }
    
    private async Task FullCatalogUpdate()
    {
        bool answer = await DisplayAlertAsync("Actualizar total el catálogo?", "Este proceso actualizará todas las imagenes de los productos.", "Actualizar", "Cancelar");

        if (!answer)
        {
            return;
        }

        DateTime dtInitialize = DateTime.Now;
        lblUpdatedInfo.Text = "Iniciada: " + dtInitialize;

        await Toast.Make("Iniciando actualización de catálogo").Show();
        ProgressBarPage progressBarPage = new ProgressBarPage();
        await Navigation.PushModalAsync(progressBarPage, true);
        bool launchSalesUpdate = true;
        progressBarPage.SetTotalPercent(0.10);

        progressBarPage.SetTitle("Actualización en línea...");

        Pipeline pipeline = new Pipeline();        
        progressBarPage.SetTitle("Descargando paquete...");
        
        //await pipeline.DownloadFromFileMode2(App.Session.odooConnection.DbNameSqliteStatic, null, App.Session.odooConnection.DbNameSqlite);

        progressBarPage.SetTitle("Descargando en línea...");
                
        await serverPuller.OnlineSyncProductProductOnlyImagesUrl(true, async (current, total) => { await UpdateProgressState(progressBarPage, current, total, "Catálogo"); });
                
        //await pipeline.UploadToFileMode2(App.Session.odooConnection.DbNameSqliteStatic, App.Session.odooConnection.DbNameSqlite);

        progressBarPage.SetTotalPercent(1);
        progressBarPage.SetTitle("Finalizado...");

        TimeSpan span = (DateTime.Now - dtInitialize);

        lblUpdatedInfo.Text += ", finalizada: " + DateTime.Now +
            " (" + String.Format("{0} días, {1} horas, {2} minutos, {3} segundos)",
            span.Days, span.Hours, span.Minutes, span.Seconds);

        await progressBarPage.DisplayAlertAsync("Actualización", "Actualización terminada", "Aceptar");

        await Navigation.PopModalAsync();
    }

    private double getSizeLocalResource(string DatabaseFilename)
    {
        //string DatabaseFilename = App.Session.odooConnection.DbNameSqliteStatic;
        string fullPath = Path.Combine(FileSystem.AppDataDirectory, DatabaseFilename);
        var fileInfo = new FileInfo(fullPath);
        long size = fileInfo.Exists ? fileInfo.Length : 0;
        double sizeDB = size / (1024.0 * 1024.0);
        return sizeDB;
    }

    private FileInfo? GetLocalResourceFile(string databaseFilename)
    {
        if (string.IsNullOrWhiteSpace(databaseFilename))
            return null;

        try
        {
            var directory = Path.GetDirectoryName(databaseFilename);

            if (string.IsNullOrEmpty(directory))
                directory = FileSystem.AppDataDirectory;

            var fileNameOnly = Path.GetFileName(databaseFilename);

            var files = Directory.GetFiles(directory, $"{fileNameOnly}_*.zip");

            return files
                .Select(f => new FileInfo(f))
                .OrderByDescending(f => f.LastWriteTimeUtc)
                .FirstOrDefault();
        }
        catch
        {
            return null;
        }
    }

    private async Task PartialCatalogUpdate()
    {
        bool answer = await DisplayAlert("Actualizar parcial el catálogo?", "Este proceso actualizará las imagenes de los productos.", "Actualizar", "Cancelar");

        if (!answer)
        {
            return;
        }

        DateTime dtInitialize = DateTime.Now;
        lblUpdatedInfo.Text = "Iniciada: " + dtInitialize;

        await Toast.Make("Iniciando actualización de catálogo").Show();
        ProgressBarPage progressBarPage = new ProgressBarPage();
        await Navigation.PushModalAsync(progressBarPage, true);
        bool launchSalesUpdate = true;
        progressBarPage.SetTotalPercent(0.10);

        progressBarPage.SetTitle("Actualización en línea...");

        Pipeline pipeline = new Pipeline();
        //var productProductPreview = new ProductProductPreviewDb(App.Session.odooConnection.DbNameSqliteStatic);
        //double sizeDB = productProductPreview.GetDatabaseSizeMB();
        double sizeDB = getSizeLocalResource(App.Session.odooConnection.DbNameSqliteStatic);
        var tmpExists = GetLocalResourceFile(App.Session.odooConnection.DbNameSqliteStatic);

        if (sizeDB < 100)
        {
            //await SqliteDbBase<object>.CloseDatabaseAsync();
            //await pipeline.DownloadFromFile(App.Session.odooConnection.DbNameSqliteStatic, tmpExists, App.Session.odooConnection.DbNameSqlite);
            await pipeline.DownloadFromFileMode2(App.Session.odooConnection.DbNameSqliteStatic, tmpExists, App.Session.odooConnection.DbNameSqlite);

        }
        //await serverPuller.OnlineSyncProductProductOnlyImagesV2(false, async (current, total) => { await UpdateProgressState(progressBarPage, current, total, "Catálogo"); });
        await serverPuller.OnlineSyncProductProductOnlyImagesUrl(false, async (current, total) => { await UpdateProgressState(progressBarPage, current, total, "Catálogo"); });

        //await pipeline.UploadToFile(App.Session.odooConnection.DbNameSqliteStatic, App.Session.odooConnection.DbNameSqlite);
        await pipeline.UploadToFileMode2(App.Session.odooConnection.DbNameSqliteStatic, App.Session.odooConnection.DbNameSqlite);

        progressBarPage.SetTotalPercent(1);
        progressBarPage.SetTitle("Finalizado...");

        TimeSpan span = (DateTime.Now - dtInitialize);

        lblUpdatedInfo.Text += ", finalizada: " + DateTime.Now +
            " (" + String.Format("{0} días, {1} horas, {2} minutos, {3} segundos)",
            span.Days, span.Hours, span.Minutes, span.Seconds);

        await progressBarPage.DisplayAlert("Actualización", "Actualización terminada", "Aceptar");

        await Navigation.PopModalAsync();
    }

    private async Task UpdateProgressState(ProgressBarPage progressBarPage,int current,int total, string title)
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

    private async Task LaunchOnlineUpdate(ProgressBarPage progressBarPage)
    {
        if (chkGroup1.IsChecked)
        {            
            await serverPuller.PullPromotions();
        }
        
        progressBarPage.SetTotalPercent(0.30);

        if (chkGroup2.IsChecked)
        {            
            await serverPuller.ProductMarca(async (current, total) => { await UpdateProgressState(progressBarPage, current, total, "Marca"); });
            await serverPuller.OnlineSyncCategoria();
            await serverPuller.OnlineSyncSubcategoria();
            await serverPuller.OnlineSyncProductLinea();
            await serverPuller.OnlineSyncProductGrupoTipo();
            await serverPuller.OnlineCalificacionCrediticia();            
            progressBarPage.SetTotalPercent(0.80);
        }

        if(chkGroup3.IsChecked)
        {            
            //await serverPuller.OnlineSyncResPartner();
            await serverPuller.OnlineSyncResPartnerFull(async (current, total) => { await UpdateProgressState(progressBarPage, current, total, "Clientes"); });
        }

        if (chkGroup7.IsChecked)
        {
            await serverPuller.UomUom(true);
            await serverPuller.OnlineSyncProductProductNoImage(async (current, total) => { await UpdateProgressState(progressBarPage, current, total, "Productos"); });
        }

        if (chkGroup4.IsChecked)
        {            
            await serverPuller.OnlineSyncProductPricelist();
            await serverPuller.OnlineSyncProductPricelistItem(async (current, total, titleProc) => { await UpdateProgressState(progressBarPage, current, total, titleProc); });            
            await serverPuller.OnlineAccountTaxes();
        }

        if (chkGroup5.IsChecked)
        {
            await serverPuller.OnlineSyncStockWarehouse(false);
            await serverPuller.OnlineSyncStockLocation();
            await serverPuller.OnlineSyncStockQuant();
            await serverPuller.OnlineSyncWmsStockQuant(async (current, total) => { await UpdateProgressState(progressBarPage, current, total, "Stock WMS"); });
            await serverPuller.UpdateWmsStockQuant(async (current, total) => { await UpdateProgressState(progressBarPage, current, total, "UpdateWms"); });            
        }

        if(chkGroup6.IsChecked)
        {
            await serverPuller.SyncSaleOrders();
        }

        
        //////Pipeline pipeline = new Pipeline();
        //////bool requiredNewUpload = await pipeline.RequiredNewUploadCustom(App.Session.odooConnection.DbNameSqlite);
        //////if (requiredNewUpload)
        //////{
        //////    (var attachData, bool successUpload) = await pipeline.UploadSqliteZip();

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
}