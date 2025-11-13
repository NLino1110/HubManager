using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;
using Newtonsoft.Json;
using RestSharp;
using System.IO.Compression;
using System.Diagnostics;
using DMSA.Models.Security;
using Newtonsoft.Json.Serialization;
using System.Reflection;
using DMSA.Models.General.Requests;
using DMSA.Models.Odoo.Native;
using DMSA.Models.Odoo.Tools;
using DMSA.Models.Odoo.Update;
using System.Text;
using Newtonsoft.Json.Linq;
using DMOrders.Services.Helpers;
using DMOrders.Services.Database.Sqlite;
using DMSA.Models.Odoo.Origin;
using DMOrders.Services.Update;
using DMSA.Models.Odoo.DMCobranzas;

namespace DMOrders.Pages.Sys;

public partial class UpdateData : ContentPage
{
    //TODO: Asignación provisional
    // ya que este valor cambiará dependiendo del estado de la sesión
    private string _rootUrl = "http://127.0.0.1/path/tmp/android/sqlite/";
    private bool useExternalNetworkForCache = false;

    public UpdateData()
    {
        InitializeComponent();
        lblUpdated.Text = "Ult. Actualización: " + App.Session.CurrentUser.log_fec_sincro.ToString("dd/MM/yyyy HH:mm:ss");
        //Asignación de URL de descarga según la configuración de la sesión
        //_rootUrl = App.Session.CacheFilesUrl;

        if (!App.Session.odooConnection.IsProduction)
        {
            BtnDeleteTables.IsVisible = true;
        }

        //TODO: Agregar alertas al iniciar este proceso
        //HACK
        //UNDONE
        //UnresolvedMergeConflict
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
            if (await SuggestCacheMode())
            {
                //chkUpdateBig.IsChecked = true;
                //chkUpdateFacDet.IsChecked = true;
                chkCacheMode.IsChecked = true;
            }

            if(await ServerOnlineStatus_Odoo())
            {
                BoxViewServerStatusOdoo.Color = Colors.LawnGreen;
            }
            else
            {
                await Toast.Make("Servidor Odoo no disponible", ToastDuration.Short, 14).Show();
                lblServerStatusOdoo.Text = "Servidor Odoo (x)";
            }

            if (await ServerOnlineStatus_Resources())
            {
                BoxViewServerStatusResBuilder.Color = Colors.LawnGreen;
            }
            else
            {
                await Toast.Make("Servidor de recursos no disponible", ToastDuration.Short, 14).Show();
                lblServerStatusResources.Text = "Servidor Recursos (x)";
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
            //text = "Actualización terminada...";
            string text = "Error al descargar...no se obtuvieron datos";
            toast = Toast.Make(text, duration, fontSize);
            await toast.Show(cancellationTokenSource.Token);
            return;
        }

        File.WriteAllBytes(Path.Combine(DeviceStorage, ZipFileName), fileBytes);
    }

    

    private async Task UploadDataMode1(ProgressBarAnimationBehaviorPage obj,
        IToast toast,
        ToastDuration duration,
        double fontSize,
        CancellationTokenSource cancellationTokenSource)
    {

        string textToast = "Enviando información...";
        obj.SetTitle("Iniciando envío de información");
        obj.SetPercentProgress(0.15);

        try
        {
            Debug.WriteLine("Iniciando Upload...");
            //Se obtiene de la base de datos
            AccountPaymentHeaderDb cobReciboCabDb = new AccountPaymentHeaderDb();
            var itemsDebug = await cobReciboCabDb.GetItemsAsync();
            var itemsCobros = (await cobReciboCabDb.GetItemsAsync()).Where(ic => ic.payment_status == DMSA.Models.CobrosEstados.PENDIENTE || ic.payment_status == DMSA.Models.CobrosEstados.ENVIANDO).ToList();
            if (itemsCobros.Count() > 0)
            {
                foreach (var itemCobro in itemsCobros)
                {
                    //ApiProcessor apiProcessor = new ApiProcessor();
                    //await apiProcessor.EnviarCobro(itemCobro);
                }
            }

            //TODO: Agregar envío de notas de crédito

        }
        catch (Exception ex)
        {
            textToast = "Error insert:" + ex.Message;
            toast = Toast.Make(textToast, duration, fontSize);
            await toast.Show(cancellationTokenSource.Token);
        }
    }

    //Esta clase sirve para intersectar la serialización de una clase, y permite excluir las propiedades
    // especificadas para que no sean serializadas
    public class ShouldSerializeContractResolver : DefaultContractResolver
    {
        protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
        {
            var property = base.CreateProperty(member, memberSerialization);

            // Excluir los campos especificados de la serialización
            if (property.PropertyName == "DETALLESPAGO" || property.PropertyName == "DETALLESDOCU")
            {
                property.ShouldSerialize = instance => false;
            }

            return property;
        }
    }

    


    private async Task<string[]> ExtractZipFile(
        string DeviceStorage, 
        string finalFileUrl,
        IToast toast,
        ToastDuration duration,
        double fontSize,
        CancellationTokenSource cancellationTokenSource)
    {
        try
        {
            string DeviceStorageJson = Path.Combine(DeviceStorage, "_extract", finalFileUrl + "_tmp");

            if (Directory.Exists(DeviceStorageJson))
            {
                Directory.Delete(DeviceStorageJson, true);
            }

            Directory.CreateDirectory(DeviceStorageJson);

            ZipFile.ExtractToDirectory(DeviceStorage + "/_zip/" + finalFileUrl, DeviceStorageJson);

            var ListFiles = Directory.GetFiles(DeviceStorageJson, "*.json");
            return ListFiles;
        }
        catch(Exception e)
        {
            //textToast = "ExtractZipFile:" + e.Message;
            toast = Toast.Make("ExtractZipFile:" + e.Message, duration, fontSize);
            await toast.Show(cancellationTokenSource.Token);
        }

        return null;
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

    public async Task<bool> DownloadAccountMoveRefund(AppSession _appSession, ApiRequestOdoo_v1 apiRequest)
    {
        JsonSerializerSettings settings = new JsonSerializerSettings();
        //settings.DateFormatString = "yyyy-MM-dd HH:mm:ss";
        settings.ContractResolver = new IncludeJsonIgnoreResolver();

        DateTime dateIni = DateTime.Now;
        DateTime dateEnd = DateTime.Now;
        
        ApiManager.HubAccountMoveSendHeader hubmanager = new ApiManager.HubAccountMoveSendHeader(_appSession);
        var resultCount = await hubmanager.GetHeaderCount(dateIni, dateEnd);

        Debug.WriteLine(resultCount.result);

        if (resultCount.result == 0)
        {
            return false;
        }

        int countTotal = resultCount.result / 300;

        var databaseHeader = new AccountMoveSendHeaderDb();
        var databaseMoveSend = new AccountMoveSendDb();
        var databaseMoveLineSend = new AccountMoveLineSendDb();

        for (int indice = 0; indice <= countTotal; indice++)
        {
            apiRequest.index = indice;

            var responseAll = await hubmanager.GetItemsFull(dateIni, dateEnd, indice);

            if (responseAll != null && responseAll.Length > 0)
            {
                //await database.InsertBatchAsync(responseAll.data);

                //Iniciando inserción
                foreach (var headerItem in responseAll)
                {
                    //var foundHeader = await databaseHeader.GetItemByGuidAsync(headerItem.guid);
                    var foundHeader = await databaseHeader.GetByRequestName(headerItem.request_name);
                    if (foundHeader != null)
                    {
                        Debug.WriteLine("Registro ya existe en la base de datos!, no se sincronizará");
                        Debug.WriteLine(foundHeader.request_name);
                        //Debug.WriteLine(foundHeader.recipe_name);
                        continue;
                    }

                    string jsonHeaderItem = JsonConvert.SerializeObject(headerItem); //, settings);
                    var newHeaderItem = JsonConvert.DeserializeObject<AccountMoveSendHeader>(jsonHeaderItem);

                    //var itemFound = await databaseHeader.GetItemByGuidAsync(newHeaderItem.guid);

                    //if (itemFound != null) continue;

                    newHeaderItem.was_odoo_synced = true;
                    int newHeaderId = await databaseHeader.InsertAsync(newHeaderItem);

                    //TODO: Podrian venir vacíos porque pudieron haberse borrado
                    if (headerItem.account_moves != null)
                    {
                        foreach (var paymentItem in headerItem.account_moves)
                        {
                            paymentItem.parent_id = newHeaderItem.id;
                            paymentItem.was_odoo_synced = true;
                            string jsonPaymentItem = JsonConvert.SerializeObject(paymentItem, settings); //, settings);
                            var newPaymentItem = JsonConvert.DeserializeObject<account_move_send>(jsonPaymentItem, settings);

                            int newPayId = await databaseMoveSend.InsertAsync(newPaymentItem);

                            if (paymentItem.lines != null)
                            {
                                foreach (var lineItem in paymentItem.lines)
                                {
                                    lineItem.parent_move_id = newPaymentItem.id;
                                    lineItem.was_odoo_synced = true;
                                    string jsonLineItem = JsonConvert.SerializeObject(lineItem, settings); //, settings);
                                    var newLineItem = JsonConvert.DeserializeObject<account_move_line_send>(jsonLineItem, settings);
                                    await databaseMoveLineSend.InsertAsync(newLineItem);
                                }
                            }
                        }
                    }
                }
            }

            Console.WriteLine("Página:" + indice);

            //TODO: Se fuerza la salida para que no se quede ciclado en caso de que haya
            // problemas de conexion con el servidor
            // el objetivo es que el servidor no se sobrecargue

            if (indice >= 600)
            {
                Console.WriteLine("Página " + indice + ": Se terminará el proceso.");
                break;
            }
        }

        TimeSpan span = (DateTime.Now - dateIni);

        Console.WriteLine(String.Format("Lapso transcurrido: {0} days, {1} hours, {2} minutes, {3} seconds",
            span.Days, span.Hours, span.Minutes, span.Seconds));

        return true;
    }
    
    private async void DeleteTables(object sender, EventArgs e)
    {
        bool answer = await DisplayAlert("Borrar los datos de cache?",
            "Esto permitirá volver a leer los datos de cache en la actualización, esto no afectará la base de datos.",
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

        AppSettingsDb appSettingsDb = new AppSettingsDb();
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

        await UploadDataMode1(obj, toast, duration, fontSize, cancellationTokenSource);

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

    [Obsolete]
    private async void LaunchLightUpdate(object sender, EventArgs e)
    {
        bool answer = await DisplayAlert("Actualizar Complementaria (NO IMPLEMENTADO)", "Esta actualización solo complementará los datos de las facturas faltantes desde la ultima fecha de actualizacion, está seguro que desea iniciar la actualización?", "Continuar", "Cancelar");
        //Debug.WriteLine("Answer: " + answer);
        if (!answer)
        {
            return;
        }

        DateTime dtInitialize = DateTime.Now;
        lblUpdatedInfo.Text = "Iniciada: " + dtInitialize;
        CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

        string text = "Iniciando actualización Complementaria...";
        ToastDuration duration = ToastDuration.Short;
        double fontSize = 14;
        var toast = Toast.Make(text, duration, fontSize);
        await toast.Show(cancellationTokenSource.Token);

        ProgressBarAnimationBehaviorPage obj = new ProgressBarAnimationBehaviorPage();
        //App.Current.MainPage = obj;

        await Navigation.PushModalAsync(obj, true);

        obj.SetTotalPercentProgress(0.10);

        obj.SetTotalPercentProgress(1);
        obj.SetTitle("Finalizado...");

        //Se fuerza con el DisplayAlert, la interacción con el usuario
        // no avanza hasta que se cierre la ventana
        await obj.DisplayAlert("Actualización Complementaria", "Actualización Complementaria terminada", "Aceptar");

        //TODO: Solucionar crasheo en Android
        // en modo sleep provoca crash porque al parecer no tiene nada a que hacerle Pop
        await Navigation.PopModalAsync();

        TimeSpan span = (DateTime.Now - dtInitialize);

        lblUpdatedInfo.Text += ", finalizada: " + DateTime.Now +
            " (" + String.Format("{0} días, {1} horas, {2} minutos, {3} segundos)",
            span.Days, span.Hours, span.Minutes, span.Seconds);
    }

    private async Task<bool> SuggestCacheMode()
    {
        var databaseDet = new AccountMoveLineDb();
        if ((await databaseDet.GetCount()) == 0)
        {
            return true;
        }

        var databaseCab = new AccountMoveDb();
        if ((await databaseCab.GetCount()) == 0)
        {
            return true;
        }

        return false;
    }

    private async Task<string> GetLastDate()
    {
        var databaseCab = new AccountMoveDb();
        //if (() != null)
        //{
        //   return true;
        //}
        string result = (await databaseCab.GetLastDate()).ToString("yyyy-MM-dd");

        return result;
    }

    private async void LaunchUpdate(object sender, EventArgs e)
    {
        //TODO: Funcionando pero no implementado
        //HubStatic hubStatic = new HubStatic(App.Session);
        //var resourceBytes = await hubStatic.GetBytesFromUrlAsync("tmp/android/json/data_groups_info.json");

        //Evaluar estado actual de los datos para proponer un modo u otro de actualización
        //if (await SuggestCacheMode())
        //{
        //    if (!chkUpdateBig.IsChecked || !chkUpdateFacDet.IsChecked || !chkCacheMode.IsChecked)
        //    {
        //        bool answerChange = await DisplayAlert("Cambiar modo de Actualización", "Se sugiere cambiar a modo cache ya que actualmente no tiene información. " +
        //    " Sino cambia el modo y procede a actualizar, el proceso podría ser muy lento.", "Cambiar", "No Cambiar");
        //        if (answerChange)
        //        {
        //            chkUpdateBig.IsChecked = true;
        //            chkUpdateFacDet.IsChecked = true;
        //            chkCacheMode.IsChecked = true;
        //        }
        //    }
        //}

        bool answer = await DisplayAlert("Actualizar datos de la aplicación?", "Este proceso realiza una sincronización de los datos hacia su dispositivo.", "Actualizar", "Cancelar");
        //Debug.WriteLine("Answer: " + answer);
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
        //App.Current.MainPage = obj;

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
            //TODO: Realizar proceso de cancelacion de actualización
            // ya que el servidor no esta disponible
            toast = Toast.Make("Servidor Odoo no disponible", duration, fontSize);
            await toast.Show(cancellationTokenSource.Token);

            BoxViewServerStatusOdoo.Color = Colors.SaddleBrown;
            lblServerStatusOdoo.Text = "Servidor Odoo (x)";

            await obj.DisplayAlert("Error de actualización", "El servidor de datos no está disponible.", "Aceptar");
            await Navigation.PopModalAsync();
            //return;
        }

        ServerPuller serverPuller = new ServerPuller();

        //Actualización por Cache
        if (chkGroup1.IsChecked)
        {
            await serverPuller.PullPromotions();

            //Sinó se realiza la actualización por cache, se hará la actualización en linea
            // esta actualización lleva muchisimo tiempo

            //Luego de realizar la actualización por cache debe realizarse la actualización en línea
            // debe obtenerse esta fecha de la base de datos para
            // saber cual es la ultima fecha existente en los registros
            fechaActualizaTablet = await GetLastDate();//"2023-09-04 00:00:00";

            Debug.WriteLine("Última fecha...");
            Debug.WriteLine(fechaActualizaTablet);

            if (fechaActualizaTablet == null || fechaActualizaTablet == "2021-01-01 00:00:00")
            {
                //No se encontraron datos y esto provocará una demora en la actualización
                // Mostras mensaje aquí                
                //Debug.WriteLine("Se debe cambiar la lógica porque en caso de que no existan datos la variable no tendrá 2021-01-01 00:00:00");
                fechaActualizaTablet = "2021-01-01 00:00:00";

                bool answerContinue = await DisplayAlert("Error de actualización", "Al parecer no se han insertado datos, por favor verifique su conexión de datos. Desea proceder con la actualización en línea?", "Continuar", "Cancelar");
                //Debug.WriteLine("Answer: " + answer);
                if (!answerContinue)
                {
                    //Se procede a cerrar
                    await Navigation.PopModalAsync();
                    return;
                }
            }            
        }
        
        obj.SetTotalPercentProgress(0.30);

        if (chkGroup2.IsChecked)
        {
            await serverPuller.ProductMarca();
            await serverPuller.OnlineSyncCategoria();
            await serverPuller.OnlineSyncSubcategoria();
            await serverPuller.OnlineSyncProductLinea();
            await serverPuller.OnlineSyncProductGrupoTipo();
            await serverPuller.OnlineCalificacionCrediticia();            
            obj.SetTotalPercentProgress(0.80);
        }

        if(chkGroup3.IsChecked)
        {
            await serverPuller.OnlineSyncResPartner();
        }

        if(chkGroup4.IsChecked)
        {
            await serverPuller.OnlineSyncProductPricelist();
            await serverPuller.OnlineSyncProductPricelistItem();
            await serverPuller.OnlineSyncProductProduct();
            await serverPuller.OnlineAccountTaxes();
        }

        if (chkGroup5.IsChecked)
        {
            await serverPuller.OnlineSyncStockWarehouse(false);
            await serverPuller.OnlineSyncStockLocation();
            await serverPuller.OnlineSyncStockQuant();
            await serverPuller.UomUom(true);
            //
            //await serverPuller.FixInventory();
        }

        //await RefreshVat();

        obj.SetTotalPercentProgress(1);

        obj.SetTitle("Finalizado...");

        TimeSpan span = (DateTime.Now - dtInitialize);

        lblUpdatedInfo.Text += ", finalizada: " + DateTime.Now +
            " (" + String.Format("{0} días, {1} horas, {2} minutos, {3} segundos)",
            span.Days, span.Hours, span.Minutes, span.Seconds);

        //Se fuerza con el DisplayAlert, la interacción con el usuario
        // no avanza hasta que se cierre la ventana
        await obj.DisplayAlert("Actualización", "Actualización terminada", "Aceptar");

        //TODO: Solucionar crasheo en Android
        // en modo sleep provoca crash porque al parecer no tiene nada a que hacerle Pop
        await Navigation.PopModalAsync();
    }

    private async void btnBack_Clicked(object sender, EventArgs e)
    {
        await Navigation.PopModalAsync();
    }
}