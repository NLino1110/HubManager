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
        public ICommand EditCommand { get; set; }
        public ICommand DeleteCommand { get; set; }

        public ContentView ViewParent
        {
            get => (ContentView)GetValue(ViewParentProperty);
            set => SetValue(ViewParentProperty, value);
        }

        public static readonly BindableProperty ViewParentProperty =
            BindableProperty.Create(nameof(ViewParent), typeof(ContentView), typeof(DataGrid));

        public static readonly BindableProperty FiltersViewProperty =
        BindableProperty.Create(
            nameof(FiltersView),
            typeof(Filters),
            typeof(DataGrid),
            default(Filters),
            validateValue: (bindable, value) => value is null || value is Filters, // opcional
            propertyChanged: OnFiltersChanged);

        public Filters FiltersView
        {
            get => (Filters)GetValue(FiltersViewProperty);
            set => SetValue(FiltersViewProperty, value);
        }

        public DataGrid()
        {
            InitializeComponent();
            BindingContext = new ListViewModel(FiltersView);            
            EditCommand = new Command(EditItem);
            DeleteCommand = new Command(DeleteItem);
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

        private void OnContentViewTapped(object sender, EventArgs e)
        {
            Console.WriteLine("Se tocó el ContentView o alguno de sus hijos");
        }

        void OnTapGestureRecognizerTapped(object sender, TappedEventArgs args)
        {
            var viewModel = (ListViewModel)BindingContext;
            viewModel.LoadDataByTimer();

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
                
        private void btnBuscar_Clicked(object sender, EventArgs e)
        {
            ((ListViewModel)this.BindingContext).LoadDataByTimer();
        }

        internal void LoadData(Filters _filters)
        {
            FiltersView = _filters;
            OnTapGestureRecognizerTapped(this, null);
        }

        private static void OnFiltersChanged(BindableObject bindable, object oldValue, object newValue)
        {
            var view = (DataGrid)bindable;
            view.ApplyFilters();
        }

        private void ApplyFilters()
        {
            if (BindingContext is ListViewModel vm)
            {
                vm.filters = FiltersView;
                //vm.LoadDataByTimer();
            }
        }

        private async void MyCollectionView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Debug.WriteLine("MyCollectionView_SelectionChanged");
        }

        private async void EditItem(object obj)
        {
            Debug.WriteLine("EditItem");
            Crud viewObj = new Crud();
            viewObj.CurrentSaleOrder = (sale_order) obj;
            viewObj.CurrentCompany = App.Session.res_Company;            
            //objPage.editionMode = true;
            viewObj.Disappearing += ViewObj_Disappearing;
            await viewObj.PrepareForm();
            await Navigation.PushModalAsync(viewObj);
        }

        private async void DeleteItem(object obj)
        {
            //App.Current.Windows[0].Handler.PlatformView.Focus();

            var leave = await Application.Current.Windows[0].Page.DisplayAlert("Atención", "Desea eliminar la orden seleccionada?", "Si", "No");

            if (!leave)
            {
                return;
            }

            ((ListViewModel)BindingContext).RemoveOrder((sale_order)obj);
        }

        private void ViewObj_Disappearing(object? sender, EventArgs e)
        {
            Debug.WriteLine("ViewObj_Disappearing");
            ((ListViewModel)this.BindingContext).LoadDataByTimer();
        }

        private async void GotoCustomers_Clicked(object sender, EventArgs e)
        {
            try
            {
                var mainPage = (MainPageTab)App.Current.MainPage;
                mainPage.SelectTab("Clientes");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error al cambiar a la pestaña Pedidos: {ex.Message}");
            }
        }

        private async void ButtonSync_Clicked(object sender, EventArgs e)
        {
            Debug.WriteLine("Send!!");
            //var leave = await DisplayAlert("Enviar", "¿Desea enviar esta orden al ERP? Los cambios realizados serán almacenados.", "Si", "No");

            //if (!leave)
            //{
            //    return;
            //}

            //await UITools.ShowLoadingPopup(this);

            //bool orderHasChanges = true;

            //if (orderHasChanges)
            //{
            //    await UITools.SetNotifyLoadingPopup("Almacenando orden...");
            //    await SaveOrder();
            //}

            //await UITools.SetNotifyLoadingPopup("Preparando orden...");

            //ServerPusher serverPusher = new ServerPusher();

            //var orderLinesList = ((CrudViewModel)this.BindingContext).OrderLines.ToList();
            //CurrentSaleOrder.order_line = new List<OrderLineWrapper>();
            //CurrentSaleOrder._center_id = App.Session.odooConnection.res_center_default;

            //foreach (var orderLine in orderLinesList)
            //{
            //    CurrentSaleOrder.order_line.Add(new OrderLineWrapper(orderLine));
            //}

            //await UITools.SetNotifyLoadingPopup("Sincronizando orden...");
            //bool sendOk = await serverPusher.SendSaleOrder(CurrentSaleOrder);

            //await UITools.HideLoadingPopup();

            //if (sendOk)
            //{
            //    await DisplayAlert("Envío de datos", "Envío correcto", "Aceptar");
            //    await Navigation.PopModalAsync();
            //}
        }
    }
}