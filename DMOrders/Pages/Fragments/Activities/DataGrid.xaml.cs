using CobranzasDMSA_Odoo.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DMOrders.Models.Filters;
using DMOrders.Pages.Fragments.Customers;
using DMOrders.Pages.Sys;
using DMSA.Models.Odoo.DMOrders.tareas;
using DMSA.Models.Odoo.Native;
using System.Diagnostics;
using System.Reflection;
using System.Windows.Input;
using UraniumUI.Dialogs;

namespace DMOrders.Pages.Fragments.Activities
{
    //[XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class DataGrid : ContentView
    {
        res_partner selectedItemData { get; set; }

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
            BindingContext = new ListViewModel(FiltersView);
            EditCommand = new Command(EditItem);            
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
            var viewModel = (ListViewModel) BindingContext;
            viewModel.filters = FiltersView;
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
            Button btnItem = (Button) sender;
            
            var data = btnItem.Parent.Parent;

            if (btnItem.Parent != null && btnItem.Parent.Parent != null)
            {
                
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

        private async Task ShowConfirmClient(object cliente)
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

        internal void LoadData(Filters _filters)
        {
            FiltersView = _filters;            
            OnTapGestureRecognizerTapped(this, null);
            //MainViewModelCustomers mainViewModelCustomers = new MainViewModelCustomers(Name);            
            //BindingContext = mainViewModelCustomers;

            //Debug.WriteLine("Tap:" + sender.ToString());
        }

        private async void MyCollectionView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Debug.WriteLine("MyCollectionView_SelectionChanged");
        }

        public ICommand EditCommand { get; set; }

        private async void EditItem(object obj)
        {
            Debug.WriteLine("EditItem");

            ProjectTask CurrentActivityHeader = null;

            // Caso ideal: ya nos pasan el modelo
            if (obj is ProjectTask pt)
            {
                CurrentActivityHeader = pt;
            }
            else
            {
                // Si nos pasan la fila (ActivityRow) o cualquier objeto que exponga "Item", intentamos obtener el modelo por reflexión
                try
                {
                    if (obj is DMOrders.Controls.CustomRows.ActivityRow ar)
                    {
                        var prop = ar.GetType().GetProperty("Item", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                        var val = prop?.GetValue(ar);
                        if (val is ProjectTask pt2) CurrentActivityHeader = pt2;
                    }
                    else if (obj != null)
                    {
                        var t = obj.GetType();
                        var p = t.GetProperty("Item", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                        var val = p?.GetValue(obj);
                        if (val is ProjectTask pt3) CurrentActivityHeader = pt3;
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"EditItem: error al extraer Item por reflexión: {ex.Message}");
                }
            }

            if (CurrentActivityHeader == null)
            {
                Debug.WriteLine($"EditItem: parámetro inválido tipo={obj?.GetType().FullName}");
                return;
            }

            Details viewObj = new Details(CurrentActivityHeader);
            viewObj.Disappearing += ViewObj_Disappearing;
            await Navigation.PushModalAsync(viewObj);
        }

        private void ViewObj_Disappearing(object? sender, EventArgs e)
        {
            Debug.WriteLine("ViewObj_Disappearing");
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