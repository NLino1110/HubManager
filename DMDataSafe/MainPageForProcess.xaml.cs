using DMDataSafe.AppPages.Sys;
using DMDataSafe.Modals;
using DMDataSafe.ViewModels;
using DMSA.Models.Odoo.Customers;
using System.Diagnostics;
using System.Windows.Input;

namespace DMDataSafe
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MainPageForProcess : ContentPage
    {
        int count = 0;

        public MainPageForProcess()
        {
            InitializeComponent();
            lblUser.Text = App.Session.CurrentUserFront.username;
            
            var empresas = App.Session.CurrentUserFront.empresas;
            var center = App.Session.res_center;

            if (center != null)
            {
                lblAgencia.Text = center.name;
                lblAgencia.TextColor = Colors.DarkGreen;
            }

            IDispatcherTimer timer;

            timer = Dispatcher.CreateTimer();
            timer.Interval = TimeSpan.FromMilliseconds(1000);
            timer.IsRepeating = false;
            
            timer.Tick += (s, e) =>
            {
                OnTapGestureRecognizerTapped(this, null);
            };
            timer.Start();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

#if ANDROID
            Microsoft.Maui.ApplicationModel.Platform.CurrentActivity.RequestedOrientation = Android.Content.PM.ScreenOrientation.Landscape;
#elif IOS
             UIKit.UIDevice.CurrentDevice.SetValueForKey(Foundation.NSNumber.FromNInt((int)(UIKit.UIInterfaceOrientation.LandscapeLeft)), new Foundation.NSString("orientation"));  

#endif
            DeviceDisplay.Current.MainDisplayInfoChanged += Current_MainDisplayInfoChanged;
        }

        private void Current_MainDisplayInfoChanged(object sender, DisplayInfoChangedEventArgs e)
        {
            if (e.DisplayInfo.Orientation == DisplayOrientation.Landscape)
            {
                NavigationPage.SetHasNavigationBar(this, false);
                Shell.SetNavBarIsVisible(this, false);
            }
        }

        protected override bool OnBackButtonPressed()
        {
            return true;
        }

        void OnTapGestureRecognizerTapped(object sender, TappedEventArgs args)
        {   
            string filterName = txtFilter.Text.ToUpper();
            MainViewModelCliAprob mainViewModelCliAprob = new MainViewModelCliAprob(filterName);            
            BindingContext = mainViewModelCliAprob;            
            Debug.WriteLine("Tap:" + sender.ToString());
        }

        async void OnTapLabelUser(object sender, TappedEventArgs args)
        {            
            Debug.WriteLine("Tap:" + sender.ToString());            
            await Navigation.PushModalAsync(new NavigationPage(new About()), false);            
        }

        private async void _dataGrid1_ItemSelected(object sender, SelectionChangedEventArgs e)
        {
            
        }

        private async void BtnSelect(object sender, EventArgs e)
        {
            Debug.WriteLine("Seleccionado");
            Button btnItem = (Button) sender;
            
            if (btnItem.Parent != null && btnItem.Parent.Parent != null)
            {                
                _dataGrid1.SelectedItem = btnItem.Parent.Parent.BindingContext;
                var rowData = _dataGrid1.SelectedItem as CustomerDataConsent;

                if (rowData is CustomerDataConsent cliente)
                {                    
                    _dataGrid1.SelectedItem = rowData;
                    await ShowConfirmClient(cliente);                    
                }
            }
        }

        private async void BtnClose_Clicked(object sender, EventArgs e)
        {
            bool answer = await DisplayAlertAsync("Salir", "¿Está seguro que desea cerrar la sessión? ",
            "Cerrar Sesión",
            "Cancelar");

            //Debug.WriteLine("Answer: " + answer);
            if (!answer)
            {
                return;
            }

            var loginPage = new Login();
            loginPage.ClearSession();
            App.Current.MainPage = loginPage;
        }

        private async Task ShowConfirmClient(CustomerDataConsent cliente)
        {
            ConfirmClient obj = new ConfirmClient();
            obj.selectedCustomer = cliente;
            obj.BindingContextObj = ((MainViewModelCliAprob)BindingContext);
            await Navigation.PushModalAsync(obj, false);

        }

        public ICommand TapCommand => new Command<string>(launch_browser);

        private async void launch_browser(string url)
        {
            Debug.WriteLine($"*** Tap: {url}");
        }

        private void btnBuscar_Clicked(object sender, EventArgs e)
        {
            OnTapGestureRecognizerTapped(this, null);
        }
    }
}