//using ClientAgree.AppPages;
//using ClientAgree.AppPages.Sys;
//using ClientAgree.Modals;
using ClientAgree.Models;
using DMOrders.ViewModels.DataGrid;
using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;
using DMSA.Models.Clientes;
using Maui.DataGrid;
using System.Diagnostics;
using System.Windows.Input;
using Microsoft.Maui.Devices;
using CommunityToolkit.Maui.Markup;
using CommunityToolkit.Maui.Behaviors;
using System.Reflection;
using DMSA.Models.Odoo.Native;

namespace DMOrders.Pages.data
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class Customers : ContentView
    {
        int count = 0;
        public ICommand IncreaseLongPressCountCommand { get; }

        res_partner selectedItemData { get; set; }

        public Customers()
        {
            InitializeComponent();
            BindingContext = new MainViewModelCustomers();

            IDispatcherTimer timer;

            timer = Dispatcher.CreateTimer();
            timer.Interval = TimeSpan.FromMilliseconds(1000);
            timer.IsRepeating = false;
            
            timer.Tick += (s, e) =>
            {
                //UpdateParticles();
                //canvasView.InvalidateSurface();
                //OnTapGestureRecognizerTapped(this, null);
            };
            timer.Start();

            //_dataGrid1.RowTappedCommand = rowTappedCommand;

            IncreaseLongPressCountCommand = new Command(() =>
            {
                // Lógica al mantener presionado
                Console.WriteLine("👉 LongPressCommand ejecutado");
            });

            var touchBehavior = new TouchBehavior
            {
                LongPressDuration = 200,
            };

            //touchBehavior.SetBinding(
            //        TouchBehavior.LongPressCommandProperty,
            //        new Binding("IncreaseLongPressCountCommand")
            //        {
            //            Source = this.BindingContext
            //        });

            //_dataGrid1.Behaviors.Add(touchBehavior);
        }

        ////        protected override void OnAppearing()
        ////        {
        ////            base.OnAppearing();

        ////#if ANDROID
        ////            Microsoft.Maui.ApplicationModel.Platform.CurrentActivity.RequestedOrientation = Android.Content.PM.ScreenOrientation.Landscape;
        ////#elif IOS
        ////             UIKit.UIDevice.CurrentDevice.SetValueForKey(Foundation.NSNumber.FromNInt((int)(UIKit.UIInterfaceOrientation.LandscapeLeft)), new Foundation.NSString("orientation"));  

        ////#endif
        ////            DeviceDisplay.Current.MainDisplayInfoChanged += Current_MainDisplayInfoChanged;
        ////        }

        protected override void OnSizeAllocated(double width, double height)
        {
            base.OnSizeAllocated(width, height);

            if (_dataGrid1 != null)
            {
                _dataGrid1.HeightRequest = height;
                _dataGrid1.WidthRequest = width;
            }
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

        ////protected override bool OnBackButtonPressed()
        ////{
        ////    return true;
        ////}

        private void OnContentViewTapped(object sender, EventArgs e)
        {
            Console.WriteLine("Se tocó el ContentView o alguno de sus hijos");
        }

        void OnTapGestureRecognizerTapped(object sender, TappedEventArgs args)
        {
            string filterName = "";

            MainViewModelCustomers mainViewModelCliAprob = new MainViewModelCustomers(filterName);
            BindingContext = mainViewModelCliAprob;

            //MainThread.BeginInvokeOnMainThread(() =>
            //{
            //    InvalidateMeasure();
            //});

            Debug.WriteLine("Tap:" + sender.ToString());
        }

        async void OnTapLabelUser(object sender, TappedEventArgs args)
        {            
            //Debug.WriteLine("Tap:" + sender.ToString());
            //await Navigation.PushModalAsync(new NavigationPage(new About()), false);
        }

        private async void _dataGrid1_ItemSelected(object sender, SelectionChangedEventArgs e)
        {
            Debug.WriteLine(e.ToString());

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

        private async void btnSelectItem(object sender, EventArgs e)
        {
            //////await Navigation.PopModalAsync(false);
            ////Debug.WriteLine("Seleccionado");
            Button btnItem = (Button) sender;
            //////Se asume que el botón esta dentro de un template y a su vez dentro del DataGridRow
            ////// por lo cual se asume que la conversión es a 2 niveles arriba 

            var data = btnItem.Parent.Parent;

            if (btnItem.Parent != null && btnItem.Parent.Parent != null)
            {
                _dataGrid1.SelectedItem = btnItem.Parent.Parent.BindingContext;
                var itemData = _dataGrid1.SelectedItem as ClienteAprobacion;
                Debug.WriteLine(itemData);

                //var type = Assembly.Load("Maui.DataGrid").GetType("Maui.DataGrid.DataGridRow");
                //if (type == null)
                //{
                //    Console.WriteLine("Tipo no encontrado.");
                //    return;
                //}

                //var instance = Activator.CreateInstance(type);

                //if (instance is View view)
                //{

                //    var touchBehavior = new TouchBehavior
                //    {
                //        LongPressDuration = 750
                //    };

                //    touchBehavior.SetBinding(
                //        TouchBehavior.LongPressCommandProperty,
                //        new Binding("IncreaseLongPressCountCommand")
                //        {
                //            Source = this.BindingContext
                //        });

                //    _dataGrid1.Behaviors.Add(touchBehavior);

                //}                

                //var dataProps = data.GetType().GetProperties();
                //foreach (var prop in dataProps)
                //{
                //    var targetProp = type.GetProperty(prop.Name,
                //        BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

                //    if (targetProp != null && targetProp.CanWrite)
                //    {
                //        var value = prop.GetValue(data);
                //        targetProp.SetValue(instance, value);
                //    }
                //}

                //var row = (Maui.DataGrid.DataGridRow) btnItem.Parent.Parent;
                //var rowData = row.BindingContext;
                ////    if (rowData is ClienteAprobacion cliente)
                ////    {
                ////        //Se realiza la seleccion manual de la fila, ya que si se hace clic en el botón no es automática
                ////        _dataGrid1.SelectedItem = rowData;
                ////        await ShowConfirmClient(cliente);
                ////        // Ejemplo: muestra una alerta con los valores de las propiedades
                ////        //DisplayAlert("Información", $"Propiedad1: {propiedad1}, Propiedad2: {propiedad2}", "Aceptar");
                ////    }
            }
        }

        private async void BtnClose_Clicked(object sender, EventArgs e)
        {
            ////bool answer = await DisplayAlert("Salir", "¿Está seguro que desea cerrar la sessión? ",
            ////"Cerrar Sesión",
            ////"Cancelar");

            //////Debug.WriteLine("Answer: " + answer);
            ////if (!answer)
            ////{
            ////    return;
            ////}
        }

        private async void _dataGrid1_ItemRowTap(object sender, TappedEventArgs e)
        {
            ////Debug.WriteLine("Tap Grid:" + sender.ToString());

            ////var row = (Maui.DataGrid.DataGridRow) sender;            
            ////var rowData = row.BindingContext;

            ////if (rowData is ClienteAprobacion cliente)
            ////{
            ////    await ShowConfirmClient(cliente);                
            ////}
        }

        private async Task ShowConfirmClient(ClienteAprobacion cliente)
        {
            //ConfirmClient obj = new ConfirmClient();
            //obj.selectedCustomer = cliente;
            //obj.BindingContextObj = ((MainViewModelCliAprob) BindingContext);
            //await Navigation.PushModalAsync(obj, false);
        }

        private void btnBuscar_Clicked(object sender, EventArgs e)
        {
            OnTapGestureRecognizerTapped(this, null);
        }

        internal void LoadData(string Name)
        {
            OnTapGestureRecognizerTapped(this, null);
            //MainViewModelCustomers mainViewModelCustomers = new MainViewModelCustomers(Name);            
            //BindingContext = mainViewModelCustomers;

            //Debug.WriteLine("Tap:" + sender.ToString());
        }
    }
}