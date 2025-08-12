using DMOrdersUI.Pages.Sys;
using DMOrdersUI.Services.Helpers;
using DMSA.Models.Odoo.Native;
using InputKit.Shared.Controls;
using System.Diagnostics;
using UraniumUI.Dialogs;
using UraniumUI.Pages;

namespace DMOrdersUI
{
    public partial class MainPage : UraniumContentPage
    {
        public bool IsDebug =>
#if DEBUG
    true;
#else
    false;
#endif

        public IDialogService DialogService { get;  }

        //private readonly IDialogService[] dialogServices;

        public MainPage(IEnumerable<IDialogService> dialogServices)
        {
            SelectionView.GlobalSetting.CornerRadius = 0;
            InitializeComponent();
            NavigationPage.SetHasNavigationBar(this, false);

            if(App.Session.res_Company != null)
                lblCompany.Text = App.Session.res_Company.name;

            if(App.Session.res_center != null)
                lblStore.Text = App.Session.res_center.name;

            if (App.Session.CurrentUser != null)
            {
                lblUser.Text = App.Session.CurrentUser.username;
                lblUserName.Text = App.Session.CurrentUser.nombres;
            }

            imageDebug.IsVisible = IsDebug;

            //this.dialogServices = dialogServices.ToArray();
            //this.DialogService = this.dialogServices.FirstOrDefault();
            DialogService = null; // dialogServices.ToList()[1];

            ServicesExposer.DialogService = DialogService;
        }

        public MainPage()
        {
            SelectionView.GlobalSetting.CornerRadius = 0;
            InitializeComponent();

            NavigationPage.SetHasNavigationBar(this, false);

            lblCompany.Text = App.Session.res_Company.name;
            lblStore.Text = App.Session.res_center.name;
        }

        private async void ShowSettings(object sender, EventArgs e)
        {
            DMOrdersUI.Pages.Sys.SettingsPage settings = new DMOrdersUI.Pages.Sys.SettingsPage();
            var result = ServicesExposer.DialogService.DisplayViewAsync(
                "Configuración", settings, "Cerrar");            
        }

        private async void AskRadioButtons(object sender, EventArgs e)
        {
            var count = 4;

            var options = GenerateOptions(count);

            var result = await DialogService.DisplayRadioButtonPromptAsync(
                "Pick one of them below",
                options,
                 "Option 1");

            Debug.WriteLine("Selected option: " + result);
            
            //result;
        }

        private static IEnumerable<string> GenerateOptions(int count)
        {
            for (int i = 1; i <= count; i++)
            {
                yield return "Option " + i;
            }
        }        

        private void ShowBottomSheet(object sender, EventArgs e)
        {            
            bottomSheet.IsPresented = true;
        }       

        private async void btnSave_Clicked(object sender, EventArgs e)
        {
        }

        private async void btnUpdate_Clicked(object sender, EventArgs e)
        {
            //BtnTopTools_OnClicked_Clicked(sender, e);
            //UpdateData obj = new UpdateData();
            //obj.Disappearing += UpdateData_Disappearing;
            //await Navigation.PushModalAsync(obj);
        }

        private async void btnExit_Clicked(object sender, EventArgs e)
        {
            Application.Current.MainPage = new DMOrdersUI.AppShellStart();
            //App.Current.MainPage = new Login(dialogServices);
        }

        private async void ViewCell_Tapped_Update(object sender, EventArgs e)
        {
            bottomSheet.IsPresented = false;
            UpdateData obj = new UpdateData();

            //obj.Sel_Company_Id = new res_company()
            //{
            //    id = se.id,
            //    name = se.name
            //};

            obj.Disappearing += UpdateData_Disappearing;

            //await Navigation.PushAsync(obj, false);

            await Navigation.PushModalAsync(obj);
        }

        private async void ViewCell_Tapped_Exit(object sender, EventArgs e)
        {
            App.Current.MainPage = new DMOrdersUI.AppShellStart();
            //App.Current.MainPage = new Login(dialogServices);
        }

        private void UpdateData_Disappearing(object? sender, EventArgs e)
        {
            //throw new NotImplementedException();
        }

        private async void ViewCell_Add_Sale(object sender, EventArgs e)
        {  

        }

        private async void ViewCell_Add_Task(object sender, EventArgs e)
        {
            
        }
    }
}