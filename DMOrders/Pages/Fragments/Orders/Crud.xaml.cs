using CommunityToolkit.Maui;
using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Extensions;
using CommunityToolkit.Maui.Views;
using DMOrders.Controls.Tools;
using DMOrders.Models;
using DMOrders.Pages.Fragments.Orders.modals;
using DMOrders.Services.Promotions;
using DMOrders.Shared;
using DMSA.Models.Odoo.DMOrders.promotions.abstractCustom;
using DMSA.Models.Odoo.Native;
using DMSA.Models.Odoo.Sales;
using DMSA.Models.Odoo.Sales.promotions.abstractCustom;
using DMSA.Sync.Core.Database.Sqlite;
using DMSA.Sync.Core.Database.Sqlite.Benefits;
using DMSA.Sync.Core.Database.Sqlite.Sales;
using DMSA.Sync.Core.Update.Pusher;
using Microsoft.Maui.Controls.Shapes;
using Newtonsoft.Json;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows.Input;

namespace DMOrders.Pages.Fragments.Orders;

[XamlCompilation(XamlCompilationOptions.Compile)]
public partial class Crud : ContentPage, IBackButtonHandler
{
    public bool LockEdition { get; set; } = false;

    private Entry _activeEntry;
    public res_company CurrentCompany { get; set; }
    public res_partner _CurrentPartner { get; set; }
    public sale_order _CurrentSaleOrder { get; set; }
    public product_pricelist CurrentPriceList { get; set; }
    public ICommand EditCommand { get; set; }
    public ICommand DeleteCommand { get; set; }
    public product_product _ProductEditing { get; set; }    
    public List<SaleOrderPromotions> saleOrderPromotions { get; set; }
    public List<res_partner> PartnerAddress { get; set; }

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
                OnPropertyChanged(nameof(PartnerDisplayName));
                OnPropertyChanged(nameof(PartnerDisplayAddress));
                OnPropertyChanged(nameof(PartnerDisplayStatus));
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
                OnPropertyChanged(nameof(PriceListDisplayName));
                OnPropertyChanged(nameof(IdReferencia));
                OnPropertyChanged(nameof(IdPrimaryKey));
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

                product_uom_qty = value;
            }
        }
    }

    public string PartnerDisplayName =>
    CurrentSaleOrder?.partner_display_name
    ?? CurrentPartner?.display_name
    ?? string.Empty;


    public string PartnerDisplayAddress =>
       CurrentSaleOrder?.partner_display_address
       ?? CurrentPartner?.street
       ?? string.Empty;

    public string StateCity { get; set; }

    public string PartnerDisplayStatus =>
    CurrentSaleOrder?.partner_display_status
    ?? (CurrentPartner != null ? (CurrentPartner.active ? "Activo" : "Inactivo") : string.Empty);

    public string PriceListDisplayName =>
        CurrentPriceList?.name
        ?? CurrentPriceList?.clave_externa
        ?? string.Empty;

    public string IdReferencia =>
        CurrentSaleOrder?.id_referencia    
        ?? string.Empty;

    public string IdPrimaryKey =>
        CurrentSaleOrder?.id.ToString()
        ?? string.Empty;

    private bool _loaded;

    private bool _saving;

    private bool _existPromotionsApplied
    {
        get
        {
            if (saleOrderPromotions.Count != 0)
            {
                return true;
            }

            return false;
        }
    }

    private bool _requiredSaveChanges { get; set; }

    public Crud()
	{
		InitializeComponent();
        EditCommand = new Command(EditItem);
        DeleteCommand = new Command(DeleteItem);
        OrderLines = new ObservableCollection<sale_order_line>();
        saleOrderPromotions = new List<SaleOrderPromotions>();
        AddLineCommand = new Command<ItemPickedArgs>(OnAddLine);
        AddLineCommandByQty = new Command<ItemPickedArgs>(OnAddLine);

        if (App.Session.odooConnection.IsTestMode)
        {
            btnPaste.IsVisible = true;
        }

        BindingContext = this;
    }

    private void SearchProductView_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        //throw new NotImplementedException();
        Debug.WriteLine(e.PropertyName);        
    }

    public async Task<int> PrepareForm()
    {
        //0 - Todo Correcto y se procede a avanzar
        //1 - Error en lista de precios
        //2 - Error en direcciones
        bool RequiredPreloadData = false;
        _loaded = true;
        _activeEntry = EntryCantidadSolicitada;
        HighlightActiveEntry(_activeEntry);

        try
        {
            await UITools.ShowLoadingPopup(this);
            await UITools.SetNotifyLoadingPopup("Cargando datos...");                        
            await Task.Yield();

            BlockControls();

            var resPartnerDb = new ResPartnerDb(App.Session.odooConnection.DbNameSqlite);
                        
            if (CurrentPartner != null)
            {
                Title = CurrentPartner.name;

                if (CurrentSaleOrder == null)
                {
                    Title += " [*]";
                    btnSend.IsVisible = false;
                    SearchProductView.IsVisible = true;
                }
                else
                {
                    Title += " []";
                    btnSend.IsVisible = true;
                }
            }
            else
            {                
                CurrentPartner = await resPartnerDb.GetItemsAsync(
                    CurrentCompany.id,
                    CurrentSaleOrder._partner_id
                );

                if(CurrentPartner == null )
                {
                    return 3;
                }

                //if (CurrentPartner != null)
                Title = CurrentPartner.name;

                if (CurrentSaleOrder != null)
                {
                    Title += " [edición]";
                    RequiredPreloadData = true;
                }
            }

            OnPropertyChanged(nameof(CurrentCompany));

            if (RequiredPreloadData)
            {
                await LoadData();
            }

            var priceListDb = new ProductPricelistDb(App.Session.odooConnection.DbNameSqlite);

            CurrentPriceList = await priceListDb.GetItemAsync(x => x.id == CurrentPartner._product_pricelist_id);

            if (CurrentPriceList == null || CurrentPartner._product_pricelist_id == 0)
            {
                await UITools.HideLoadingPopup();                                
                return 1;
            }

            var addresses = await resPartnerDb.GetItemsAsync(x =>
                x._parent_id == CurrentPartner.id && x._type == "other"
            );

            if (addresses == null || addresses.Count == 0)
            {
                await UITools.HideLoadingPopup();
                return 2;
            }

            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                PartnerAddress = addresses;

                ddfAddress.ItemsSource = PartnerAddress;
                ddfAddress.ItemDisplayBinding = new Binding("display_full_address");
                ddfAddress.SelectedItem = PartnerAddress[0];

                if (RequiredPreloadData)
                {
                    var selected = PartnerAddress.FirstOrDefault(x =>
                        x.id == CurrentSaleOrder._partner_invoice_id);

                    if (selected != null)
                        ddfAddress.SelectedItem = selected;
                }
                else
                {
                    var selectedAddress = (res_partner)ddfAddress.SelectedItem;
                    CurrentPartner.street = selectedAddress.street;
                    OnPropertyChanged(nameof(PartnerDisplayAddress));
                }

                ddfAddress.SelectedItemChanged += (sender, e) =>
                {
                    if (ddfAddress.SelectedItem != null) 
                    {
                        var selectedAddress = (res_partner) ddfAddress.SelectedItem;
                        if (CurrentSaleOrder != null)
                        {
                            CurrentSaleOrder.partner_display_address = selectedAddress.street;
                        }
                        else
                        {
                            if(CurrentPartner!= null)
                            {
                                CurrentPartner.street = selectedAddress.street;
                            }
                        }
                        OnPropertyChanged(nameof(PartnerDisplayAddress));
                    }
                };

                SearchProductView.CurrentPriceList = CurrentPriceList;

                LockEdition = CurrentSaleOrder?.is_synchronized ?? false;

                OnPropertyChanged(nameof(LockEdition));
                OnPropertyChanged(nameof(PriceListDisplayName));
            });
        }
        finally
        {
            UnlockControls();
            if(!RequiredPreloadData)
            {
                //await Task.Delay(500);
            }            
            await UITools.HideLoadingPopup();            
        }

        return 0;
    }

    public async Task<bool> OnBackButtonPressedAsync()
    {
        bool result = await DisplayAlertAsync("Confirmación", "Minimizar la aplicación, ¿Desea continuar?", "Sí", "No");
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
            if(LockEdition)
            {
                await Navigation.PopModalAsync(false);
            }

            if (SearchProductView.IsVisible)
            {
                await Toast.Make("Antes de cerrar la venta de la orden debe cerrar, la búsqueda de productos.").Show();
                return;
            }

            var leave = await DisplayAlertAsync("Atención", "Los cambios que haya realizado no se guardarán. ¿Desea continuar?", "Si", "No");

            if (leave)
            {                
                await Navigation.PopModalAsync(false);
            }
        });

        return true;        
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
        if (_saving)
        {
            Debug.WriteLine("Guardado ya en proceso, por favor espere...");
            return;
        }
                
        BlockControls();

        _saving = true;
        List<string> applyPromo = null;

        try
        {            
            bool saved_data = false;
            sale_order targetOrder = null;

            if (_existPromotionsApplied)
            {
                var leave = await DisplayAlertAsync("⚠️ Confirmación requerida",
                    "Este pedido ya tiene promociones aplicadas.\n\nSi continúas, se eliminarán y se recalcularán.",
                    "Sí, eliminar",
                    "No eliminar");

                if (leave)
                {
                    await CleanPromotionStatusFull(CurrentSaleOrder);
                    await Toast.Make("Promociones eliminadas.").Show();
                }
            }

            await UITools.ShowLoadingPopup(this);
            await UITools.SetNotifyLoadingPopup("Calculando promociones...");
            await Task.Yield();

            targetOrder = CurrentSaleOrder;
            
            _requiredSaveChanges = true;

            if (_requiredSaveChanges)
            {
                targetOrder = await SaveOrder();
            }
            
            if (targetOrder != null)
            {                
                saved_data = true;
                applyPromo = await ApplyPromo(targetOrder);

                if (applyPromo!=null && applyPromo.Count > 0)
                {                    
                    targetOrder = await SaveOrder();
                }
            }
        }
        finally
        {
            _saving = false;
            UnlockControls();
            await UITools.HideLoadingPopup();

            //if (applyPromo == null)
            //{
                await Navigation.PopModalAsync(false);
            //}
        }
    }

    [Obsolete("Debe eliminarse")]
    private async void ButtonPromo_Clicked(object sender, EventArgs e)
    {
        var leave = await DisplayAlertAsync("Atención", "Se guardarán los cambios antes de aplicar las promociones. ¿Desea continuar?", "Si", "No");

        if (!leave)
        {
            return;
        }

        try
        {
            BlockControls();

            sale_order targetOrder = await SaveOrder();

            if (targetOrder != null)
            {
                var applyPromo = await ApplyPromo(targetOrder);

                if (applyPromo.Count > 0)
                {

                }
            }
        }
        finally
        {
            UnlockControls();
        }
    }

    private async Task CleanPromotionStatusFull(sale_order saleOrder)
    {
        for (var i = saleOrderPromotions.Count - 1; i >= 0; i--)
        {
            //var promo = saleOrderPromotions[i];
            //saleOrderPromotions.Remove(promo);
            saleOrderPromotions.RemoveAt(i);
        }

        int deletedGifts = 0;

        for (int i = OrderLines.Count - 1; i >= 0; i--)
        {
            if (OrderLines[i].is_gift)
            {
                OrderLines.RemoveAt(i);
                deletedGifts++;
            }
        }

        for (int i = 0; i < OrderLines.Count; i++)
        {
            //Lineas con descuento se resetean a precio original y se eliminan promociones
            if (!OrderLines[i].is_gift && OrderLines[i].discount > 0)
            {
                OrderLines[i].discount = 0;
                //continue;
            }

            var line = OrderLines[i];
            line.promotion_data = null;            

            var productDb = new ProductProductDb(App.Session.odooConnection.DbNameSqlite);
            var product_item = await productDb.GetItemAsync(x => x.id == line.product_id);

            if (product_item == null)
            {
                Debug.WriteLine("Error: no se encontró el producto para actualizar la línea de orden.");
                return;
            }

            product_item.list_price = (float)line.price_unit;
            UpdateOrderLineLite(line, product_item);
        }

        UpdateTotals();        
    }

    public static string ObtenerIniciales(string nombreCompleto)
    {
        var partes = nombreCompleto
            .Trim()
            .Split(' ', StringSplitOptions.RemoveEmptyEntries);

        if (partes.Length == 0)
            return "";

        if (partes.Length == 1)
            return partes[0][0].ToString().ToUpper();

        // Si hay 2 palabras → iniciales = primera + segunda
        if (partes.Length == 2)
            return (partes[0][0].ToString() + partes[1][0].ToString()).ToUpper();

        // Si hay 3 o más → primera + tercera (como tu lógica original)
        return (partes[0][0].ToString() + partes[2][0].ToString()).ToUpper();
    }


    public static string GenerarCodigo(string nombreCompleto, int secuencial, bool esMovil = true)
    {
        string iniciales = ObtenerIniciales(nombreCompleto);
        string prefijo = esMovil ? "M" : "W";
        string secuencialFormateado = secuencial.ToString("D3");
        string fecha = DateTime.Now.ToString("ddMMyyyy");

        return $"{prefijo}{secuencialFormateado}-{iniciales}{fecha}";
    }

    

    private async Task ResetPromotions(sale_order targetOrder)
    {
        var saleOrderPromoDb = new SaleOrderPromotionsDb(App.Session.odooConnection.DbNameSqlite);
        await saleOrderPromoDb.DeleteItemOfParent(targetOrder);
    }

    private async Task SavePromotions(bool autos, bool manuals)
    {
        var saleOrderPromoDb = new SaleOrderPromotionsDb(App.Session.odooConnection.DbNameSqlite);
        
        foreach (var orderPromo in saleOrderPromotions)
        {
            if (autos)
            {
                if (orderPromo.promotion_type_id == 2 && orderPromo.promotion_selection_type_id != 2 ||
                    orderPromo.promotion_type_id == 4 ||
                    orderPromo.promotion_type_id == 6)
                {
                    await saleOrderPromoDb.InsertAsync(orderPromo);
                }
            }

            if (manuals)
            {
                if (orderPromo.promotion_type_id == 2 && orderPromo.promotion_selection_type_id == 2)
                    await saleOrderPromoDb.InsertAsync(orderPromo);                
            }
        }
    }

    


    public async Task EvalPromotions(sale_order saleOrder)
    {
        try
        {
            decimal totalOrder = saleOrder.amount_total;
            decimal totalProductAmount = 0m;

            AppliedPromotionResults ??= new ObservableCollection<PromotionEvalResult>();
            AppliedPromotionResults.Clear();

            string dbNameSqlite = App.Session.odooConnection.DbNameSqlite;

            var repo = new PromotionRepository();            
            var engine = new PromotionEngineLite(repo);

            AppliedPromotionResults = await engine.EvaluatePromotions(
                saleOrder: saleOrder
            );           


            Debug.WriteLine($"Promociones aplicadas: {AppliedPromotionResults.Count}");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error evaluando promociones: {ex}");
        }
    }

    private async void ButtonSync_Clicked(object sender, EventArgs e)
    {
        var leave = await DisplayAlertAsync("Enviar", "¿Desea enviar esta orden al ERP? Los cambios realizados serán almacenados.", "Si", "No");

        if (!leave)
        {
            return;
        }

        await UITools.ShowLoadingPopup(this);
        
        bool orderHasChanges = true;

        if(orderHasChanges)
        {
            await UITools.SetNotifyLoadingPopup("Almacenando orden...");
            //await SaveOrder();
        }

        await UITools.SetNotifyLoadingPopup("Preparando orden...");

        SaleOrders serverPusher = new SaleOrders();

        var orderLinesList = OrderLines.ToList();
        CurrentSaleOrder.order_line = new List<OrderLineWrapper>();
        CurrentSaleOrder._center_id = App.Session.odooConnection.res_center_default;

        foreach (var orderLine in orderLinesList)
        {
            CurrentSaleOrder.order_line.Add(new OrderLineWrapper(orderLine));
        }

        await UITools.SetNotifyLoadingPopup("Sincronizando orden...");
        bool sendOk = await serverPusher.SendSaleOrder(CurrentSaleOrder);

        await UITools.HideLoadingPopup();

        if(sendOk)
        {
            await DisplayAlertAsync("Envío de datos", "Envío correcto", "Aceptar");
            await Navigation.PopModalAsync(false);
        }
    }

    private async void EditItem(object obj)
    {
        Debug.WriteLine(obj);
        Debug.WriteLine("EditItem");
    }

    private async void DeleteItem(object obj)
    {        
        Debug.WriteLine("DeleteItem");
        //((CrudViewModel)this.BindingContext).OrderLines.Remove((sale_order_line) obj);
        RemoveOrderLine((sale_order_line)obj); 
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
        ProductProductDb productProductDb = new ProductProductDb(App.Session.odooConnection.DbNameSqlite);
        ProductEditing = await productProductDb.GetItem(SaleOrderLine.product_id);
        ProductEditing.list_price = (float) SaleOrderLine.price_unit;
        ProductEditing.uom_sale_display = SaleOrderLine.uom_category_display;

        OnPropertyChanged(nameof(ProductEditing));        

        product_uom_qty_real = SaleOrderLine.product_uom_qty_real;
        product_uom_qty = SaleOrderLine.product_uom_qty;

        if(SaleOrderLine.is_gift)
        {
            _activeEntry = null;
            labelGifInfo.IsVisible = true;
            btnApplyQty.IsEnabled = false;
            btnResetQty.IsEnabled = false;
        }
        else
        {
            _activeEntry = EntryCantidadSolicitada;
            labelGifInfo.IsVisible = false;
            btnApplyQty.IsEnabled = true;
            btnResetQty.IsEnabled = true;
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
        if (CurrentSaleOrderLine != null)
        {
            product_uom_qty_real = CurrentSaleOrderLine.product_uom_qty_real;
            product_uom_qty = CurrentSaleOrderLine.product_uom_qty;            
        }
    }

    private async void ApplyValueChanges(object sender, EventArgs e)
    {
        if (CurrentSaleOrderLine != null)
        {
            if (product_uom_qty == 0 || product_uom_qty_real == 0)
            {   
                //await DisplayAlert("Alerta", "La cantidad no puede ser 0.", "Aceptar");
                //return;
            }

            if (product_uom_qty > product_uom_qty_real)
            {
                await DisplayAlertAsync("Alerta", "La cantidad solicitada no puede ser mayor a la cantidad real.", "Aceptar");
                return;
            }

            if ((decimal)ProductEditing.cantidad_disponible < product_uom_qty)
            {
                CurrentSaleOrderLine = null;
                ProductEditing = null;
                product_uom_qty_real = 0;
                product_uom_qty = 0;                
                OrderLinesCl.SelectedItem = null;

                await DisplayAlertAsync("Alerta", "La cantidad solicitada no puede ser mayor a la disponible en inventario.", "Aceptar");
                return;
            }

            if(CurrentSaleOrderLine.product_uom_qty_real == product_uom_qty_real && CurrentSaleOrderLine.product_uom_qty == product_uom_qty)
            {
                await Toast.Make("No hay cambios para aplicar").Show();
                return;
            }

            bool requireRefresh = false;
            if (product_uom_qty_real < CurrentSaleOrderLine.product_uom_qty_real || product_uom_qty < CurrentSaleOrderLine.product_uom_qty)
            {
                requireRefresh = true;
            }

            CurrentSaleOrderLine.product_uom_qty_real = product_uom_qty_real;
            CurrentSaleOrderLine.product_uom_qty = product_uom_qty;

            var productDb = new ProductProductDb(App.Session.odooConnection.DbNameSqlite);
            var product_item = await productDb.GetItemAsync(x => x.id == CurrentSaleOrderLine.product_id);

            if(product_item == null)
            {
                Debug.WriteLine("Error: no se encontró el producto para actualizar la línea de orden.");
                return;
            }

            product_item.list_price = (float) CurrentSaleOrderLine.price_unit;

            if (requireRefresh)
            {
                UpdateOrderLineRefresh(CurrentSaleOrderLine, product_item);
            }
            else
            {
                PromotionEngineRunner promotionEngineRunner = new PromotionEngineRunner();
                await promotionEngineRunner.ResetManualGiftBenefitSoft(CurrentSaleOrder, saleOrderPromotions);

                UpdateOrderLine(CurrentSaleOrderLine, product_item);
            }
            
            CurrentSaleOrderLine = null;
            ProductEditing = null;
            product_uom_qty_real = 0;
            product_uom_qty = 0;

            OrderLinesCl.SelectedItem = null;

            _activeEntry = EntryCantidadSolicitada;
            HighlightActiveEntry(_activeEntry);
        }
    }

    private void BlockControls()
    {
        btnAddProd.IsEnabled = false;
        btnPromo.IsEnabled = false;
        btnSave.IsEnabled = false;
    }

    private void UnlockControls()
    {
        btnAddProd.IsEnabled = true;
        btnPromo.IsEnabled = true;
        btnSave.IsEnabled = true;
    }
}