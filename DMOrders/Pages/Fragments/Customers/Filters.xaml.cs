using CommunityToolkit.Maui.Alerts;
using DMOrders.Controls.Tools;
using DMOrders.Models.Filters;
using DMOrders.Services.PatchManager.Reset;
using DMSA.Models.Odoo.Native;
using DMSA.Models.Odoo.Sales;
using DMSA.Sync.Core.Database.Sqlite;
using DMSA.Sync.Core.Database.Sqlite.Sales;
using Spinner.MAUI;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows.Input;

namespace DMOrders.Pages.Fragments.Customers;

public partial class Filters : ContentView
{
    public event EventHandler OnSearchButtonClicked;
    
    FDays[] Days { get; set; }

    FStatus[] Status { get; set; }

    public ObservableCollection<product_pricelist> Product_Pricelists { get; set; } = new();

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

        ddfDays.ItemsSource = Days;
        ddfDays.ItemDisplayBinding = new Binding("Name");
        ddfDays.SelectedItem = Days[0];
        ddfDays.SelectedItemChanged += DdfDays_SelectedItemChanged;
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
        ddfStatus.SelectedItemChanged += DdfStatus_SelectedItemChanged;

        LoadProductPricelists();
    }

    public async Task LoadProductPricelists()
    {
        ProductPricelistDb productPriceListsDb = new ProductPricelistDb(App.Session.odooConnection.DbNameSqlite);
        
        var pplItems = await productPriceListsDb.GetItemsAsync(x=>x._tipo_canal_id == 1 && x.use_mobile_app);

        MainThread.BeginInvokeOnMainThread(() =>
        {
            Product_Pricelists =
            [
                new product_pricelist { id = 0, name = "Todos" },                
            ];

            foreach (var marca in pplItems)
                Product_Pricelists.Add(marca);

            ddfChannel.ItemsSource = Product_Pricelists;
            ddfChannel.ItemDisplayBinding = new Binding("name");
            ddfChannel.SelectedItem = Product_Pricelists[0];
            ddfChannel.SelectedItemChanged += DdfChannel_SelectedItemChanged;            
        });
    }

    private void DdfStatus_SelectedItemChanged(object? sender, object e)
    {        
        Debug.WriteLine(e);
        //Button_Clicked(sender, null);
    }

    private void DdfDays_SelectedItemChanged(object? sender, object e)
    {       
        Debug.WriteLine(e);
        //Button_Clicked(sender, null);
    }

    private void DdfChannel_SelectedItemChanged(object? sender, object e)
    {        
        Debug.WriteLine(e);        
        //Button_Clicked(sender, null);
    }

    private void Button_Clicked(object sender, EventArgs e)
    {
        OnSearchButtonClicked?.Invoke(this, EventArgs.Empty);
    }

    private async void btnClearPartners_Clicked(object sender, EventArgs e)
    {
        var page = Application.Current?.Windows?[0]?.Page
            ?? Application.Current?.MainPage;

        if (page == null)
            return;

        bool confirm = await page.DisplayAlertAsync(
            "⚠️ Vaciar personas locales",
            "Se eliminarán de la tablet: clientes, direcciones y cuentas bancarias locales.\n\n" +
            "No se borran pedidos ni catálogo.\n\n" +
            "Después deberá sincronizar de nuevo (Personas / Clientes).",
            "Vaciar",
            "Cancelar");

        if (!confirm)
            return;

        btnClearPartners.IsEnabled = false;

        try
        {
            if (page is ContentPage contentPage)
            {
                await UITools.ShowLoadingPopup(contentPage);
                await UITools.SetNotifyLoadingPopup("Vaciando personas...");
            }

            await Task.Yield();
            await new ExecuteTask().ClearPartnersSyncDataAsync();

            btnClear_Clicked(sender, e);
            OnSearchButtonClicked?.Invoke(this, EventArgs.Empty);

            await Toast.Make("Personas locales vaciadas. Sincronice de nuevo para traer datos.").Show();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"btnClearPartners: {ex}");
            await page.DisplayAlertAsync("Error", "No se pudo vaciar las personas.\n\n" + ex.Message, "Aceptar");
        }
        finally
        {
            await UITools.HideLoadingPopup();
            btnClearPartners.IsEnabled = true;
        }
    }

    private void btnClear_Clicked(object sender, EventArgs e)
    {
        entryCode.Text = "";
        entryVat.Text = "";
        entryName.Text = "";
        ddfDays.SelectedItem = Days[0];
        ddfStatus.SelectedItem = Status[0];
        ddfChannel.SelectedItem = Product_Pricelists[0];
    }

    internal string getCode()
    {
        return entryCode.Text;
    }

    internal string getVat()
    {
        return entryVat.Text;
    }

    internal string getName()
    {
        return entryName.Text;
    }

    internal int getDays()
    {
        return ((FDays)ddfDays.SelectedItem).id;
    }

    internal int getStatus()
    {
        return ((FStatus)ddfStatus.SelectedItem).id;
    }

    internal int getChannel()
    {
        if (ddfChannel.SelectedItem != null)
            return ((product_pricelist)ddfChannel.SelectedItem).id;
        else
            return 0;
    }
}