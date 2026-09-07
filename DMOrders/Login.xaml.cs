using ApiManager;
using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;
using CommunityToolkit.Mvvm.Input;
using DMOrders.Controls.Tools;
using DMOrders.Pages.Sys;
using DMOrders.Services.Helpers;
using DMOrders.Services.PatchManager;
using DMOrders.Services.Update;
using DMSA.Models.Odoo.Abstract;
using DMSA.Models.Odoo.Accounting;
using DMSA.Models.Odoo.Native;
using DMSA.Models.Odoo.Security;
using DMSA.Models.Odoo.Tools;
using DMSA.Models.Security;
using DMSA.Sync.Core.Database.Sqlite;
using DMSA.Sync.Core.Database.Sqlite.Accounting;
using DMSA.Sync.Core.Update;
using Newtonsoft.Json;
using System.Buffers;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Timers;
using System.Windows.Input;

namespace DMOrders;
public partial class Login : ContentPage
{
    public OdooConnection SelConnection { get; set; }

    public res_company SelCompany { get; set; } = new();

    public ObservableCollection<OdooConnection> OdooConnectionItems { get; set; } = new();

    public ObservableCollection<res_center> Agencies { get; set; } = new();


    private int _tapCount = 0;
    private System.Timers.Timer _timer;
    private const double TimeToReset = 2000;

    private bool _isFirstAppearing = true;

    public ICommand ActionCommand { get; set; }

    private CancellationTokenSource _longPressCts;
    private bool _executed;
    private const int LONG_PRESS_MS = 1500;

    /// <summary>
    /// true: bloquea login mientras prepara conexión, evita race al cambiar conexión
    ///       y restaura la conexión guardada con Recordarme.
    /// false: comportamiento anterior (revertir fix).
    /// </summary>
    private const bool UseConnectionSessionFixes = true;

    private int _connectionChangeVersion;
    private bool _isConnectionLoading;
    private const string BtnTryLoginDefaultText = "Iniciar sesión";

    public Login()
    {
        InitializeComponent();
        BindingContext = this;
    }

    [RelayCommand]
    private async void Capture()
    {
        CaptureScreen capturePage = new CaptureScreen();
        await Navigation.PushModalAsync(capturePage);
    }

    [RelayCommand]
    private async void ShowConnections()
    {
        Connections objPage = new Connections();
        objPage.Disappearing += ObjSettingPage_Disappearing;
        await Navigation.PushModalAsync(objPage);
    }

    //private async void ShowConnections()
    //{
    //    SettingsPage objPage = new SettingsPage();
    //    await Navigation.PushModalAsync(objPage);
    //}

    //public Login(IEnumerable<IDialogService> dialogServices)
    //{
    //    InitializeComponent();
    //    //ActionCommand = new Command(ShowConnections);
    //}

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

    public async Task RootPatch()
    {
        PatchRunner patchRunner = new PatchRunner();
        await patchRunner.RootPatchExecuter(this);
    }

    private void SetConnectionLoading(bool loading)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            _isConnectionLoading = loading;

            if (!UseConnectionSessionFixes)
            {
                BtnTryLogin.IsEnabled = true;
                BtnTryLogin.Text = BtnTryLoginDefaultText;
                return;
            }

            BtnTryLogin.IsEnabled = !loading;
            ddCompany.IsEnabled = !loading;
            BtnTryLogin.Text = loading ? "Preparando conexión..." : BtnTryLoginDefaultText;
        });
    }

    private static void ApplyConnectionToSession(OdooConnection connection)
    {
        App.Session.odooConnection = connection;
        App.Session.CurrentUser = new User
        {
            username = connection.Username,
            password = connection.Password,
            databasename = connection.DbName,
        };
        DMSA.Sync.Core.Constants.Session = App.Session;
    }

    private int? GetPreferredConnectionId()
    {
        if (!UseConnectionSessionFixes)
            return null;

        if (App.Session?.odooConnection?.Id > 0)
            return App.Session.odooConnection.Id;

        if (!Preferences.Get("is_rememberme", false))
            return null;

        var appSession = Preferences.Get("App.Session", string.Empty);
        if (string.IsNullOrEmpty(appSession))
            return null;

        try
        {
            var loaded = JsonConvert.DeserializeObject<AppSession>(appSession);
            return loaded?.odooConnection?.Id;
        }
        catch
        {
            return null;
        }
    }

    private async Task LoadAgenciesForConnectionAsync(OdooConnection connection)
    {
        ddAgency.ItemsSource = null;

        CompanyDb companyDb = new CompanyDb(connection.DbNameSqlite);
        SelCompany = (await companyDb.GetItemsAsync())
            .Where(x => x.id == connection.CompanyId)
            .FirstOrDefault();

        if (SelCompany == null)
        {
            await Toast.Make("Error: No se encontró la empresa asociada a la conexión.").Show();
            return;
        }

        var storesDb = new ResCenterDb(connection.DbNameSqlite);
        var storesItems = (await Task.Run(async () => await storesDb.GetItemsAsync()))
            .Where(s => s.company_id == SelCompany.id && s.type_center == "M")
            .ToArray();

        ddAgency.ItemsSource = storesItems;
        ddAgency.ItemDisplayBinding = new Binding("name");
        ddAgency.SelectedItem = storesItems.FirstOrDefault();
    }

    private async Task OnCompanyConnectionChangedAsync(object sender, object selectedItem)
    {
        if (selectedItem is not OdooConnection connection)
            return;

        if (!UseConnectionSessionFixes)
        {
            await OnCompanyConnectionChangedLegacyAsync(connection);
            return;
        }

        var version = ++_connectionChangeVersion;
        SetConnectionLoading(true);

        try
        {
            SelConnection = connection;
            ApplyConnectionToSession(connection);

            Debug.WriteLine($"[Conexión] Name={connection.Name}, DbName={connection.DbName}, Sqlite={connection.DbNameSqlite}");

            PatchRunner patchRunner = new PatchRunner();
            await patchRunner.PatchExecuter(connection, this);
            if (version != _connectionChangeVersion)
                return;

            LoadEnvironment();

            var pullResult = await new ServerPuller().Pull();
            if (version != _connectionChangeVersion)
                return;

            if (!pullResult)
                await Toast.Make("Datos base incorrectos.").Show();
            else
                await Toast.Make("Datos base correctos.").Show();

            await LoadAgenciesForConnectionAsync(connection);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error preparando conexión: {ex}");
            await Toast.Make("Error al preparar la conexión.").Show();
        }
        finally
        {
            if (version == _connectionChangeVersion)
                SetConnectionLoading(false);
        }
    }

    private async Task OnCompanyConnectionChangedLegacyAsync(OdooConnection connection)
    {
        SelConnection = connection;
        App.Session.odooConnection = connection;
        App.Session.CurrentUser = new User
        {
            username = connection.Username,
            password = connection.Password,
            databasename = connection.DbName,
        };

        PatchRunner patchRunner = new PatchRunner();
        await patchRunner.PatchExecuter(connection, this);

        LoadEnvironment();

        var pullResult = await new ServerPuller().Pull();

        if (!pullResult)
            await Toast.Make("Datos base incorrectos.").Show();
        else
            await Toast.Make("Datos base correctos.").Show();

        await LoadAgenciesForConnectionAsync(connection);
    }

    public async Task SetupLogin()
    {
        await AppTools.GlobalSettingInit(App.Session);
        AppTools.BuildPushRelay();

        OdooConnectionItems = new ObservableCollection<OdooConnection>();
        ddCompany.ItemsSource = OdooConnectionItems;        
        ddCompany.ItemDisplayBinding = new Binding("Name");

        ddCompany.SelectedItemChanged += async (s, e) =>
        {
            if (ddCompany.SelectedItem == null)
                return;

            await OnCompanyConnectionChangedAsync(s, ddCompany.SelectedItem);
        };

        await LoadSettingsFromDb();
        await PrepareConnections();

        App.Session.useOfflineMode = false;

        Debug.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
        Debug.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss").Substring(0, 10));
        
        lblAppVersion.Text = "Versión " + App.Session.AppVersion;
        await RefreshLastSyncLabelAsync();
        RefreshAppUpdateDateLabel();

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

        //if (App.Session.odooConnection.preload_email_domain)
        //{
        //    txtUser.Text = "@" + App.Session.odooConnection.email_domain;
        //    txtPassword.Text = "";
        //}

        //if (App.Session.odooConnection.IsTestMode)
        //{
        //    txtUser.Text = "jchonillo@macronegocios.ec";
        //    txtPassword.Text = App.Session.odooConnection.PasswordFront;
        //}
        //else
        //{
        //    if (!App.Session.odooConnection.preload_email_domain)
        //    {
        //        txtUser.Text = "";
        //        txtPassword.Text = "";
        //    }
        //}

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
        Debug.WriteLine("SettingsPage");
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
        HubUser hubUser = new HubUser(App.Session);

        var resultValidacion = await hubUser.ValidaSincronizacionAsync(resultUser, currentDate);

        if (resultValidacion != null)
        {            
            Debug.WriteLine("Validación:" + resultValidacion.data[0].companies);
        }
        else if(resultValidacion == null || resultValidacion.data == null || resultValidacion.message == null)
        {
            await Toast.Make("Se requiere verificación en linea por falta de datos, pero no se encontró servidor.").Show();
            return false;
        }

        resultUser.log_fec_acceso = resultValidacion.data[0].datetime;
        
        App.Session.CurrentUserFront = resultUser;
        App.Session.CurrentUserFront.empresas = resultValidacion.data[0].companies;
        
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
        if (foundUser != null)
        {
            // Conservar fechas de sincronización locales (no vienen del login online)
            itemInsert.log_fec_sincro = foundUser.log_fec_sincro;
            itemInsert.log_fec_sincro_nc = foundUser.log_fec_sincro_nc;
            resultUser.log_fec_sincro = foundUser.log_fec_sincro;
            resultUser.log_fec_sincro_nc = foundUser.log_fec_sincro_nc;
            App.Session.CurrentUserFront.log_fec_sincro = foundUser.log_fec_sincro;
            App.Session.CurrentUserFront.log_fec_sincro_nc = foundUser.log_fec_sincro_nc;
            await database.UpdateAsync(itemInsert);
        }
        else
        {
            await database.InsertAsync(itemInsert);
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
                obj.check_journal_id_
                );
        }
    }

    [Obsolete]
    private async Task<res_company[]> PrepareCompanies(user_access userFound)
    {
        if (userFound.companies == null)
        {
            return new res_company[0];
        }

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
        IEnumerable<OdooConnection> filtered = await connectionsDb.GetItemsAsync(c => c.Active);
        OdooConnectionItems = new ObservableCollection<OdooConnection>(filtered.ToList());
        ddCompany.ItemsSource = OdooConnectionItems;
        ddCompany.ItemDisplayBinding = new Binding("Name");

        OdooConnection selectedConnection;

        if (UseConnectionSessionFixes)
        {
            var preferredId = GetPreferredConnectionId();
            selectedConnection = preferredId.HasValue
                ? OdooConnectionItems.FirstOrDefault(c => c.Id == preferredId.Value)
                : null;
            selectedConnection ??= OdooConnectionItems.FirstOrDefault();

            Debug.WriteLine(preferredId.HasValue
                ? $"Conexión preferida (Recordarme/sesión): Id={preferredId.Value}, Name={selectedConnection?.Name}"
                : "Conexión preferida: primera activa");
        }
        else
        {
            selectedConnection = OdooConnectionItems.FirstOrDefault();
        }

        ddCompany.SelectedItem = selectedConnection;
        Debug.WriteLine("Conexiones cargadas!!");
    }
        
    public async Task<bool> SetDataSessionOffLine(User resultUser, user_access userFound, DateTime currentDate)
    {
        var database = new UserAccessDb(App.Session.odooConnection.DbNameSqlite);
        res_company[] _empresas = new res_company[0];

        _empresas = await PrepareCompanies(userFound);

        if (_empresas.Length == 0)
        {
            return await SetDataSessionOnLine(resultUser, currentDate);
        }

        resultUser.log_fec_acceso = userFound.log_fec_acceso;
        resultUser.log_fec_sincro = userFound.log_fec_sincro;
        resultUser.log_fec_sincro_nc = userFound.log_fec_sincro_nc;
        
        App.Session.CurrentUserFront = resultUser;
        App.Session.CurrentUserFront.empresas = _empresas;

        //Se vuelve a reasignar la sesión global
        DMSA.Sync.Core.Constants.Session = App.Session;

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
        if (UseConnectionSessionFixes && _isConnectionLoading)
        {
            await Toast.Make("Espere, preparando conexión...").Show();
            return;
        }

        if (string.IsNullOrEmpty(txtUser.Text.Trim()) || string.IsNullOrEmpty(txtPassword.Text.Trim()))
        {
            await Toast.Make($"Debe ingresar sus credenciales").Show();
            Debug.WriteLine($"Debe ingresar sus credenciales");
            return;
        }

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
                username = txtUser.Text.Trim(),
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
                    LoginSelector.IsVisible = false;
                    CompanySelector.IsVisible = true;

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

                LoginSelector.IsVisible = false;
                CompanySelector.IsVisible = true;
                await RefreshLastSyncLabelAsync();
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
                        databasename = App.Session.odooConnection.DbName,
                        log_fec_acceso = currentDate.Date
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
                await RefreshLastSyncLabelAsync();
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

    public void SaveSession(bool rememberMe, bool autologin)
    {
        Preferences.Set("is_rememberme", rememberMe);
        Preferences.Set("is_autologin", autologin);

        if (!rememberMe)
        {
            Preferences.Remove("App.Session");
            return;
        }

        if (App.Session == null)
            return;

        try
        {
            var sessionSerialized = JsonConvert.SerializeObject(App.Session);
            Preferences.Set("App.Session", sessionSerialized);
        }
        catch
        {

        }
    }

    public void LoadSession()
    {
        try
        {
            var rememberMe = Preferences.Get("is_rememberme", false);
            var autologin = Preferences.Get("is_autologin", false);

            if (!rememberMe)
            {
                //App.Session = null;
                return;
            }
            
            string appSession = Preferences.Get("App.Session", string.Empty);

            if (string.IsNullOrEmpty(appSession))
            {
                //App.Session = null;
                return;
            }

            var LoadedSession = JsonConvert.DeserializeObject<AppSession>(appSession);

            chkRememberme.IsChecked = rememberMe;
            txtUser.Text = LoadedSession.CurrentUserFront?.username ?? string.Empty;
            txtPassword.Text = LoadedSession.CurrentUserFront != null ? CryptoHelper.Decrypt(LoadedSession.CurrentUserFront.password) : string.Empty;
            _ = RefreshLastSyncLabelAsync(LoadedSession);

            if (!autologin)
            {
                return;
            }

            App.Session = LoadedSession;
            if (UseConnectionSessionFixes)
                DMSA.Sync.Core.Constants.Session = App.Session;
            App.Current.MainPage = new MainPageTab();
        }
        catch
        {
            Preferences.Remove("App.Session");
            App.Session = null;
        }
    }

    private async Task RefreshLastSyncLabelAsync(AppSession session = null)
    {
        try
        {
            var syncDate = DateTime.MinValue;

            // 1) Sesión en memoria (solo si la fecha es válida)
            var source = session ?? App.Session;
            if (source?.CurrentUserFront != null && source.CurrentUserFront.log_fec_sincro.Year > 2000)
                syncDate = source.CurrentUserFront.log_fec_sincro;

            // 2) Preference dedicada (sobrevive ClearSession / login online)
            if (syncDate.Year <= 2000)
            {
                var raw = Preferences.Get("last_log_fec_sincro", string.Empty);
                if (!string.IsNullOrEmpty(raw) && DateTime.TryParse(raw, out var prefDate) && prefDate.Year > 2000)
                    syncDate = prefDate;
            }

            // 3) Sesión guardada (Recordarme)
            if (syncDate.Year <= 2000)
            {
                var appSession = Preferences.Get("App.Session", string.Empty);
                if (!string.IsNullOrEmpty(appSession))
                {
                    var loaded = JsonConvert.DeserializeObject<AppSession>(appSession);
                    if (loaded?.CurrentUserFront != null && loaded.CurrentUserFront.log_fec_sincro.Year > 2000)
                        syncDate = loaded.CurrentUserFront.log_fec_sincro;
                }
            }

            // 4) SQLite local
            if (syncDate.Year <= 2000 && App.Session?.odooConnection != null
                && !string.IsNullOrWhiteSpace(App.Session.odooConnection.DbNameSqlite))
            {
                var userDb = new UserAccessDb(App.Session.odooConnection.DbNameSqlite);
                user_access found = null;

                if (source?.CurrentUserFront?.uid > 0)
                    found = await userDb.GetItemAsync(source.CurrentUserFront.uid);

                if (found == null && !string.IsNullOrWhiteSpace(txtUser?.Text))
                {
                    var userName = txtUser.Text.Trim();
                    var users = await userDb.GetItemsAsync();
                    found = users?.FirstOrDefault(u => u.username == userName);
                }

                if (found == null)
                {
                    var users = await userDb.GetItemsAsync();
                    found = users?
                        .Where(u => u.log_fec_sincro.Year > 2000)
                        .OrderByDescending(u => u.log_fec_sincro)
                        .FirstOrDefault();
                }

                if (found != null && found.log_fec_sincro.Year > 2000)
                    syncDate = found.log_fec_sincro;
            }

            MainThread.BeginInvokeOnMainThread(() =>
            {
                lblLastSync.Text = syncDate.Year > 2000
                    ? "Ult. sincronizacion: " + syncDate.ToString("dd/MM/yyyy HH:mm")
                    : "Ult. sincronizacion: -";
            });
        }
        catch
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                lblLastSync.Text = "Ult. sincronizacion: -";
            });
        }
    }

    private void RefreshAppUpdateDateLabel()
    {
        try
        {
            var updateDate = AppTools.GetAppInstallOrUpdateDate();
            lblAppUpdateDate.Text = updateDate.HasValue && updateDate.Value.Year > 2000
                ? "Actualizacion APK: " + updateDate.Value.ToString("dd/MM/yyyy HH:mm")
                : "Actualizacion APK: -";
        }
        catch
        {
            lblAppUpdateDate.Text = "Actualizacion APK: -";
        }
    }

    public void ClearSession()
    {
        //Preferences.Remove("App.Session");
        //Preferences.Set("is_rememberme", false);
        //Preferences.Set("is_autologin", false);
        //App.Session = null;

        App.Session = new AppSession();
        App.Session.AppVersion = AppInfo.Current.VersionString;
        App.Session.SqliteCoreDbName = "DMOrders_app";
        App.Session.AppCodeOdoo = "02";
        App.Session.AppMobileId = 2;

        if (DeviceInfo.Platform == DevicePlatform.Android)
        {
            App.Session.AppVersion = AppInfo.Current.VersionString + "." + AppInfo.Current.BuildString;
        }
    }

    private void btnAccess_Clicked(object sender, EventArgs e)
    {
        if(ddCompany.SelectedItem != null && ddAgency.SelectedItem != null)
        {
            App.Session.res_Company = SelCompany;
            App.Session.res_center = (res_center) ddAgency.SelectedItem;
            SaveSession(chkRememberme.IsChecked, chkAutoLogin.IsChecked);
            
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
                await RootPatch();
                LoadSession();
                await SetupLogin();
            });

            _isFirstAppearing = false;
        }
        else
        {
            // Esto ocurre cada vez que vuelvas a la página
            Console.WriteLine("La página ya apareció antes.");
            _ = RefreshLastSyncLabelAsync();
        }        
    }

    private async void DragGestureRecognizer_DragStarting(object sender, DragStartingEventArgs e)
    {
        if (_executed)
            return;

        Debug.WriteLine("Drag iniciado");

        _longPressCts = new CancellationTokenSource();
        _executed = false;

        try
        {
            await Task.Delay(LONG_PRESS_MS, _longPressCts.Token);

            _executed = true;
            Debug.WriteLine("LONG PRESS EJECUTADO");

            ShowConnectionsCommand?.Execute(null);
        }
        catch (TaskCanceledException)
        {
            Debug.WriteLine("Cancelado");
        }
    }
}