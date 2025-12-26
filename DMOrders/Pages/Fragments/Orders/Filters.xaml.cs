using CommunityToolkit.Maui.Extensions;
using CommunityToolkit.Maui.Views;
using DMOrders.Controls;
using DMOrders.Models.Filters;
using DMSA.Models.Odoo.Native;
using DMSA.Sync.Core.Database.Sqlite;
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
                new res_partner { id = -1, name = "🔍 Buscar..." }
            ];

        //Task.Run(async () => await LoadTopCustomersAsync());
        LoadTopCustomersAsync();        
    }

    private async Task LoadTopCustomersAsync()
    {
        int adic_comercial_id = App.Session.CurrentUserFront.partner_id;
        ResPartnerDb resPartnerDb = new ResPartnerDb(App.Session.odooConnection.DbNameSqlite);

        var topPartners = await resPartnerDb.GetItemsAsync(x=> x._adic_comercial_id == adic_comercial_id);
        topPartners = topPartners.OrderByDescending(x => x.credit).Take(5).ToList();

        MainThread.BeginInvokeOnMainThread(() =>
        {
            Partners =
            [
                new res_partner { id = 0, name = "No seleccionada" },
                new res_partner { id = -1, name = "🔍 Buscar..." }
            ];

            foreach (var partnerItem in topPartners)
            {
                partnerItem.name = partnerItem.name + "- $" + partnerItem.credit.ToString();
                Partners.Add(partnerItem);
            }

            ddfCustomer.ItemsSource = Partners;
            ddfCustomer.ItemDisplayBinding = new Binding("name");
            ddfCustomer.SelectedItem = Partners[0];
            ddfCustomer.SelectedItemChanged += DdfCustomer_SelectedItemChanged;
            selected_partner = Partners[0];
        });
    }

    private async void DdfCustomer_SelectedItemChanged(object? sender, object e)
    {
        res_partner new_selected_partner = (res_partner)e;

        if (new_selected_partner != null && (new_selected_partner.id == -1 || new_selected_partner.id == 0))
        {
            if (new_selected_partner.id == -1)
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

        if (new_selected_partner == null)
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
            selected_partner = new_selected_partner;
        }
    }

    async Task<res_partner> PopupResPartner(object sender, EventArgs e)
    {
        res_partner selected_partner_popup = null;
        var popupSizeConstants = new PopupSizeConstants(DeviceDisplay.Current);
        popupSizeConstants.CalculateSizes(DeviceDisplay.Current);
        
        var returnResultPopup = new PopupSelectPartner(popupSizeConstants);

        returnResultPopup.Company = App.Session.res_Company;
        returnResultPopup.CanBeDismissedByTappingOutsideOfPopup = false;

        var result = await PopupExtensions.ShowPopupAsync<res_partner>(App.Current.MainPage, returnResultPopup);

        if (result.Result != null)
        {
            selected_partner_popup = result.Result;
            //_inputResPartner.Text = Sel_Res_Partner.id.ToString() + " - " + Sel_Res_Partner.name;
            //_res_partnerItem = resPartner;
        }

        return selected_partner_popup;
    }

    private void Button_Clicked(object sender, EventArgs e)
    {
        OnSearchButtonClicked?.Invoke(this, EventArgs.Empty);
    }

    private void btnClear_Clicked(object sender, EventArgs e)
    {
        entryDocNumber.ClearValue();
        
        Partners[0] = new res_partner { id = 0, name = "No seleccionada" };
        ddfCustomer.SelectedItem = Partners[0];
        ddfStatus.SelectedItem = Status[0];        
        datePickerStart.Date = DateTime.Now.AddDays(-7);
        datePickerEnd.Date = DateTime.Now;
        //entryId.Text = "";
        //entryName.Text = "";
    }

    internal int getStatus()
    {
        return ((FStatus)ddfStatus.SelectedItem).id;
    }

    internal int getSelectedPartner()
    {
        if (ddfCustomer.SelectedItem != null)
            selected_partner = (res_partner)ddfCustomer.SelectedItem;
        else
            selected_partner = Partners[0];

        return selected_partner.id;
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