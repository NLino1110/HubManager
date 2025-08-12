using CommunityToolkit.Maui.Views;
using DMOrders.Controls;
using DMOrders.Models.Filters;
using DMSA.Models.Odoo.Native;
using Spinner.MAUI;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows.Input;

namespace DMOrders.Pages.Fragments.Orders;

public partial class Filters : ContentView
{
    public event EventHandler OnSearchButtonClicked;

    FDays[] Days { get; set; }

    FStatus[] Status { get; set; }
    public ObservableCollection<res_partner> Partners { get; set; } = new();
    res_partner selected_partner { get; set; }

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

        Status = new FStatus[10]
        {
            new FStatus { id = 0, Name = "Todos" },
            new FStatus { id = 1, Name = "Activo" },
            new FStatus { id = 2, Name = "Inactivo" },
            new FStatus { id = 3, Name = "Anulado" },
            new FStatus { id = 5, Name = "Facturado" },
            new FStatus { id = 6, Name = "Aprobado" },
            new FStatus { id = 26, Name = "Proceso WMS" },
            new FStatus { id = 10, Name = "Restringido" },
            new FStatus { id = 27, Name = "Sincronizado" },
            new FStatus { id = 78, Name = "Espera Aprob" },
        };

        ddfStatus.ItemsSource = Status;
        ddfStatus.ItemDisplayBinding = new Binding("Name");
        ddfStatus.SelectedItem = Status[0];
        //ddfDays.SelectedItemChanged += DdfDays_SelectedItemChanged;



        Partners =
        [
            new res_partner { id = 0, name = "No seleccionada" },
            new res_partner { id = -1, name = "🔍 Buscar..." },
            new res_partner { id = 1, name = "Cliente 1" },
            new res_partner { id = 2, name = "Cliente 2" },
            new res_partner { id = 3, name = "Cliente 3" },
            new res_partner { id = 4, name = "Cliente 4" },
            new res_partner { id = 5, name = "Cliente 5" },
        ];

        ddfCustomer.ItemsSource = Partners;
        ddfCustomer.ItemDisplayBinding = new Binding("name");
        ddfCustomer.SelectedItem = Partners[0];
        ddfCustomer.SelectedItemChanged += DdfBrands_SelectedItemChanged;
        selected_partner = Partners[0];
    }

    private async void DdfBrands_SelectedItemChanged(object? sender, object e)
    {
        res_partner new_selected_brand = (res_partner)e;

        if (new_selected_brand != null && (new_selected_brand.id == -1 || new_selected_brand.id == 0))
        {
            if (new_selected_brand.id == -1)
            {
                Debug.WriteLine("Buscar");
                ddfCustomer.SelectedItem = selected_partner;
                var selectedResPartner = await PopupResPartner(sender, null);

                if (selectedResPartner != null)
                {
                    ddfCustomer.IsEnabled = false;
                    ddfCustomer.ItemsSource = null;
                    Partners[0] = selectedResPartner;
                    ddfCustomer.ItemsSource = Partners;
                    ddfCustomer.SelectedItem = selectedResPartner;
                    selected_partner = selectedResPartner;
                    ddfCustomer.IsEnabled = true;
                    return;
                }
            }
        }

        if (new_selected_brand == null)
        {
            ddfCustomer.IsEnabled = false;
            Debug.WriteLine("Clear");
            ddfCustomer.ItemsSource = null;
            var nsResPartner = new res_partner { id = 0, name = "No seleccionada" };
            Partners[0] = nsResPartner;
            ddfCustomer.ItemsSource = Partners;
            ddfCustomer.SelectedItem = nsResPartner;
            selected_partner = nsResPartner;
            ddfCustomer.IsEnabled = true;
        }
        else
        {
            Debug.WriteLine("Seleccion valida directa...");
            selected_partner = new_selected_brand;
        }
    }

    async Task<res_partner> PopupResPartner(object sender, EventArgs e)
    {
        res_partner selected_product_brand = null;

        var popupSizeConstants = new PopupSizeConstants(DeviceDisplay.Current);
        popupSizeConstants.CalculateSizes(DeviceDisplay.Current);
        //Size = popupSizeConstants.Medium;

        var returnResultPopup = new PopupSelectPartner(popupSizeConstants);

        returnResultPopup.Company = App.Session.res_Company;
        returnResultPopup.CanBeDismissedByTappingOutsideOfPopup = false;

        //if (!isWindows)
        //returnResultPopup.Size = popupSizeConstants.Large;

        var result = await PopupExtensions.ShowPopupAsync(App.Current.MainPage, returnResultPopup);

        if (result != null)
        {
            selected_product_brand = (res_partner)result;
            //_inputResPartner.Text = Sel_Res_Partner.id.ToString() + " - " + Sel_Res_Partner.name;
            //_res_partnerItem = resPartner;
        }

        return selected_product_brand;
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
        return (FStatus)ddfStatus.SelectedItem;
    }

    internal res_partner getSelectedPartner()
    {
        return selected_partner;
    }

    internal string getDocNumber()
    {
        return entryDocNumber.Text;
    }

    internal DateTime? getDateStart()
    {
        return datePickerStart.Date;
    }

    internal DateTime? getDateEnd()
    {
        return datePickerEnd.Date;
    }
}