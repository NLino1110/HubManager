using ApiManager;
using DMCobranzas.AppPages;
using DMCobranzas.Models;
using DMCobranzas.Services;
using DMCobranzas.Settings.helpers;
using DMCobranzas.Settings.Sqlite;
using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;
using CommunityToolkit.Maui.Markup;
using CommunityToolkit.Maui.Views;
using DMSA.Models.Odoo;
using DMSA.Models.Odoo.General.Responses;
using DMSA.Models.Odoo.Native;
using DMSA.Models.Security;
using Microsoft.Maui.ApplicationModel.Communication;
using Microsoft.Maui.Controls;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using System.Buffers;
using System.Diagnostics;
using System.Threading;
using System.Timers;
using DMSA.Models.Odoo.DMApps;
//using DMCobranzas;

namespace DMCobranzas;

public partial class Login : ContentPage
{
    private int _tapCount = 0;
    private System.Timers.Timer _timer;
    private const double TimeToReset = 2000;

    public Login()
    {
        InitializeComponent();

        Task.Run(async () =>
        {
            SetupTapGesture();

            await LoadSettingsFromDb();

            Debug.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            Debug.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss").Substring(0, 10));

            txtEnvironment.Text = "Desarrollo";

            lblAppVersion.Text = "Versión " + App.Session.AppVersion;

            if (App.Session.odooConnection.IsProduction)
            {
                txtEnvironment.Text = "Producción";
            }
            else
            {
#if DEBUG
                txtEnvironment.Text += " + DEBUG";
#endif
            }

            if (App.Session.odooConnection.IsTestMode)
            {
                EntryUserName.Text = "admin";
                EntryPassword.Text = "admin";
            }

            Debug.WriteLine(txtEnvironment.Text);
            Debug.WriteLine(lblAppVersion.Text);
        });        
                
        Application.Current.UserAppTheme = AppTheme.Light;
    }

    private void OnSingleTapped()
    {
        //throw new NotImplementedException();
        Debug.WriteLine("1-Tap-Click");
    }
    private void OnDoubleTapped()
    {
        //throw new NotImplementedException();
        Debug.WriteLine("2-Tap-Click");
    }

    private void SetupTapGesture()
    {
        var tapGesture = new TapGestureRecognizer();
        tapGesture.Tapped += OnLabelTapped;
        tapGesture.NumberOfTapsRequired = 1;
        txtEnvironment.GestureRecognizers.Add(tapGesture);

        _timer = new System.Timers.Timer(TimeToReset);
        _timer.Elapsed += OnTimerElapsed;
        _timer.AutoReset = false; // Para que el timer no se reinicie automáticamente
    }

    private void OnLabelTapped(object sender, EventArgs e)
    {
        _tapCount++;
        if (_tapCount == 1)
        {
            _timer.Start(); // Iniciar/reiniciar el temporizador en el primer toque
        }
        else if (_tapCount == 6)
        {
            ResetTapCount();
            PerformAction();
        }
        else
        {
            // Restablecer el timer cada vez que se registra un toque, excepto el primero y el último
            _timer.Stop();
            _timer.Start();
        }
    }

    private async void PerformAction()
    {
        // Aquí ejecutarás la acción deseada después de 6 toques
        //Debug.WriteLine("Acción ejecutada después de 6 toques");
        Debug.WriteLine("SettingsPage");
        SettingsPage objPage = new SettingsPage();
        await Navigation.PushModalAsync(objPage);
    }

    private void OnTimerElapsed(object sender, ElapsedEventArgs e)
    {
        ResetTapCount(); // Restablecer el contador si el tiempo se agota
    }

    private void ResetTapCount()
    {
        _tapCount = 0;
        _timer.Stop(); // Asegurarte de detener el timer
    }

    //protected async override void OnAppearing()
    //{
    //    base.OnAppearing();
    //    //await UITools.ShowLoadingPopup(this);
    //    //await Task.Delay(1000);
    //    //await UITools.SetNotifyLoadingPopup("Notificacion 1/3");
    //    //await Task.Delay(1000);
    //    //await UITools.SetNotifyLoadingPopup("Notificacion 2/3");
    //    //await Task.Delay(1000);
    //    //await UITools.SetNotifyLoadingPopup("Notificacion 3/3");
    //    //await Task.Delay(1000);
    //    //await UITools.HideLoadingPopup();
    //}

    private void Entry_Completed(object sender, EventArgs e)
    {
        Debug.WriteLine("Logged");
        TryLogin(sender, e);
    }

    private void OnEntryCompleted(object sender, EventArgs e)
    {
        Debug.WriteLine("Logged");
    }

    public async Task<bool> LoadSettingsFromDb()
    {
        AppSettingsDb appSettingsDb = new AppSettingsDb();
        await appSettingsDb.InitDefault();

        //App.Session.isProduction = await appSettingsDb.getBoolean("is_production");
        //App.Session.isTestMode = await appSettingsDb.getBoolean("is_test_mode");
        
        //App.Session.EndPointServerProd = await appSettingsDb.getString("endpoint_server_prod");
        //App.Session.EndPointServer = await appSettingsDb.getString("endpoint_server_dev");
        
        ////App.Session.EndPointResourceServer = await appSettingsDb.getString("url_resources_dev");

        //App.Session.StaticResources_Server = await appSettingsDb.getString("url_resources_dev");
        //App.Session.StaticResources_Server_Prod = await appSettingsDb.getString("url_resources_prod");

        //App.Session.CacheFilesUrl = await appSettingsDb.getString("url_cache_files_internal");
        //App.Session.CacheFilesUrlExternal = await appSettingsDb.getString("url_cache_files_external");

        //App.Session.UrlReportServer = await appSettingsDb.getString("url_report_server");
        //App.Session.DefaultDatabase = await appSettingsDb.getString("default_database");

        //var usernameback = await appSettingsDb.getString("back_user");
        //var passwordback = await appSettingsDb.getString("back_user_password");

        //App.Session.CurrentUserFront = new User()
        //{
        //    username = usernameback,
        //    password = passwordback
        //};

        return true;
    }

    public async Task<bool> SetDataSessionOnLine(User resultUser, DateTime currentDate)
    {
        var database = new UserAccessDb();

        ApiManager.HubUser hubUser = new ApiManager.HubUser(App.Session);
        var resultValidacion = await hubUser.ValidaSincronizacionAsync(resultUser, currentDate);

        if (resultValidacion != null)
        {
            //resultValidacion.empresas
            Debug.WriteLine("Validación:" + resultValidacion.data[0].companies);
        }

        if(resultValidacion == null)
        {
            await Toast.Make("Se requiere verificación en linea por falta de datos, pero no se encontró servidor.").Show();
            return false;
        }

        //var resultUser = Newtonsoft.Json.JsonConvert.DeserializeObject<User>(result);

        //dynamic resultUsers = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(result);
        //Console.WriteLine(resultUsers.data.ToString());
        //var resultUser = Newtonsoft.Json.JsonConvert.DeserializeObject<List<User>>(resultUsers.data.ToString());
        //App.Current.MainPage = new MainPage();
        //App.Current.MainPage = new AppShell();

        resultUser.log_fec_acceso = resultValidacion.data[0].datetime;
        //Se prepara para la sesión el scope de la aplicación
        //AppSession ns = new AppSession();
        App.Session.CurrentUserFront = resultUser;
        App.Session.CurrentUserFront.empresas = resultValidacion.data[0].companies;
        //App.Session = ns;

        //Se realiza inserción/actualización en la tabla

        user_access itemInsert = new user_access();
        itemInsert.name = resultUser.nombres;
        itemInsert.uid = resultUser.uid;
        itemInsert.pwd = resultUser.password;
        itemInsert.username = resultUser.username;

        itemInsert.api_key = resultUser.api_key;
        itemInsert.token_type = resultUser.token_type;
        itemInsert.access_token = resultUser.access_token;
        itemInsert.databasename = resultUser.databasename;

        //TODO: Nombre confuso fechasincronizado
        //itemInsert.ULTIMAACTUALIZACION = resultUser.fechasincronizado;

        //FIX: 
        //---
        //itemInsert.ULTIMAACTUALIZACION = resultValidacion.data[0].datetime.ToString("dd/MM/yyyy HH:mm:ss");
        //+++
        itemInsert.log_fec_acceso = resultValidacion.data[0].datetime;
        //NO SE ASIGNA PORQUE SE ASUME QUE NO HAY DATOS DE SINCRONIZACIÓN
        //itemInsert.log_fec_sincro = resultValidacion.fecha.AddDays(-3);

        //TODO: Nombre confuso FECHAACTUAL
        //itemInsert.FECHAACTUAL = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");
        //TODO: Nombre confuso FECHAACTNC
        //itemInsert.FECHAACTNC = "";

        itemInsert.companies = Newtonsoft.Json.JsonConvert.SerializeObject(resultValidacion.data[0].companies);

        var foundUser = await database.GetItemAsync(resultUser.uid);
        if (foundUser == null)
        {
            await database.InsertAsync(itemInsert);
        }
        else
        {
            await database.UpdateAsync(itemInsert);
        }

        return true;
    }

    public class CompanyComparer : IEqualityComparer<res_company>
    {
        public bool Equals(res_company x, res_company y)
        {
            return x.id == y.id
                && x.name == y.name
                && x.partner_id_ == y.partner_id_
                && x.email == y.email
                && x.phone == y.phone
                && x.mobile == y.mobile
                && x.social_twitter == y.social_twitter
                && x.social_facebook == y.social_facebook
                && x.social_github == y.social_github
                && x.social_linkedin == y.social_linkedin
                && x.social_youtube == y.social_youtube
                && x.check_journal_id_ == y.check_journal_id_
                && x.credit_note_journal_id_ == y.credit_note_journal_id_;
        }

        public int GetHashCode(res_company obj)
        {
            return HashCode.Combine(
                obj.name,
                obj.partner_id_ ,
                obj.email,
                obj.phone,
                obj.mobile,
                //obj.social_twitter,
                //obj.social_facebook,
                //obj.social_github,
                //obj.social_linkedin,
                //obj.social_youtube,
                obj.check_journal_id_
                );
        }
    }

    private async Task<res_company[]> PrepareCompanies(user_access userFound)
    {
        res_company[] _empresas = new res_company[0];
        _empresas = Newtonsoft.Json.JsonConvert.DeserializeObject<List<res_company>>(userFound.companies).ToArray();

        int[] _companyIds = _empresas.Select(x => x.id).ToArray();

        CompanyDb companyDb = new CompanyDb();
        var listCompany = (await companyDb.GetItemsAsync()).Where(x=> _companyIds.Contains(x.id));
        bool areEqual = _empresas.ToList().SequenceEqual(listCompany, new CompanyComparer());

        if (!areEqual)
        {
            _empresas = listCompany.ToArray();
        }

        return _empresas;
    }

    public async Task<bool> SetDataSessionOffLine(User resultUser, user_access userFound, DateTime currentDate)
    {
        var database = new UserAccessDb();
        res_company[] _empresas = new res_company[0];

        //if(userFound.companies!= null && userFound.companies != "")
        //{
            //_empresas = Newtonsoft.Json.JsonConvert.DeserializeObject<List<res_company>>(userFound.companies).ToArray();
            _empresas = await PrepareCompanies(userFound);
        //}
        
        if (_empresas.Length == 0)
        {
            return await SetDataSessionOnLine(resultUser, currentDate);
        }

        //var resultUser = Newtonsoft.Json.JsonConvert.DeserializeObject<User>(result);
        resultUser.log_fec_acceso = userFound.log_fec_acceso;
        resultUser.log_fec_sincro = userFound.log_fec_sincro;
        resultUser.log_fec_sincro_nc = userFound.log_fec_sincro_nc;
        //Se prepara para la sesión el scope de la aplicación
        //AppSession ns = new AppSession();
        App.Session.CurrentUserFront = resultUser;
        App.Session.CurrentUserFront.empresas = _empresas;
        //App.Session = ns;

        //Se realiza inserción/actualización en la tabla
        
        user_access itemInsert = new user_access();
        itemInsert.name = resultUser.nombres;
        itemInsert.uid = resultUser.uid;
        itemInsert.pwd = resultUser.password;
        itemInsert.username = resultUser.username;
        itemInsert.api_key = resultUser.api_key;
        itemInsert.token_type = resultUser.token_type;
        itemInsert.access_token = resultUser.access_token;

        //TODO: Nombre confuso (fechasincronizado)
        //itemInsert.ULTIMAACTUALIZACION = resultUser.fechasincronizado;
        //TODO: Nombre confuso (FECHAACTUAL)
        //itemInsert.FECHAACTUAL = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");
        //itemInsert.FECHAACTNC = "";

        //DateTime log_fec_acceso = resultUser.fechasincronizado
        //FIX:
        itemInsert.log_fec_acceso = DateTime.Now;
        itemInsert.log_fec_sincro = userFound.log_fec_sincro;
        itemInsert.log_fec_sincro_nc = userFound.log_fec_sincro_nc;

        itemInsert.companies = Newtonsoft.Json.JsonConvert.SerializeObject(_empresas);

        var foundUser = await database.GetItemAsync(resultUser.uid);
        if (foundUser == null)
        {
            await database.InsertAsync(itemInsert);
        }
        else
        {
            await database.UpdateAsync(itemInsert);
        }

        return true;
    }

    //////private DateTime toDateExact(string dateString)
    //////{
    //////    // Intentamos convertir el string a DateTime usando el formato específico
    //////    if (DateTime.TryParseExact(dateString, "dd/MM/yyyy", null, System.Globalization.DateTimeStyles.None, out DateTime result))
    //////    {
    //////        // La conversión fue exitosa, result contiene la fecha sin datos de tiempo
    //////        Console.WriteLine("Fecha sin datos de tiempo: " + result);
    //////    }
    //////    else
    //////    {
    //////        // El string no coincide con el formato esperado
    //////        Console.WriteLine("El formato del string no es válido.");
    //////    }
    //////    return result;
    //////}

    public async void TryLogin(object sender, EventArgs e)
    {
        //DateTime currentDate = DateTime.Now;
        DateTime currentDate = DateTime.Now;

        User user = new User();
        user.username = EntryUserName.Text;
        user.password = EntryPassword.Text;
        //user.fechatablet = currentDate.ToString("yyyy-MM-dd HH:mm:ss");

#if !DEBUG
        if (!App.Session.isTestMode)
        {
            if (user.username.Length <= 3 || user.codclave.Length <= 3)
            {
                await Toast.Make("Al parecer los datos del usuario y password están incorrectos.").Show();
                return;
            }
        }
#endif

        BtnTryLogin.IsEnabled = false;
        var database = new UserAccessDb();

        ApiManager.HubUser hubUser = new ApiManager.HubUser(App.Session);

        User resultUser = null;

        //Primero se intenta OFFLINE
        var userList = await database.GetItemsAsync();
        //var userFound = userList.Where(
        //    u=>u.CODUSUARIO == EntryUserName.Text && 
        //    u.CLAVE == EntryPassword.Text &&
        //    Convert.ToDateTime(u.FECHAACTUAL).Date == currentDate.Date).FirstOrDefault();

        var userFound = userList.Where(
            u => u.username == EntryUserName.Text &&
            u.pwd == EntryPassword.Text &&
            u.log_fec_acceso.Date == currentDate.Date).FirstOrDefault();

        //Se encontró el usuario en los datos locales
        //  siempre y cuando haya iniciado sesión el mismo día
        if (userFound != null)
        {
            //Asignación de datos desde la base de datos
            resultUser = new User();
            resultUser.uid = userFound.uid;
            resultUser.username = userFound.username;
            resultUser.nombres = userFound.name;
            resultUser.password = userFound.pwd;
            resultUser.api_key = userFound.api_key;
            resultUser.token_type = userFound.token_type;
            resultUser.access_token = userFound.access_token;
            

            if(userFound.databasename == null)
            {
                userFound.databasename = App.Session.odooConnection.DbName;
            }

            resultUser.databasename = userFound.databasename;

            //TODO: Corregir nombres, son confusos, en este campo se debe hacer referencia 
            // a la ultima fecha/hora de inicio de sesión (FECHAACTUAL)
            //resultUser.fechasincronizado = userFound.FECHAACTUAL;

            //TODO: Corregir nombres, ULTIMAACTUALIZACION ???
            //resultUser.fechasincronizado = userFound.ULTIMAACTUALIZACION;

            resultUser.log_fec_acceso = userFound.log_fec_acceso;

            //resultUser.exito = "true";

            //Tareas de actualización de datos
            if(!(await SetDataSessionOffLine(resultUser, userFound, currentDate)))
            {
                BtnTryLogin.IsEnabled = true;
                Debug.WriteLine("Error en login");
                return;
            }
        }
        else
        {

            ApiChecker apiChecker = new ApiChecker(App.Session.odooConnection.Host + "/connect/checkonline");
            bool isOnline = await apiChecker.IsApiAvailable();

            if(!isOnline)
            {
                BtnTryLogin.IsEnabled = true;
                await Toast.Make("Offline o servidor inválido!").Show();
                Debug.WriteLine("Offline o servidor inválido!");
                return;
            }

            //Sino lo encuentra en los datos locales se intenta ONLINE            
            var responseUser = await hubUser.TryLoginRpcWeb(user, currentDate);

            if(responseUser == null)
            {
                BtnTryLogin.IsEnabled = true;
                await Toast.Make("Offline o servidor inválido!").Show();
                Debug.WriteLine("Offline o servidor inválido!");
                return;
            }

            if (responseUser.error != null)
            {
                BtnTryLogin.IsEnabled = true;
                await Toast.Make(responseUser.error.message + ": " + responseUser.error.data.message).Show();
                Debug.WriteLine(responseUser.error.message + ": " + responseUser.error.data.message);
                return;
            }

            if (responseUser.result != null ) //"true")
            {
                resultUser = new User();

                //await Toast.Make("Error: " + resultUser.mensaje).Show();
                //return;
                //Se asigna la clave ya que el api no la devuelve
                resultUser.username = EntryUserName.Text;
                resultUser.password = EntryPassword.Text;
                resultUser.uid = responseUser.result.uid;
                resultUser.api_key = "-";
                resultUser.token_type = "-";
                resultUser.access_token = "";

                //Se extrae informacion del usuario
                // Odoo no devuelve por defecto los datos y necesitamos volver a usar el api
                //HubPartner hubPartner = new HubPartner(App.Session);
                //hubPartner.setApiKey(responseUser.result.api_key);                
                var partner = await hubUser.GetById(resultUser.uid);
                if(partner!=null)
                {
                    resultUser.nombres = partner.result[0].name;
                }

                if (resultUser.databasename == null)
                {
                    resultUser.databasename = App.Session.odooConnection.DbName;
                }

                //Tareas de actualización de datos
                await SetDataSessionOnLine(resultUser, currentDate);
            }
        }

        BtnTryLogin.IsEnabled = true;

        if (resultUser != null)
        {
            if (resultUser.uid > 0)
            {
                //var parametrosDb = new ParametrosDb();
                //var itemsParametros = (await parametrosDb.GetItemsAsync()).Where(p => p.CODUSUARIO == resultUser.codusuario).ToList();

                ////var resultValidacion = await hubUser.ValidaSincronizacionAsync(resultUser.codusuario);

                ////if(resultValidacion != null)
                ////{
                ////    //resultValidacion.empresas
                ////    Debug.WriteLine("Validación:" + resultValidacion.empresas);
                ////}

                //////var resultUser = Newtonsoft.Json.JsonConvert.DeserializeObject<User>(result);

                //////dynamic resultUsers = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(result);
                //////Console.WriteLine(resultUsers.data.ToString());
                //////var resultUser = Newtonsoft.Json.JsonConvert.DeserializeObject<List<User>>(resultUsers.data.ToString());
                //////App.Current.MainPage = new MainPage();
                //////App.Current.MainPage = new AppShell();

                //////Se prepara para la sesión el scope de la aplicación
                //////AppSession ns = new AppSession();
                ////App.Session.CurrentUser = resultUser;
                ////App.Session.CurrentUser.empresas = resultValidacion.empresas;
                //////App.Session = ns;

                //////Se realiza inserción/actualización en la tabla

                ////CobUsuarios itemInsert = new CobUsuarios();
                ////itemInsert.NOMBRE = resultUser.nombres;
                ////itemInsert.CODUSUARIO = resultUser.codusuario;
                ////itemInsert.CLAVE = user.codclave;
                ////itemInsert.ULTIMAACTUALIZACION = resultUser.fechasincronizado;
                ////itemInsert.FECHAACTUAL = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");
                ////itemInsert.FECHAACTNC = "";
                ////itemInsert.CLIENTESTODOS = Newtonsoft.Json.JsonConvert.SerializeObject(resultValidacion.empresas);

                ////var foundUser = await database.GetItemAsync(resultUser.codusuario);
                ////if (foundUser == null)
                ////{
                ////    await database.InsertAsync(itemInsert);
                ////}
                ////else
                ////{
                ////    await database.UpdateAsync(itemInsert);
                ////}

                ////var items = await database.GetItemsAsync();
                //////Inicializar tablas en caso de que no existan

                //////items.Where(x => x.)
                ////Debug.WriteLine("Total:" + items.Count);

                App.Current.MainPage = new AppFlyout();
            }
            else
            {
                Debug.WriteLine("Error en login");

                //string text = resultUser.mensaje;
                ToastDuration duration = ToastDuration.Short;
                double fontSize = 14;
                var toast = Toast.Make("Error", duration, fontSize);
                await toast.Show();
            }
        }
        else
        {
            await Toast.Make("Error al intentar el login.").Show();
        }
    }

    private async void ShowSettings(object sender, EventArgs e)
    {
        Debug.WriteLine("SettingsPage");
        SettingsPage objPage = new SettingsPage();
        await Navigation.PushModalAsync(objPage);
        //await Navigation.PushAsync(objPage, false);
    }
}