using CommunityToolkit.Maui.Extensions;
using CommunityToolkit.Maui.Views;
using DMOrders.Controls;
using DMOrders.Models.Filters;
using DMSA.Models.Odoo.Native;
using DynamicData;
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
    public ObservableCollection<product_brand> Brands { get; set; } = new();
    public ObservableCollection<product_category> Product_Categories { get; set; } = new();
    product_brand selected_brand { get; set; }
    product_category selected_product_category { get; set; }
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
        //ddfDays.SelectedItemChanged += DdfDays_SelectedItemChanged;

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


        Product_Categories =
        [
            new product_category { id = 0, name = "No seleccionada" },
            new product_category { id = -1, name = "🔍 Buscar..." },
            new product_category { id = 1, name = "Categoria 1" },
            new product_category { id = 2, name = "Categoria 2" },
            new product_category { id = 3, name = "Categoria 3" },
            new product_category { id = 4, name = "Categoria 4" },
            new product_category { id = 5, name = "Categoria 5" },
            new product_category { id = 6, name = "Categoria 6" },
            new product_category { id = 7, name = "Categoria 7" },
            new product_category { id = 8, name = "Categoria 8" },
        ];

        ddfCategory.ItemsSource = Product_Categories;
        ddfCategory.ItemDisplayBinding = new Binding("name");
        ddfCategory.SelectedItem = Product_Categories[0];
        ddfCategory.SelectedItemChanged += DdfCategory_SelectedItemChanged;
        selected_product_category = Product_Categories[0];
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

    private async void DdfCategory_SelectedItemChanged(object? sender, object e)
    {
        product_category new_selected_product_category = (product_category)e;

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
            var nsBrand = new product_category { id = 0, name = "No seleccionada" };
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

    async Task<product_brand> PopupBrand(object sender, EventArgs e)
    {
        product_brand selected_product_brand = null;

        var popupSizeConstants = new PopupSizeConstants(DeviceDisplay.Current);
        popupSizeConstants.CalculateSizes(DeviceDisplay.Current);
        //Size = popupSizeConstants.Medium;

        var returnResultPopup = new PopupSelectBrand(popupSizeConstants);

        returnResultPopup.Company = App.Session.res_Company;

        //Evita que se cierre cuando se haga clic (tap) fuera de la ventana
        returnResultPopup.CanBeDismissedByTappingOutsideOfPopup = false;

        //if (!isWindows)
            //returnResultPopup.Size = popupSizeConstants.Large;

        var result = await PopupExtensions.ShowPopupAsync(App.Current.MainPage, returnResultPopup);
        //var result = await this.ShowPopupAsync(returnResultPopup);

        if (result != null)
        {
            selected_product_brand = (product_brand) result;
            //_inputResPartner.Text = Sel_Res_Partner.id.ToString() + " - " + Sel_Res_Partner.name;
            //_res_partnerItem = resPartner;
        }

        return selected_product_brand;
    }

    async Task<product_category> PopupProductCategory(object sender, EventArgs e)
    {
        product_category selected_product_category = null;

        var popupSizeConstants = new PopupSizeConstants(DeviceDisplay.Current);
        popupSizeConstants.CalculateSizes(DeviceDisplay.Current);
        
        var returnResultPopup = new PopupSelectProductCategory(popupSizeConstants);

        returnResultPopup.Company = App.Session.res_Company;        
        returnResultPopup.CanBeDismissedByTappingOutsideOfPopup = false;

        //if (!isWindows)
        //returnResultPopup.Size = popupSizeConstants.Large;

        var result = await PopupExtensions.ShowPopupAsync(App.Current.MainPage, returnResultPopup);        

        if (result != null)
        {
            selected_product_category = (product_category)result;            
        }

        return selected_product_category;
    }


    //private void RefreshSelectedItem<T>(
    //DropdownField dropdownField,
    //IList<T> listSource,
    //T newSelectedItem,
    //Action<T> assignSelectedModel,
    //Action<IList<T>>? updateListCallback = null)
    //{
    //    dropdownField.IsEnabled = false;
    //    // Detach ItemsSource to force UI refresh
    //    dropdownField.ItemsSource = null;

    //    // Update list content if needed
    //    if (updateListCallback != null)
    //        updateListCallback(listSource);

    //    // Reassign ItemsSource and select item
    //    dropdownField.ItemsSource = listSource.ToList();
    //    dropdownField.SelectedItem = newSelectedItem;

    //    // Update selected model
    //    assignSelectedModel?.Invoke(newSelectedItem);

    //    dropdownField.IsEnabled = true;
    //}

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
        //entryCode.Text = "";
        //entryId.Text = "";
        //entryName.Text = "";
    }    

    //internal FDays getDays()
    //{
    //    return (FDays) ddfDays.SelectedItem;
    //}

    internal FStatus getStatus()
    {
        return (FStatus) ddfStatus.SelectedItem;
    }

    private void Chip_DestroyClicked(object sender, EventArgs e)
    {
        //chipMarca.Text = "   MARCA 222";
        //chipMarca.BackgroundColor = Colors.Pink;
        //Debug.WriteLine("Chip_DestroyClicked Debug");        
    }
}