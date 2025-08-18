using CommunityToolkit.Maui.Extensions;
using CommunityToolkit.Maui.Views;
using DMOrders.Controls;
using DMOrders.Models.Filters;
using DMSA.Models.Odoo.Native;
using Microsoft.Maui.Controls;
using ReactiveUI;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows.Input;

namespace DMOrders.Pages.Fragments.Orders.modals;

public partial class CatalogViewer : ContentView
{
    private Popup<product_product> _parentPopup;
    public static ICommand CommandSelectListItem { get; set; }    
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
    product_brand selected_brand { get; set; }
    public ObservableCollection<product_brand> Brands { get; set; } = new();

    FStatus[] newProducts { get; set; }
    FStatus[] stockProducts { get; set; }
    FStatus[] sortProducts { get; set; }

    FStatus selected_newProducts { get; set; }
    FStatus selected_stockProducts { get; set; }
    FStatus selected_sortProducts { get; set; }

    public CatalogViewer(Popup<product_product> parentPopup)
    {
        InitializeComponent();
        //TODO: Corregir ----
        _parentPopup = parentPopup;
        Setup();
    }

    public CatalogViewer()
    {
        InitializeComponent();
        Setup();
    }

    public void Setup()
	{
		InitializeComponent();
        
        CommandSelectListItem = new Command(SelectListItem);
        //BindingContext = new CatalogViewerModel();

        Brands =
            [
                new product_brand { id = 0, name = "No seleccionada" },
                new product_brand { id = -1, name = "🔍 Buscar..." },
                new product_brand { id = 1, name = "Marca 1" },
                new product_brand { id = 2, name = "Marca 2" },
                new product_brand { id = 3, name = "Marca 3" },
                new product_brand { id = 4, name = "Marca 4" },
                new product_brand { id = 5, name = "Marca 5" },
                new product_brand { id = 6, name = "Marca 6" },
                new product_brand { id = 7, name = "Marca 7" },
                new product_brand { id = 8, name = "Marca 8" },
            ];

        ddfBrands.ItemsSource = Brands;
        ddfBrands.ItemDisplayBinding = new Binding("name");
        ddfBrands.SelectedItem = Brands[0];
        ddfBrands.SelectedItemChanged += DdfBrands_SelectedItemChanged;
        selected_brand = Brands[0];

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
        //ddfNews.SelectedItemChanged += DdfBrands_SelectedItemChanged;
        selected_newProducts = newProducts[0];

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
        //ddfNews.SelectedItemChanged += DdfBrands_SelectedItemChanged;
        selected_stockProducts = stockProducts[0];

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
        //ddfNews.SelectedItemChanged += DdfBrands_SelectedItemChanged;
        selected_sortProducts = sortProducts[0];
    }

    private async void DdfBrands_SelectedItemChanged(object? sender, object e)
    {
        product_brand new_selected_brand = (product_brand)e;

        if (new_selected_brand != null && (new_selected_brand.id == -1 || new_selected_brand.id == 0))
        {
            if (new_selected_brand.id == -1)
            {
                Debug.WriteLine("Buscar");
                ddfBrands.SelectedItem = selected_brand;
                var selectedBrand = await PopupBrand(sender, null);

                if (selectedBrand != null)
                {
                    ddfBrands.IsEnabled = false;
                    ddfBrands.ItemsSource = null;
                    Brands[0] = selectedBrand;
                    ddfBrands.ItemsSource = Brands;
                    ddfBrands.SelectedItem = selectedBrand;
                    selected_brand = selectedBrand;
                    ddfBrands.IsEnabled = true;
                    return;
                }
            }
        }

        if (new_selected_brand == null)
        {
            ddfBrands.IsEnabled = false;
            Debug.WriteLine("Clear");
            ddfBrands.ItemsSource = null;
            var nsBrand = new product_brand { id = 0, name = "No seleccionada" };
            Brands[0] = nsBrand;
            ddfBrands.ItemsSource = Brands;
            ddfBrands.SelectedItem = nsBrand;
            selected_brand = nsBrand;
            ddfBrands.IsEnabled = true;
        }
        else
        {
            Debug.WriteLine("Seleccion valida directa...");
            selected_brand = new_selected_brand;
        }
    }

    async Task<product_brand> PopupBrand(object sender, EventArgs e)
    {
        product_brand selected_product_brand = null;

        var popupSizeConstants = new PopupSizeConstants(DeviceDisplay.Current);
        popupSizeConstants.CalculateSizes(DeviceDisplay.Current);
        
        var returnResultPopup = new PopupSelectBrand(popupSizeConstants);

        returnResultPopup.Company = App.Session.res_Company;
                
        returnResultPopup.CanBeDismissedByTappingOutsideOfPopup = false;

        var result = await PopupExtensions.ShowPopupAsync(App.Current.MainPage, returnResultPopup);
        
        if (result != null)
        {
            selected_product_brand = (product_brand)result;            
        }

        return selected_product_brand;
    }

    private async void SelectListItem(object objItem)
    {
        //if (objItem != null)
        //{
        //    //((CatalogViewerModel)this.BindingContext).SelectedItem = (product_product)objItem;
        //    _parentPopup.CloseAsync(objItem);
        //}
        //else
        //{
        //    Debug.WriteLine("Error de objeto");
        //}

        if (objItem is product_product item) // aquí validas y conviertes
        {
            await _parentPopup.CloseAsync(item);
        }
        else
        {
            Debug.WriteLine("Error: el objeto no es del tipo esperado.");
        }
    }

    private async void SelectSingleItem(object sender, EventArgs e)
    {
        var objItem = ((CatalogViewerModel)this.BindingContext).SelectedItem;
        if (objItem is product_product item)
        {
            await _parentPopup.CloseAsync(objItem);
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

    private async void OnCloseClicked(object sender, EventArgs e)
    {
        Debug.WriteLine(((CatalogViewerModel)this.BindingContext).SelectedItem);
        
        await _parentPopup.CloseAsync(default(product_product));
    }

    private async void SelectionView_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        Debug.WriteLine($"SelectionView Property Changed: {e.PropertyName}");
        if(e.PropertyName.Equals("SelectedIndex"))
        {
            await SetViewMode(-1);
        }
    }

    private async Task SetViewMode(int ViewMode)
    {
        var vm = BindingContext as CatalogViewerModel;
        var selector = Resources["ProductTemplateSelector"] as ProductTemplateSelector;
        if (selector != null && vm != null)
        {
            selector.ViewMode = vm.ViewModesListSelectedIndex;

            if(vm.ViewModesListSelectedIndex == 0)
            {
                vm.PageSize = 40;
                span_columns = 4;
                SelectButtonUnique.IsVisible = false;
            }
            else if(vm.ViewModesListSelectedIndex == 1)
            {
                vm.PageSize = 40;
                span_columns = 4;
                SelectButtonUnique.IsVisible = false;
            }
            else if(vm.ViewModesListSelectedIndex == 2)
            {
                vm.PageSize = 1;
                span_columns = 1;
                SelectButtonUnique.IsVisible = true;
            }
            
            (MyCollectionView.ItemsLayout as GridItemsLayout).Span = span_columns;

            Debug.WriteLine(vm.PageSize);
            Debug.WriteLine(span_columns);
            //await Task.Delay(500);
            await vm.LoadData();
        }
    }
}