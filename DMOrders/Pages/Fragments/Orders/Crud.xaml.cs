
using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Extensions;
using CommunityToolkit.Maui.Views;
using CommunityToolkit.Mvvm.Input;
using DMOrders.Controls;
using DMOrders.Pages.Fragments.Orders.modals;
using DMOrders.Services.Database.Sqlite;
using DMOrders.Services.Helpers;
using DMOrders.Services.Update.Pusher;
using DMOrders.Shared;
using DMSA.Models.Odoo.DMOrders;
using DMSA.Models.Odoo.Native;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using UraniumUI.Material.Controls;

namespace DMOrders.Pages.Fragments.Orders;

[XamlCompilation(XamlCompilationOptions.Compile)]
public partial class Crud : ContentPage, IBackButtonHandler
{    
    private Entry _activeEntry;

    public res_company CurrentCompany { get; set; }
    public res_partner _CurrentPartner { get; set; }
    public sale_order _CurrentSaleOrder { get; set; }
    public ICommand EditCommand { get; set; }
    public ICommand DeleteCommand { get; set; }

    public product_product _ProductEditing { get; set; }

    public product_product ProductEditing
    {
        get => _ProductEditing;
        set
        {
            if (_ProductEditing != value)
            {
                _ProductEditing = value;
                OnPropertyChanged(nameof(ProductEditing));
            }
        }
    }

    public sale_order_line _CurrentSaleOrderLine { get; set; }

    public sale_order_line CurrentSaleOrderLine
    {
        get => _CurrentSaleOrderLine;
        set
        {
            if (_CurrentSaleOrderLine != value)
            {
                _CurrentSaleOrderLine = value;
                OnPropertyChanged(nameof(CurrentSaleOrderLine));
            }
        }
    }

    public res_partner CurrentPartner
    {
        get => _CurrentPartner;
        set
        {
            if (_CurrentPartner != value)
            {
                _CurrentPartner = value;
                OnPropertyChanged(nameof(CurrentPartner));
            }
        }
    }

    public sale_order CurrentSaleOrder
    {
        get => _CurrentSaleOrder;
        set
        {
            if (_CurrentSaleOrder != value)
            {
                _CurrentSaleOrder = value;
                OnPropertyChanged(nameof(CurrentSaleOrder));

                OnPropertyChanged(nameof(PartnerDisplayName));
                OnPropertyChanged(nameof(PartnerDisplayAddress));
                OnPropertyChanged(nameof(PartnerDisplayStatus));
            }
        }
    }

    public decimal _product_uom_qty { get; set; }
    public decimal _product_uom_qty_real { get; set; }

    public decimal product_uom_qty 
    { 
        get => _product_uom_qty;
        set
        {
            if(_product_uom_qty != value)
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
            }
        }
    }

    public string PartnerDisplayName => CurrentSaleOrder?.partner_display_name ?? string.Empty;
    public string PartnerDisplayAddress => CurrentSaleOrder?.partner_display_address ?? string.Empty;
    public string PartnerDisplayStatus => CurrentSaleOrder?.partner_display_status ?? string.Empty;

    protected override void OnAppearing()
    {
        base.OnAppearing();

        // El primero activo por defecto
        _activeEntry = EntryCantidadSolicitada;
        HighlightActiveEntry(_activeEntry);
    }

    public Crud()
	{
		InitializeComponent();        
        BindingContext = new CrudViewModel();

        EditCommand = new Command(EditItem);
        DeleteCommand = new Command(DeleteItem);

        SearchProductView.PropertyChanged += SearchProductView_PropertyChanged;
    }

    private void SearchProductView_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        //throw new NotImplementedException();
        Debug.WriteLine(e.PropertyName);
        //if(e.PropertyName== "IsVisible" && !SearchProductView.IsVisible)
        //{
            //OnPropertyChanged(nameof(OrderLinesCl));
        //}
    }

    protected override void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        base.OnPropertyChanged(propertyName);
        if(propertyName.Contains("CurrentPartner"))
        {
            //PrepareForm();
        }
        Debug.WriteLine($"Property changed: {propertyName}");
    }

    public async Task PrepareForm()
    {
        if( CurrentPartner != null )
        {
            //DESDE LISTA DE CLIENTES PARA AGREGAR NUEVA ORDEN
            Title = CurrentPartner.name;
            if(CurrentSaleOrder == null)
            {
                Title += " [*]";
                btnSend.IsVisible = false;
            }
            else
            {
                Title += " [nuevo]";
                btnSend.IsVisible = true;
            }

            ((CrudViewModel)this.BindingContext)._CurrentPartner = CurrentPartner;
            ((CrudViewModel)this.BindingContext).CurrentCompany = CurrentCompany;
            ((CrudViewModel)this.BindingContext).CurrentSaleOrder = CurrentSaleOrder;
            await ((CrudViewModel)this.BindingContext).LoadData();
        }
        else
        {
            ResPartnerDb resPartnerDb = new ResPartnerDb();
            var CurrentPartner = await resPartnerDb.GetItemsAsync(CurrentCompany.id , CurrentSaleOrder._partner_id);
            if(CurrentPartner != null)
            {
                Title = CurrentPartner.name;
            }

            if (CurrentSaleOrder != null)
            {
                Title += " [edición]";
            }

            ((CrudViewModel)this.BindingContext).CurrentCompany = CurrentCompany;
            ((CrudViewModel)this.BindingContext).CurrentSaleOrder = CurrentSaleOrder;
            await ((CrudViewModel)this.BindingContext).LoadData();
        }
    }

    public async Task<bool> OnBackButtonPressedAsync()
    {
        bool result = await DisplayAlert("Confirmación", "Minimizar la aplicación, ¿Desea continuar?", "Sí", "No");
        if (result)
        {
#if ANDROID
            Platform.CurrentActivity?.MoveTaskToBack(true);
#endif
        }
        return !result; // true => lo manejo yo y no cierro la app; false => dejar cerrar
    }

    protected override bool OnBackButtonPressed()
    {
        var tcs = new TaskCompletionSource<bool>();

        Dispatcher.Dispatch(async () =>
        {
            if (SearchProductView.IsVisible)
            {
                await Toast.Make("Primero cierre la búsqueda de productos.").Show();
                return;
            }

            var leave = await DisplayAlert("Atención", "Los cambios que haya realizado no se guardarán. ¿Desea continuar?", "Si", "No");

            if (leave)
            {                
                await Navigation.PopModalAsync();
            }
        });

        return true;        
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        //Debug.WriteLine("Details page is disappearing.");

        //if (true)
        //{
        //    // Si no se permite el cierre, evitar que la página se cierre
        //    Navigation.PopModalAsync(false);
        //}
    }

    private async void ButtonClose_Clicked(object sender, EventArgs e)
    {
        SendBackButtonPressed();
        //await Navigation.PopAsync();
    }

    private async void ButtonAddNew_Clicked(object sender, EventArgs e)
    {
        SearchProductView.IsVisible = true;
    }

    private async void ButtonSave_Clicked(object sender, EventArgs e)
    {
        var viewModel = (CrudViewModel)this.BindingContext;
        var orderLines = viewModel.OrderLines;
        var saleOrderDb = new SaleOrderDb();
        var saleOrderLineDb = new SaleOrderLineDb();

        sale_order targetOrder;

        bool isNew = CurrentSaleOrder == null;

        if (isNew)
        {
            targetOrder = new sale_order
            {
                _partner_id = _CurrentPartner.id,
                _company_id = CurrentCompany.id,
                date_order = DateTime.Now,
                //note = fieldNote.Text
            };

            if (await saleOrderDb.InsertAsync(targetOrder) <= 0)
            {
                await Toast.Make("Error al crear la orden").Show();
                return;
            }
        }
        else
        {
            targetOrder = CurrentSaleOrder;
            targetOrder.write_date = DateTime.Now;
            //targetOrder.note = fieldNote.Text;

            if (await saleOrderDb.UpdateAsync(targetOrder) <= 0)
            {
                await Toast.Make("Error al actualizar la orden").Show();
                return;
            }

            // Eliminar líneas anteriores antes de insertar las nuevas
            await saleOrderLineDb.DeleteItemOfParent(targetOrder);
        }

        int ordinal = 1;
        // Asignar el ID de la orden a las líneas y guardar
        foreach (var orderLine in orderLines)
        {
            orderLine._order_id = targetOrder.id;
            orderLine.ordinal = ordinal;
            await saleOrderLineDb.InsertAsync(orderLine);
            ordinal++;
        }

        await Toast.Make(isNew ? "Orden creada" : "Orden actualizada").Show();

        //if (await saleOrderLineDb.InsertBatchAsync(orderLines.ToArray()) > 0)
        //{
        //    await Toast.Make(isNew ? "Orden creada" : "Orden actualizada").Show();
        //}
        //else
        //{
        //    await Toast.Make("Error al guardar líneas").Show();
        //}

        //await Navigation.PopAsync();
        await Navigation.PopModalAsync();
        //SendBackButtonPressed();
    }

    private async void ButtonSync_Clicked(object sender, EventArgs e)
    {
        ServerPusher serverPusher = new ServerPusher();

        var orderLinesList = ((CrudViewModel)this.BindingContext).OrderLines.ToList();
        CurrentSaleOrder.order_line = new List<OrderLineWrapper>();
        CurrentSaleOrder._center_id = App.Session.odooConnection.res_center_default;

        foreach (var orderLine in orderLinesList)
        {
            //orderLine.price_subtotal = 1;
            //orderLine.price_unit = 1;
            //orderLine.product_uom_qty = 1;

            CurrentSaleOrder.order_line.Add(new OrderLineWrapper(orderLine));
        }

        await serverPusher.SendSaleOrder(CurrentSaleOrder);
    }

    private async void EditItem(object obj)
    {
        Debug.WriteLine(obj);
        Debug.WriteLine("EditItem");
    }

    private async void DeleteItem(object obj)
    {        
        Debug.WriteLine("DeleteItem");
        ((CrudViewModel)this.BindingContext).OrderLines.Remove((sale_order_line) obj);
    }

    private async void detail_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        Debug.WriteLine("detail_SelectionChanged");
        //((CrudViewModel)this.BindingContext).OrderLines[0].qty_to_deliver = 5;
        //LoadDetailInfo();
        if (e.CurrentSelection != null && e.CurrentSelection.Count > 0)
        {
            CurrentSaleOrderLine = (sale_order_line)e.CurrentSelection[0];
            await LoadDetailInfo(CurrentSaleOrderLine);
            //Debug.WriteLine(CurrentSaleOrderLine.product_display);
        }
    }

    private async Task LoadDetailInfo(sale_order_line SaleOrderLine)
    {
        //SaleOrderLine.qty_to_deliver = 6;
        ProductProductDb productProductDb = new ProductProductDb();
        ProductEditing = await productProductDb.GetItem(SaleOrderLine.product_id);
        //OnPropertyChanged(nameof(ProductEditing));

        product_uom_qty_real = SaleOrderLine.product_uom_qty_real;
        product_uom_qty = SaleOrderLine.product_uom_qty;        
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

    private static string Normalize(string s)
    {
        // Evitar "-0" accidentales o múltiples ceros iniciales (opcional)
        if (s == "") return "";
        if (s == ".") return "0.";     // si el usuario empieza con punto
        if (s.StartsWith("0") && s != "0" && !s.StartsWith("0."))
            s = s.TrimStart('0');      // 0005 -> 5
        if (s == "") s = "0";
        return s;
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
        if (CurrentSaleOrderLine != null)
        {
            product_uom_qty_real = CurrentSaleOrderLine.product_uom_qty_real;
            product_uom_qty = CurrentSaleOrderLine.product_uom_qty;
        }
    }

    private void ApplyValueChanges(object sender, EventArgs e)
    {
        if (CurrentSaleOrderLine != null)
        {
            CurrentSaleOrderLine.product_uom_qty_real = product_uom_qty_real;
            CurrentSaleOrderLine.product_uom_qty = product_uom_qty;
            CurrentSaleOrderLine = null;
            ProductEditing = null;
            product_uom_qty_real = 0;
            product_uom_qty = 0;
        }

        //var vmOrderLines = ((CrudViewModel)this.BindingContext).OrderLines;
        //var foundLine = vmOrderLines.Where(x => x.id == CurrentSaleOrderLine.id).FirstOrDefault();
        //foundLine = CurrentSaleOrderLine;
        //foundLine.product_uom_qty_real = product_uom_qty_real;
    }
}