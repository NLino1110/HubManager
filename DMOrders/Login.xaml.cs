using CommunityToolkit.Maui.Alerts;
using DMOrders.Services.Database.Sqlite;
using DMOrders.Services.Helpers;
using DMOrders.Services.Update;
using DMSA.Models.Odoo.DMApps;
using DMSA.Models.Odoo.Native;
using DMSA.Models.Security;
using System.Buffers;
using System.Diagnostics;
using System.Timers;
using UraniumUI.Dialogs;
using DMOrders.Pages.Sys;
using System.Collections.ObjectModel;

namespace DMOrders;

public partial class Login : ContentPage
{
    public ObservableCollection<res_company> Companies { get; set; } = new();
    public ObservableCollection<res_center> Agencies { get; set; } = new();

    public IDialogService DialogService { get; }

    private int _tapCount = 0;
    private System.Timers.Timer _timer;
    private const double TimeToReset = 2000;

    public Login()
    {
        InitializeComponent();        
    }

    public Login(IEnumerable<IDialogService> dialogServices)
    {
        InitializeComponent();

        //Se selecciona manualmente el modo CommunityToolkit
        // ya que los DialogService de UraniumUI no son incompatibles con los
        // popus de CommunityToolkit
        //DialogService = dialogServices.ToList()[1];
    }
    
    public async Task SetupLogin()
    {
        
        SetupTapGesture();

        await LoadSettingsFromDb();

        App.Session.useOfflineMode = true;

        Debug.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
        Debug.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss").Substring(0, 10));

        txtEnvironment.Text = "Desarrollo";

        lblAppVersion.Text = "Versión " + App.Session.AppVersion;

        if (App.Session.isProduction)
        {
            txtEnvironment.Text = "Producción";
        }
        else
        {
#if DEBUG
            txtEnvironment.Text += " + DEBUG";
#endif
        }

        if (App.Session.isTestMode)
        {
            txtUser.Text = "rchonillo@macronegocios.ec";
            txtPassword.Text = "mnsa_18";
        }

        Debug.WriteLine(txtEnvironment.Text);
        Debug.WriteLine(lblAppVersion.Text);

        if (App.Session.useOfflineMode)
        {
            Debug.WriteLine("Se usará modo modo offline.");
            await Toast.Make("Se usará modo modo offline.").Show();
        }

        //ddCompany.ItemsSource = companies;
        //ddAgency.ItemsSource = storesItems;
        //ddCompany.ItemDisplayBinding = new Binding("name");
        //ddAgency.ItemDisplayBinding = new Binding("name");
        
        Application.Current.UserAppTheme = AppTheme.Light;
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
        SettingsPage objPage = new SettingsPage();
        objPage.Disappearing += ObjSettingPage_Disappearing;
        await Navigation.PushModalAsync(objPage);
    }

    private void ObjSettingPage_Disappearing(object? sender, EventArgs e)
    {
        Task.Run(async () =>
        {
            LoadSettingsFromDb().Wait();
        });
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
        AppSettingsDb appSettingsDb = new AppSettingsDb();
        await Task.Run(async () => await appSettingsDb.InitDefault());

        App.Session.isProduction = await appSettingsDb.getBoolean("is_production");
        App.Session.isTestMode = await appSettingsDb.getBoolean("is_test_mode");

        App.Session.EndPointServerProd = await appSettingsDb.getString("endpoint_server_prod");
        App.Session.EndPointServer = await appSettingsDb.getString("endpoint_server_dev");
        
        //App.Session.EndPointResourceServer = await appSettingsDb.getString("url_resources_dev");

        App.Session.StaticResources_Server = await appSettingsDb.getString("url_resources_dev");
        App.Session.StaticResources_Server_Prod = await appSettingsDb.getString("url_resources_prod");

        App.Session.CacheFilesUrl = await appSettingsDb.getString("url_cache_files_internal");
        App.Session.CacheFilesUrlExternal = await appSettingsDb.getString("url_cache_files_external");

        App.Session.UrlReportServer = await appSettingsDb.getString("url_report_server");
        App.Session.DefaultDatabase = await appSettingsDb.getString("default_database");

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

        if (resultValidacion == null)
        {
            await Toast.Make("Se requiere verificación en linea por falta de datos, pero no se encontró servidor.").Show();
            return false;
        }

        resultUser.log_fec_acceso = resultValidacion.data[0].datetime;
        //Se prepara para la sesión el scope de la aplicación
        //AppSession ns = new AppSession();
        App.Session.CurrentUser = resultUser;
        App.Session.CurrentUser.empresas = resultValidacion.data[0].companies;
        //App.Session = ns;

        //Se realiza inserción/actualización en la tabla

        user_access itemInsert = new user_access();
        itemInsert.name = resultUser.nombres;
        itemInsert.uid = resultUser.uid;
        itemInsert.pwd = resultUser.codclave;
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

    private async Task<res_company[]> PrepareCompanies(user_access userFound)
    {
        res_company[] _empresas = new res_company[0];
        _empresas = Newtonsoft.Json.JsonConvert.DeserializeObject<List<res_company>>(userFound.companies).ToArray();

        int[] _companyIds = _empresas.Select(x => x.id).ToArray();

        CompanyDb companyDb = new CompanyDb();
        var listCompany = (await companyDb.GetItemsAsync()).Where(x => _companyIds.Contains(x.id));
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
        App.Session.CurrentUser = resultUser;
        App.Session.CurrentUser.empresas = _empresas;

        //App.Session = ns;

        //Se realiza inserción/actualización en la tabla

        user_access itemInsert = new user_access();
        itemInsert.name = resultUser.nombres;
        itemInsert.uid = resultUser.uid;
        itemInsert.pwd = resultUser.codclave;
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
            await TryLoginAsync();
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
                codclave = txtPassword.Text,
                databasename = App.Session.DefaultDatabase
            };

#if !DEBUG
        if (!App.Session.isTestMode)
        {
            if (user.username.Length <= 3 || user.codclave.Length <= 3)
            {
                await Toast.Make("Datos incorrectos, verifique usuario y contraseña.").Show();
                return;
            }
        }
#endif

            BtnTryLogin.IsEnabled = false;

            var database = new UserAccessDb();
            var hubUser = new ApiManager.HubUser(App.Session);
            User resultUser = null;

            // --- Ejecutar operaciones pesadas en un hilo de fondo ---
            var userList = await Task.Run(async () => await database.GetItemsAsync());

            var userFound = userList.FirstOrDefault(
                u => u.username == txtUser.Text &&
                     u.pwd == txtPassword.Text &&
                     u.log_fec_acceso.Date == currentDate.Date);

            if (userFound != null)
            {
                // Usuario encontrado offline
                resultUser = new User
                {
                    uid = userFound.uid,
                    username = userFound.username,
                    nombres = userFound.name,
                    codclave = userFound.pwd,
                    api_key = userFound.api_key,
                    token_type = userFound.token_type,
                    access_token = userFound.access_token,
                    databasename = userFound.databasename ?? App.Session.DefaultDatabase,
                    log_fec_acceso = userFound.log_fec_acceso
                };                
            }
            else
            {
                // Modo offline
                if (App.Session.useOfflineMode)
                {
                    LoginSelector.IsVisible = false;
                    CompanySelector.IsVisible = true;

                    userFound = userList.Where(
                        u => u.username == txtUser.Text &&
                        u.pwd == txtPassword.Text).FirstOrDefault();

                    resultUser = new User
                    {
                        username = txtUser.Text,
                        codclave = txtPassword.Text,
                        uid = userFound?.uid ?? 0,
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

                LoginSelector.IsVisible = false;
                CompanySelector.IsVisible = true;

                var companies = await Task.Run(async () => await PrepareCompanies(userFound));

                ddCompany.ItemsSource = companies;
                ddCompany.ItemDisplayBinding = new Binding("name");
                ddCompany.SelectedItemChanged += async (s, e) =>
                {
                    var selectedCompany = (res_company)ddCompany.SelectedItem;
                    var storesDb = new ResCenterDb();

                    var storesItems = (await Task.Run(async () => await storesDb.GetItemsAsync()))
                                      .Where(s => s.company_id == selectedCompany.id)
                                      .ToArray();
                    ddAgency.ItemsSource = storesItems;
                    ddAgency.ItemDisplayBinding = new Binding("name");
                    ddAgency.SelectedItem = storesItems.FirstOrDefault();
                };

                ddCompany.SelectedItem = companies.FirstOrDefault();
                Debug.WriteLine($"Empresas: {companies.Length}");
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
                codclave = txtPassword.Text,
                databasename = App.Session.DefaultDatabase
            };

#if !DEBUG
        if (!App.Session.isTestMode)
        {
            if (user.username.Length <= 3 || user.codclave.Length <= 3)
            {
                await Toast.Make("Datos incorrectos, verifique usuario y contraseña.").Show();
                return;
            }
        }
#endif

            BtnTryLogin.IsEnabled = false;

            var database = new UserAccessDb();
            var hubUser = new ApiManager.HubUser(App.Session);
            User resultUser = null;

            // --- Ejecutar operaciones pesadas en un hilo de fondo ---
            var userList = await Task.Run(async () => await database.GetItemsAsync());

            var userFound = userList.FirstOrDefault(
                u => u.username == txtUser.Text &&
                     u.pwd == txtPassword.Text &&
                     u.log_fec_acceso.Date == currentDate.Date);

            if (userFound != null)
            {
                // Usuario encontrado offline
                resultUser = new User
                {
                    uid = userFound.uid,
                    username = userFound.username,
                    nombres = userFound.name,
                    codclave = userFound.pwd,
                    api_key = userFound.api_key,
                    token_type = userFound.token_type,
                    access_token = userFound.access_token,
                    databasename = userFound.databasename ?? App.Session.DefaultDatabase,
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
                var apiChecker = new ApiChecker(App.Session.EndPointServer + "/connect/checkonline");
                bool isOnline = await apiChecker.IsApiAvailable();

                if (!isOnline)
                {
                    BtnTryLogin.IsEnabled = true;
                    await Toast.Make("Offline o servidor inválido!").Show();
                    Debug.WriteLine("Offline o servidor inválido!");
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
                        codclave = txtPassword.Text,
                        uid = responseUser.result.uid,
                        api_key = "-",
                        token_type = "-",
                        access_token = "-",
                        databasename = App.Session.DefaultDatabase
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
                            u.pwd == txtPassword.Text &&
                            u.log_fec_acceso.Date == currentDate.Date);
                

                if (userFound == null)
                {
                    await Toast.Make("Dato no coincide, verifique la fecha y hora de su dispositivo").Show();
                    BtnTryLogin.IsEnabled = true;
                    return;
                }
            }

            BtnTryLogin.IsEnabled = true;

            // Configuración post-login
            if (resultUser?.uid > 0)
            {                
                var serverPuller = new ServerPuller();
                await serverPuller.Pull();                

                LoginSelector.IsVisible = false;
                CompanySelector.IsVisible = true;
                                
                var companies = await Task.Run(async () => await PrepareCompanies(userFound));

                ddCompany.ItemsSource = companies;
                ddCompany.ItemDisplayBinding = new Binding("name");
                ddCompany.SelectedItemChanged += async (s, e) =>
                {
                    var selectedCompany = (res_company)ddCompany.SelectedItem;
                    var storesDb = new ResCenterDb();                    

                    var storesItems = (await Task.Run(async () => await storesDb.GetItemsAsync()))
                                      .Where(s => s.company_id == selectedCompany.id)
                                      .ToArray();
                    ddAgency.ItemsSource = storesItems;
                    ddAgency.ItemDisplayBinding = new Binding("name");
                    ddAgency.SelectedItem = storesItems.FirstOrDefault();
                };

                ddCompany.SelectedItem = companies.FirstOrDefault();
                Debug.WriteLine($"Empresas: {companies.Length}");
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
            App.Session.res_Company = (res_company) ddCompany.SelectedItem;
            //App.Session.res_Store = (res_store) ddAgency.SelectedItem;
            App.Session.res_center = (res_center)ddAgency.SelectedItem;
            
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
        Dispatcher.Dispatch(async () =>
        {
            await SetupLogin();
        });
    }
}