using CommunityToolkit.Maui.Alerts;
using DMOrders.AppPages.Sys;
using DMOrders.Controls.Tools;
using DMOrders.Pages.Fragments.Activities;
using DMOrders.Pages.Sys;
using DMOrders.Services.Update;
using DMSA.Models.Odoo.Tareas;
using DMSA.Sync.Core.Database.Sqlite.Sales;
using DMSA.Sync.Core.Update;
using DMSA.Sync.Core.Update.Pusher;
using System.Collections.ObjectModel;
using System.Diagnostics;
namespace DMOrders;

public partial class MainPageTab : ContentPage
{
    public bool IsDebug =>
#if DEBUG
    true;
#else
    false;
#endif

    bool isExpanded = false;

    bool is_loading_page = false;

    public class MenuItemModel
    {
        public string Icon { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public Func<Task> Action { get; set; }
    }

    public ObservableCollection<MenuItemModel> MenuItems { get; set; }

    public MainPageTab()
	{
		InitializeComponent();

        NavigationPage.SetHasNavigationBar(this, false);

        if (App.Session.res_Company != null)
            lblCompany.Text = App.Session.res_Company.name;

        if (App.Session.res_center != null)
            lblStore.Text = App.Session.res_center.name;

        if (App.Session.CurrentUser != null)
        {
            lblUser.Text = App.Session.CurrentUserFront.username;
            lblUserName.Text = App.Session.CurrentUserFront.nombres;
        }

        if (App.Session.odooConnection != null)
        {
            lblConnection.Text = App.Session.odooConnection.Name;
        }

        imageDebug.IsVisible = IsDebug;

        MenuItems = new ObservableCollection<MenuItemModel>
        {
            new() { Icon = "\uf279", Title = "Nueva actividad", Description = "Seguimiento de proceso.", Action = async () => ViewCell_Add_Task(null, EventArgs.Empty) },
            new() { Icon = "\uf1d8", Title = "Enviar datos", Description = "Sincronizar datos locales.", Action = () => SendFullData() },
            new() { Icon = "\uf103", Title = "Actualización", Description = "Sincronizar los datos principales.", Action = async () => ViewCell_Tapped_Update(null, EventArgs.Empty) },
            new() { Icon = "\uf2f5", Title = "Salir", Description = "Volver a ingresar credenciales.", Action = async () => await Exit_Special() },
            new() { Icon = "\uf05a", Title = "Acerca de", Description = "Informacíón de la aplicación.", Action = async () => ViewCell_Tapped_About(null, EventArgs.Empty) },
        };

        Loaded += (_, __) => ReloadData();
        BindingContext = this;
    }

    private void ReloadData()
    {
        tabCustomers?.ReloadData();
    }

    bool isUpdated = false;
    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (isUpdated)
            return;

        isUpdated = true;

        try
        {
            await AutoUpdate();

            tabCustomers.ReloadData();
            tabCustomers.ReloadFilter();
            tabProducts.ReloadFilter();
            tabOrders.ReloadFilter();
        }
        catch (Exception ex)
        {
            await Toast.Make("Error en actualización: " + ex.Message).Show();
        }
    }

    private async Task<bool> AutoUpdate()
    {
        await UITools.ShowLoadingPopup(this);
        await UITools.SetNotifyLoadingPopup("Ejecutando actualización...");
        
        LaunchManager launchManager = new LaunchManager();
        await launchManager.Execute();

        await Task.Delay(500);
        await UITools.HideLoadingPopup();        
        return true;
    }

    private async Task<bool> SendFullData()
    {
        bool result = await DisplayAlertAsync("¿Enviar datos?", "Si envía los datos ya no podrá modificarlos", "Sí", "No");
        if (!result)
        {
            return false;
        }

        //Enviará las ordenes y las tareas que no se han sincronizado
        await UITools.ShowLoadingPopup(this);
        await UITools.SetNotifyLoadingPopup("Ejecutando envío de datos...");
        
        SaleOrders serverPusher = new SaleOrders();
        await serverPusher.SendAllSaleOrders();
        await serverPusher.SendAllProjectTask();

        await UITools.SetNotifyLoadingPopup("Ejecutando extracción de datos...");

        ServerPuller serverPuller = new ServerPuller();
        await serverPuller.SyncSaleOrders();
                
        await UITools.HideLoadingPopup();

        return true;
    }

    private void OnMenuItemSelected(object sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is MenuItemModel item)
        {
            item.Action?.Invoke();
            ((CollectionView)sender).SelectedItem = null; // deselecciona
        }
    }

    private async void OnTogglePanelTapped(object sender, EventArgs e)
    {
        if (isExpanded)
        {
            // Contraer
            await OverlayPanel.TranslateTo(0, 0, 100, Easing.CubicIn);
            OverlayPanel.HeightRequest = 33;
        }
        else
        {
            // Expandir
            await OverlayPanel.TranslateTo(0, 0, 100, Easing.CubicOut);
            OverlayPanel.HeightRequest = 220;
        }

        isExpanded = !isExpanded;
    }

    public async Task AddnewActivity()
    {
        Debug.WriteLine("AddnewActivity");

        int partner_id = App.Session.CurrentUserFront.partner_id;
        int user_id = App.Session.CurrentUserFront.uid;

        ProjectTaskDb projectTaskDb = new ProjectTaskDb(App.Session.odooConnection.DbNameSqlite);
        string nameTodayTask = DateTime.Now.ToString("yyyy-MM-dd");
        var foundTodayTasks = await projectTaskDb.GetItemByNameAsync(App.Session.res_Company.id, nameTodayTask, user_id);

        ProjectTask CurrentActivityHeader = null;

        if (foundTodayTasks != null && foundTodayTasks.Count > 0)
        {
            CurrentActivityHeader = foundTodayTasks[0];
        }
        else
        {
            var newTask = new ProjectTask()
            {
                name = nameTodayTask,
                company_id = App.Session.res_Company.id,
                create_uid = App.Session.CurrentUserFront.uid,
                stage_id_ = 57,
                project_id_ = App.Session.odooConnection.project_id,
                parent_id = 1,
                date_assign = DateTime.Now,
                date_deadline = DateTime.Now,
                display_in_project = true,
                id_sync = 0,
                user_id = App.Session.CurrentUserFront.uid,
                user_ids = new int[App.Session.CurrentUserFront.uid],
                state = "draft"
            };

            //viewObj.CurrentActivityHeader = new DMSA.Models.Odoo.DMOrders.tareas.ProjectTask() { id = 0, name = nameTodayTask };
            await projectTaskDb.InsertAsync(newTask);
            CurrentActivityHeader = newTask;
        }

        Details viewObj = new Details(CurrentActivityHeader);
        viewObj.Disappearing += viewAddTask_Disappearing;
        await Navigation.PushModalAsync(viewObj);
    }

    private async void ViewCell_Add_Task(object sender, EventArgs e)
    {
        await AddnewActivity();        
    }

    private void viewAddTask_Disappearing(object? sender, EventArgs e)
    {        
        tabActivities.ReloadData();
    }

    private async void ViewCell_Tapped_Update(object sender, EventArgs e)
    {
        if (is_loading_page)
            return;

        is_loading_page = true;

        UpdateData obj = new UpdateData();
        obj.Disappearing += UpdateData_Disappearing;        
        await Navigation.PushModalAsync(obj);
    } 

    private async void ViewCell_Tapped_About(object sender, EventArgs e)
    {
        if (is_loading_page)
            return;

        is_loading_page = true;

        About obj = new About();
        obj.Disappearing += UpdateData_Disappearing;
        await Navigation.PushModalAsync(obj);
    }

    private async Task Exit_Special()
    {
        bool result = await DisplayAlertAsync("¿Cerrar la sesión?", "Regresar a la pantalla de login", "Sí", "No");
        if (!result)
        {
            return;
        }

        await MainThread.InvokeOnMainThreadAsync(async () =>
        {
            await AppTools.ClearCacheData();

            var window = App.Current.Windows.FirstOrDefault();
            if (window == null)
                return;

            if (window.Page?.Navigation?.ModalStack?.Count > 0)
            {
                await window.Page.Navigation.PopModalAsync(false);
            }

            await Task.Delay(50);

            var loginPage = new Login();
            loginPage.ClearSession();
            window.Page = loginPage;
        });
    }

    private async Task ViewCell_Tapped_Exit()
    {
        var loginPage = new Login();
        loginPage.ClearSession();
        App.Current.Windows[0].Page = loginPage;
    }

    private async void ShowSettings(object sender, EventArgs e)
    {
        Debug.WriteLine("SettingsPage");
        Connections objPage = new Connections();
        await Navigation.PushModalAsync(objPage);
    }        

    private void UpdateData_Disappearing(object? sender, EventArgs e)
    {
        is_loading_page = false;
        tabProducts.ClearCache();
    }

    private void tabViewMain_SelectedTabChanged(object sender, UraniumUI.Material.Controls.TabItem e)
    {
        Debug.WriteLine(sender);
        Debug.WriteLine(e);

        if(e.Title.ToLower() == "clientes")
        {
            tabCustomers?.ReloadData();
        }

        if (e.Title.ToLower() == "artículos")
        {            
            tabProducts?.ReloadData();
        }

        if (e.Title.ToLower() == "pedidos")
        {            
            tabOrders?.ReloadData();
        }

        if (e.Title.ToLower() == "actividades")
        {            
            tabActivities?.ReloadData();
        }
    }

    public void SelectTab(string tabTitle)
    {
        foreach (var tab in tabViewMain.Tabs)
        {
            if (tab.Title.Equals(tabTitle, StringComparison.OrdinalIgnoreCase))
            {
                tabViewMain.SelectedTab = tab;
                break;
            }
        }
    }
}