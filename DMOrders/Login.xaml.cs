using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;
using DMOrders.Controls.Tools;
using DMOrders.Pages.Sys;
using DMOrders.Services.Helpers;
using DMSA.Models.Odoo.Abstract;
using DMSA.Models.Odoo.DMApps;
using DMSA.Models.Odoo.Native;
using DMSA.Models.Odoo.Tools;
using DMSA.Models.Security;
using DMSA.Sync.Core.Database.Sqlite;
using DMSA.Sync.Core.Update;
using System.Buffers;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Timers;
using UraniumUI.Dialogs;
using UraniumUI.Material.Controls;

namespace DMOrders;

public partial class Login : ContentPage
{
    public OdooConnection SelConnection { get; set; }

    public res_company SelCompany { get; set; } = new();

    public ObservableCollection<OdooConnection> OdooConnectionItems { get; set; } = new();

    public ObservableCollection<res_center> Agencies { get; set; } = new();

    public IDialogService DialogService { get; }

    private int _tapCount = 0;
    private System.Timers.Timer _timer;
    private const double TimeToReset = 2000;

    private bool _isFirstAppearing = true;
    public Login()
    {
        InitializeComponent();        
    }

    public Login(IEnumerable<IDialogService> dialogServices)
    {
        InitializeComponent();
    }

    public static class ToastHelper
    {
        public static async Task RunWithToastAsync(string message, Func<Task> action)
        {
            using var cts = new CancellationTokenSource();

            var toast = Toast.Make(message, ToastDuration.Long, textSize: 14);
            var toastTask = toast.Show(cts.Token);

            try
            {
                // Ejecuta la acción que le pases
                await action();
            }
            finally
            {
                // Al terminar, se oculta el toast
                cts.Cancel();
            }
        }
    }

    public async Task SetupLogin()
    {        
        SetupTapGesture();
                
        OdooConnectionItems = new ObservableCollection<OdooConnection>();
        ddCompany.ItemsSource = OdooConnectionItems;
        //ddCompany.ItemDisplayBinding = new Binding(nameof(OdooConnection.Name));
        ddCompany.ItemDisplayBinding = new Binding("Name");

        ddCompany.SelectedItemChanged += async (s, e) =>
        {
            if (ddCompany.SelectedItem == null)
                return;

            SelConnection = (OdooConnection)ddCompany.SelectedItem;
            App.Session.odooConnection = SelConnection;
            App.Session.CurrentUser = new User
            {
                username = App.Session.odooConnection.Username,
                password = App.Session.odooConnection.Password,
                databasename = App.Session.odooConnection.DbName,
            };

            LoadEnvironment();

            CompanyDb companyDb = new CompanyDb(App.Session.odooConnection.DbNameSqlite);
            SelCompany = (await companyDb.GetItemsAsync()).Where(x => x.id == SelConnection.CompanyId).FirstOrDefault();

            if (SelCompany == null)
            {
                await Toast.Make("Error: No se encontró la empresa asociada a la conexión.").Show();
                return;
            }

            var storesDb = new ResCenterDb(App.Session.odooConnection.DbNameSqlite);

            var storesItems = (await Task.Run(async () => await storesDb.GetItemsAsync()))
                              .Where(s => s.company_id == SelCompany.id && s.type_center == "M")
                              .ToArray();
            ddAgency.ItemsSource = storesItems;
            ddAgency.ItemDisplayBinding = new Binding("name");
            ddAgency.SelectedItem = storesItems.FirstOrDefault();
        };

        await LoadSettingsFromDb();
        await PrepareConnections();

        //await ToastHelper.RunWithToastAsync("Procesando...", async () =>
        //{
        //    // Aquí tu lógica
        //    await Task.Delay(15000); // simula proceso
        //});

        //var cts = new CancellationTokenSource();
        //var toast = Toast.Make("Cargando...");
        //var toastTask = toast.Show(cts.Token);

        var serverPuller = new ServerPuller();
        var pullResult = await serverPuller.Pull();

        //if(true)
        //    await serverPuller.PullPromotions();
        
        if (!pullResult)
        {
            await Toast.Make("Datos base incorrectos.").Show();
        }
        else
        {
            await Toast.Make("Datos base correctos.").Show();
        }

        //cts.Cancel();

        App.Session.useOfflineMode = false;

        Debug.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
        Debug.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss").Substring(0, 10));
        
        lblAppVersion.Text = "Versión " + App.Session.AppVersion;

        if (App.Session.useOfflineMode)
        {
            Debug.WriteLine("Se usará modo modo offline.");
            await Toast.Make("Se usará modo modo offline.").Show();
        }
        
        Application.Current.UserAppTheme = AppTheme.Light;
    }

    private void LoadEnvironment()
    {
        if (App.Session.odooConnection.IsProduction)
        {
            txtEnvironment.Text = "Producción";
        }
        else
        {
#if DEBUG
            txtEnvironment.Text = "Desarrollo";
            txtEnvironment.Text += " + DEBUG";
#endif
        }

        if (App.Session.odooConnection.IsTestMode)
        {
            txtUser.Text = "jchonillo@macronegocios.ec";
            txtPassword.Text = "mnsa_18";
        }

        Debug.WriteLine(txtEnvironment.Text);
        Debug.WriteLine(lblAppVersion.Text);
    }

    private void Login_Loaded(object? sender, EventArgs e)
    {
        
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
        
        //SettingsPage objPage = new SettingsPage();

        Connections objPage = new Connections();
        objPage.Disappearing += ObjSettingPage_Disappearing;        
        await Navigation.PushModalAsync(objPage);
    }

    private async void ObjSettingPage_Disappearing(object? sender, EventArgs e)
    {
        var senderObject = (Connections) sender;
        if (senderObject.Navigation.ModalStack.Count == 0)
        {
            Debug.WriteLine("Si es el cierre correcto");            
            await LoadSettingsFromDb();
            await PrepareConnections();            
        }
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
        //TryLogin(sender, e);
    }

    private void OnEntryCompleted(object sender, EventArgs e)
    {
        Debug.WriteLine("Logged");
    }
    
    public async Task<bool> LoadSettingsFromDb()
    {
        OdooConnectionDb odooConnectionDb = new OdooConnectionDb();
        await Task.Run(async () => await odooConnectionDb.InitDefault());

        //AppSettingsDb appSettingsDb = new AppSettingsDb();
        //await Task.Run(async () => await appSettingsDb.InitDefault());

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

        //passwordback = CryptoHelper.Decrypt(passwordback);

        App.Session.CurrentUserFront = new User()
        {
            //username = usernameback,
            //password = passwordback,
            //databasename = App.Session.DefaultDatabase
        };

        return true;
    }

    public async Task<bool> SetDataSessionOnLine(User resultUser, DateTime currentDate)
    {
        var database = new UserAccessDb(App.Session.odooConnection.DbNameSqlite);

        ApiManager.HubUser hubUser = new ApiManager.HubUser(App.Session);
        var resultValidacion = await hubUser.ValidaSincronizacionAsync(resultUser, currentDate);

        if (resultValidacion != null)
        {
            //resultValidacion.empresas
            Debug.WriteLine("Validación:" + resultValidacion.data[0].companies);
        }
        else if(resultValidacion == null || resultValidacion.data == null || resultValidacion.message == null)
        {
            await Toast.Make("Se requiere verificación en linea por falta de datos, pero no se encontró servidor.").Show();
            return false;
        }

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
        itemInsert.partner_id = resultUser.partner_id;
        itemInsert.pwd = resultUser.password;
        itemInsert.username = resultUser.username;
        itemInsert.api_key = resultUser.api_key;
        itemInsert.token_type = resultUser.token_type;
        itemInsert.access_token = resultUser.access_token;
        itemInsert.databasename = resultUser.databasename;
        itemInsert.log_fec_acceso = resultUser.log_fec_acceso;        

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
                obj.partner_id_,
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

    [Obsolete]
    private async Task<res_company[]> PrepareCompanies(user_access userFound)
    {
        res_company[] _empresas = new res_company[0];
        _empresas = Newtonsoft.Json.JsonConvert.DeserializeObject<List<res_company>>(userFound.companies).ToArray();

        int[] _companyIds = _empresas.Select(x => x.id).ToArray();

        CompanyDb companyDb = new CompanyDb(App.Session.odooConnection.DbNameSqlite);
        var listCompany = (await companyDb.GetItemsAsync()).Where(x => _companyIds.Contains(x.id));
        bool areEqual = _empresas.ToList().SequenceEqual(listCompany, new CompanyComparer());

        if (!areEqual)
        {
            _empresas = listCompany.ToArray();
        }

        return _empresas;
    }

    private async Task PrepareConnections()
    {
        ddCompany.ItemsSource = null;
        OdooConnectionItems = new ObservableCollection<OdooConnection>();

        OdooConnectionDb connectionsDb = new OdooConnectionDb();
        IEnumerable<OdooConnection> filtered = (await connectionsDb.GetItemsAsync()).Where(c => c.Active);
        OdooConnectionItems = new ObservableCollection<OdooConnection>(filtered.ToList());
        ddCompany.ItemsSource = OdooConnectionItems;
        ddCompany.ItemDisplayBinding = new Binding("Name");        

        ddCompany.SelectedItem = OdooConnectionItems.FirstOrDefault();
        Debug.WriteLine("Conexiones cargadas!!");
    }
        
    public async Task<bool> SetDataSessionOffLine(User resultUser, user_access userFound, DateTime currentDate)
    {
        var database = new UserAccessDb(App.Session.odooConnection.DbNameSqlite);
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
        itemInsert.partner_id = resultUser.partner_id;
        itemInsert.pwd = resultUser.password;
        itemInsert.username = resultUser.username;
        itemInsert.api_key = resultUser.api_key;
        itemInsert.token_type = resultUser.token_type;
        itemInsert.access_token = resultUser.access_token;

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

    private async void OnLoginClicked(object sender, EventArgs e)
    {
        if (App.Session.useOfflineMode)
        {
            await TryLoginAsyncOffline();
        }
        else
        {
            await UITools.ShowLoadingPopup(this);
            await UITools.SetNotifyLoadingPopup("Iniciando sesión...");
            if (await TryLoginBackUserAsync())
            {
                await UITools.SetNotifyLoadingPopup("Comprobado backuser...");
                await TryLoginAsync();
            }
            await UITools.HideLoadingPopup();
        }
    }

    public async Task TryLoginAsyncOffline()
    {
        try
        {
            DateTime currentDate = DateTime.Now;

            User user = new User
            {
                username = txtUser.Text,
                password = CryptoHelper.Encrypt(txtPassword.Text),
                databasename = App.Session.odooConnection.DbName
            };

#if !DEBUG
        //if (!App.Session.isTestMode)
        //{
        //    if (user.username.Length <= 3 || user.codclave.Length <= 3)
        //    {
        //        await Toast.Make("Datos incorrectos, verifique usuario y contraseña.").Show();
        //        return;
        //    }
        //}
#endif

            BtnTryLogin.IsEnabled = false;

            var database = new UserAccessDb(App.Session.odooConnection.DbNameSqlite);
            var hubUser = new ApiManager.HubUser(App.Session);
            User resultUser = null;

            // --- Ejecutar operaciones pesadas en un hilo de fondo ---
            var userList = await Task.Run(async () => await database.GetItemsAsync());

            var userFound = userList.FirstOrDefault(
                u => u.username == txtUser.Text &&
                     u.pwd == CryptoHelper.Encrypt(txtPassword.Text) &&
                     u.log_fec_acceso.Date == currentDate.Date);

            if (userFound != null)
            {
                // Usuario encontrado offline
                resultUser = new User
                {
                    uid = userFound.uid,
                    partner_id = userFound.partner_id,
                    username = userFound.username,
                    nombres = userFound.name,
                    password = userFound.pwd,
                    api_key = userFound.api_key,
                    token_type = userFound.token_type,
                    access_token = userFound.access_token,
                    databasename = userFound.databasename ?? App.Session.odooConnection.DbName,
                    log_fec_acceso = userFound.log_fec_acceso
                };                
            }
            else
            {
                // Modo offline
                if (App.Session.useOfflineMode)
                {
                    //LoginSelector.IsVisible = false;
                    //CompanySelector.IsVisible = true;

                    userFound = userList.Where(
                        u => u.username == txtUser.Text &&
                        u.pwd == CryptoHelper.Encrypt(txtPassword.Text)).FirstOrDefault();

                    resultUser = new User
                    {
                        username = txtUser.Text,
                        password = CryptoHelper.Encrypt(txtPassword.Text),
                        uid = userFound?.uid ?? 0,
                        partner_id = userFound?.partner_id ?? 0,
                        api_key = "-",
                        token_type = "-",
                        access_token = "-"
                    };
                }                
            }

            BtnTryLogin.IsEnabled = true;

            // Configuración post-login
            if (resultUser?.uid > 0)
            {
                if (!(await SetDataSessionOffLine(resultUser, userFound, currentDate)))
                {
                    BtnTryLogin.IsEnabled = true;
                    Debug.WriteLine("Error en login offline");
                    return;
                }

                //LoginSelector.IsVisible = false;
                //CompanySelector.IsVisible = true;

                //var companies = await Task.Run(async () => await PrepareCompanies(userFound));

                //ddCompany.ItemsSource = companies;
                //ddCompany.ItemDisplayBinding = new Binding("name");
                //ddCompany.SelectedItemChanged += async (s, e) =>
                //{
                //    var selectedCompany = (res_company)ddCompany.SelectedItem;
                //    var storesDb = new ResCenterDb();

                //    var storesItems = (await Task.Run(async () => await storesDb.GetItemsAsync()))
                //                      .Where(s => s.company_id == selectedCompany.id)
                //                      .ToArray();
                //    ddAgency.ItemsSource = storesItems;
                //    ddAgency.ItemDisplayBinding = new Binding("name");
                //    ddAgency.SelectedItem = storesItems.FirstOrDefault();
                //};

                //ddCompany.SelectedItem = companies.FirstOrDefault();
                //Debug.WriteLine($"Empresas: {companies.Length}");
            }
            else
            {
                await Toast.Make("Error en login").Show();
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Login (offline) error: {ex}");
            await Toast.Make("Ha ocurrido un error durante el login").Show();
        }
    }

    // Cambiar async void → async Task
    public async Task TryLoginAsync()
    {   
        try
        {
            DateTime currentDate = DateTime.Now;

            User user = new User
            {
                username = txtUser.Text,
                password = CryptoHelper.Encrypt(txtPassword.Text),
                databasename = App.Session.odooConnection.DbName
            };

#if !DEBUG
        if (!App.Session.odooConnection.IsTestMode)
        {
            if (user.username.Length <= 3 || user.password.Length <= 3)
            {
                await Toast.Make("Datos incorrectos, verifique usuario y contraseña.").Show();
                return;
            }
        }
#endif

            BtnTryLogin.IsEnabled = false;

            var database = new UserAccessDb(App.Session.odooConnection.DbNameSqlite);
            var hubUser = new ApiManager.HubUser(App.Session);
            User resultUser = null;

            // --- Ejecutar operaciones pesadas en un hilo de fondo ---
            var userList = await Task.Run(async () => await database.GetItemsAsync());

            var userFound = userList.FirstOrDefault(
                u => u.username == txtUser.Text &&
                     u.pwd == CryptoHelper.Encrypt(txtPassword.Text) &&
                     u.log_fec_acceso.Date == currentDate.Date);

            if (userFound != null)
            {
                // Usuario encontrado offline
                resultUser = new User
                {
                    uid = userFound.uid,
                    partner_id = userFound.partner_id,
                    username = userFound.username,
                    nombres = userFound.name,
                    password = userFound.pwd,
                    api_key = userFound.api_key,
                    token_type = userFound.token_type,
                    access_token = userFound.access_token,
                    databasename = userFound.databasename ?? App.Session.odooConnection.DbName,
                    log_fec_acceso = userFound.log_fec_acceso
                };

                if (!(await SetDataSessionOffLine(resultUser, userFound, currentDate)))
                {
                    BtnTryLogin.IsEnabled = true;
                    Debug.WriteLine("Error en login offline");
                    return;
                }
            }
            else
            {  
                // Verificar conexión
                var apiChecker = new ApiChecker(App.Session.odooConnection.Host + "/connect/checkonline");
                bool isOnline = await apiChecker.IsApiAvailable();

                if (!isOnline)
                {
                    BtnTryLogin.IsEnabled = true;
                    await Toast.Make("Offline o servidor inválido! [Módulo de móvil debe estar instalado]").Show();
                    Debug.WriteLine("Offline o servidor inválido! [Módulo de móvil debe estar instalado]");
                    return;
                }

                // Intentar login online
                var responseUser = await hubUser.TryLoginRpcWeb(user, currentDate);

                if (responseUser?.error != null)
                {
                    BtnTryLogin.IsEnabled = true;
                    await Toast.Make($"{responseUser.error.message}: {responseUser.error.data.message}").Show();
                    Debug.WriteLine($"{responseUser.error.message}: {responseUser.error.data.message}");
                    return;
                }

                if (responseUser?.result != null)
                {
                    resultUser = new User
                    {
                        username = txtUser.Text,
                        password = CryptoHelper.Encrypt(txtPassword.Text),
                        uid = responseUser.result.uid,
                        partner_id = responseUser.result.partner_id,
                        api_key = "-",
                        token_type = "-",
                        access_token = "-",
                        databasename = App.Session.odooConnection.DbName
                    };

                    var partner = await hubUser.GetById(resultUser.uid);
                    if (partner != null)
                        resultUser.nombres = partner.result[0].name;

                    await SetDataSessionOnLine(resultUser, currentDate);
                }

                // Releer lista actualizada desde la base local
                userList = await Task.Run(async () => await database.GetItemsAsync());

                userFound = userList.FirstOrDefault(
                    u => u.username == txtUser.Text &&
                            u.pwd == CryptoHelper.Encrypt(txtPassword.Text) &&
                            u.log_fec_acceso.Date == currentDate.Date);
                

                if (userFound == null)
                {
                    await Toast.Make("Dato no coincide, verifique la fecha y hora de su dispositivo").Show();
                    BtnTryLogin.IsEnabled = true;
                    return;
                }
            }            

            // Configuración post-login
            if (resultUser?.uid > 0)
            {                
                

                LoginSelector.IsVisible = false;
                CompanySelector.IsVisible = true;
                                
                //var companies = await Task.Run(async () => await PrepareCompanies(userFound));

                //ddCompany.ItemsSource = companies;
                //ddCompany.ItemDisplayBinding = new Binding("name");
                //ddCompany.SelectedItemChanged += async (s, e) =>
                //{
                //    var selectedCompany = (res_company)ddCompany.SelectedItem;
                //    var storesDb = new ResCenterDb();                    

                //    var storesItems = (await Task.Run(async () => await storesDb.GetItemsAsync()))
                //                      .Where(s => s.company_id == selectedCompany.id)
                //                      .ToArray();
                //    ddAgency.ItemsSource = storesItems;
                //    ddAgency.ItemDisplayBinding = new Binding("name");
                //    ddAgency.SelectedItem = storesItems.FirstOrDefault();
                //};

                //ddCompany.SelectedItem = companies.FirstOrDefault();
                //Debug.WriteLine($"Empresas: {companies.Length}");
            }
            else
            {
                await Toast.Make("Error en login").Show();
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Login error: {ex}");
            await Toast.Make("Ha ocurrido un error durante el login").Show();
        }
        finally
        {
            BtnTryLogin.IsEnabled = true;            
        }
    }

    public async Task<bool> TryLoginBackUserAsync()
    {
        try
        {
            DateTime currentDate = DateTime.Now;

            User user = App.Session.CurrentUser;

            var database = new UserAccessDb(App.Session.odooConnection.DbNameSqlite);
            var hubUser = new ApiManager.HubUser(App.Session);
            User resultUser = null;

            // Intentar login online
            var responseUser = await hubUser.TryLoginRpcWeb(user, currentDate);

            if (responseUser?.error != null)
            {                
                await Toast.Make($"{responseUser.error.message}: {responseUser.error.data.message} - BackUser").Show();
                Debug.WriteLine($"{responseUser.error.message}: {responseUser.error.data.message}");
                return false;
            }

            //if (responseUser?.result != null)
            //{
            //    resultUser = new User
            //    {
            //        username = txtUser.Text,
            //        password = txtPassword.Text,
            //        uid = responseUser.result.uid,
            //        api_key = "-",
            //        token_type = "-",
            //        access_token = "-",
            //        databasename = App.Session.DefaultDatabase
            //    };

            //    var partner = await hubUser.GetById(resultUser.uid);
            //    if (partner != null)
            //        resultUser.nombres = partner.result[0].name;
            //}

            // Releer lista actualizada desde la base local
            //var userList = await Task.Run(async () => await database.GetItemsAsync());

            //var userFound = userList.FirstOrDefault(
            //    u => u.username == txtUser.Text &&
            //            u.pwd == txtPassword.Text &&
            //            u.log_fec_acceso.Date == currentDate.Date);

            //if (userFound == null)
            //{
            //    await Toast.Make("Dato no coincide, verifique la fecha y hora de su dispositivo").Show();
            //    BtnTryLogin.IsEnabled = true;
            //    return false;
            //}
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Login error: {ex}");
            await Toast.Make("Ha ocurrido un error durante el login").Show();
        }

        return true;
    }

    private async void ShowSettings(object sender, EventArgs e)
    {        
        SettingsPage objPage = new SettingsPage();
        await Navigation.PushModalAsync(objPage);
    }

    private void btnAccess_Clicked(object sender, EventArgs e)
    {
        if(ddCompany.SelectedItem != null && ddAgency.SelectedItem != null)
        {
            App.Session.res_Company = SelCompany;
            //App.Session.res_Store = (res_store) ddAgency.SelectedItem;
            App.Session.res_center = (res_center) ddAgency.SelectedItem;            
            App.Current.MainPage = new MainPageTab();
        }
    }

    private void btnBack_Clicked(object sender, EventArgs e)
    {
        CompanySelector.IsVisible = false;
        LoginSelector.IsVisible = true;
    }
    
    private void ContentPage_Appearing(object sender, EventArgs e)
    {
        if (_isFirstAppearing)
        {
            Dispatcher.Dispatch(async () =>
            {
                await SetupLogin();
            });

            _isFirstAppearing = false;
        }
        else
        {
            // Esto ocurre cada vez que vuelvas a la página
            Console.WriteLine("La página ya apareció antes.");
        }        
    }    
}