using CobranzasDMSA_Odoo.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DMOrders.Models.Filters;
using DMOrders.Pages.Sys;
using DMSA.Models.Clientes;
using DMSA.Models.Odoo.DMOrders;
using DMSA.Models.Odoo.Native;
using System.Diagnostics;
using System.Reflection;
using System.Windows.Input;
using UraniumUI.Dialogs;

namespace DMOrders.Pages.Fragments.Orders
{
    //[XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class DataGrid : ContentView
    {
        res_partner selectedItemData { get; set; }

        string filterCode = "";
        string filterId = "";
        string filterName = "";
        FDays filterDays = null;
        FStatus filterStatus = null;

        public ICommand EditCommand { get; set; }

        public ContentView ViewParent
        {
            get => (ContentView)GetValue(ViewParentProperty);
            set => SetValue(ViewParentProperty, value);
        }

        public static readonly BindableProperty ViewParentProperty =
            BindableProperty.Create(nameof(ViewParent), typeof(ContentView), typeof(DataGrid));

        public DataGrid()
        {
            InitializeComponent();
            BindingContext = new ListViewModel();

            //IDispatcherTimer timer;

            //timer = Dispatcher.CreateTimer();
            //timer.Interval = TimeSpan.FromMilliseconds(1000);
            //timer.IsRepeating = false;

            //timer.Tick += (s, e) =>
            //{
            //UpdateParticles();
            //canvasView.InvalidateSurface();
            //OnTapGestureRecognizerTapped(this, null);
            //};
            //timer.Start();

            //_dataGrid1.RowTappedCommand = rowTappedCommand;
            
            EditCommand = new Command(EditItem);
            //EditCommand = new RelayCommand<ActivityHeader>(EditItem);
        }

        //protected override void OnSizeAllocated(double width, double height)
        //{
        //    base.OnSizeAllocated(width, height);

        //    //if (_dataGrid1 != null)
        //    //{
        //    //    _dataGrid1.HeightRequest = height;
        //    //    _dataGrid1.WidthRequest = width;
        //    //}
        //}

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
            ListViewModel mainViewModelCliAprob = new ListViewModel();
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
            //BUG: Crash sino se hace esta validación
            if (e.CurrentSelection.Count == 0) return;

            Debug.WriteLine(ViewParent);
            Debug.WriteLine(e.ToString());

            var customerContainer = (Customers.Container)ViewParent;
            customerContainer.LoadInfo((res_partner)e.CurrentSelection[0]);

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
            ((ListViewModel)this.BindingContext).LoadData();

            //OnTapGestureRecognizerTapped(this, null);
        }

        internal void LoadData(res_partner ResPartner, string DocNumber, DateTime? dateStart, DateTime? dateEnd, FStatus Status)
        {
            //filterCode = Code;
            //filterId = Id;
            //filterName = Name;
            //filterDays = Days;
            //filterStatus = Status;

            ((ListViewModel)this.BindingContext).LoadData();
        }

        private async void MyCollectionView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Debug.WriteLine("MyCollectionView_SelectionChanged");
        }

        private async void EditItem(object obj)
        {
            Debug.WriteLine("EditItem");
            Details viewObj = new Details();
            viewObj.CurrentSaleOrder = (sale_order) obj;
            viewObj.CurrentCompany = App.Session.res_Company;
            //objPage.Sel_AccountMoveSendHeader = (AccountMoveSendHeader)obj;
            //objPage.editionMode = true;
            viewObj.Disappearing += ViewObj_Disappearing;
            await viewObj.PrepareForm();
            await Navigation.PushModalAsync(viewObj);
        }

        private void ViewObj_Disappearing(object? sender, EventArgs e)
        {
            Debug.WriteLine("ViewObj_Disappearing");
        }
    }
}