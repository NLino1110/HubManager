using CommunityToolkit.Maui.Extensions;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;
using DMOrders.Controls;
using DMOrders.Models;
using DMOrders.Models.Filters;
using DMSA.Models.Odoo.Native;
using DMSA.Models.Odoo.Sales;
using DMSA.Sync.Core.Database.Sqlite;
using MPowerKit.VirtualizeListView;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows.Input;

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

    public ICommand ItemPickedInternalCommand { get; set; }

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

    double swipeThreshold = 50;
    double panX = 0;
        
    public ObservableCollection<product_marca> Brands { get; set; } = new();

    FStatus[] newProducts { get; set; }
    FStatus[] stockProducts { get; set; }
    FStatus[] sortProducts { get; set; }

    string filter_code { get; set; }
    string filter_name { get; set; }    
    product_marca filter_brand_select { get; set; }
    int filter_brand { get; set; }
    int filter_new { get; set; }
    int filter_stock { get; set; }
    int filter_sort { get; set; }


    public ICommand ItemTappedCommand { get; set; }
    //public product_pricelist CurrentPriceList { get; set; }
    private ProductProductDb _db { get; set; }
    public ICommand CommandSelectListItem { get; set; }
    public class ViewModesList
    {
        public int id { get; set; }
        public string name { get; set; }
    }

    public ObservableCollection<ViewModesList> viewModesList { get; private set; } = new();

    private int _viewModesListSelectedIndex;
    public int ViewModesListSelectedIndex
    {
        get => _viewModesListSelectedIndex;
        set
        {
            if (_viewModesListSelectedIndex != value)
            {
                _viewModesListSelectedIndex = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(CurrentViewMode));
            }
        }
    }

    public res_company CurrentCompany { get; set; }

    public int CurrentViewMode => viewModesList.ElementAtOrDefault(ViewModesListSelectedIndex)?.id ?? 1;

    private ObservableCollection<product_product> _itemsData;
    private product_product _selectedItem;
    private bool _isRefreshing;
    private bool _headerBordersVisible = true;
    private bool _paginationEnabled = false;

    private int _totalItems;
    private int _pageSize = 1;
    private int _page = 1;

    public bool CanGoNext => (_page * PageSize) < TotalItems;
    public bool CanGoPrevious => _page > 1;

    public int TotalPages => (int)Math.Ceiling((double)TotalItems / PageSize);

    public int TotalItems
    {
        get => _totalItems;
        set
        {
            _totalItems = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(TotalPages));
            OnPropertyChanged(nameof(CanGoNext));
            OnPropertyChanged(nameof(CanGoPrevious));
        }
    }

    public product_product SelectedItem
    {
        get => _selectedItem;
        set
        {
            if (_selectedItem != value)
            {
                _selectedItem = value;
                OnPropertyChanged();
            }
        }
    }

    private bool _isLoading;
    public bool IsLoading
    {
        get => _isLoading;
        set
        {
            _isLoading = value;
            OnPropertyChanged();
        }
    }

    private string _lastFilterSignature;

    private string BuildFilterSignature() =>
        $"{filter_code}|{filter_name}|{filter_brand}|{filter_new}|{filter_stock}|{filter_sort}";

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


    public ObservableCollection<product_product> ItemsData
    {
        get => _itemsData;
        set
        {
            _itemsData = value;
            OnPropertyChanged();
        }
    }

    public bool HeaderBordersVisible
    {
        get => _headerBordersVisible;
        set
        {
            _headerBordersVisible = value;
            OnPropertyChanged();
        }
    }

    public bool PaginationEnabled
    {
        get => _paginationEnabled;
        set
        {
            _paginationEnabled = value;
            OnPropertyChanged();
        }
    }

    public int PageSize
    {
        get => _pageSize;
        set
        {
            _pageSize = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(CanGoNext));
            OnPropertyChanged(nameof(CanGoPrevious));
        }
    }

    public int Page
    {
        get => _page;
        set
        {
            _page = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(CanGoNext));
            OnPropertyChanged(nameof(CanGoPrevious));
        }
    }

    public bool IsRefreshing
    {
        get => _isRefreshing;
        set
        {
            _isRefreshing = value;
            OnPropertyChanged();
        }
    }

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
                new product_marca { id = 0, name = "No seleccionada", active = true, clave_externa = "000" },
                new product_marca { id = -1, name = "🔍 Buscar..." , active = true, clave_externa = "000"}
            ];

            foreach (var marca in itemsTopMarcas)
                Brands.Add(marca);

            ddfBrands.ItemsSource = Brands;
            ddfBrands.ItemDisplayBinding = new Binding("name");
            ddfBrands.SelectedItem = Brands[0];
            ddfBrands.SelectedItemChanged += DdfBrands_SelectedItemChanged;
            filter_brand_select = Brands[0];
        });
    }

    public void Setup()
    {
        CurrentCompany = App.Session.res_Company;

        _db = new ProductProductDb(App.Session.odooConnection.DbNameSqlite);
        InitViewModes();
        RefreshCommand = new Command(async () => await CmdRefresh());
        ItemTappedCommand = new Command<product_product>(OnItemTapped);
        ItemPickedInternalCommand = new Command<product_product>(AddProductButtonInternal);

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


        BindingContext = this;
    }

    private async void DdfBrands_SelectedItemChanged(object? sender, object e)
    {
        product_marca new_selected_brand = (product_marca) e;
        
        if (new_selected_brand != null && (new_selected_brand.id == -1 || new_selected_brand.id == 0))
        {
            if (new_selected_brand.id == -1)
            {
                Debug.WriteLine("Buscar");
                ddfBrands.SelectedItem = filter_brand_select;
                var selectedBrand = await PopupBrand(sender, null);

                if (selectedBrand != null)
                {
                    ddfBrands.IsEnabled = false;
                    ddfBrands.ItemsSource = null;
                    Brands[0] = selectedBrand;
                    ddfBrands.ItemsSource = Brands;
                    ddfBrands.SelectedItem = selectedBrand;
                    filter_brand_select = selectedBrand;
                    ddfBrands.IsEnabled = true;
                }
                else
                {
                    ddfBrands.SelectedItem = filter_brand_select;                    
                }

                SetFilterBrand(filter_brand_select.id);                
                await LoadData();
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
            filter_brand_select = nsBrand;
            ddfBrands.IsEnabled = true;
            SetFilterBrand(filter_brand_select.id);            
        }
        else
        {
            Debug.WriteLine("Seleccion valida directa...");
            filter_brand_select = new_selected_brand;
            SetFilterBrand(filter_brand_select.id);
            await LoadData();
        }
    }

    private async void DdfNews_SelectedItemChanged(object? sender, object e)
    {
        try
        {
            int filter_new_selected = 0;
            if (e != null)
            {
                FStatus new_selected_item = (FStatus)e;
                filter_new_selected = new_selected_item.id;
            }
            else
            {
                filter_new_selected = 0;
            }            
                
            SetFilterNew(filter_new_selected);
            await LoadData();
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
            int filter_stock_selected = 0;
            if (e != null)
            {
                FStatus new_selected_item = (FStatus)e;
                filter_stock_selected = new_selected_item.id;
            }
            else
            {
                filter_stock_selected = 0;
            }

            SetFilterStock(filter_stock_selected);
            await LoadData();
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
            int filter_sort_selected = 0;
            if (e != null)
            {
                FStatus new_selected_item = (FStatus)e;
                filter_sort_selected = new_selected_item.id;
            }
            else
            {
                filter_sort_selected = 0;
            }
                       
            SetFilterSort(filter_sort_selected);
            await LoadData();
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

    private void SelectSingleItem(object sender, EventArgs e)
    {
        var objItem = SelectedItem;
        
        if (objItem != null)
        {            
            this.IsVisible = false;

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
        var objItem = SelectedItem;
        decimal qty_real = product_uom_qty_real;
        decimal qty_sol = product_uom_qty;

        if (objItem != null)
        {
            ItemPickedArgs itemPickedArgs = new ItemPickedArgs
            {
                product = objItem,
                qty_real = qty_real,
                qty_sol = qty_sol,
            };


            if (ItemPickedByQtyCommand?.CanExecute(itemPickedArgs) != null)
                ItemPickedByQtyCommand.Execute(itemPickedArgs);

            product_uom_qty_real = 0;
            
            SelectedItem = null;
            ClearSelection(SelectedItem);
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

                    if (PageSize == 1)
                    {
                        if (panX < 0 && CanGoNext)
                            NextPageCommand.Execute(null);
                        else if (panX > 0 && CanGoPrevious)
                            PreviousPageCommand.Execute(null);
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
        if (e.PropertyName.Equals("SelectedIndex"))
        {
            await SetViewMode(-1);
        }
    }

    private async void TextCode_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
    {        
        if (e.PropertyName.Equals("Text"))
        {
            filter_code = TextCode.Text;            
            SetFilterCode(filter_code);
        }
    }

    private async void TextDescription_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
    {        
        if (e.PropertyName.Equals("Text"))
        {
            filter_name = TextDescription.Text;
            
            SetFilterName(filter_name);
        }
    }

    private async Task SetViewMode(int ViewMode)
    {        
        var selector = Resources["ProductTemplateSelector"] as ProductTemplateSelector;

        if (selector == null)
            return;

        selector.ViewMode = ViewModesListSelectedIndex;

        switch (ViewModesListSelectedIndex)
        {
            case 0:
                PageSize = 40;
                SpanColumns = 1;
                SelectButtonUnique.IsVisible = false;
                GridTitleSearch.IsVisible = true;
                break;

            case 1:
                PageSize = 40;
                SpanColumns = 4;
                SelectButtonUnique.IsVisible = false;
                GridTitleSearch.IsVisible = false;
                break;

            case 2:
                PageSize = 1;
                SpanColumns = 1;
                SelectButtonUnique.IsVisible = true;
                GridTitleSearch.IsVisible = false;
                break;
        }
                
        var newLayout = new MPowerKit.VirtualizeListView.GridLayout
        {
            Span = SpanColumns,
            HorizontalItemSpacing = 1,
            VerticalItemSpacing = 1
        };
                
        listViewProduct.ItemsLayout = newLayout;
        listViewProduct.ItemTemplate = (DataTemplate)Resources["ProductTemplateSelector"];

        await Task.Delay(30);
        
        await PublicRefresh();
    }

    private async Task SetViewModeWin(int ViewMode)
    {        
        var selector = Resources["ProductTemplateSelector"] as ProductTemplateSelector;
        if (selector != null)
        {
            selector.ViewMode = ViewModesListSelectedIndex;

            if (ViewModesListSelectedIndex == 0)
            {
                PageSize = 40;
                SpanColumns = 1;
                SelectButtonUnique.IsVisible = false;
                GridTitleSearch.IsVisible = true;
            }
            else if (ViewModesListSelectedIndex == 1)
            {
                PageSize = 40;
                SpanColumns = 4;
                SelectButtonUnique.IsVisible = false;
                GridTitleSearch.IsVisible = false;
            }
            else if (ViewModesListSelectedIndex == 2)
            {
                PageSize = 1;
                SpanColumns = 1;
                SelectButtonUnique.IsVisible = true;
                GridTitleSearch.IsVisible = false;
            }

            (listViewProduct.ItemsLayout as GridLayout).Span = SpanColumns;
            //(MyCollectionView.ItemsLayout as GridItemsLayout).Span = span_columns;

            Debug.WriteLine(PageSize);
            Debug.WriteLine(SpanColumns);
            await LoadData();
        }
    }

    private void btnClear_Clicked(object sender, EventArgs e)
    {
        TextCode.ClearValue();
        TextDescription.ClearValue();
        
        ddfNews.SelectedItem = newProducts[0];
        ddfStock.SelectedItem = stockProducts[0];
        ddfSort.SelectedItem = sortProducts[0];
                
        var btn = (View) sender;
        var origin = btn.GetBoundingBoxIn(Confetti).Center;
        Confetti.TriggerAt(origin);
        //Confetti.TriggerCenter();
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

                // si quedó vacío, coloca "0"
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
        var ProductEditing = SelectedItem;

        if (ProductEditing != null)
        {
            product_uom_qty_real = 0; // CurrentSaleOrderLine.product_uom_qty_real;
            product_uom_qty = 0; // CurrentSaleOrderLine.product_uom_qty;
            SelectedItem = null;
            ClearSelection(SelectedItem);
        }
    }

    private async void ApplyValueChanges(object sender, EventArgs e)
    {
        AddSingleItem(sender, e);
    }


    private void InitViewModes()
    {
        viewModesList.Add(new ViewModesList { id = 1, name = "▤" });
        viewModesList.Add(new ViewModesList { id = 2, name = "▦" });
        viewModesList.Add(new ViewModesList { id = 3, name = "⧉" });

        CommandSelectListItem = new Command(AddSelectedItem);
    }

    private async void AddSelectedItem(object objItem)
    {
        if (objItem != null)
        {
            
        }
        else
        {
            Debug.WriteLine("Error de objeto");
        }
    }

    [RelayCommand]
    private void RelayRowTapped()
    {
        Debug.WriteLine("RelayRowTapped called");
    }

    public ICommand RefreshCommand { get; set; }

    private async Task CmdRefresh()
    {
        IsRefreshing = true;
        await LoadData();
        IsRefreshing = false;
    }

    public async Task PublicRefresh()
    {
        IsRefreshing = true;
        await LoadData();
        IsRefreshing = false;
    }

    private readonly SemaphoreSlim _loadLock = new(1, 1);
    private CancellationTokenSource _cts;

    public async Task LoadData()
    {
        if (IsLoading) return;

        var signature = BuildFilterSignature();
        var filtersChanged = signature != _lastFilterSignature;

        if (filtersChanged)
        {
            Page = 1;
            _lastFilterSignature = signature;
        }

        _cts?.Cancel();
        _cts = new CancellationTokenSource();
        var ct = _cts.Token;

        await _loadLock.WaitAsync(ct);
        var stopwatch = Stopwatch.StartNew();

        try
        {
            IsLoading = true;
            int pricelist_id = -1;
            if (CurrentPriceList != null)
                pricelist_id = CurrentPriceList.id;

            var (items, total) = await _db.GetPagedAsync(
                filter_code, filter_name, filter_brand, filter_new, filter_stock, filter_sort,
                0, 0, pricelist_id,
                Page, PageSize, ct);

            TotalItems = total;

            if (ItemsData == null)
                ItemsData = new ObservableCollection<product_product>();
            else
                ItemsData.Clear();

            foreach (var it in items)
                ItemsData.Add(it);

            if (ViewModesListSelectedIndex == 2 && ItemsData.Count > 0)
            {
                SelectedItem = ItemsData[0];               
                OnPropertyChanged(nameof(SelectedItem));
            }
        }
        catch (OperationCanceledException oex)
        {            
            Debug.WriteLine("OperationCanceledException");
            Debug.WriteLine(oex);
        }
        catch (Exception ex)
        {
            ItemsData = new ObservableCollection<product_product>();
            Debug.WriteLine(ex);
        }
        finally
        {
            IsLoading = false;
            _loadLock.Release();
            stopwatch.Stop();
            Debug.WriteLine($"[CatalogViewerModel] Carga en {stopwatch.ElapsedMilliseconds} ms | TotalItems: {TotalItems}, Page: {Page}, PageSize: {PageSize}, Filtro: {filter_code ?? filter_name ?? "sin filtro"}");
        }
    }

    public ICommand NextPageCommand => new Command(async () =>
    {
        if (CanGoNext)
        {
            Page++;
            await LoadData();
        }
    });

    public ICommand PreviousPageCommand => new Command(async () =>
    {
        if (CanGoPrevious)
        {
            Page--;
            await LoadData();
        }
    });

    public void SetFilterCode(string _filter_code)
    {
        filter_code = _filter_code;
    }

    public void SetFilterName(string _filter_name)
    {
        filter_name = _filter_name;
    }

    public void SetFilterBrand(int _filter_brand)
    {
        filter_brand = _filter_brand;
    }
    public void SetFilterNew(int _filter_new)
    {
        filter_new = _filter_new;
    }

    public void SetFilterStock(int _filter_stock)
    {
        filter_stock = _filter_stock;
    }

    public void SetFilterSort(int _filter_sort)
    {
        filter_sort = _filter_sort;
    }

    private product_product _lastSelectedItem;

    public void OnItemTapped(product_product tappedItem)
    {
        if (_lastSelectedItem != null && _lastSelectedItem != tappedItem)
            _lastSelectedItem.IsSelected = false;

        tappedItem.IsSelected = true;
        _lastSelectedItem = tappedItem;

        SelectedItem = tappedItem;

        WeakReferenceMessenger.Default.Send(new ItemSelectedMessage(tappedItem));
    }

    public void AddProductButtonInternal(product_product itemAdd)
    {
        if (itemAdd != null)
        {
            ItemPickedArgs itemPickedArgs = new ItemPickedArgs
            {
                product = itemAdd,
                qty_real = 1,
                qty_sol = 1,
            };

            if (ItemPickedCommand?.CanExecute(itemPickedArgs) != null)
                ItemPickedCommand.Execute(itemPickedArgs);
        }
        else
        {
            Debug.WriteLine("Error de objeto");
        }
    }

    public void ClearSelection(product_product tappedItem)
    {
        if (_lastSelectedItem != null)
        {
            _lastSelectedItem.IsSelected = false;
            _lastSelectedItem = null;
        }

        SelectedItem = null;
    }

    public class ItemSelectedMessage : ValueChangedMessage<product_product>
    {
        public ItemSelectedMessage(product_product value) : base(value) { }
    }
}