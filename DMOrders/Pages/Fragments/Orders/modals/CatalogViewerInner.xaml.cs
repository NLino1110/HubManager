using CommunityToolkit.Maui.Extensions;
using CommunityToolkit.Maui.Views;
using DMOrders.Controls;
using DMOrders.Models.Filters;
using DMOrders.Services.Database.Sqlite;
using DMSA.Models.Odoo.Native;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows.Input;

namespace DMOrders.Pages.Fragments.Orders.modals;

public partial class CatalogViewerInner : ContentView
{
    public static readonly BindableProperty ItemPickedCommandProperty =
        BindableProperty.Create(nameof(ItemPickedCommand), typeof(ICommand), typeof(CatalogViewerInner), default(ICommand));

    public ICommand ItemPickedCommand
    {
        get => (ICommand)GetValue(ItemPickedCommandProperty);
        set => SetValue(ItemPickedCommandProperty, value);
    }

    public ContentView ViewParent
    {
        get => (ContentView)GetValue(ViewParentProperty);
        set => SetValue(ViewParentProperty, value);
    }

    public static readonly BindableProperty ViewParentProperty =
        BindableProperty.Create(nameof(ViewParent), typeof(ContentView), typeof(DataGrid));

    private int span_columns = 4;
    double swipeThreshold = 50; // Distancia mínima para considerar un swipe
    double panX = 0;

    
    public ObservableCollection<product_marca> Brands { get; set; } = new();

    FStatus[] newProducts { get; set; }
    FStatus[] stockProducts { get; set; }
    FStatus[] sortProducts { get; set; }

    string filter_code { get; set; }
    string filter_name { get; set; }
    product_marca filter_brand { get; set; }
    int filter_new { get; set; }
    int filter_stock { get; set; }
    int filter_sort { get; set; }

    public CatalogViewerInner()
    {
        InitializeComponent();
        Setup();
    }

    private async Task LoadTopMarcasAsync()
    {
        //int[] topMarcas = new int[] { 66, 21, 46, 59, 24, 31, 68, 44, 57, 3, 22, 69, 64 };
        int[] topMarcas = new int[] { 545, 669, 716, 773, 869, 512, 517, 701, 554, 968, 872, 960, 682 };

        ProductMarcaDb marcasDb = new ProductMarcaDb();
        var itemsTopMarcas = await marcasDb.GetItemsAsync(topMarcas);

        MainThread.BeginInvokeOnMainThread(() =>
        {
            Brands =
            [
                new product_marca { id = 0, name = "No seleccionada" },
                new product_marca { id = -1, name = "🔍 Buscar..." }
            ];

            foreach (var marca in itemsTopMarcas)
                Brands.Add(marca);

            ddfBrands.ItemsSource = Brands;
            ddfBrands.ItemDisplayBinding = new Binding("name");
            ddfBrands.SelectedItem = Brands[0];
            ddfBrands.SelectedItemChanged += DdfBrands_SelectedItemChanged;
            filter_brand = Brands[0];
        });
    }

    public void Setup()
    {
        //CommandSelectListItem = new Command(SelectListItem);
        BindingContext = new CatalogViewerModel();

        //Brands =
        //    [
        //        new product_marca { id = 0, name = "No seleccionada" },
        //        new product_marca { id = -1, name = "🔍 Buscar..." },
        //        //new product_marca { id = 1, name = "Marca 1" },
        //        //new product_marca { id = 2, name = "Marca 2" },
        //        //new product_marca { id = 3, name = "Marca 3" },
        //        //new product_marca { id = 4, name = "Marca 4" },
        //        //new product_marca { id = 5, name = "Marca 5" },
        //        //new product_marca { id = 6, name = "Marca 6" },
        //        //new product_marca { id = 7, name = "Marca 7" },
        //        //new product_marca { id = 8, name = "Marca 8" },
        //    ];

        LoadTopMarcasAsync();

        //ddfBrands.ItemsSource = Brands;
        //ddfBrands.ItemDisplayBinding = new Binding("name");
        //ddfBrands.SelectedItem = Brands[0];
        //ddfBrands.SelectedItemChanged += DdfBrands_SelectedItemChanged;
        //filter_brand = Brands[0];

        newProducts =
        [
            new FStatus { id = 0, Name = "Todos" },
            new FStatus { id = 1, Name = "Ultimo mes" },
            new FStatus { id = 2, Name = "Ultimos 3 meses" },
            new FStatus { id = 3, Name = "Ultimos 6 meses" },
        ];

        ddfNews.ItemsSource = newProducts;
        ddfNews.ItemDisplayBinding = new Binding("Name");
        ddfNews.SelectedItem = newProducts[0];
        ddfNews.SelectedItemChanged += DdfNews_SelectedItemChanged;
        filter_new = newProducts[0].id;

        stockProducts =
        [
            new FStatus { id = 0, Name = "Todos" },
            new FStatus { id = 1, Name = "Mayor a 5" },
            new FStatus { id = 2, Name = "Mayor a 25" },
            new FStatus { id = 3, Name = "Mayor a 50" },
            new FStatus { id = 4, Name = "Mayor a 75" },
            new FStatus { id = 5, Name = "Mayor a 100" }
        ];

        ddfStock.ItemsSource = stockProducts;
        ddfStock.ItemDisplayBinding = new Binding("Name");
        ddfStock.SelectedItem = stockProducts[0];
        ddfStock.SelectedItemChanged += DdfStock_SelectedItemChanged;
        filter_stock = stockProducts[0].id;

        sortProducts =
        [
            new FStatus { id = 0, Name = "Todos" },
            new FStatus { id = 1, Name = "Por clasificacion" },
            new FStatus { id = 2, Name = "Por codigo" },
            new FStatus { id = 3, Name = "Por descripcion" },
        ];

        ddfSort.ItemsSource = sortProducts;
        ddfSort.ItemDisplayBinding = new Binding("Name");
        ddfSort.SelectedItem = sortProducts[0];
        ddfSort.SelectedItemChanged += DdfSort_SelectedItemChanged;
        filter_sort = sortProducts[0].id;
    }

    private async void DdfBrands_SelectedItemChanged(object? sender, object e)
    {
        var vm = BindingContext as CatalogViewerModel;

        product_marca new_selected_brand = (product_marca) e;
        
        if (new_selected_brand != null && (new_selected_brand.id == -1 || new_selected_brand.id == 0))
        {
            if (new_selected_brand.id == -1)
            {
                Debug.WriteLine("Buscar");
                ddfBrands.SelectedItem = filter_brand;
                var selectedBrand = await PopupBrand(sender, null);

                if (selectedBrand != null)
                {
                    ddfBrands.IsEnabled = false;
                    ddfBrands.ItemsSource = null;
                    Brands[0] = selectedBrand;
                    ddfBrands.ItemsSource = Brands;
                    ddfBrands.SelectedItem = selectedBrand;
                    filter_brand = selectedBrand;
                    ddfBrands.IsEnabled = true;
                }
                else
                {
                    ddfBrands.SelectedItem = filter_brand;                    
                }                
                
                vm.SetFilterBrand(filter_brand.id);
                return;
            }
        }

        if (new_selected_brand == null)
        {
            ddfBrands.IsEnabled = false;
            Debug.WriteLine("Clear");
            ddfBrands.ItemsSource = null;
            var nsBrand = new product_marca { id = 0, name = "No seleccionada" };
            Brands[0] = nsBrand;
            ddfBrands.ItemsSource = Brands;
            ddfBrands.SelectedItem = nsBrand;
            filter_brand = nsBrand;
            ddfBrands.IsEnabled = true;
        }
        else
        {
            Debug.WriteLine("Seleccion valida directa...");
            filter_brand = new_selected_brand;
        }
        
        vm.SetFilterBrand(filter_brand.id);
    }

    private async void DdfNews_SelectedItemChanged(object? sender, object e)
    {
        try
        {
            var vm = BindingContext as CatalogViewerModel;
            FStatus new_selected_item = (FStatus)e;
            filter_new = new_selected_item.id;
            vm.SetFilterNew(filter_new);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error en DdfNews_SelectedItemChanged: {ex.Message}");
        }
    }

    private async void DdfStock_SelectedItemChanged(object? sender, object e)
    {
        try 
        { 
            var vm = BindingContext as CatalogViewerModel;
            FStatus new_selected_item = (FStatus)e;
            filter_stock = new_selected_item.id;
            vm.SetFilterStock(filter_stock);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error en DdfStock_SelectedItemChanged: {ex.Message}");
        }
    }

    private async void DdfSort_SelectedItemChanged(object? sender, object e)
    {
        try
        { 
            var vm = BindingContext as CatalogViewerModel;
            FStatus new_selected_item = (FStatus)e;
            filter_sort = new_selected_item.id;
            vm.SetFilterSort(filter_sort);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error en DdfSort_SelectedItemChanged: {ex.Message}");
        }
    }

    async Task<product_marca> PopupBrand(object sender, EventArgs e)
    {
        product_marca selected_product_brand = null;

        var popupSizeConstants = new PopupSizeConstants(DeviceDisplay.Current);
        popupSizeConstants.CalculateSizes(DeviceDisplay.Current);

        var returnResultPopup = new PopupSelectMarca(popupSizeConstants);

        returnResultPopup.Company = App.Session.res_Company;

        returnResultPopup.CanBeDismissedByTappingOutsideOfPopup = false;

        var result = await PopupExtensions.ShowPopupAsync<product_marca>(App.Current.MainPage, returnResultPopup);

        if (result.Result != null)
        {
            selected_product_brand = result.Result;
        }

        return selected_product_brand;
    }

    //private async void SelectListItem(object objItem)
    //{
    //    if (objItem != null)
    //    {
    //        //((CatalogViewerModel)this.BindingContext).SelectedItem = (product_product)objItem;
    //        //_parentPopup.Close(objItem);
    //        Debug.WriteLine(objItem);
    //    }
    //    else
    //    {
    //        Debug.WriteLine("Error de objeto");
    //    }
    //}

    private void SelectSingleItem(object sender, EventArgs e)
    {
        var objItem = ((CatalogViewerModel)this.BindingContext).SelectedItem;
        if (objItem != null)
        {
            //ItemPickedCommand
            //_parentPopup.Close(objItem);
            this.IsVisible = false;

            if(ItemPickedCommand?.CanExecute(objItem) != null)
                ItemPickedCommand.Execute(objItem);

        }
        else
        {
            Debug.WriteLine("Error de objeto");
        }
    }

    private void OnPanUpdated(object sender, PanUpdatedEventArgs e)
    {
        switch (e.StatusType)
        {
            case GestureStatus.Started:
                panX = 0;
                break;
            case GestureStatus.Running:
                panX += e.TotalX;
                break;
            case GestureStatus.Completed:
                if (Math.Abs(panX) > swipeThreshold)
                {
                    var vm = BindingContext as CatalogViewerModel;

                    if (vm.PageSize == 1)
                    {
                        if (panX < 0 && vm.CanGoNext)
                            vm.NextPageCommand.Execute(null);
                        else if (panX > 0 && vm.CanGoPrevious)
                            vm.PreviousPageCommand.Execute(null);
                    }
                }
                break;
        }
    }

    private async void MyCollectionView_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        //BUG: Crash sino se hace esta validación
        if (e.CurrentSelection.Count == 0) return;

        Debug.WriteLine(ViewParent);
        Debug.WriteLine(e.ToString());
    }

    private void OnCloseClicked(object sender, EventArgs e)
    {
        //Debug.WriteLine(((CatalogViewerModel)this.BindingContext).SelectedItem);
        //_parentPopup.Close(null);
        IsVisible = false;
    }

    private async void SelectionView_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        //Debug.WriteLine($"SelectionView Property Changed: {e.PropertyName}");
        if (e.PropertyName.Equals("SelectedIndex"))
        {
            await SetViewMode(-1);
        }
    }

    private async void TextCode_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        //Debug.WriteLine($"TextCode Property Changed: {e.PropertyName}");
        if (e.PropertyName.Equals("Text"))
        {
            filter_code = TextCode.Text;
            var vm = BindingContext as CatalogViewerModel;
            vm.SetFilterCode(filter_code);
        }
    }

    private async void TextDescription_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        //Debug.WriteLine($"TextDescription Property Changed: {e.PropertyName}");
        if (e.PropertyName.Equals("Text"))
        {
            filter_name = TextDescription.Text;
            var vm = BindingContext as CatalogViewerModel;
            vm.SetFilterName(filter_name);
        }
    }

    private async Task SetViewMode(int ViewMode)
    {
        var vm = BindingContext as CatalogViewerModel;
        var selector = Resources["ProductTemplateSelector"] as ProductTemplateSelector;
        if (selector != null && vm != null)
        {
            selector.ViewMode = vm.ViewModesListSelectedIndex;

            if (vm.ViewModesListSelectedIndex == 0)
            {
                vm.PageSize = 40;
                span_columns = 1;
                SelectButtonUnique.IsVisible = false;
                GridTitleSearch.IsVisible = true;
            }
            else if (vm.ViewModesListSelectedIndex == 1)
            {
                vm.PageSize = 40;
                span_columns = 4;
                SelectButtonUnique.IsVisible = false;
                GridTitleSearch.IsVisible = false;
            }
            else if (vm.ViewModesListSelectedIndex == 2)
            {
                vm.PageSize = 1;
                span_columns = 1;
                SelectButtonUnique.IsVisible = true;
                GridTitleSearch.IsVisible = false;
            }

            (MyCollectionView.ItemsLayout as GridItemsLayout).Span = span_columns;

            Debug.WriteLine(vm.PageSize);
            Debug.WriteLine(span_columns);
            //await Task.Delay(500);
            await vm.LoadData();
        }
    }
}