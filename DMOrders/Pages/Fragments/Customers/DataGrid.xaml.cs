using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DMOrders.Controls.Tools;
using DMOrders.Models.Filters;
using DMOrders.Pages.Fragments.Orders;
using DMSA.Models.Clientes;
using DMSA.Models.Odoo.Native;
using MPowerKit.VirtualizeListView;
using System.Diagnostics;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Input;

namespace DMOrders.Pages.Fragments.Customers
{   
    public partial class DataGrid : ContentView
    {
        res_partner SelectedItem { get; set; }

        string filterCode = "";
        string filterId = "";
        string filterName = "";
        FDays filterDays = null;
        FStatus filterStatus = null;

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

        //public ICommand EditCommand { get; set; }
        //public ICommand NewOrderCommand { get; set; }
        public DataGrid()
        {
            InitializeComponent();
            BindingContext = new ViewModel(FiltersView);

            //EditCommand = new Command(AddProcess);
            //NewOrderCommand = new Command(NewOrder);                                          
        }

        private static void OnFiltersChanged(BindableObject bindable, object oldValue, object newValue)
        {
            var view = (DataGrid)bindable;
            view.ApplyFilters();
        }

        private void ApplyFilters()
        {
            if (BindingContext is ViewModel vm)
            {
                vm.filters = FiltersView;
                //vm.LoadDataByTimer();
            }
        }
        //private async void AddProcess(object obj)
        //{
        //    SelectedItem = (res_partner)obj;

        //    Crud viewObj = new Crud();
        //    viewObj.CurrentPartner = SelectedItem;
        //    viewObj.CurrentCompany = App.Session.res_Company;
        //    viewObj.CurrentSaleOrder = null;
        //    await Application.Current.MainPage.Navigation.PushModalAsync(new NavigationPage(viewObj));
        //}

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
            //ViewModel viewModelObject = new ViewModel(filterCode, filterId, filterName, filterDays, filterStatus);
            //BindingContext = viewModelObject;

            var viewModel = (ViewModel) BindingContext;
            viewModel.LoadDataByTimer();

            //MainThread.BeginInvokeOnMainThread(() =>
            //{
            //    InvalidateMeasure();
            //});

            //listView.ItemsSource = viewModel.ItemsData;
            
            Debug.WriteLine("Tap:" + sender.ToString());
        }

        async void OnTapLabelUser(object sender, TappedEventArgs args)
        {            
            //Debug.WriteLine("Tap:" + sender.ToString());
            //await Navigation.PushModalAsync(new NavigationPage(new About()), false);
        }

        private async void _dataGrid1_ItemSelected(object sender, SelectionChangedEventArgs e)
        {
            Dispatcher.Dispatch(async () =>
            {
                //BUG: Crash sino se hace esta validación
                if (e.CurrentSelection.Count == 0) return;

                Debug.WriteLine(ViewParent);
                Debug.WriteLine(e.ToString());

                var customerContainer = (Customers.Container)ViewParent;
                customerContainer.LoadInfo((res_partner)e.CurrentSelection[0]);
            });           

        }

        //public static T FindParentOfType<T>(Element element) where T : Element
        //{
        //    while (element != null)
        //    {
        //        if (element is T parent)
        //            return parent;

        //        element = element.Parent;
        //    }
        //    return null;
        //}

        //[Obsolete("???")]
        //private async void btnSelectItem(object sender, EventArgs e)
        //{  

        //    //////await Navigation.PopModalAsync(false);
        //    ////Debug.WriteLine("Seleccionado");
        //    Button btnItem = (Button) sender;
        //    //////Se asume que el botón esta dentro de un template y a su vez dentro del DataGridRow
        //    ////// por lo cual se asume que la conversión es a 2 niveles arriba 

        //    var data = btnItem.Parent.Parent;

        //    if (btnItem.Parent != null && btnItem.Parent.Parent != null)
        //    {
                
        //        var itemData = SelectedItem;
        //        Debug.WriteLine(itemData);

        //        Crud viewObj = new Crud();
        //        viewObj.CurrentPartner = itemData;
        //        viewObj.CurrentCompany = App.Session.res_Company;
        //        viewObj.CurrentSaleOrder = null;
        //        viewObj.Disappearing += NewOrderPopup_Disappearing;
        //        await viewObj.PrepareForm();
        //        await Navigation.PushAsync(viewObj);

        //    }
        //}
        
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

        private void btnBuscar_Clicked(object sender, EventArgs e)
        {
            OnTapGestureRecognizerTapped(this, null);
        }

        internal void LoadData(Filters _filters)
        {
            FiltersView = _filters;
            OnTapGestureRecognizerTapped(this, null);            
        }

        //internal void LoadData(string Code, string Id, string Name, FDays Days, FStatus Status)
        //{
        //    filterCode = Code;
        //    filterId = Id;
        //    filterName = Name;
        //    filterDays = Days;
        //    filterStatus = Status;

        //    OnTapGestureRecognizerTapped(this, null);
        //    //MainViewModelCustomers mainViewModelCustomers = new MainViewModelCustomers(Name);            
        //    //BindingContext = mainViewModelCustomers;

        //    //Debug.WriteLine("Tap:" + sender.ToString());
        //}

        private async void FixedRefreshView_Refreshing(object sender, EventArgs e)
        {
            //await Task.Delay(5000);

            //FillItems();

            //(sender as FixedRefreshView).IsRefreshing = false;
        }

        private bool _isProcessing;

        private async void Button_Clicked(object sender, EventArgs e)
        {
            if (_isProcessing) return;

            var button = sender as Button;

            if (button == null || !button.IsEnabled)
                return;

            _isProcessing = true;
            button.IsEnabled = false;

            try
            { 

                SelectedItem = (res_partner)button.BindingContext;
                var viewModel = (ViewModel)BindingContext;
                viewModel.OnItemTapped(SelectedItem);

                if (SelectedItem != null)
                {
                    if (SelectedItem.misc_estado != "activo")
                    {
                        await Toast.Make("No se pueden crear ordenes para clientes inactivos.").Show();
                        button.IsEnabled = true;
                        _isProcessing = false;
                        return;
                    }
                    
                    var viewObj = new Crud();
                    viewObj.CurrentPartner = SelectedItem;
                    viewObj.CurrentCompany = App.Session.res_Company;
                    viewObj.CurrentSaleOrder = null;
                    await viewObj.PrepareForm();

                    viewObj.Unloaded += (sender, e) =>
                    {                        
                        button.IsEnabled = true;
                        _isProcessing = false;
                        
                    };                

                    await Navigation.PushModalAsync(viewObj, false);                    
                }
            }
            finally
            {
                //button.IsEnabled = true;
                //_isProcessing = false;
            }
        }

        //private async void NewOrder(object obj)
        //{
        //    SelectedItem = (res_partner)obj;

        //    var itemData = SelectedItem;
        //    Debug.WriteLine(itemData);

        //    Crud viewObj = new Crud();
        //    viewObj.CurrentPartner = itemData;
        //    viewObj.CurrentCompany = App.Session.res_Company;
        //    viewObj.CurrentSaleOrder = null;            
        //    //await viewObj.PrepareForm();
        //    await Navigation.PushModalAsync(viewObj);
        //}

        private void OnItemPressed(object sender, PointerEventArgs e)
        {
            if (sender is not BindableObject bo)
                return;

            if (bo.BindingContext is not res_partner item)
                return;

            if (bo.BindingContext is res_partner itemYes)
            {
                //CustomersCollectionView.SelectedItem = itemYes;
                Debug.WriteLine("OnItemPressed: " + itemYes.name);
            }

            //var vm = BindingContext as CustomersViewModel;
            //if (vm == null)
            //    return;

            //vm.SelectItem(item);
        }
    }
}