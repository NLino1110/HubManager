using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;
using Newtonsoft.Json;
using RestSharp;
using System.IO.Compression;
using System.Diagnostics;
using DMSA.Models.Security;
using DMCobranzas.Services.ApiHub;
using Newtonsoft.Json.Serialization;
using System.Reflection;
using DMSA.Models.General.Requests;
using DMSA.Models.Odoo.Native;
using DMCobranzas.Settings.helpers;
using DMSA.Models.Odoo.Update;
using System.Text;
using Newtonsoft.Json.Linq;
using DMSA.Models.Odoo.Origin;
using DMSA.Sync.Core.Database.Sqlite;
using DMSA.Sync.Core.Database.Sqlite.Payments;
using DMSA.Sync.Core.Controls;

namespace DMCobranzas.AppPages;

public partial class UpdateData : ContentPage
{
    DMSA.Sync.Core.Update.ServerPuller serverPuller { get; set; }

    //TODO: Asignación provisional
    // ya que este valor cambiará dependiendo del estado de la sesión
    private string _rootUrl = "http://192.168.204.108:8081/MyBusinessWeb/tmp/android/sqlite/";
    private bool useExternalNetworkForCache = false;

    private int db_limit_default = 0;
    public UpdateData()
    {
        InitializeComponent();
        lblUpdated.Text = "Ult. Actualización: " + App.Session.CurrentUser.log_fec_sincro.ToString("dd/MM/yyyy HH:mm:ss");
        //Asignación de URL de descarga según la configuración de la sesión
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
        ApiChecker apiChecker = new ApiChecker(App.Session.odooConnection.HostDump + "/api/status/checkonline");
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

        //string ZipFileName = "FACNOTACREDITODET.zip";
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

    private async Task ProcPartner(ProgressBarAnimationBehaviorPage obj, string[] ListFiles)
    {
        var database = new ResPartnerDb(App.Session.odooConnection.DbNameSqlite);
        //if (ListFiles.Length > 0)
        //{
        //    await database.Truncate();
        //}
        int fileIndex = 1;

        foreach (var fileNameJson in ListFiles)
        {
            Debug.WriteLine("Procesando archivo de cache:");
            Debug.WriteLine(fileNameJson);

            //database = null;
            //database = new FacNotaCreditoDetDb();
            obj.SetTitle($"Proc. {Path.GetFileName(fileNameJson)} ({fileIndex}/{ListFiles.Length})");

            //string jsonFileItem = File.ReadAllText(Path.Combine(DeviceStorageJson, ZipFileName.Replace(".zip", ".json")));
            string jsonFileItem = File.ReadAllText(fileNameJson);
            var listObjects = JsonConvert.DeserializeObject<PartnerOrigin>(jsonFileItem);

            int totalItems = listObjects.res_partner.Length;
            int curIndex = 1;
            double percentProcess = 0;

            try
            {
                List<res_partner> res_partnerFinal = new List<res_partner>();

                foreach (var partnerItem in listObjects.res_partner)
                {
                    if (partnerItem.vat != null && partnerItem.vat.Trim().Equals("false"))
                    {
                        partnerItem.vat = "";
                    }

                    if (partnerItem.email != null && partnerItem.email.Trim().Equals("false"))
                    {
                        partnerItem.email = "";
                    }

                    if (partnerItem.street != null && partnerItem.street.Trim().Equals("false"))
                    {
                        partnerItem.street = "";
                    }

                    if (partnerItem.street2 != null && partnerItem.street2.Trim().Equals("false"))
                    {
                        partnerItem.street2 = "";
                    }

                    //if ( partnerItem.company_id == 0)
                    //{
                    //    //TODO: Crear loop para agregar las compañias existentes
                    //    partnerItem.company_id = 1;
                    //    res_partnerFinal.Add(partnerItem);

                    //    res_partner res_PartnerNew = new res_partner();
                    //    res_PartnerNew.id = partnerItem.id;
                    //    res_PartnerNew.name = partnerItem.name;
                    //    res_PartnerNew.vat = partnerItem.vat;
                    //    res_PartnerNew.street = partnerItem.street;
                    //    res_PartnerNew.street2 = partnerItem.street2;
                    //    res_PartnerNew.email = partnerItem.email;
                    //    res_PartnerNew.credit = partnerItem.credit;
                    //    res_PartnerNew.debit = partnerItem.debit;
                    //    res_PartnerNew.total_invoiced = partnerItem.total_invoiced;
                    //    res_PartnerNew.total_due = partnerItem.total_due;
                    //    res_PartnerNew.total_overdue = partnerItem.total_overdue;
                    //    res_PartnerNew.user_id = partnerItem.user_id;
                    //    res_PartnerNew.user_login = partnerItem.user_login;
                    //    res_PartnerNew.total_to_beat = partnerItem.total_to_beat;

                    //    res_PartnerNew.positive_balance = partnerItem.positive_balance;
                    //    res_PartnerNew.client_type_id = partnerItem.client_type_id;
                    //    res_PartnerNew.client_type_name = partnerItem.client_type_name;

                    //    res_PartnerNew.company_id = 2;

                    //    res_partnerFinal.Add(res_PartnerNew);
                    //}
                    //else
                    //{
                    res_partnerFinal.Add(partnerItem);
                    //}
                }

                //res_partner[] res_partner
                //await database.InsertBatchAsync(listObjects.res_partner);
                await database.InsertBatchAsync(res_partnerFinal.ToArray());
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Debug.WriteLine(ex.Message);
                Debug.WriteLine("Error: provocado por " + fileNameJson);
                Debug.WriteLine("Error: " + listObjects.res_partner);
            }

            fileIndex++;
        }
    }

    private async Task ProcProducts(ProgressBarAnimationBehaviorPage obj, string[] ListFiles)
    {
        var database = new ProductTemplateDb(App.Session.odooConnection.DbNameSqlite);
        //if (ListFiles.Length > 0)
        //{
        //    await database.Truncate();
        //}
        int fileIndex = 1;

        foreach (var fileNameJson in ListFiles)
        {
            Debug.WriteLine("Procesando archivo de cache:");
            Debug.WriteLine(fileNameJson);
                        
            obj.SetTitle($"Proc. {Path.GetFileName(fileNameJson)} ({fileIndex}/{ListFiles.Length})");

            //string jsonFileItem = File.ReadAllText(Path.Combine(DeviceStorageJson, ZipFileName.Replace(".zip", ".json")));
            string jsonFileItem = File.ReadAllText(fileNameJson);
            var listObjects = JsonConvert.DeserializeObject<ProductTemplateOrigin>(jsonFileItem);

            int totalItems = listObjects.product_template.Length;
            int curIndex = 1;
            double percentProcess = 0;

            try
            {
                List<product_template> res_partnerFinal = new List<product_template>();

                res_partnerFinal = listObjects.product_template.ToList();

                //foreach (var partnerItem in listObjects.product_template)
                //{
                    //if (partnerItem.vat != null && partnerItem.vat.Trim().Equals("false"))
                    //{
                    //    partnerItem.vat = "";
                    //}

                    //if (partnerItem.email != null && partnerItem.email.Trim().Equals("false"))
                    //{
                    //    partnerItem.email = "";
                    //}

                    //if (partnerItem.street != null && partnerItem.street.Trim().Equals("false"))
                    //{
                    //    partnerItem.street = "";
                    //}

                    //if (partnerItem.street2 != null && partnerItem.street2.Trim().Equals("false"))
                    //{
                    //    partnerItem.street2 = "";
                    //}

                    //Debug.WriteLine(partnerItem.id);
                    //res_partnerFinal.Add(partnerItem);
                //}

                //res_partner[] res_partner
                //await database.InsertBatchAsync(listObjects.res_partner);
                await database.InsertBatchAsync(res_partnerFinal.ToArray());
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Debug.WriteLine(ex.Message);
                Debug.WriteLine("Error: provocado por " + fileNameJson);
                Debug.WriteLine("Error: " + listObjects.product_template);
            }

            fileIndex++;
        }
    }

    private account_move[] FixAccountMove(account_move[] account_Moves)
    {
        foreach (var amItem in account_Moves)
        {
            //if (amItem.reversed_entry_id != null && amItem.reversed_entry_id.Length > 0)
            //{
            //    amItem._reversed_entry_id = amItem.reversed_entry_id[0].id;
            //}

            //if (amItem.partner_id != null && amItem.partner_id.Length > 0)
            //{
            //    amItem._partner_id = amItem.partner_id[0].id;
            //}

            //if (amItem.journal_id != null && amItem.journal_id.Length > 0)
            //{
            //    amItem._journal_id = amItem.journal_id[0].id;
            //}

            //if (amItem.l10n_latam_document_type_id != null && amItem.l10n_latam_document_type_id.Length > 0)
            //{
            //    amItem._l10n_latam_document_type_id = amItem.l10n_latam_document_type_id[0].id;
            //}

            //if (amItem.invoice_user_id != null && amItem.invoice_user_id.Length > 0)
            //{
            //    amItem._invoice_user_id = amItem.invoice_user_id[0].id;
            //}

            //if (amItem.printer_id != null && amItem.printer_id.Length > 0)
            //{
            //    amItem._printer_id = amItem.printer_id[0].id;
            //}

            //if (amItem.printer_id != null && amItem.printer_id.Length > 0)
            //{
            //    amItem._printer_id = amItem.printer_id[0].id;
            //}

            //if (amItem.company_id != null && amItem.company_id.Length > 0)
            //{
            //    amItem._company_id = amItem.company_id[0].id;
            //}

            //if (amItem.team_id != null && amItem.team_id.Length > 0)
            //{
            //    amItem._team_id = amItem.team_id[0].id;
            //}
        }

        return account_Moves;
    }

    private async Task ProcAccountMove(ProgressBarAnimationBehaviorPage obj, string[] ListFiles)
    {
        var database = new AccountMoveDb(App.Session.odooConnection.DbNameSqlite);
        //if (ListFiles.Length > 0)
        //{
        //    await database.Truncate();
        //}
        int fileIndex = 1;

        foreach (var fileNameJson in ListFiles)
        {
            Debug.WriteLine("Procesando archivo de cache:");
            Debug.WriteLine(fileNameJson);

            //database = null;
            //database = new FacNotaCreditoDetDb();
            obj.SetTitle($"Proc. {Path.GetFileName(fileNameJson)} ({fileIndex}/{ListFiles.Length})");

            //string jsonFileItem = File.ReadAllText(Path.Combine(DeviceStorageJson, ZipFileName.Replace(".zip", ".json")));
            string jsonFileItem = File.ReadAllText(fileNameJson);
            var listObjects = JsonConvert.DeserializeObject<AccountMoveOrigin>(jsonFileItem);

            int totalItems = listObjects.account_move.Length;
            int curIndex = 1;
            double percentProcess = 0;

            try
            {
                listObjects.account_move = FixAccountMove(listObjects.account_move);

                await database.InsertBatchAsync(listObjects.account_move);
                //await database.UpdateAllAsync(listObjects.FACNOTACREDITODET);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Debug.WriteLine(ex.Message);
                Debug.WriteLine("Error: provocado por " + fileNameJson);
                Debug.WriteLine("Error: " + listObjects.account_move);
            }

            fileIndex++;
        }
    }

    private async Task ProcAccountJournal(ProgressBarAnimationBehaviorPage obj, string[] ListFiles)
    {
        var database = new AccountJournalDb(App.Session.odooConnection.DbNameSqlite);
        
        //if (ListFiles.Length > 0)
        //{
        //    await database.Truncate();
        //}

        int fileIndex = 1;

        foreach (var fileNameJson in ListFiles)
        {
            Debug.WriteLine("Procesando archivo de cache:");
            Debug.WriteLine(fileNameJson);

            obj.SetTitle($"Proc. {Path.GetFileName(fileNameJson)} ({fileIndex}/{ListFiles.Length})");

            //string jsonFileItem = File.ReadAllText(Path.Combine(DeviceStorageJson, ZipFileName.Replace(".zip", ".json")));
            string jsonFileItem = File.ReadAllText(fileNameJson);
            var listObjects = JsonConvert.DeserializeObject<AccountJournalOrigin>(jsonFileItem);

            int totalItems = listObjects.account_journal.Length;
            int curIndex = 1;
            double percentProcess = 0;

            try
            {
                //listObjects.account_journal = FixAccountMove(listObjects.account_journal);
                //await database.InsertBatchAsync(listObjects.account_journal);
                //await database.UpdateAllAsync(listObjects.account_journal);
                
                List<string> accounts_journal_ids_list = new List<string>();
                InboundPaymentMethodDb inboundPaymentMethodDb = new InboundPaymentMethodDb(App.Session.odooConnection.DbNameSqlite);
                await inboundPaymentMethodDb.Truncate();

                foreach (var itemData in listObjects.account_journal)
                {
                    //Se evalúa si debe usarse en la app
                    if (!itemData.use_mobile_app)
                    {
                        var isForApp = itemData.mobile_app_tag_ids.Where(i => i.code == App.Session.AppCodeOdoo).FirstOrDefault();
                        if (isForApp != null)
                        {

                        }
                        else
                        {
                            continue;
                        }
                    }

                    //accountJournalDb.InsertAsync(itemData);
                    //itemData._bank_account_id = 0;
                    //if (itemData.bank_account_id.Count > 0)
                    //{
                    //    itemData._bank_account_id = itemData.bank_account_id.FirstOrDefault().id;

                    //    //Se agrega a la lista
                    //    accounts_journal_ids_list.Add(itemData.bank_account_id.FirstOrDefault().id.ToString());
                    //}

                    //itemData._company_id = 0;
                    //if (itemData.company_id.Count > 0)
                    //{
                    //    itemData._company_id = itemData.company_id.FirstOrDefault().id;
                    //}

                    //if (itemData.inbound_payment_method_line_ids.Count > 0)
                    //{
                    //    itemData.inbound_payment_method_line_ids.ForEach(x => x.parent_id = itemData.id);
                    //    await inboundPaymentMethodDb.InsertBatchAsync(itemData.inbound_payment_method_line_ids.ToArray());
                    //}

                    //Solo se insertarán las cuentas que tengan habilitado su uso en las apps móviles
                    await database.InsertAsync(itemData);
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Debug.WriteLine(ex.Message);
                Debug.WriteLine("Error: provocado por " + fileNameJson);
                Debug.WriteLine("Error: " + listObjects.account_journal);
            }

            fileIndex++;
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

    private async Task ProcAccountMoveLine(ProgressBarAnimationBehaviorPage obj, string[] ListFiles)
    {
        var database = new AccountMoveLineDb(App.Session.odooConnection.DbNameSqlite);
        //if (ListFiles.Length > 0)
        //{
        //    await database.Truncate();
        //}
        int fileIndex = 1;

        foreach (var fileNameJson in ListFiles)
        {
            Debug.WriteLine("Procesando archivo de cache:");
            Debug.WriteLine(fileNameJson);

            //database = null;
            //database = new FacNotaCreditoDetDb();
            obj.SetTitle($"Proc. {Path.GetFileName(fileNameJson)} ({fileIndex}/{ListFiles.Length})");

            //string jsonFileItem = File.ReadAllText(Path.Combine(DeviceStorageJson, ZipFileName.Replace(".zip", ".json")));
            string jsonFileItem = File.ReadAllText(fileNameJson);
            var listObjects = JsonConvert.DeserializeObject<AccountMoveLineOrigin>(jsonFileItem);

            int totalItems = listObjects.account_move_line.Length;
            int curIndex = 1;
            double percentProcess = 0;

            try
            {
                foreach (var amlItem in listObjects.account_move_line)
                {
                    amlItem._product_id = get_from_token(amlItem.product_id);
                    amlItem._account_id = get_from_token(amlItem.account_id);
                    amlItem._move_id = get_from_token(amlItem.move_id);

                    //if (amlItem.product_id != null && amlItem.product_id.Length > 0)
                    //{
                    //    amlItem.productId = amlItem.product_id[0].id;
                    //}

                    //if (amlItem.account_id != null && amlItem.account_id.Length > 0)
                    //{
                    //    amlItem.accountId = amlItem.account_id[0].id;
                    //}

                    //if (amlItem.move_id != null && amlItem.move_id.Length > 0)
                    //{
                    //    amlItem.moveId = amlItem.move_id[0].id;
                    //}
                }

                await database.InsertBatchAsync(listObjects.account_move_line);
                //await database.UpdateAllAsync(listObjects.FACNOTACREDITODET);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Debug.WriteLine(ex.Message);
                Debug.WriteLine("Error: provocado por " + fileNameJson);
                Debug.WriteLine("Error: " + listObjects.account_move_line);
                //Se inicia el intento de insersión individual

                //TODO: Crear metodo para esta clase
                //await TryIndividualInsertAsync(listObjects.account_move);
            }

            fileIndex++;
        }
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
            AccountPaymentHeaderDb cobReciboCabDb = new AccountPaymentHeaderDb(App.Session.odooConnection.DbNameSqlite);
            var itemsDebug = await cobReciboCabDb.GetItemsAsync();
            var itemsCobros = (await cobReciboCabDb.GetItemsAsync()).Where(ic => ic.payment_status == DMSA.Models.CobrosEstados.PENDIENTE || ic.payment_status == DMSA.Models.CobrosEstados.ENVIANDO).ToList();
            if (itemsCobros.Count() > 0)
            {
                foreach (var itemCobro in itemsCobros)
                {
                    ApiProcessor apiProcessor = new ApiProcessor();
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

    private List<Tuple<int, int, int>> ConstruirListaFechas()
    {
        int limitBackYear = 2;

        List<Tuple<int, int, int>> fechas = new List<Tuple<int, int, int>>();

        // Fecha actual hasta ayer
        DateTime fechaActual = DateTime.Now.Date.AddDays(-1);

        // Hace dos años
        DateTime fechaInicio = fechaActual.AddYears(-limitBackYear).AddMonths(1).AddDays(-fechaActual.Day + 1);

        // Agregar todas las fechas desde hace dos años hasta ayer
        for (DateTime fecha = fechaInicio; fecha <= fechaActual; fecha = fecha.AddDays(1))
        {
            fechas.Add(new Tuple<int, int, int>(fecha.Year, fecha.Month, fecha.Day));
        }

        return fechas;
    }

    private List<Tuple<int, int, int>> RequiredDataUpdate(string current_model)
    {
        var listaFechas = ConstruirListaFechas();

        //foreach (var fechaUnica in listaFechas)
        //{
        //    fechas.RemoveAll(fecha => fecha.Item1 == fechaEncontrada.Item1 && fecha.Item2 == fechaEncontrada.Item2 && fecha.Item3 == fechaEncontrada.Item3);
        //}

        //int anios_retraso = 1;

        List<Tuple<int, int, int>> fechasEncontradas = new List<Tuple<int, int, int>>();

        //string current_model = "account_move";

        var directorio = Path.Combine(Directory.GetCurrentDirectory(),
            "wwwroot/resources",
            "tmp",
            "android", "sqlite", current_model);

        if (!Directory.Exists(directorio))
        {
            Directory.CreateDirectory(directorio);
        }

        // Obtener todos los archivos zip en el directorio y subdirectorios
        string[] archivosZip = Directory.GetFiles(directorio, "*.zip", SearchOption.AllDirectories);

        foreach (string carpetaAnio in Directory.EnumerateDirectories(directorio))
        {
            string nombreAnio = Path.GetFileNameWithoutExtension(carpetaAnio);
            if (!int.TryParse(nombreAnio, out int anio))
            {
                // Manejar error si el nombre del año no tiene el formato correcto
                Console.WriteLine($"Error al procesar carpeta: {carpetaAnio}");
                continue;
            }

            foreach (string carpetaMes in Directory.EnumerateDirectories(carpetaAnio))
            {
                string nombreMes = Path.GetFileNameWithoutExtension(carpetaMes);
                if (!int.TryParse(nombreMes, out int mes))
                {
                    // Manejar error si el nombre del mes no tiene el formato correcto
                    continue;
                }

                foreach (string fileItem in Directory.EnumerateFiles(carpetaMes))
                {
                    string nombreFile = Path.GetFileNameWithoutExtension(fileItem);

                    if (!int.TryParse(nombreFile, out int dia))
                    {
                        continue;
                    }

                    //Console.WriteLine($"{anio}-{mes:00}: {String.Join(", ", dia)}");
                    Console.WriteLine($"{anio}-{mes:00}: {dia:00}");
                    fechasEncontradas.Add(new Tuple<int, int, int>(anio, mes, dia));
                }
            }
        }

        // Mostrar las fechas encontradas
        //Console.WriteLine("Fechas encontradas:");
        //foreach (var fecha in fechasEncontradas)
        //{
        //    Console.WriteLine($"{fecha.Item1}-{fecha.Item2}-{fecha.Item3}");
        //}

        foreach (var fechaEncontrada in fechasEncontradas)
        {
            listaFechas.RemoveAll(fecha => fecha.Item1 == fechaEncontrada.Item1 && fecha.Item2 == fechaEncontrada.Item2 && fecha.Item3 == fechaEncontrada.Item3);
        }

        //Console.WriteLine("Fechas finales:");
        //foreach (var fecha in listaFechas)
        //{
        //    Console.WriteLine($"{fecha.Item1}-{fecha.Item2}-{fecha.Item3}");
        //}

        //Console.ReadLine();

        return listaFechas;
    }

    //public static List<fileData> GetDistinctFiles(update_pack_info source, update_pack_info destination)
    //{
    //    var distinctFiles = new List<fileData>();

    //    var allModels = source.details.Select(d => d.model).Union(destination.details.Select(d => d.model)).Distinct();

    //    foreach (var model in allModels)
    //    {
    //        var sourceDetail = source.details.FirstOrDefault(d => d.model == model);
    //        var destDetail = destination.details.FirstOrDefault(d => d.model == model);

    //        var sourceFiles = sourceDetail?.files ?? Array.Empty<fileData>();
    //        var destFiles = destDetail?.files ?? Array.Empty<fileData>();

    //        foreach (var sourceFile in sourceFiles)
    //        {
    //            var destFile = destFiles.FirstOrDefault(f =>
    //                f.name == sourceFile.name &&
    //                f.year == sourceFile.year &&
    //                f.month == sourceFile.month &&
    //                f.day == sourceFile.day);

    //            if (destFile == null ||
    //                destFile.create_date != sourceFile.create_date ||
    //                destFile.hash != sourceFile.hash)
    //            {
    //                distinctFiles.Add(sourceFile);
    //            }
    //        }

    //        foreach (var destFile in destFiles)
    //        {
    //            var sourceFile = sourceFiles.FirstOrDefault(f =>
    //                f.name == destFile.name &&
    //                f.year == destFile.year &&
    //                f.month == destFile.month &&
    //                f.day == destFile.day);

    //            if (sourceFile == null ||
    //                sourceFile.create_date != destFile.create_date ||
    //                sourceFile.hash != destFile.hash)
    //            {
    //                distinctFiles.Add(destFile);
    //            }
    //        }
    //    }

    //    return distinctFiles;
    //}

    public static update_pack_info GetDistinctUpdatePackInfo(update_pack_info source, update_pack_info destination)
    {
        var distinctUpdatePackInfo = new update_pack_info
        {
            pack_base_date = DateTime.Now,
            description = "Distinct Files Update",
            details = new List<Detail>().ToArray()
        };

        var allModels = source.details.Select(d => d.model).Union(destination.details.Select(d => d.model)).Distinct();
        var distinctDetails = new List<Detail>();

        foreach (var model in allModels)
        {
            var sourceDetail = source.details.FirstOrDefault(d => d.model == model);
            var destDetail = destination.details.FirstOrDefault(d => d.model == model);

            var sourceFiles = sourceDetail?.files ?? Array.Empty<fileData>();
            var destFiles = destDetail?.files ?? Array.Empty<fileData>();

            var distinctFiles = new List<fileData>();

            foreach (var sourceFile in sourceFiles)
            {
                var destFile = destFiles.FirstOrDefault(f =>
                    f.name == sourceFile.name &&
                    f.year == sourceFile.year &&
                    f.month == sourceFile.month &&
                    f.day == sourceFile.day);

                if (destFile == null ||
                    destFile.create_date != sourceFile.create_date ||
                    destFile.hash != sourceFile.hash)
                {
                    distinctFiles.Add(sourceFile);
                }
            }

            foreach (var destFile in destFiles)
            {
                var sourceFile = sourceFiles.FirstOrDefault(f =>
                    f.name == destFile.name &&
                    f.year == destFile.year &&
                    f.month == destFile.month &&
                    f.day == destFile.day);

                if (sourceFile == null ||
                    sourceFile.create_date != destFile.create_date ||
                    sourceFile.hash != destFile.hash)
                {
                    distinctFiles.Add(destFile);
                }
            }

            if (distinctFiles.Any())
            {
                distinctDetails.Add(new Detail
                {
                    model = model,
                    total_files = distinctFiles.Count,
                    files = distinctFiles.ToArray()
                });
            }
        }

        distinctUpdatePackInfo.details = distinctDetails.ToArray();
        return distinctUpdatePackInfo;
    }

    public static void UpdateFiles(update_pack_info source, update_pack_info destination)
    {
        foreach (var sourceDetail in source.details)
        {
            var destDetail = destination.details.FirstOrDefault(d => d.model == sourceDetail.model);
            if (destDetail != null)
            {
                var updatedFiles = destDetail.files.ToList();
                foreach (var sourceFile in sourceDetail.files)
                {
                    var destFile = updatedFiles.FirstOrDefault(f => f.name == sourceFile.name && 
                    f.year == sourceFile.year &&
                    f.month == sourceFile.month &&
                    f.day == sourceFile.day);
                    if (destFile != null)
                    {
                        if (destFile.create_date != sourceFile.create_date || destFile.hash != sourceFile.hash)
                        {
                            // Update destination file data
                            destFile.create_date = sourceFile.create_date;
                            destFile.hash = sourceFile.hash;
                            destFile.year = sourceFile.year;
                            destFile.month = sourceFile.month;
                            destFile.day = sourceFile.day;
                        }
                    }
                    else
                    {
                        // Add new file data to destination
                        updatedFiles.Add(sourceFile);
                    }
                }
                destDetail.files = updatedFiles.ToArray();
                destDetail.total_files = destDetail.files.Length;
            }
            else
            {
                // Add new detail to destination
                var updatedDetails = destination.details.ToList();
                updatedDetails.Add(sourceDetail);
                destination.details = updatedDetails.ToArray();
            }
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


    private async Task LaunchCacheModeByChunks(ProgressBarAnimationBehaviorPage obj,
        IToast toast,
        ToastDuration duration,
        double fontSize,
        CancellationTokenSource cancellationTokenSource)
    {

        string textToast = "Actualización por cache iniciada...";
        await Toast.Make(textToast, duration, fontSize).Show();
        //Se crea el directorio para descargas
        string DeviceStorage = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "tmp");
        System.IO.Directory.CreateDirectory(DeviceStorage);

        obj.SetTitle("Iniciando descarga de archivos de cache...");

        string[] models = {
            "account_move",
            "account_move_line",
            "account_journal",
            "res_partner",
            "product_template"
        };

        if (Directory.Exists(Path.Combine(DeviceStorage, "_bulk")))
        {
            Directory.Delete(Path.Combine(DeviceStorage, "_bulk"), true);
        }

        Directory.CreateDirectory(Path.Combine(DeviceStorage, "_bulk"));

        if (!Directory.Exists(Path.Combine(DeviceStorage, "_bulk_data")))
        {
            Directory.CreateDirectory(Path.Combine(DeviceStorage, "_bulk_data"));
        }

        foreach (string current_model in models)
        {
            obj.SetSubTitle("Leyendo informacion de " + current_model);

            try
            {
                string base_filename = "_bulk/" + current_model + "_base.json";
                string update_filename = "_bulk/" + current_model + "_update.json";

                await DownloadResource(obj,
                    toast,
                    duration,
                    fontSize,
                    cancellationTokenSource,
                    DeviceStorage,
                    base_filename);

                await DownloadResource(obj,
                    toast,
                    duration,
                    fontSize,
                    cancellationTokenSource,
                    DeviceStorage,
                    update_filename);

                //Procesar lista de archivos para ser descargados

                update_pack_info update_Pack_Info_base = null;
                update_pack_info update_Pack_Info_data = null;

                if (File.Exists(Path.Combine(DeviceStorage, base_filename)))
                {
                    string base_filename_content = File.ReadAllText(Path.Combine(DeviceStorage, base_filename));

                    update_Pack_Info_base = JsonConvert.DeserializeObject<update_pack_info>(base_filename_content);

                    if (File.Exists(Path.Combine(DeviceStorage, update_filename)))
                    {
                        string update_filename_content = File.ReadAllText(Path.Combine(DeviceStorage, update_filename));
                        if(update_filename_content != "")
                        {
                            update_pack_info update_Pack_Info_update = JsonConvert.DeserializeObject<update_pack_info>(update_filename_content);
                            UpdateFiles(update_Pack_Info_update, update_Pack_Info_base);
                        }
                    }
                }

                bool ShouldCreateFile = false;
                if (ShouldCreateFile || !File.Exists(Path.Combine(DeviceStorage, "_bulk_data/" + current_model + "_base.json")))
                {
                    update_pack_info update_Pack_Info_update_new = new update_pack_info();
                    update_Pack_Info_update_new.pack_base_date = DateTime.Now;
                    update_Pack_Info_update_new.description = update_Pack_Info_base.description;
                    update_Pack_Info_update_new.details = new Detail[1] {
                        new Detail()
                        { 
                            files = new fileData[0],
                            model = current_model,
                            total_files = 0
                        }
                    };
                    //update_Pack_Info_update_new.details[0].files = ;
                    //Se crea archivo de dato para indicar la ultima actualizacion
                    //if (!File.Exists(Path.Combine(DeviceStorage, "_bulk_data/" + current_model + "_base.json")))
                    //{
                    //Sino existe o si es una actualizacion forzada se almacena nuevo
                    string base_filename_content_mixed = JsonConvert.SerializeObject(update_Pack_Info_update_new);
                    File.WriteAllBytes(Path.Combine(DeviceStorage, "_bulk_data/" + current_model + "_base.json"),
                        Encoding.ASCII.GetBytes(base_filename_content_mixed));
                    //}
                }

                string data_base_filename_content = File.ReadAllText(Path.Combine(DeviceStorage, "_bulk_data/" + current_model + "_base.json"));
                update_Pack_Info_data = JsonConvert.DeserializeObject<update_pack_info>(data_base_filename_content);

                update_pack_info update_Pack_Info_data_tmp = JsonConvert.DeserializeObject<update_pack_info>(data_base_filename_content);

                UpdateFiles(update_Pack_Info_base, update_Pack_Info_data);

                //Hubo cambios (solo se comparan los detalles)
                if (update_Pack_Info_data.details != update_Pack_Info_data_tmp.details)
                {
                    update_Pack_Info_data.pack_base_date = DateTime.Now;
                    //Se vuelve a guardar el archivo de datos locales con los nuevos datos de actualización
                    string base_filename_content_mixed_new = JsonConvert.SerializeObject(update_Pack_Info_data);
                    File.WriteAllBytes(Path.Combine(DeviceStorage, "_bulk_data/" + current_model + "_base.json"),
                        Encoding.ASCII.GetBytes(base_filename_content_mixed_new));
                }

                //Solo se intentarán actualizar los archivos diferenciados
                var distinctFiles = GetDistinctUpdatePackInfo(update_Pack_Info_data_tmp, update_Pack_Info_data);

                //Console.WriteLine(distinctFiles);

                //Listado de archivos que serán actualizados
                //Download
                foreach (var detail in distinctFiles.details)
                {
                    foreach (var fileItem in detail.files)
                    {
                        try
                        {
                            string finalFileUrl = detail.model + "/" +
                                fileItem.year.ToString() + "/" +
                                fileItem.month.ToString() + "/" +
                                fileItem.name.ToString() + "";

                            //Descarga/Descrompresión/Insersión de cada uno de los archivos de la lista
                            await DownloadResource(obj,
                                toast,
                                duration,
                                fontSize,
                                cancellationTokenSource,
                                DeviceStorage + "/_zip",
                                finalFileUrl);
                        }
                        catch (Exception ex)
                        {
                            textToast = "Error download:" + ex.Message;
                            toast = Toast.Make(textToast, duration, fontSize);
                            await toast.Show(cancellationTokenSource.Token);

                            //Se elimina el archivo que provocó el error
                            //update_Pack_Info_data.

                            continue;
                        }
                    }
                }

                //Extract
                foreach (var detail in distinctFiles.details)
                {
                    int account_move_count = 0;
                    int account_move_line_count = 0;
                    int res_partner_count = 0;
                    int account_journal_count = 0;
                    int product_template_count = 0;

                    foreach (var fileItem in detail.files)
                    {
                        string finalFileUrl = detail.model + "/" +
                            fileItem.year.ToString() + "/" +
                            fileItem.month.ToString() + "/" +
                            fileItem.name.ToString() + "";
                           
                        string DeviceStorageJson = Path.Combine(DeviceStorage, "_extract", finalFileUrl + "_tmp");

                        var ListFiles = await ExtractZipFile(DeviceStorage, finalFileUrl, toast, duration, fontSize, cancellationTokenSource);

                        //Console.WriteLine(ListFiles);

                        switch (detail.model)
                        {
                            case "account_move":
                                {
                                    if(account_move_count == 0)
                                    {
                                        var database = new AccountMoveDb(App.Session.odooConnection.DbNameSqlite);
                                        await database.Truncate();
                                    }
                                    await ProcAccountMove(obj, ListFiles);
                                    account_move_count++;
                                }
                                break;
                            case "account_move_line":
                                {
                                    if (account_move_line_count == 0)
                                    {
                                        var database = new AccountMoveLineDb(App.Session.odooConnection.DbNameSqlite);
                                        await database.Truncate();
                                    }
                                    await ProcAccountMoveLine(obj, ListFiles);
                                    account_move_line_count++;
                                }
                                break;
                            case "res_partner":
                                {
                                    if (res_partner_count == 0)
                                    {
                                        var database = new ResPartnerDb(App.Session.odooConnection.DbNameSqlite);
                                        await database.Truncate();
                                    }
                                    await ProcPartner(obj, ListFiles);
                                    res_partner_count++;
                                }
                                break;
                            case "account_journal":
                                {
                                    if (account_journal_count == 0)
                                    {
                                        var database = new AccountJournalDb(App.Session.odooConnection.DbNameSqlite);
                                        await database.Truncate();
                                    }
                                    await ProcAccountJournal(obj, ListFiles);
                                    account_journal_count++;
                                }
                                break;
                            case "product_template":
                                {
                                    if (product_template_count == 0)
                                    {
                                        var database = new ProductTemplateDb(App.Session.odooConnection.DbNameSqlite);
                                        await database.Truncate();
                                    }
                                    await ProcProducts(obj, ListFiles);
                                    product_template_count++;
                                }
                                break;
                        }

                        //Eliminar carpeta descomprimida temporal
                        Directory.Delete(DeviceStorageJson, true);
                    }
                }
            }
            catch (Exception ex)
            {
                textToast = "Error download:" + ex.Message;
                toast = Toast.Make(textToast, duration, fontSize);
                await toast.Show(cancellationTokenSource.Token);
                continue;
            }
        }
    }

    [Obsolete]
    private async Task LaunchCacheMode(ProgressBarAnimationBehaviorPage obj,
        IToast toast,
        ToastDuration duration,
        double fontSize,
        CancellationTokenSource cancellationTokenSource)
    {

        string textToast = "Actualización por cache iniciada...";
        await Toast.Make(textToast, duration, fontSize).Show();
        //Se crea el directorio para descargas
        string DeviceStorage = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "tmp");
        System.IO.Directory.CreateDirectory(DeviceStorage);

        obj.SetTitle("Iniciando descarga de archivos de cache...");

        //TODO: Revisar si es que se continua usando para importar (excluidos mientras)...
        // "COBCARTERACAB.zip",
        // "COBCARTERADET.zip",

        string[] ZipFiles = {
            "account_move.zip",
            "account_move_line.zip",
            "res_partner.zip",
            "account_journal.zip",
            "product_template.zip"
        };

        foreach (string ZipFileName in ZipFiles)
        {
            obj.SetSubTitle("Descargando " + ZipFileName);

            try
            {
                await DownloadResource(obj,
                    toast,
                    duration,
                    fontSize,
                    cancellationTokenSource,
                    DeviceStorage,
                    ZipFileName);

                obj.SetTitle("Descomprimiendo e insertando " + ZipFileName);
                obj.SetPercentProgress(0.15);
                string DeviceStorageJson = Path.Combine(DeviceStorage, "json", ZipFileName + "_tmp");

                if (Directory.Exists(DeviceStorageJson))
                {
                    Directory.Delete(DeviceStorageJson, true);
                }

                Directory.CreateDirectory(DeviceStorageJson);

                if (File.Exists(Path.Combine(DeviceStorage, ZipFileName)))
                {
                    Debug.WriteLine(Path.Combine(DeviceStorage, ZipFileName));
                    Debug.WriteLine("Archivo encontrado");
                }

                //TODO: Sería útil una validación del zip, porque es posible que el archivo 
                //  sea devuelto corrompido, ya se solucionó desde el servidor, pero es aún
                //  una posibilidad. Sin dicha validación (try) la aplicación crashea.
                ZipFile.ExtractToDirectory(Path.Combine(DeviceStorage, ZipFileName), DeviceStorageJson);

                var ListFiles = Directory.GetFiles(DeviceStorageJson, "*.json");

            }
            catch (Exception ex)
            {
                textToast = "Error download:" + ex.Message;
                toast = Toast.Make(textToast, duration, fontSize);
                await toast.Show(cancellationTokenSource.Token);
                break;
            }
        }

        foreach (string ZipFileName in ZipFiles)
        {
            obj.SetSubTitle("Descargando " + ZipFileName);

            try
            {
                string DeviceStorageJson = Path.Combine(DeviceStorage, "json", ZipFileName + "_tmp");

                var ListFiles = Directory.GetFiles(DeviceStorageJson, "*.json");

                switch (ZipFileName)
                {
                    case "account_move.zip":
                        {
                            await ProcAccountMove(obj, ListFiles);
                        }
                        break;
                    case "account_move_line.zip":
                        {
                            await ProcAccountMoveLine(obj, ListFiles);
                        }
                        break;
                    case "res_partner.zip":
                        {
                            await ProcPartner(obj, ListFiles);
                        }
                        break;
                    case "account_journal.zip":
                        {
                            await ProcAccountJournal(obj, ListFiles);
                        }
                        break;
                    case "product_template.zip":
                        {
                            await ProcProducts(obj, ListFiles);
                        }
                        break;
                }

                //Eliminar carpeta descomprimida temporal
                Directory.Delete(DeviceStorageJson, true);
            }
            catch (Exception ex)
            {
                textToast = "Error insert:" + ex.Message;
                toast = Toast.Make(textToast, duration, fontSize);
                await toast.Show(cancellationTokenSource.Token);
                break;
            }

            textToast = "Actualización por cache terminada " + ZipFileName;
            //text = "Insertado en " + ZipFileName + " " + await database.GetCount();
            toast = Toast.Make(textToast, duration, fontSize);
            await toast.Show(cancellationTokenSource.Token);
        }
    }
    
    [Obsolete]
    private async Task LaunchCacheMode_v2(ProgressBarAnimationBehaviorPage obj,
        IToast toast,
        ToastDuration duration,
        double fontSize,
        CancellationTokenSource cancellationTokenSource)
    {

        string textToast = "Actualización por cache iniciada...";
        await Toast.Make(textToast, duration, fontSize).Show();
        //Se crea el directorio para descargas
        string DeviceStorage = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "tmp");
        System.IO.Directory.CreateDirectory(DeviceStorage);

        obj.SetTitle("Iniciando descarga de archivos de cache...");

        //TODO: Revisar si es que se continua usando para importar (excluidos mientras)...
        // "COBCARTERACAB.zip",
        // "COBCARTERADET.zip",

        string[] ZipFiles = {
            "account_move.zip",
            "account_move_line.zip",
            "res_partner.zip",
            "account_journal.zip",
            "product_template.zip"
        };

        foreach (string ZipFileName in ZipFiles)
        {
            obj.SetSubTitle("Descargando " + ZipFileName);

            try
            {
                await DownloadResource(obj,
                    toast,
                    duration,
                    fontSize,
                    cancellationTokenSource,
                    DeviceStorage,
                    ZipFileName);

                obj.SetTitle("Descomprimiendo e insertando " + ZipFileName);
                obj.SetPercentProgress(0.15);
                string DeviceStorageJson = Path.Combine(DeviceStorage, "json", ZipFileName + "_tmp");

                if (Directory.Exists(DeviceStorageJson))
                {
                    Directory.Delete(DeviceStorageJson, true);
                }

                Directory.CreateDirectory(DeviceStorageJson);

                if (File.Exists(Path.Combine(DeviceStorage, ZipFileName)))
                {
                    Debug.WriteLine(Path.Combine(DeviceStorage, ZipFileName));
                    Debug.WriteLine("Archivo encontrado");
                }

                //TODO: Sería útil una validación del zip, porque es posible que el archivo 
                //  sea devuelto corrompido, ya se solucionó desde el servidor, pero es aún
                //  una posibilidad. Sin dicha validación (try) la aplicación crashea.
                ZipFile.ExtractToDirectory(Path.Combine(DeviceStorage, ZipFileName), DeviceStorageJson);

                var ListFiles = Directory.GetFiles(DeviceStorageJson, "*.json");

            }
            catch (Exception ex)
            {
                textToast = "Error download:" + ex.Message;
                toast = Toast.Make(textToast, duration, fontSize);
                await toast.Show(cancellationTokenSource.Token);
                break;
            }
        }

        foreach (string ZipFileName in ZipFiles)
        {
            obj.SetSubTitle("Descargando " + ZipFileName);

            try
            {
                string DeviceStorageJson = Path.Combine(DeviceStorage, "json", ZipFileName + "_tmp");

                var ListFiles = Directory.GetFiles(DeviceStorageJson, "*.json");

                switch (ZipFileName)
                {
                    case "account_move.zip":
                        {
                            await ProcAccountMove(obj, ListFiles);
                        }
                        break;
                    case "account_move_line.zip":
                        {
                            await ProcAccountMoveLine(obj, ListFiles);
                        }
                        break;
                    case "res_partner.zip":
                        {
                            await ProcPartner(obj, ListFiles);
                        }
                        break;
                    case "account_journal.zip":
                        {
                            await ProcAccountJournal(obj, ListFiles);
                        }
                        break;
                    case "product_template.zip":
                        {
                            await ProcProducts(obj, ListFiles);
                        }
                        break;
                }

                //Eliminar carpeta descomprimida temporal
                Directory.Delete(DeviceStorageJson, true);

            }
            catch (Exception ex)
            {
                textToast = "Error insert:" + ex.Message;
                toast = Toast.Make(textToast, duration, fontSize);
                await toast.Show(cancellationTokenSource.Token);
                break;
            }

            textToast = "Actualización por cache terminada " + ZipFileName;
            //text = "Insertado en " + ZipFileName + " " + await database.GetCount();
            toast = Toast.Make(textToast, duration, fontSize);
            await toast.Show(cancellationTokenSource.Token);
        }
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

    ////private async Task RefreshVat()
    ////    //(ProgressBarAnimationBehaviorPage obj,
    ////    //IToast toast,
    ////    //ToastDuration duration,
    ////    //double fontSize,
    ////    //CancellationTokenSource cancellationTokenSource,
    ////    //string fechaActualizaTablet)
    ////{
    ////    //TODO: Se requiere actualizacion de clientes, probablemente se los deba agregar al cache
    ////    //  ya que se estima que serán muchos

    ////    CobCarteraCabDb cobCarteraCabDb = new CobCarteraCabDb();
    ////    FacNotaCreditoCabDb facNotaCreditoCabDb = new FacNotaCreditoCabDb();

    ////    List<int> idsInteresantes = (await facNotaCreditoCabDb.GetItemsAsync()).Select(objeto => objeto.CODCLIENTE).ToList();
    ////    var resCartera = (await cobCarteraCabDb.GetItemsAsync()).Where(o => idsInteresantes.Contains(o.CODCLIENTE)).ToArray();

    ////    foreach(var itemCartera in resCartera)
    ////    {
    ////        //Debug.WriteLine(itemCartera.VAT);
    ////        var itemsFac = await facNotaCreditoCabDb.GetItemsByClientAsync(itemCartera.CODEMPRESA, itemCartera.CODCLIENTE);
    ////        itemsFac.ForEach(objeto =>
    ////        {                
    ////            objeto.VAT = itemCartera.VAT;
    ////        });

    ////        await facNotaCreditoCabDb.UpdateAllAsync(itemsFac.ToArray());
    ////    }
    ////}

    private async Task<bool> SuggestCacheMode()
    {
        var databaseDet = new AccountMoveLineDb(App.Session.odooConnection.DbNameSqlite);
        if ((await databaseDet.GetCount()) == 0)
        {
            return true;
        }

        var databaseCab = new AccountMoveDb(App.Session.odooConnection.DbNameSqlite);
        if ((await databaseCab.GetCount()) == 0)
        {
            return true;
        }

        return false;
    }

    private async Task<string> GetLastDate()
    {
        //var databaseCab = new AccountMoveDb(App.Session.odooConnection.DbNameSqlite);
        ////if (() != null)
        ////{
        ////   return true;
        ////}
        //string result = (await databaseCab.GetLastDate()).ToString("yyyy-MM-dd");

        return DateTime.Now.ToString();
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
        //Actualización por Cache
        if (chkUpdateBig.IsChecked || chkUpdateFacDet.IsChecked)
        {            
            //TODO: Se desactiva siempre la actualización por Cache
            //chkCacheMode.IsChecked = false;

            //Modo con cache de datos
            // es más veloz pero hay que asegurarse de que el cache esté actualizado
            if (chkCacheMode.IsChecked)
            {
                //if (await ServerOnlineStatus_Resources())
                //{
                //    BoxViewServerStatusResBuilder.Color = Colors.LawnGreen;
                //    lblServerStatusResources.Text = "Servidor Recursos";

                //    //await LaunchCacheMode(obj, toast, duration, fontSize, cancellationTokenSource);
                //    await LaunchCacheModeByChunks(obj, toast, duration, fontSize, cancellationTokenSource);
                //}
                //else
                //{
                //    toast = Toast.Make("Servidor de recursos no disponible", duration, fontSize);
                //    await toast.Show(cancellationTokenSource.Token);

                //    BoxViewServerStatusResBuilder.Color = Colors.SaddleBrown;
                //    lblServerStatusResources.Text = "Servidor Recursos (x)";

                //    launchSalesUpdate = false;
                //}
            }

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

                bool answerContinue = await DisplayAlert("Error de actualización", "Al parecer no se han insertado datos, por favor verifique su conexión de datos. Desea proceder con la actualización en línea (ACTUALIZACION LENTA)?", "Continuar", "Cancelar");
                //Debug.WriteLine("Answer: " + answer);
                if (!answerContinue)
                {
                    //Se procede a cerrar
                    await Navigation.PopModalAsync();
                    return;
                }
            }

            //else
            
        }

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

        if (launchSalesUpdate)
        {
            // Si es que ayer es menor que la fecha del registro
            //if(DateTime.Today.AddDays(-1).Date < App.Session.CurrentUser.log_fec_sincro_nc.Date)
            //{
            //    fechaActualizaTablet = App.Session.CurrentUser.log_fec_sincro_nc.ToString("yyyy-MM-dd 00:00:00");
            //}

            //Si no se selecciona el modo cache, utilizaremos el modo tradicional
            // es mucho mas lento pero obtiene los datos más actualizados
            await serverPuller.OnlineSyncFacturas(obj, toast, duration, fontSize, cancellationTokenSource, fechaActualizaTablet);
        }

        obj.SetTotalPercentProgress(0.30);

        //Actualización en linea
        if (chkUpdateSmall.IsChecked)
        {
            //Modulos, Tipos de Modulos, Bancos, Cuentas bancarias, etc

            //YA NO SE USARÁ
            //await LaunchMode2(obj, toast, duration, fontSize, cancellationTokenSource);

            //TODO: Ya no se usará la actualizacion en linea de AccountJournal
            // se estima que serán muchos datos, por lo cual se pasó a modo cache
            // y el AccountJournal no tiene fecha de creación en la devolución de datos por 
            // medio del api
            await serverPuller.OnlineSyncResPartner();
            await serverPuller.OnlineSyncJournal();
            await serverPuller.OnlineSyncBank();
            await serverPuller.OnlineSyncAccountModule();
            await serverPuller.OnlineSyncAccountTypeModule();
            await serverPuller.OnlineSyncCompany(false);

            await serverPuller.OnlineCreditNotesRelated();


            obj.SetTotalPercentProgress(0.80);
        }

        if(chkUpdateUserData.IsChecked)
        {
            await serverPuller.OnlineSyncPaymentHeader();
            await serverPuller.OnlineSyncAccountPaymentDaily();
            await serverPuller.DownloadAccountMoveRefund();
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
}