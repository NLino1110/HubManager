using CommunityToolkit.Maui.Markup;
using CommunityToolkit.Maui.Sample.ViewModels.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using DMOrders.Controls;
using DMOrders.Pages;
using DMOrders.Pages.Sys;
using DMSA.Models.Odoo.Native;
using Microsoft.Maui.Layouts;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace DMOrders;

public partial class MainPageTab : ContentPage
{
    readonly PopupSizeConstants popupSizeConstants;
    readonly CsharpBindingPopupViewModel csharpBindingPopupViewModel;

    bool isExpanded = false;
    public MainPageTab()
	{
		InitializeComponent();
        BindingContext = new MainViewModel();

        NavigationPage.SetHasNavigationBar(this, false);

        tabViewMain.SelectedTab = tabViewMain.Tabs[1];

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
        DMOrders.Pages.Sys.SettingsPage settings = new DMOrders.Pages.Sys.SettingsPage();
        
    }

    public partial class MainViewModel : ObservableObject
    {
        [ObservableProperty]
        private Tab selectedTab;

        public ObservableCollection<Tab> Tabs { get; set; } = new();
        public ObservableCollection<Tab> Tabs2 { get; set; } = new();
        public ObservableCollection<Tab> Tabs3 { get; set; } = new();

        public MainViewModel()
        {            
            //Tabs.Add(new Tab()
            //{
            //    Title = "Tab1 - TOP",
            //    Content = new Label() { Text = "Tab1 Label" },
            //    Icon = "cat.png"
            //});
            //Tabs.Add(new Tab()
            //{
            //    Title = "Tab2 - TOP",
            //    Content = new Label() { Text = "Tab2 Label" },
            //    Icon = "dog.png"
            //});
            //Tabs2.Add(new Tab()
            //{
            //    Title = "Tab1",
            //    Content = new Label() { Text = "Tab1 Label" },
            //    Icon = "cat.png"
            //});
            //Tabs2.Add(new Tab()
            //{
            //    Title = "Tab2",
            //    Content = new Label() { Text = "Tab2 Label" },
            //    Icon = "dog.png"
            //});
            //Tabs3.Add(new Tab()
            //{
            //    Title = "Tab1",
            //    Content = new Label() { Text = "Tab1 Label" },
            //    Icon = "cat.png"
            //});
            //Tabs3.Add(new Tab()
            //{
            //    Title = "Tab2",
            //    Content = new Label() { Text = "Tab2 Label" },
            //    Icon = "dog.png"
            //});
            //SelectedTab = Tabs2[0];
        }
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