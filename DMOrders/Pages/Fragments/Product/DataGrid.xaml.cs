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

namespace DMOrders.Pages.Fragments.Product
{
    //[XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class DataGrid : ContentView
    {
        product_product selectedItemData { get; set; }

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

        public DataGrid()
        {
            InitializeComponent();
            BindingContext = new ProductListViewModel(FiltersView);

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


        private static void OnFiltersChanged(BindableObject bindable, object oldValue, object newValue)
        {
            var view = (DataGrid)bindable;
            view.ApplyFilters();
        }

        private void ApplyFilters()
        {
            if (BindingContext is ProductListViewModel vm)
            {
                vm.filters = FiltersView;
                //vm.LoadDataByTimer();
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
            var viewModel = (ProductListViewModel)BindingContext;
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

            //var customerContainer = (Customers.Container)ViewParent;
            //await Task.Run(async () => await customerContainer.LoadInfo((res_partner)e.CurrentSelection[0]));
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

        internal void LoadData(Filters _filters)
        {
            FiltersView = _filters;
            OnTapGestureRecognizerTapped(this, null);
        }

        private async void ProductCollectionView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //BUG: Crash sino se hace esta validación
            if (e.CurrentSelection.Count == 0) return;

            Debug.WriteLine(ViewParent);
            Debug.WriteLine(e.ToString());

            var customerContainer = (Product.Container) ViewParent;
            await customerContainer.LoadInfo((product_product) e.CurrentSelection[0]);
        }


        public ICommand EditCommand { get; set; }

        private async void EditItem(object obj)
        {
            Debug.WriteLine("EditItem");
            //Details viewObj = new Details();
            //viewObj.CurrentActivityHeader = (ActivityHeader)obj;
            ////objPage.Sel_AccountMoveSendHeader = (AccountMoveSendHeader)obj;
            ////objPage.editionMode = true;
            //viewObj.Disappearing += ViewObj_Disappearing;
            //await Navigation.PushAsync(viewObj);
        }

        private void ViewObj_Disappearing(object? sender, EventArgs e)
        {
            Debug.WriteLine("ViewObj_Disappearing");
        }

        protected override void OnSizeAllocated(double width, double height)
        {
            base.OnSizeAllocated(width, height);

            int desiredItemWidth = 180;
            int span = Math.Max(1, (int)(width / desiredItemWidth));

            //if (MyCollectionView.ItemsLayout is GridItemsLayout layout)
            //{
            //    layout.Span = span;
            //}
        }

        private void listViewProduct_ItemTapped(object sender, object e)
        {
            if (e == null) return;

            Debug.WriteLine(ViewParent);
            Debug.WriteLine(e.ToString());

            var customerContainer = (Product.Container) ViewParent;
            product_product selectedProduct = (product_product) e;
            customerContainer.LoadInfo(selectedProduct);
        }
    }
}