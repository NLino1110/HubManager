using CommunityToolkit.Maui.Extensions;
using CommunityToolkit.Maui.Views;
using CommunityToolkit.Mvvm.Messaging;
using DMOrders.Controls;
using DMOrders.Models;
using DMOrders.Models.Filters;
using DMOrders.Services.Database.Sqlite;
using DMSA.Models.Odoo.Native;
using DMSA.Models.Odoo.Sales;
using DMSA.Sync.Core.Database.Sqlite;
using MPowerKit.VirtualizeListView;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows.Input;
using static DMOrders.Pages.Fragments.Orders.modals.CatalogViewerModel;

namespace DMOrders.Pages.Fragments.Orders.modals;

public static class ViewExtensions
{
    public static Rect GetBoundingBoxIn(this VisualElement view, VisualElement container)
    {
        var p = view.GetAbsoluteBounds();
        var c = container.GetAbsoluteBounds();
        return new Rect(p.X - c.X, p.Y - c.Y, p.Width, p.Height);
    }

    public static Rect GetAbsoluteBounds(this VisualElement view)
    {
        var x = view.X; var y = view.Y;
        Element? parent = view.Parent;
        while (parent is VisualElement ve)
        {
            x += ve.X; y += ve.Y; parent = ve.Parent;
        }
        return new Rect(x, y, view.Width, view.Height);
    }
}
public partial class CatalogViewerInner : ContentView
{
    public product_pricelist CurrentPriceList
    {
        get => (product_pricelist)GetValue(CurrentPriceListProperty);
        set => SetValue(CurrentPriceListProperty, value);
    }

    public static readonly BindableProperty CurrentPriceListProperty =
        BindableProperty.Create(
            propertyName: nameof(CurrentPriceList),
            returnType: typeof(product_pricelist),
            declaringType: typeof(CatalogViewerInner),
            defaultValue: null,
            propertyChanged: OnCurrentPriceListChanged);

    private static void OnCurrentPriceListChanged(BindableObject bindable, object oldValue, object newValue)
    {
        var control = (CatalogViewerInner)bindable;

        if (control.BindingContext == null || newValue == null)
            return;

        var vmType = control.BindingContext.GetType();
        var propertyInfo = vmType.GetProperty(nameof(CurrentPriceList));

        if (propertyInfo != null && propertyInfo.CanWrite)
        {
            propertyInfo.SetValue(control.BindingContext, newValue);
        }
    }

    public static readonly BindableProperty ItemPickedCommandProperty =
        BindableProperty.Create(nameof(ItemPickedCommand), typeof(ICommand), typeof(CatalogViewerInner), default(ICommand));

    public ICommand ItemPickedCommand
    {
        get => (ICommand)GetValue(ItemPickedCommandProperty);
        set => SetValue(ItemPickedCommandProperty, value);
    }

    public static readonly BindableProperty ItemPickedByQtyCommandProperty =
        BindableProperty.Create(nameof(ItemPickedByQtyCommand), typeof(ICommand), typeof(CatalogViewerInner), default(ICommand));

    public ICommand ItemPickedByQtyCommand
    {
        get => (ICommand)GetValue(ItemPickedByQtyCommandProperty);
        set => SetValue(ItemPickedByQtyCommandProperty, value);
    }


    public ContentView ViewParent
    {
        get => (ContentView)GetValue(ViewParentProperty);
        set => SetValue(ViewParentProperty, value);
    }

    public static readonly BindableProperty ViewParentProperty =
        BindableProperty.Create(nameof(ViewParent), typeof(ContentView), typeof(DataGrid));

    private int SpanColumns = 4;
    //public static readonly BindableProperty SpanColumnsProperty =
    //    BindableProperty.Create(nameof(SpanColumns), typeof(ContentView), typeof(DataGrid));

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
        //int[] topMarcas = new int[] { 545, 669, 716, 773, 869, 512, 517, 701, 554, 968, 872, 960, 682 };
        ProductProductDb productDb = new ProductProductDb(App.Session.odooConnection.DbNameSqlite);
        int[] topMarcas = await productDb.GetTopMarcas(13);

        ProductMarcaDb marcasDb = new ProductMarcaDb(App.Session.odooConnection.DbNameSqlite);
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
        BindingContext = new CatalogViewerModel();

        LoadTopMarcasAsync();

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

        //_activeEntry = EntryCantidadSolicitada;
        HighlightActiveEntry(_activeEntry);

        WeakReferenceMessenger.Default.Register<ItemSelectedMessage>(this, async (r, m) =>
        {
            //await LoadInfo(m.Value);
            Debug.WriteLine("Message Weak");
            Debug.WriteLine(m.Value);

            _activeEntry = EntryCantidadSolicitada;
            HighlightActiveEntry(_activeEntry);
        });
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

            //ItemPickedArgs itemPickedArgs = new ItemPickedArgs
            //{
            //    product = objItem,
            //    qty = 1,
            //};

            if (ItemPickedCommand?.CanExecute(objItem) != null)
                ItemPickedCommand.Execute(objItem);

        }
        else
        {
            Debug.WriteLine("Error de objeto");
        }
    }

    private void AddSingleItem(object sender, EventArgs e)
    {
        var objItem = ((CatalogViewerModel)this.BindingContext).SelectedItem;
        decimal qty_real = product_uom_qty;
        decimal qty_sol = product_uom_qty_real;

        if (objItem != null)
        {
            ItemPickedArgs itemPickedArgs = new ItemPickedArgs
            {
                product = objItem,
                qty_real = qty_real,
                qty_sol = qty_sol,
            };

            //this.IsVisible = false;

            if (ItemPickedByQtyCommand?.CanExecute(itemPickedArgs) != null)
                ItemPickedByQtyCommand.Execute(itemPickedArgs);

            //product_uom_qty = 0;
            product_uom_qty_real = 0;
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

        if (selector == null || vm == null)
            return;

        selector.ViewMode = vm.ViewModesListSelectedIndex;

        switch (vm.ViewModesListSelectedIndex)
        {
            case 0:
                vm.PageSize = 40;
                SpanColumns = 1;
                SelectButtonUnique.IsVisible = false;
                GridTitleSearch.IsVisible = true;
                break;

            case 1:
                vm.PageSize = 40;
                SpanColumns = 4;
                SelectButtonUnique.IsVisible = false;
                GridTitleSearch.IsVisible = false;
                break;

            case 2:
                vm.PageSize = 1;
                SpanColumns = 1;
                SelectButtonUnique.IsVisible = true;
                GridTitleSearch.IsVisible = false;
                break;
        }

        //// ⚠️ Importante: congelar temporalmente el binding
        //var items = listViewProduct.ItemsSource;

        // Crear un nuevo layout con el nuevo Span
        var newLayout = new MPowerKit.VirtualizeListView.GridLayout
        {
            Span = SpanColumns,
            HorizontalItemSpacing = 1,
            VerticalItemSpacing = 1
        };

        // Reasignar el layout y la plantilla
        listViewProduct.ItemsLayout = newLayout;
        listViewProduct.ItemTemplate = (DataTemplate)Resources["ProductTemplateSelector"];

        //// Reasignar el ItemsSource para forzar el refresco
        //listViewProduct.ItemsSource = null;
        //listViewProduct.ItemsSource = items;

        // Forzar una ligera pausa para que el UI se estabilice
        await Task.Delay(30);

        // Forzar recarga desde el ViewModel
        await vm.PublicRefresh();
    }

    private async Task SetViewModeWin(int ViewMode)
    {
        var vm = BindingContext as CatalogViewerModel;
        var selector = Resources["ProductTemplateSelector"] as ProductTemplateSelector;
        if (selector != null && vm != null)
        {
            selector.ViewMode = vm.ViewModesListSelectedIndex;

            if (vm.ViewModesListSelectedIndex == 0)
            {
                vm.PageSize = 40;
                SpanColumns = 1;
                SelectButtonUnique.IsVisible = false;
                GridTitleSearch.IsVisible = true;
            }
            else if (vm.ViewModesListSelectedIndex == 1)
            {
                vm.PageSize = 40;
                SpanColumns = 4;
                SelectButtonUnique.IsVisible = false;
                GridTitleSearch.IsVisible = false;
            }
            else if (vm.ViewModesListSelectedIndex == 2)
            {
                vm.PageSize = 1;
                SpanColumns = 1;
                SelectButtonUnique.IsVisible = true;
                GridTitleSearch.IsVisible = false;
            }

            (listViewProduct.ItemsLayout as GridLayout).Span = SpanColumns;            
            //(MyCollectionView.ItemsLayout as GridItemsLayout).Span = span_columns;

            Debug.WriteLine(vm.PageSize);
            Debug.WriteLine(SpanColumns);
            //await Task.Delay(500);
            await vm.LoadData();
        }
    }


    private void btnClear_Clicked(object sender, EventArgs e)
    {
        TextCode.ClearValue();
        TextDescription.ClearValue();
        ddfBrands.SelectedItem = Brands[0];
        ddfNews.SelectedItem = newProducts[0];
        ddfStock.SelectedItem = stockProducts[0];
        ddfSort.SelectedItem = sortProducts[0];
                
        var btn = (View) sender;

        // convertir la coordenada del botón al espacio del overlay
        var origin = btn.GetBoundingBoxIn(Confetti).Center;
        Confetti.TriggerAt(origin);

        //Confetti.TriggerCenter();
    }

    private Entry _activeEntry;

    public decimal _product_uom_qty { get; set; }
    public decimal _product_uom_qty_real { get; set; }

    public decimal product_uom_qty
    {
        get => _product_uom_qty;
        set
        {
            if (_product_uom_qty != value)
            {
                _product_uom_qty = value;
                OnPropertyChanged(nameof(product_uom_qty));
            }
        }
    }

    public decimal product_uom_qty_real
    {
        get => _product_uom_qty_real;
        set
        {
            if (_product_uom_qty_real != value)
            {
                _product_uom_qty_real = value;
                OnPropertyChanged(nameof(product_uom_qty_real));

                product_uom_qty = value;
            }
        }
    }

    private void OnEntryTapped(object sender, EventArgs e)
    {
        if (sender is not Entry tappedEntry)
            return;

        // Si haces clic en el mismo, no hagas nada
        if (_activeEntry == tappedEntry)
            return;

        // Cambiar estados visuales
        HighlightActiveEntry(tappedEntry);
        _activeEntry = tappedEntry;
    }

    private void HighlightActiveEntry(Entry active)
    {
        // Resalta el activo y apaga el otro
        EntryCantidadSolicitada.BackgroundColor = active == EntryCantidadSolicitada
            ? Colors.LightBlue
            : Colors.LightGray;

        EntryCantidadFinal.BackgroundColor = active == EntryCantidadFinal
           ? Colors.LightBlue
           : Colors.LightGray;
    }

    private void OnKeyClicked(object sender, EventArgs e)
    {
        if (_activeEntry is null) return;
        if (sender is not Button btn) return;

        var key = btn.Text;
        var text = _activeEntry.Text ?? string.Empty;

        switch (key)
        {
            case "⌫":
                if (text.Length > 0)
                    text = text[..^1]; // elimina el último carácter

                // 👇 si quedó vacío, coloca "0"
                if (string.IsNullOrEmpty(text))
                    text = "0";
                break;

            case ".":
                if (!text.Contains("."))
                {
                    text = text.Length == 0 ? "0." : text + ".";
                }
                break;

            default:
                if (key.Length == 1 && char.IsDigit(key[0]))
                {
                    if (text == "0")
                        text = key; // reemplaza 0 inicial
                    else
                        text += key;
                }
                break;
        }

        _activeEntry.Text = text;
    }

    private void ResetOriginalValues(object sender, EventArgs e)
    {
        var vm = BindingContext as CatalogViewerModel;
        var ProductEditing = vm.SelectedItem;

        if (ProductEditing != null)
        {
            product_uom_qty_real = 0; // CurrentSaleOrderLine.product_uom_qty_real;
            product_uom_qty = 0; // CurrentSaleOrderLine.product_uom_qty;
            vm.SelectedItem = null;
            vm.ClearSelection(vm.SelectedItem);
        }
    }

    private async void ApplyValueChanges(object sender, EventArgs e)
    {
        AddSingleItem(sender, e);


        //////var vm = BindingContext as CatalogViewerModel;
        //////var ProductEditing = vm.SelectedItem;

        //////if (ProductEditing != null)
        //////{
        //////    if ((decimal)ProductEditing.qty_available < product_uom_qty)
        //////    {
        //////        ProductEditing = null;
        //////        ProductEditing = null;
        //////        product_uom_qty_real = 0;
        //////        product_uom_qty = 0;
        //////        //OrderLinesCl.SelectedItem = null;

        //////        //await DisplayAlert("Alerta", "La cantidad solicitada no puede ser mayor a la disponible en inventario.", "Aceptar");
        //////        return;
        //////    }

        //////    //CurrentSaleOrderLine.product_uom_qty_real = product_uom_qty_real;
        //////    //CurrentSaleOrderLine.product_uom_qty = product_uom_qty;

        //////    //////////////////////////////
        //////    //var productDb = new ProductProductDb(App.Session.odooConnection.DbNameSqlite);
        //////    //var product_item = await productDb.GetItemAsync(x => x.id == CurrentSaleOrderLine.product_id);

        //////    //if (product_item == null)
        //////    //{
        //////    //    Debug.WriteLine("Error: no se encontró el producto para actualizar la línea de orden.");
        //////    //    return;
        //////    //}

        //////    //product_item.list_price = (float)CurrentSaleOrderLine.price_unit;
        //////    //((CrudViewModel)BindingContext).UpdateOrderLine(CurrentSaleOrderLine, product_item);
        //////    //////////////////////////////

        //////    //CurrentSaleOrderLine = null;
        //////    ProductEditing = null;
        //////    product_uom_qty_real = 0;
        //////    product_uom_qty = 0;
                        
        //////    //OrderLinesCl.SelectedItem = null;
        //////}
    }
}