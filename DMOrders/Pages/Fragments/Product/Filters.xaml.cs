using CommunityToolkit.Maui.Extensions;
using CommunityToolkit.Maui.Views;
using DMOrders.Controls;
using DMOrders.Models.Filters;
using DMOrders.Services.Database.Sqlite;
using DMSA.Models.Odoo.Native;
using DMSA.Sync.Core.Database.Sqlite;
using Spinner.MAUI;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows.Input;
using UraniumUI.Controls;
using UraniumUI.Material.Controls;

namespace DMOrders.Pages.Fragments.Product;

public partial class Filters : ContentView
{
    public event EventHandler OnSearchButtonClicked;
    
    FDays[] Days { get; set; }

    FStatus[] Status { get; set; }
    public ObservableCollection<product_marca> Brands { get; set; } = new();
    public ObservableCollection<product_categoria> Product_Categories { get; set; } = new();
    product_marca selected_brand { get; set; }
    product_categoria selected_product_category { get; set; }
    public Filters()
	{
		InitializeComponent();

        Days = new FDays[8]
        {
            new FDays { id = 0, Name = "Todos" },
            new FDays { id = 1, Name = "Lunes" },
            new FDays { id = 2, Name = "Martes" },
            new FDays { id = 3, Name = "Miercoles" },
            new FDays { id = 4, Name = "Jueves" },
            new FDays { id = 5, Name = "Viernes" },
            new FDays { id = 6, Name = "Sabado" },
            new FDays { id = 7, Name = "Domingo" },
        };

        //ddfDays.ItemsSource = Days;
        //ddfDays.ItemDisplayBinding = new Binding("Name");
        //ddfDays.SelectedItem = Days[0];
        //ddfDays.SelectedItemChanged += DdfDays_SelectedItemChanged;
        //ddfDays.ItemDisplayBinding = new ;

        Status = new FStatus[3]
        {
            new FStatus { id = 0, Name = "Todos" },
            new FStatus { id = 1, Name = "Activo" },
            new FStatus { id = 2, Name = "Inactivo" },            
        };

        ddfStatus.ItemsSource = Status;
        ddfStatus.ItemDisplayBinding = new Binding("Name");
        ddfStatus.SelectedItem = Status[0];
                
        LoadTopMarcasAsync();
        LoadTopCategoriesAsync();

        ddfBrands.SelectedItemChanged += DdfBrands_SelectedItemChanged;
        ddfCategory.SelectedItemChanged += DdfCategory_SelectedItemChanged;
    }

    private async void DdfBrands_SelectedItemChanged(object? sender, object e)
    {
        product_marca new_selected_brand = (product_marca)e;

        if (new_selected_brand != null && (new_selected_brand.id == -1 || new_selected_brand.id == 0))
        {
            if (new_selected_brand.id == -1)
            {                
                Debug.WriteLine("Buscar");
                ddfBrands.SelectedItem = selected_brand;
                var selectedBrand = await PopupBrand(sender, null);

                if (selectedBrand!=null)
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
            var nsBrand = new product_marca { id = 0, name = "No seleccionada" };
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

    public async Task LoadTopMarcasAsync()
    {       
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
            
            //filter_brand = Brands[0];
        });
    }

    public async Task LoadTopCategoriesAsync()
    {

        int[] topMarcas = new int[] { 20,
                    21,
                    22,
                    23,
                    24,
                    25,
                    26,
                    27,
                    28,                    
                     };

        ProductCategoriaDb categoriesDb = new ProductCategoriaDb(App.Session.odooConnection.DbNameSqlite);
        //var itemsTopCategories = await categoriesDb.GetItemsAsync(topMarcas);
        var itemsTopCategories = await categoriesDb.GetItemsAsync("");        
        
        MainThread.BeginInvokeOnMainThread(() =>
        {
            Product_Categories =
            [
                new product_categoria { id = 0, name = "No seleccionada" },
                new product_categoria { id = -1, name = "🔍 Buscar..." }
            ];

            foreach (var categoria in itemsTopCategories)
                Product_Categories.Add(categoria);

            ddfCategory.ItemsSource = Product_Categories;
            ddfCategory.ItemDisplayBinding = new Binding("name");
            ddfCategory.SelectedItem = Product_Categories[0];
            
            //filter_brand = Brands[0];
        });
    }

    private async void DdfCategory_SelectedItemChanged(object? sender, object e)
    {
        product_categoria new_selected_product_category = (product_categoria) e;

        if (new_selected_product_category != null && (new_selected_product_category.id == -1 || new_selected_product_category.id == 0))
        {
            if (new_selected_product_category.id == -1)
            {
                Debug.WriteLine("Buscar");
                ddfCategory.SelectedItem = selected_product_category;
                var selectedProductCategory = await PopupProductCategory(sender, null);

                if (selectedProductCategory != null)
                {
                    ddfCategory.IsEnabled = false;
                    ddfCategory.ItemsSource = null;
                    Product_Categories[0] = selectedProductCategory;
                    ddfCategory.ItemsSource = Product_Categories;
                    ddfCategory.SelectedItem = selectedProductCategory;
                    selected_product_category = selectedProductCategory;
                    ddfCategory.IsEnabled = true;
                    return;
                }
            }
        }

        if (new_selected_product_category == null)
        {
            ddfCategory.IsEnabled = false;
            Debug.WriteLine("Clear");
            ddfCategory.ItemsSource = null;
            var nsBrand = new product_categoria { id = 0, name = "No seleccionada" };
            Product_Categories[0] = nsBrand;
            ddfCategory.ItemsSource = Product_Categories;
            ddfCategory.SelectedItem = nsBrand;
            selected_product_category = nsBrand;
            ddfCategory.IsEnabled = true;
        }
        else
        {
            Debug.WriteLine("Seleccion valida directa...");
            selected_product_category = new_selected_product_category;
        }
    }

    async Task<product_marca> PopupBrand(object sender, EventArgs e)
    {
        product_marca selected_product_brand = null;

        var popupSizeConstants = new PopupSizeConstants(DeviceDisplay.Current);
        popupSizeConstants.CalculateSizes(DeviceDisplay.Current);
        //Size = popupSizeConstants.Medium;

        var returnResultPopup = new PopupSelectMarca(popupSizeConstants);

        returnResultPopup.Company = App.Session.res_Company;

        //Evita que se cierre cuando se haga clic (tap) fuera de la ventana
        returnResultPopup.CanBeDismissedByTappingOutsideOfPopup = false;

        //if (!isWindows)
            //returnResultPopup.Size = popupSizeConstants.Large;

        var result = await PopupExtensions.ShowPopupAsync<product_marca>(App.Current.MainPage, returnResultPopup);
        //var result = await this.ShowPopupAsync(returnResultPopup);

        if (result.Result != null)
        {
            selected_product_brand = result.Result;
            //_inputResPartner.Text = Sel_Res_Partner.id.ToString() + " - " + Sel_Res_Partner.name;
            //_res_partnerItem = resPartner;
        }

        return selected_product_brand;
    }

    async Task<product_categoria> PopupProductCategory(object sender, EventArgs e)
    {
        product_categoria selected_product_category = null;

        var popupSizeConstants = new PopupSizeConstants(DeviceDisplay.Current);
        popupSizeConstants.CalculateSizes(DeviceDisplay.Current);
        
        var returnResultPopup = new PopupSelectProductCategory(popupSizeConstants);

        returnResultPopup.Company = App.Session.res_Company;        
        returnResultPopup.CanBeDismissedByTappingOutsideOfPopup = false;

        //if (!isWindows)
        //returnResultPopup.Size = popupSizeConstants.Large;

        var result = await PopupExtensions.ShowPopupAsync<product_categoria>(App.Current.MainPage, returnResultPopup);        

        if (result.Result != null)
        {
            selected_product_category = result.Result;            
        }

        return selected_product_category;
    }

    private void DdfDays_SelectedItemChanged(object? sender, object e)
    {
        //throw new NotImplementedException();
        Debug.WriteLine(e);
    }

    private void Button_Clicked(object sender, EventArgs e)
    {
        OnSearchButtonClicked?.Invoke(this, EventArgs.Empty);
    }

    private void btnClear_Clicked(object sender, EventArgs e)
    {
        TextCode.Text = "";
        TextDescription.Text = "";
        //entryName.Text = "";

        ddfStatus.SelectedItem = Status[0];

        ClearFilters();
    }

    private void ClearFilters()
    {
        var brandsCopy = new ObservableCollection<product_marca>();

        foreach (var b in Brands)
        {
            brandsCopy.Add(new product_marca
            {
                id = b.id,
                name = b.name,                
            });
        }

        brandsCopy[0] = new product_marca { id = 0, name = "No seleccionada" };
        Brands = brandsCopy;
        ddfBrands.SelectedItem = Brands[0];

        var categoriesCopy = new ObservableCollection<product_categoria>();

        foreach (var p in Product_Categories)
        {
            categoriesCopy.Add(new product_categoria
            {
                id = p.id,
                name = p.name,
            });
        }

        categoriesCopy[0] = new product_categoria { id = 0, name = "No seleccionada" };
        Product_Categories = categoriesCopy;
        ddfCategory.SelectedItem = Product_Categories[0];        
    }

    internal int getStatus()
    {
        return ((FStatus) ddfStatus.SelectedItem).id;
    }

    internal string getCode()
    {
        return TextCode.Text;
    }

    internal string getName()
    {
        return TextDescription.Text;
    }

    internal int getBrand()
    {
        if (ddfBrands.SelectedItem != null)
            selected_brand = (product_marca) ddfBrands.SelectedItem;

        if (selected_brand == null)
            return 0;

        return selected_brand.id;
    }

    internal int getCategory()
    {
        if (ddfCategory.SelectedItem != null)
            selected_product_category = (product_categoria)ddfCategory.SelectedItem;

        if (selected_product_category == null)
            return 0;

        return selected_product_category.id;
    }
}