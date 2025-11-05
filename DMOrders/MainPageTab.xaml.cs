using DMOrders.Pages.Fragments.Activities;
using DMOrders.Pages.Sys;
using DMOrders.Services.Database.Sqlite;
using DMSA.Models.Odoo.DMOrders.tareas;
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

        imageDebug.IsVisible = IsDebug;

        MenuItems = new ObservableCollection<MenuItemModel>
        {
            new() { Icon = "\uf279", Title = "Nueva actividad", Description = "Seguimiento de proceso.", Action = async () => ViewCell_Add_Task(null, EventArgs.Empty) },
            new() { Icon = "\uf0c7", Title = "Actualización", Description = "Sincronizar los datos principales.", Action = async () => ViewCell_Tapped_Update(null, EventArgs.Empty) },
            new() { Icon = "\uf2f5", Title = "Salir", Description = "Volver a ingresar credenciales.", Action = async () => ViewCell_Tapped_Exit_Regular(null, EventArgs.Empty) },
            new() { Icon = "\uf7d9", Title = "Configuraciones", Description = "Modificar rutas y entorno.", Action = async () => ShowSettings(null, EventArgs.Empty) },
        };

        BindingContext = this;
    }

    private void OnMenuItemSelected(object sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is MenuItemModel item)
        {
            item.Action?.Invoke();
            ((CollectionView)sender).SelectedItem = null; // deselecciona
        }
    }

    //protected override void OnSizeAllocated(double width, double height)
    //{
    //    base.OnSizeAllocated(width, height);

    //    tabViewMain.HeightRequest = height - 50;
    //    tabViewMain.WidthRequest = width;

    //    tabCustomers.HeightRequest = height - 100;
    //    //tabCustomers.WidthRequest = width - 100;
    //}

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

    private async void ViewCell_Add_Task(object sender, EventArgs e)
    {
        Debug.WriteLine("EditItem");
        
        ProjectTaskDb projectTaskDb = new ProjectTaskDb();
        string nameTodayTask = DateTime.Now.ToString("yyyy-MM-dd");
        var foundTodayTasks = await projectTaskDb.GetItemByNameAsync(App.Session.res_Company.id, nameTodayTask);
        
        ProjectTask CurrentActivityHeader = null;

        if (foundTodayTasks != null && foundTodayTasks.Count > 0)
        {
            CurrentActivityHeader = foundTodayTasks[0];
        }
        else
        {
            var newTask = new DMSA.Models.Odoo.DMOrders.tareas.ProjectTask()
            {
                name = nameTodayTask,
                company_id = App.Session.res_Company.id,
                create_uid = App.Session.CurrentUserFront.uid,
                stage_id_ = 1,
                project_id_ = 1, //proyecto predeterminado
                parent_id = 1, //tarea predeterminada
                date_assign = DateTime.Now,
                date_deadline = DateTime.Now,
                display_in_project = true,
                id_sync = 0,
                user_id = App.Session.CurrentUserFront.uid,
                user_ids = new int [App.Session.CurrentUserFront.uid] 
            };            

            //viewObj.CurrentActivityHeader = new DMSA.Models.Odoo.DMOrders.tareas.ProjectTask() { id = 0, name = nameTodayTask };
            await projectTaskDb.InsertAsync(newTask);
            CurrentActivityHeader = newTask;
        }

        Details viewObj = new Details(CurrentActivityHeader);
        viewObj.Disappearing += viewAddTask_Disappearing;
        await Navigation.PushModalAsync(viewObj);
    }

    private void viewAddTask_Disappearing(object? sender, EventArgs e)
    {        
        tabActivities.ReloadData();
    }

    private async void ViewCell_Tapped_Update(object sender, EventArgs e)
    {        
        UpdateData obj = new UpdateData();
        obj.Disappearing += UpdateData_Disappearing;
        await Navigation.PushModalAsync(obj);
    }

    private async void ViewCell_Tapped_Exit_Regular(object sender, EventArgs e)
    {
        //App.Current.MainPage = new DMOrders.AppShellStart();
        //App.Current.MainPage = new Login();
        App.Current.Windows[0].Page = new Login();
    }

    private async Task ViewCell_Tapped_Exit()
    {
        //ViewCell_Tapped_Exit_Regular(new object { }, null);
        //await MainThread.InvokeOnMainThreadAsync(() =>
        //{
            App.Current.Windows[0].Page = new Login();
        //});
    }

    private async void ShowSettings(object sender, EventArgs e)
    {
        Debug.WriteLine("SettingsPage");        
        Connections objPage = new Connections();        
        await Navigation.PushModalAsync(objPage);
    }        

    private void UpdateData_Disappearing(object? sender, EventArgs e)
    {
        //throw new NotImplementedException();
    }
}