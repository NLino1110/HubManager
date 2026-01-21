using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;
using DMCobranzas.Services.ApiHub;
using DMCobranzas.Settings.helpers;
using DMSA.Models.General.Requests;
using DMSA.Models.Odoo.Native;
using DMSA.Models.Odoo.Origin;
using DMSA.Models.Odoo.Update;
using DMSA.Models.Security;
using DMSA.Sync.Core.Controls;
using DMSA.Sync.Core.Database.Sqlite;
using DMSA.Sync.Core.Database.Sqlite.Payments;
using DMSA.Sync.Core.Update;
using DMSA.Sync.Core.Update.Cloud;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using RestSharp;
using System.Diagnostics;
using System.IO.Compression;
using System.Reflection;
using System.Text;
using System.Threading;
using static System.Net.Mime.MediaTypeNames;

namespace DMCobranzas.AppPages;

public partial class UpdateData : ContentPage
{
    ServerPuller serverPuller { get; set; }

    //TODO: Asignación provisional
    // ya que este valor cambiará dependiendo del estado de la sesión
    private string _rootUrl = "http://192.168.204.108:8081/MyBusinessWeb/tmp/android/sqlite/";
    private bool useExternalNetworkForCache = false;

    private int db_limit_default = 0;
    public UpdateData()
    {
        InitializeComponent();
        lblUpdated.Text = "Ult. Actualización: " + App.Session.CurrentUser.log_fec_sincro.ToString("dd/MM/yyyy HH:mm:ss");
        //Asignación de URL de descarga seg�n la configuración de la sesión
        _rootUrl = App.Session.odooConnection.DumpService;

        if (!App.Session.odooConnection.IsProduction)
        {
            BtnDeleteTables.IsVisible = true;
        }

        db_limit_default = App.Session.odooConnection.DbLimitDefault;

        //TODO: Agregar alertas al iniciar este proceso
        //HACK
        //UNDONE
        //UnresolvedMergeConflict
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

    private async Task<bool> ServerOnlineStatus_Resources()
    {
        ApiChecker apiChecker = new ApiChecker(App.Session.odooConnection.Host + "/api/status/checkonline");
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

        _rootUrl = App.Session.odooConnection.HostDump;

        if (File.Exists(Path.Combine(DeviceStorage, ZipFileName)))
            File.Delete(Path.Combine(DeviceStorage, ZipFileName));

        _rootUrl += App.Session.odooConnection.DumpService;

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
        restClientOptions.BaseUrl = new Uri($"{_rootUrl}");
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
            //text = "Actualizaci�n terminada...";
            string text = "Error al descargar...no se obtuvieron datos";
            toast = Toast.Make(text, duration, fontSize);
            await toast.Show(cancellationTokenSource.Token);
            return;
        }

        File.WriteAllBytes(Path.Combine(DeviceStorage, ZipFileName), fileBytes);
    }

    //Esta clase sirve para intersectar la serialización de una clase, y permite excluir las propiedades
    // especificadas para que no sean serializadas
    public class ShouldSerializeContractResolver : DefaultContractResolver
    {
        protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
        {
            var property = base.CreateProperty(member, memberSerialization);

            // Excluir los campos especificados de la serializaci�n
            if (property.PropertyName == "DETALLESPAGO" || property.PropertyName == "DETALLESDOCU")
            {
                property.ShouldSerialize = instance => false;
            }

            return property;
        }
    }

    public void set_to_token(JToken token, int value)
    {
        if (token is JArray array && array.Count > 0)
                {
            array[0] = value;
            }
        else if (token is JValue)
                {
            token = new JArray { value };
            }
            else
            {
            token = new JArray { value };
        }
    }

    public int get_from_token(JToken token)
    {
        if (token is JArray array && array.Count > 0)
                    {
            return array[0].Type == JTokenType.Integer ? (int)array[0] : 0;
                }

        else if (token is JValue value && value.Type == JTokenType.Boolean)
                        {
            return 0;
                    }
        return 0;
                }
                           
    private async void DeleteTables(object sender, EventArgs e)
    {
        bool answer = await DisplayAlert("Borrar los datos de cache?",
            "Esto permitir� volver a leer los datos de cache en la actualizaci�n, esto no afectar� la base de datos.",
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

        //ParametrosDb database = new ParametrosDb();
        ////Se eliminan todos los datos de la tabla antes de volver a insertar
        //await database.TruncateAsync();

        //AccountPaymentHeaderDb database_r = new AccountPaymentHeaderDb();
        //await database_r.Drop();
        //await database_r.TruncateAsync();

        //CobReciboDetDb database_rd = new CobReciboDetDb();
        //await database_rd.Drop();
        //await database_rd.TruncateAsync();

        //CobCierreDb database_cobCierreDb = new CobCierreDb();
        //await database_cobCierreDb.TruncateAsync();

        //SolicitudesNCDb database_solicitudesNC = new SolicitudesNCDb();
        //await database_solicitudesNC.Drop();
        //await database_solicitudesNC.TruncateAsync();

        UserAccessDb database_CobUsuarios = new UserAccessDb(App.Session.odooConnection.DbNameSqlite);
        //await database_CobUsuarios.Drop();

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
        //Debug.WriteLine("Answer: " + answer);
        if (!answer)
        {
            return;
        }

        CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        string text = "Datos enviados!";
        ToastDuration duration = ToastDuration.Short;
        double fontSize = 14;
        var toast = Toast.Make(text, duration, fontSize);

        ProgressBarAnimationBehaviorPage obj = new ProgressBarAnimationBehaviorPage();
        //App.Current.MainPage = obj;

        await Navigation.PushModalAsync(obj, true);

        obj.SetTitle("Finalizado...");

        //Se fuerza con el DisplayAlert, la interacción con el usuario
        // no avanza hasta que se cierre la ventana
        await obj.DisplayAlert("Actualización", "Actualización terminada", "Aceptar");

        //TODO: Solucionar crasheo en Android
        // en modo sleep provoca crash porque al parecer no tiene nada a que hacerle Pop
        await Navigation.PopModalAsync();

        toast = Toast.Make(text, duration, fontSize);
        await toast.Show(cancellationTokenSource.Token);
    }

    private async Task<string> GetLastDate()
    {
        //var databaseCab = new AccountMoveDb();
        //if (() != null)
        //{
        //   return true;
        //}
        string result = DateTime.Now.AddDays(-60).ToString("yyyy-MM-dd");

        return result;
    }

    private async void LaunchUpdate(object sender, EventArgs e)
    {        
        bool answer = await DisplayAlert("Actualizar datos de la aplicación?", "Este proceso realiza una sincronización de los datos hacia su dispositivo.", "Actualizar", "Cancelar");
        
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
        ProgressBarAnimationBehaviorPage obj = new ProgressBarAnimationBehaviorPage();        
        await Navigation.PushModalAsync(obj, true);
        bool launchSalesUpdate = true;
        obj.SetTotalPercentProgress(0.10);

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

            await obj.DisplayAlert("Error de actualización", "El servidor de datos no está disponible.", "Aceptar");
            await Navigation.PopModalAsync();           
        }

        //Esta porción de código servirá en caso de que no se haya realizado actualización por ningun medio
        //////fechaActualizaTablet = await GetLastDate();//"2023-09-04 00:00:00";

        //////Debug.WriteLine("Última fecha...");
        //////Debug.WriteLine(fechaActualizaTablet);

        //////if (fechaActualizaTablet == null || fechaActualizaTablet == "2021-01-01 00:00:00")
        //////{
        //////    fechaActualizaTablet = "2021-01-01 00:00:00";

        //////    bool answerContinue = await DisplayAlert("Error de actualización", "Al parecer no se han insertado datos, por favor verifique su conexión de datos. Desea proceder con la actualización en línea?", "Continuar", "Cancelar");

        //////    if (!answerContinue)
        //////    {
        //////        await Navigation.PopModalAsync();
        //////        return;
        //////    }
        //////}


        Pipeline pipeline = new Pipeline();

        bool packageReady = await pipeline.ExistAttachRecord();

        if(!packageReady)
        {
            //await SqliteDbBase<object>.CloseDatabaseAsync();            
            //Pipeline pipeline = new Pipeline();
                        
            var packFound = await pipeline.NewestZipPack();

            if (packFound != null)
            {
                await SqliteDbBase<object>.CloseDatabaseAsync();
                obj.SetTitle("Iniciando actualización rápida...");
                obj.SetTotalPercentProgress(0.2);
                
                if(await pipeline.DownloadSqliteZip(true))
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

        obj.SetTitle("Actualización en línea...");

        await LaunchOnlineUpdate(obj);       

        obj.SetTotalPercentProgress(1);
        obj.SetTitle("Finalizado...");

        TimeSpan span = (DateTime.Now - dtInitialize);

        lblUpdatedInfo.Text += ", finalizada: " + DateTime.Now +
            " (" + String.Format("{0} días, {1} horas, {2} minutos, {3} segundos)",
            span.Days, span.Hours, span.Minutes, span.Seconds);

        await obj.DisplayAlert("Actualización", "Actualización terminada", "Aceptar");        
        
            await Navigation.PopModalAsync();
    }

    private async Task LaunchOnlineUpdate(ProgressBarAnimationBehaviorPage obj)
    {
        CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        string text = "Actualización en linea";
        ToastDuration duration = ToastDuration.Short;
        double fontSize = 14;
        var toast = Toast.Make(text, duration, fontSize);
        await toast.Show(cancellationTokenSource.Token);

        //Actualización en linea
        if (chkGroup1.IsChecked)
        {
            //Modulos, Tipos de Modulos, Bancos, Cuentas bancarias, etc

            //YA NO SE USARÁ            
            await serverPuller.OnlineSyncFacturas(obj, toast, duration, fontSize, cancellationTokenSource);

            await serverPuller.OnlineSyncResPartnerFull();
            await serverPuller.OnlineSyncJournal();
            await serverPuller.OnlineSyncBank();
            await serverPuller.OnlineSyncAccountModule();
            await serverPuller.OnlineSyncAccountTypeModule();
            await serverPuller.OnlineSyncCompany(false);

            await serverPuller.OnlineCreditNotesRelated();

            obj.SetTotalPercentProgress(0.80);
        }

        if(chkGroup2.IsChecked)
        {
            await serverPuller.GetTarjetas();
            await serverPuller.GetTarjetasTipoPago();
            await serverPuller.GetTarjetasPlazosBanco();
            await serverPuller.OnlineSyncPaymentHeader();
            await serverPuller.OnlineSyncAccountPaymentDaily();
            await serverPuller.DownloadAccountMoveRefund();
        }

        if(chkGroup4.IsChecked)
        {
            await serverPuller.GetReceiptReceiptsLine();
        }

        //await RefreshVat();

        obj.SetTotalPercentProgress(1);

        obj.SetTitle("Finalizado...");

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
                    //TODO: Campo nombre confuso
                    //--
                    //foundUser.FECHAACTNC = responseSync.current_datetime.ToString("dd/MM/yyyy HH:mm:ss"); // DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");

                    foundUser.log_fec_sincro = responseValSync.data[0].datetime;
                    foundUser.log_fec_sincro_nc = responseSync.current_datetime;

                    App.Session.CurrentUserFront.log_fec_sincro = foundUser.log_fec_sincro;
                    App.Session.CurrentUserFront.log_fec_sincro_nc = foundUser.log_fec_sincro_nc;

                    //    await database.InsertAsync(itemInsert);
                    //}
                    //else
                    //{
                    await database.UpdateAsync(foundUser);
                }
            }
        }




        Pipeline pipeline = new Pipeline();
        bool requiredNewUpload = await pipeline.RequiredNewUpload();
        if (requiredNewUpload)
        {
            (var attachData, bool successUpload) = await pipeline.UploadSqliteZip();

            if (successUpload)
            {
                if(!await pipeline.ExistAttachRecord())
                    await pipeline.InsertAttachRecord(attachData);
                }
            }
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
        await Navigation.PopModalAsync();
    }
}