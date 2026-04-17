using ApiManager;
using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;
using CommunityToolkit.Mvvm.Input;
using DMDataSafePreview.Services;
using DMSA.Models.Odoo.Abstract;
using DMSA.Models.Odoo.Accounting;
using DMSA.Models.Odoo.Native;
using DMSA.Models.Odoo.Security;
using DMSA.Models.Odoo.Tools;
using DMSA.Models.Security;
using DMSA.Sync.Core.Database.Sqlite;
using DMSA.Sync.Core.Database.Sqlite.Accounting;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Timers;
using System.Windows.Input;

namespace DMDataSafePreview.Pages;

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

    public Login()
    {
        InitializeComponent();
        BindingContext = this;
    }

    [RelayCommand]
    private async void ShowConnections()
    {
        await Navigation.PushModalAsync(new ContentPage());
    }

    public async Task SetupLogin()
    {
        OdooConnectionItems = new ObservableCollection<OdooConnection>();
        ddCompany.ItemsSource = OdooConnectionItems;        
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

        App.Session.useOfflineMode = false;

        lblAppVersion.Text = "Versión " + App.Session.AppVersion;

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

        if(App.Session.odooConnection.preload_email_domain)
        {
            txtUser.Text = "@" + App.Session.odooConnection.email_domain;
            txtPassword.Text = "";
        }

        if (App.Session.odooConnection.IsTestMode)
        {
            txtUser.Text = "djimenez@macronegocios.ec";
            txtPassword.Text = App.Session.odooConnection.PasswordFront;
        }
        else
        {
            if (!App.Session.odooConnection.preload_email_domain)
            {
                txtUser.Text = "";
                txtPassword.Text = "";
            }
        }
    }

    public async Task<bool> LoadSettingsFromDb()
    {
        OdooConnectionDb odooConnectionDb = new OdooConnectionDb();
        await Task.Run(async () => await odooConnectionDb.InitDefault());

        App.Session.CurrentUserFront = new User();

        return true;
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

        ddCompany.SelectedItem = OdooConnectionItems.FirstOrDefault();
        Debug.WriteLine("Conexiones cargadas!!");
    }

    private void btnAccess_Clicked(object sender, EventArgs e)
    {
        if(ddCompany.SelectedItem != null && ddAgency.SelectedItem != null)
        {
            App.Session.res_Company = SelCompany;
            App.Session.res_center = (res_center) ddAgency.SelectedItem;            
            //App.Current.MainPage = new MainPage();
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
    }
}
