using ClientAgree.AppPages;
using ClientAgree.AppPages.Sys;
using ClientAgree.Modals;
using ClientAgree.Models;
using ClientAgree.ViewModels;
using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;
using Maui.DataGrid;
using System.Diagnostics;
using System.Windows.Input;
using Microsoft.Maui.Devices;
using Models.DMSA.Mbw.Clientes;

namespace ClientAgree
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MainPageForProcess : ContentPage
    {
        int count = 0;

        public MainPageForProcess()
        {
            InitializeComponent();            
            //BindingContext = new MainViewModelCliAprob();

            lblUser.Text = App.Session.CurrentUser.nombreUsuario;

            //Se evalúa si los datos de la agencia a la que pertenece el usuario
            // fueron cargados correctamente
            if (App.Session.CurrentUser.accesos.Length > 0 && 
                App.Session.CurrentUser.accesos[0].agencias.Length > 0)
            {
                lblAgencia.Text = App.Session.CurrentUser.accesos[0].agencias[0].Nombre;
                lblAgencia.TextColor = Colors.DarkGreen;
            }

            IDispatcherTimer timer;

            timer = Dispatcher.CreateTimer();
            timer.Interval = TimeSpan.FromMilliseconds(1000);
            timer.IsRepeating = false;
            
            timer.Tick += (s, e) =>
            {
                //UpdateParticles();
                //canvasView.InvalidateSurface();
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
                //if you use navigatepage    
                NavigationPage.SetHasNavigationBar(this, false);
                //if you use shell    
                Shell.SetNavBarIsVisible(this, false);
            }
        }

        //private void OnCounterClicked(object sender, EventArgs e)
        //{
        //    count++;

        //    if (count == 1)
        //        CounterBtn.Text = $"Clicked {count} time";
        //    else
        //        CounterBtn.Text = $"Clicked {count} times";

        //    SemanticScreenReader.Announce(CounterBtn.Text);
        //}

        protected override bool OnBackButtonPressed()
        {
            return true;
        }

        void OnTapGestureRecognizerTapped(object sender, TappedEventArgs args)
        {
            //_dataGrid1.
            // Handle the tap
            //BindingContext = new MainViewModelCliAprob();
            //(new MainViewModelCliAprob()).RefreshCommand.Execute(this);

            string filterName = txtFilter.Text.ToUpper();

            MainViewModelCliAprob mainViewModelCliAprob = new MainViewModelCliAprob(filterName);
            //MainViewModelCliAprob mainViewModelCliAprob = (MainViewModelCliAprob) BindingContext;
            //mainViewModelCliAprob.FilterName = txtFilter.Text;
            BindingContext = mainViewModelCliAprob;

            //((MainViewModelCliAprob) BindingContext).RefreshCommand.Execute(this);
            Debug.WriteLine("Tap:" + sender.ToString());
        }

        async void OnTapLabelUser(object sender, TappedEventArgs args)
        {            
            Debug.WriteLine("Tap:" + sender.ToString());
            //About obj = new About();
            await Navigation.PushModalAsync(new NavigationPage(new About()), false);
            //await Shell.Current.GoToAsync("about");
        }

        private async void _dataGrid1_ItemSelected(object sender, SelectionChangedEventArgs e)
        {
            //Console.W
            //e.CurrentSelection

            //if (e.CurrentSelection.Count == 0) return;

            //CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
            //ClienteAprobacion team = (ClienteAprobacion)e.CurrentSelection[0];

            //string text = "Seleccionado: " + team.NOMBRESCLIENTE;
            //ToastDuration duration = ToastDuration.Short;
            //double fontSize = 14;
            ////var toast = Toast.Make(text, duration, fontSize);
            ////toast.Show(cancellationTokenSource.Token).Wait();

            //ConfirmClient obj = new ConfirmClient();
            //obj.selectedCustomer = team;
            ////App.Current.MainPage = obj;
            //await Navigation.PushModalAsync(obj, false);
            ////await Task.Delay(2000);
            ////await Navigation.PopModalAsync();
            ////App.Current.MainPage = obj;
            ////await Navigation.PushModalAsync(obj, true);
        }

        private async void BtnSelect(object sender, EventArgs e)
        {
            //await Navigation.PopModalAsync(false);
            Debug.WriteLine("Seleccionado");
            Button btnItem = (Button) sender;
            //Se asume que el botón esta dentro de un template y a su vez dentro del DataGridRow
            // por lo cual se asume que la conversión es a 2 niveles arriba 

            if (btnItem.Parent != null && btnItem.Parent.Parent != null)
            {
                //var row = (Maui.DataGrid.DataGridRow) btnItem.Parent.Parent;
                //var rowData = row.BindingContext;

                _dataGrid1.SelectedItem = btnItem.Parent.Parent.BindingContext;
                var rowData = _dataGrid1.SelectedItem as ClienteAprobacion;

                if (rowData is ClienteAprobacion cliente)
                {
                    //Se realiza la seleccion manual de la fila, ya que si se hace clic en el botón no es automática
                    _dataGrid1.SelectedItem = rowData;
                    await ShowConfirmClient(cliente);
                    // Ejemplo: muestra una alerta con los valores de las propiedades
                    //DisplayAlert("Información", $"Propiedad1: {propiedad1}, Propiedad2: {propiedad2}", "Aceptar");
                }
            }
        }

        private async void BtnClose_Clicked(object sender, EventArgs e)
        {
            bool answer = await DisplayAlert("Salir", "¿Está seguro que desea cerrar la sessión? ",
            "Cerrar Sesión",
            "Cancelar");

            //Debug.WriteLine("Answer: " + answer);
            if (!answer)
            {
                return;
            }

            //App.Session = new DMSA.Models.Security.Proformas.AppSession();
            App.Current.MainPage = new Login();
        }

        //private async void _dataGrid1_ItemRowTap(object sender, TappedEventArgs e)
        //{
        //    Debug.WriteLine("Tap Grid:" + sender.ToString());

        //    var row = (Maui.DataGrid.DataGridRow) sender;            
        //    var rowData = row.BindingContext;

        //    if (rowData is ClienteAprobacion cliente)
        //    {
        //        await ShowConfirmClient(cliente);
        //        // Ejemplo: muestra una alerta con los valores de las propiedades
        //        //DisplayAlert("Información", $"Propiedad1: {propiedad1}, Propiedad2: {propiedad2}", "Aceptar");
        //    }
        //}

        private async Task ShowConfirmClient(ClienteAprobacion cliente)
        {
            ConfirmClient obj = new ConfirmClient();
            obj.selectedCustomer = cliente;
            obj.BindingContextObj = ((MainViewModelCliAprob)BindingContext);
            await Navigation.PushModalAsync(obj, false);

            //((MainViewModelCliAprob)BindingContext).RefreshCommand.Execute(this);
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