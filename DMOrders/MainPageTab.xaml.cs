using CommunityToolkit.Maui.Markup;
using CommunityToolkit.Maui.Sample.ViewModels.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using DMOrders.Controls;
using DMOrders.Pages;
using DMOrders.Pages.Fragments.Activities;
using DMOrders.Pages.Sys;
using DMOrders.Services.Database.Sqlite;
using DMSA.Models.Odoo.DMOrders;
using DMSA.Models.Odoo.DMOrders.tareas;
using DMSA.Models.Odoo.Native;
using Microsoft.Maui.Layouts;
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

    readonly PopupSizeConstants popupSizeConstants;
    readonly CsharpBindingPopupViewModel csharpBindingPopupViewModel;

    bool isExpanded = false;
    public MainPageTab()
	{
		InitializeComponent();        

        NavigationPage.SetHasNavigationBar(this, false);

        //tabViewMain.SelectedTab = tabViewMain.Tabs[1];

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

        //var floatingButtonsLayout = new AbsoluteLayout();

        //floatingButtonsLayout.BackgroundColor = Colors.Aqua;
        //floatingButtonsLayout.WidthRequest = 100;

        //var button1 = new Button
        //{
        //    CornerRadius = 80,
        //    Text = "+"
        //};

        //AbsoluteLayout.SetLayoutFlags(button1, AbsoluteLayoutFlags.PositionProportional);
        //AbsoluteLayout.SetLayoutBounds(button1, new Rect(1, 1, AbsoluteLayout.AutoSize, AbsoluteLayout.AutoSize));
        //floatingButtonsLayout.Children.Add(button1);

        //var button2 = new Button
        //{
        //    CornerRadius = 80,
        //    Text = "-"
        //};

        //AbsoluteLayout.SetLayoutFlags(button2, AbsoluteLayoutFlags.PositionProportional);
        //AbsoluteLayout.SetLayoutBounds(button2, new Rect(2, 0.9, AbsoluteLayout.AutoSize, AbsoluteLayout.AutoSize));
        //floatingButtonsLayout.Children.Add(button2);

        //MainContent.Children.Add(floatingButtonsLayout);
        //if (popupSizeConstants == null)
        //{
        //    this.popupSizeConstants = new PopupSizeConstants(DeviceDisplay.Current);
        //}
        //else
        //{
        //    this.popupSizeConstants = popupSizeConstants;
        //}
        //this.csharpBindingPopupViewModel = csharpBindingPopupViewModel;

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

    //private async void BtnTopTools_OnClicked_Clicked(object sender, EventArgs e)
    //{
    //    var fntSrc = (FontImageSource) btnTopTools.ImageSource;

    //    int unicodevalue = char.ConvertToUtf32(fntSrc.Glyph, 0);

    //    if (unicodevalue == 61641)
    //    {
    //        await btnTopTools.RotateTo(90, 200);
    //        btnTopTools.Rotation = 0;
    //        fntSrc.Glyph = "\uf00d";
    //        TopTools.IsVisible = true;
    //    }
    //    else
    //    {
    //        await btnTopTools.RotateTo(-90, 200);
    //        btnTopTools.Rotation = 0;
    //        fntSrc.Glyph = "\uf0c9";
    //        TopTools.IsVisible = false;
    //    }
    //}

    private async void btnSave_Clicked(object sender, EventArgs e)
    {
    }

    private async void btnExit_Clicked(object sender, EventArgs e)
    {
        //SendBackButtonPressed();
        App.Current.MainPage = new Login(null);
    }

    private async void ViewCell_Add_Sale(object sender, EventArgs e)
    {

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
                stage_id = 1,
                project_id = 1, //proyecto predeterminado
                parent_id = 1, //tarea predeterminada
                date_assign = DateTime.Now,
                date_deadline = DateTime.Now,
                display_in_project = true,
                user_id = App.Session.CurrentUserFront.uid
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
        Debug.WriteLine("viewAddTask_Disappearing");
    }

    private async void ViewCell_Tapped_Update(object sender, EventArgs e)
    {        
        UpdateData obj = new UpdateData();
        obj.Disappearing += UpdateData_Disappearing;
        await Navigation.PushModalAsync(obj);
    }

    private async void ViewCell_Tapped_Exit(object sender, EventArgs e)
    {
        //App.Current.MainPage = new DMOrders.AppShellStart();
        App.Current.MainPage = new Login();
    }

    private async void ShowSettings(object sender, EventArgs e)
    {
        Debug.WriteLine("SettingsPage");
        //SettingsPage objPage = new SettingsPage();
        Connections objPage = new Connections();
        //objPage.Disappearing += ObjSettingPage_Disappearing;
        await Navigation.PushModalAsync(objPage);
    }        

    private void TabView_ActiveTabChanged(object sender, int e)
    {
        Debug.WriteLine(sender);
        Debug.WriteLine(e);
    }

    //private async void btnUpdate_Clicked(object sender, EventArgs e)
    //{
    //    BtnTopTools_OnClicked_Clicked(sender, e);

    //    UpdateData obj = new UpdateData();

    //    //obj.Sel_Company_Id = new res_company()
    //    //{
    //    //    id = se.id,
    //    //    name = se.name
    //    //};

    //    obj.Disappearing += UpdateData_Disappearing;

    //    //await Navigation.PushAsync(obj, false);

    //    await Navigation.PushModalAsync(obj);
    //}

    private void UpdateData_Disappearing(object? sender, EventArgs e)
    {
        //throw new NotImplementedException();
    }
}