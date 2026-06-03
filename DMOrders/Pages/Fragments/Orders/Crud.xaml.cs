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
using System.Runtime.CompilerServices;
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

    //public string PartnerDisplayAddress =>
    //    CurrentSaleOrder?.partner_display_address
    //    ?? CurrentPartner?.street
    //    ?? string.Empty;

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

    //protected override async void OnAppearing()
    //{
    //    base.OnAppearing();

    //    if (_loaded)
    //        return;

    //    _loaded = true;

    //    _activeEntry = EntryCantidadSolicitada;
    //    HighlightActiveEntry(_activeEntry);
        
    //    await Task.Yield();
    //    await LoadAsync();
    //}

    //private async Task LoadAsync()
    //{
    //    try
    //    {
    //        //await Task.Delay(100);
    //        await PrepareForm();
    //    }
    //    catch (Exception ex)
    //    {
    //        Debug.WriteLine(ex);
    //    }
    //}

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
        //if(e.PropertyName== "IsVisible" && !SearchProductView.IsVisible)
        //{
            //OnPropertyChanged(nameof(OrderLinesCl));
        //}
    }

    //protected override void OnPropertyChanged([CallerMemberName] string propertyName = null)
    //{
    //    base.OnPropertyChanged(propertyName);
    //    if(propertyName.Contains("CurrentPartner"))
    //    {
            
    //    }
    //    Debug.WriteLine($"Property changed: {propertyName}");
    //}

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

                if (CurrentPartner != null)
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

                        //StateCity = ""; // selectedAddress._state_id.ToString() + "" + selectedAddress.city;
                        //OnPropertyChanged(nameof(StateCity));
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

    //public async Task PrepareForm()
    //{
    //    try
    //    {
    //        await UITools.ShowLoadingPopup(this);
    //        await UITools.SetNotifyLoadingPopup("Cargando datos...");

    //        bool RequiredPreloadData = false;

    //        BlockControls();

    //        ResPartnerDb resPartnerDb = new ResPartnerDb(App.Session.odooConnection.DbNameSqlite);

    //        if (CurrentPartner != null)
    //        {
    //            Title = CurrentPartner.name;
    //            if (CurrentSaleOrder == null)
    //            {
    //                Title += " [*]";
    //                btnSend.IsVisible = false;
    //                SearchProductView.IsVisible = true;
    //            }
    //            else
    //            {
    //                Title += " []";
    //                btnSend.IsVisible = true;
    //            }

    //            await LoadData();
    //        }
    //        else
    //        {

    //            CurrentPartner = await resPartnerDb.GetItemsAsync(CurrentCompany.id, CurrentSaleOrder._partner_id);
    //            if (CurrentPartner != null)
    //            {
    //                Title = CurrentPartner.name;
    //            }

    //            if (CurrentSaleOrder != null)
    //            {
    //                Title += " [edición]";
    //                RequiredPreloadData = true;
    //            }

    //            await LoadData();

    //            LockEdition = CurrentSaleOrder.is_synchronized;
    //            OnPropertyChanged(nameof(LockEdition));
    //        }

    //        var PriceListDb = new ProductPricelistDb(App.Session.odooConnection.DbNameSqlite);
    //        CurrentPriceList = await PriceListDb.GetItem(CurrentPartner._product_pricelist_id);

    //        if (CurrentPriceList == null || CurrentPartner._product_pricelist_id == 0)
    //        {
    //            await DisplayAlert("Alerta", "El cliente no tiene lista de precio asignada, no se puede continuar", "Aceptar");
    //            await Navigation.PopModalAsync();
    //            return;
    //        }

    //        PartnerAddress = new List<res_partner>();
    //        PartnerAddress = await resPartnerDb.GetItemsAsync(x => x._parent_id == CurrentPartner.id && x._type == "other");
    //        if (PartnerAddress != null && PartnerAddress.Count > 0)
    //        {

    //        }
    //        else
    //        {
    //            PartnerAddress = new List<res_partner>();
    //            await DisplayAlert("Alerta", "El cliente no tiene lista de direcciones asignadas, no se puede continuar", "Aceptar");
    //            await Navigation.PopModalAsync();
    //            return;
    //        }

    //        ddfAddress.ItemsSource = PartnerAddress;
    //        ddfAddress.ItemDisplayBinding = new Binding("display_full_address");
    //        ddfAddress.SelectedItem = PartnerAddress[0];

    //        if (RequiredPreloadData)
    //        {
    //            for (var i = 0; i < PartnerAddress.Count; i++)
    //            {
    //                if (PartnerAddress[i].id == CurrentSaleOrder._partner_invoice_id)
    //                {
    //                    ddfAddress.SelectedItem = PartnerAddress[i];
    //                    break;
    //                }
    //            }
    //        }

    //        SearchProductView.CurrentPriceList = CurrentPriceList;
    //        OnPropertyChanged(nameof(PriceListDisplayName));
    //    }
    //    finally
    //    {
    //        UnlockControls();
    //        await UITools.HideLoadingPopup();
    //    }
    //}

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
                await Toast.Make("Primero cierre la búsqueda de productos.").Show();
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

    //protected override void OnDisappearing()
    //{
    //    base.OnDisappearing();        
    //}

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

        try
        {            
            bool saved_data = false;
            sale_order targetOrder = null;

            if (_existPromotionsApplied)
            {
                var leave = await DisplayAlertAsync("Eliminar promociones anteriores?", "Este pedido ya tiene promociones aplicadas", "Si", "No");
                //await DisplayAlert("Eliminar promociones aplicadas", "Este pedido ya tiene promociones aplicadas, se eliminarán las promociones anteriores.", "Continuar");
                if (leave)
                {
                    await CleanPromotionStatusFull(CurrentSaleOrder);
                    await Toast.Make("Promociones eliminadas.").Show();
                }
            }

            targetOrder = CurrentSaleOrder;
            
            _requiredSaveChanges = true;

            if (_requiredSaveChanges)
            {
                targetOrder = await SaveOrder();
            }
            
            if (targetOrder != null)
            {                
                saved_data = true;
                var applyPromo = await ApplyPromo(targetOrder);

                if (applyPromo!=null && applyPromo.Count > 0)
                {                    
                    targetOrder = await SaveOrder();
                }
                else
                {
                    await Navigation.PopModalAsync();
                }
            }                      
        }
        finally
        {
            _saving = false;
            UnlockControls();
        }
    }

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
            //saleOrderPromotions?.Clear();

            sale_order targetOrder = await SaveOrder();

            if (targetOrder != null)
            {
                //await CleanPromotionStatus(targetOrder);

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

    [Obsolete("Ya no se usa al parecer")]
    private async Task CleanPromotionStatus(sale_order saleOrder)
    {
        //saleOrderPromotions?.Clear();
        for(var i=0; i < saleOrderPromotions.Count(); i++)
        {
            var promo = saleOrderPromotions[i];
            if (promo.promotion_type_id != 2 && promo.promotion_selection_type_id != 2)
            {
                saleOrderPromotions.Remove(promo);
            }
        }

        for (int i = 0; i < OrderLines.Count; i++)
        {
            //automatico se elimina
            if (OrderLines[i].is_gift && !OrderLines[i].is_manual)
            {
                OrderLines.RemoveAt(i);
                continue;
            }

            //manual se mantiene
            if (OrderLines[i].is_gift && OrderLines[i].is_manual)
                continue;

            //Lineas con descuento se mantienen
            if (!OrderLines[i].is_gift && OrderLines[i].discount > 0)
                continue;

            var line = OrderLines[i];
            line.promotion_data = null;
            DMSA.Models.Odoo.Promotions.Tools.ClearPromotionData(line);

            var productDb = new ProductProductDb(App.Session.odooConnection.DbNameSqlite);
            var product_item = await productDb.GetItemAsync(x => x.id == line.product_id);

            if (product_item == null)
            {
                Debug.WriteLine("Error: no se encontró el producto para actualizar la línea de orden.");
                return;
            }

            product_item.list_price = (float)line.price_unit;
            UpdateOrderLine(line, product_item);
        }
    }

    private async Task CleanPromotionStatusFull(sale_order saleOrder)
    {
        //saleOrderPromotions?.Clear();
        //for (var i = 0; i < saleOrderPromotions.Count(); i++)
        //{
        //    var promo = saleOrderPromotions[i];
        //    saleOrderPromotions.Remove(promo);
        //}

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

    private async Task<sale_order> SaveOrder()
    {        
        var orderLines = OrderLines;
        var orderPromotions = saleOrderPromotions;
        var saleOrderDb = new SaleOrderDb(App.Session.odooConnection.DbNameSqlite);
        var saleOrderLineDb = new SaleOrderLineDb(App.Session.odooConnection.DbNameSqlite);
        
        sale_order targetOrder;

        bool isNew = CurrentSaleOrder == null;

        int warehouseId = 0;
        int partner_invoice_id = ((res_partner) ddfAddress.SelectedItem).id;

        StockWareHouseDb stockWareHouseDb = new StockWareHouseDb(App.Session.odooConnection.DbNameSqlite);
        var warehouseList = await stockWareHouseDb.GetDefaultByResCenter(App.Session.res_center.id);
        if (warehouseList != null && warehouseList.Count > 0)
        {
            warehouseId = warehouseList[0].id;
        }

        if (isNew)
        {
            string new_id_referencia = GenerarCodigo(CurrentPartner.name, await saleOrderDb.GetNextSecuentialId());

            targetOrder = new sale_order
            {
                _partner_id = _CurrentPartner.id,
                _company_id = CurrentCompany.id,
                date_order = DateTime.Now,
                mobile_create_date = DateTime.Now,
                _center_id = App.Session.res_center.id,
                _warehouse_id = warehouseId,
                sale_channel = App.Session.odooConnection.sale_channel_default,
                id_referencia = new_id_referencia,
                _pricelist_id = CurrentPriceList.id,
                amount_total = Total,
                amount_tax = Impuesto,
                amount_untaxed = Subtotal,
                state = "draft",
                partner_display_name = CurrentPartner?.name,
                partner_display_address = CurrentPartner?.street,
                partner_display_status = (CurrentPartner != null ? (CurrentPartner.active ? "Activo" : "Inactivo") : string.Empty),
                partner_sale_id = App.Session.CurrentUserFront.partner_id,
                _partner_invoice_id = partner_invoice_id,
                _partner_shipping_id = partner_invoice_id,
                note2 = Note2,
                external_create_uid = App.Session.CurrentUserFront.uid,                
                external_guid = Guid.NewGuid().ToString("N"),
                mobile_sync = true
            };

            if (await saleOrderDb.InsertAsync(targetOrder) <= 0)
            {
                await Toast.Make("Error al crear la orden").Show();
                return null;
            }

            CurrentSaleOrder = targetOrder;
            //CurrentSaleOrder = targetOrder;
        }
        else
        {
            targetOrder = CurrentSaleOrder;

            //if (targetOrder._partner_invoice_id != partner_invoice_id || targetOrder._partner_shipping_id != partner_invoice_id)
            //{
            //    await Toast.Make("Dirección modificada.").Show();
            //}

            targetOrder.write_date = DateTime.Now;
            targetOrder._center_id = App.Session.res_center.id;
            targetOrder._warehouse_id = warehouseId;
            targetOrder.sale_channel = App.Session.odooConnection.sale_channel_default;
            targetOrder._partner_invoice_id = partner_invoice_id;
            targetOrder._partner_shipping_id = partner_invoice_id;
            targetOrder._pricelist_id = CurrentPriceList.id;
            targetOrder.amount_total = Total;
            targetOrder.amount_tax = Impuesto;
            targetOrder.amount_untaxed = Subtotal;
            targetOrder.note2 = Note2;

            targetOrder.promotion_ids = orderPromotions?
                .Where(x => x != null)
                .Select(x => x.promotion_id)
                .Distinct()
                .ToArray()
                ?? Array.Empty<int>();

            if (await saleOrderDb.UpdateAsync(targetOrder) <= 0)
            {
                await Toast.Make("Error al actualizar la orden").Show();
                return null;
            }

            // Eliminar líneas anteriores antes de insertar las nuevas
            await saleOrderLineDb.DeleteItemOfParent(targetOrder);
            //await saleOrderPromoDb.DeleteItemOfParent(targetOrder);
            await ResetPromotions(targetOrder);
        }

        int ordinal = 1;
        
        targetOrder.order_line = new List<OrderLineWrapper>();

        // Asignar el ID de la orden a las líneas y guardar
        foreach (var orderLine in orderLines)
        {
            orderLine._order_id = targetOrder.id;
            orderLine.sequence = ordinal;
            await saleOrderLineDb.InsertAsync(orderLine);

            //Datos referenciales
            targetOrder.order_line.Add(new OrderLineWrapper(orderLine));
            
            ordinal++;
        }

        // Guardar promociones aplicadas
        //foreach (var orderPromo in orderPromotions)
        //{
        //    //orderPromo._sale_order_id = targetOrder.id;            
        //    await saleOrderPromoDb.InsertAsync(orderPromo);
        //}

        await SavePromotions(true, false);

        try
        {
            var mainPage = (MainPageTab) App.Current.MainPage;
            mainPage.SelectTab("Pedidos");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error al cambiar a la pestaña Pedidos: {ex.Message}");
        }

        await Toast.Make(isNew ? "Orden creada" : "Orden actualizada").Show();        
        return targetOrder;
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

    private async Task<List<string>> ApplyPromo(sale_order saleOrder)
    {
        List<string> resultData = new List<string>();

        await EvalPromotions(saleOrder);

        bool ShowPromoPopup = false;

        if(AppliedPromotionResults.Count == 0)
        {
            await Toast.Make("No hay promociones aplicables").Show();
            return new List<string>();
        }

        foreach(var promoResult in AppliedPromotionResults)
        {
            foreach(var promoResItem in promoResult.Items)
            {
                resultData.Add(promoResItem.Promotion.name);

                if (promoResItem.Promotion._promotion_type_id == 2) //REGALO
                {
                    ShowPromoPopup = true;
                    break;
                }

                if (promoResItem.Promotion._promotion_type_id == 4) // es NXN
                {
                    //await ApplyNxN(saleOrder, promoResItem);
                    //((CrudViewModel)BindingContext).UpdateTotals();
                    ShowPromoPopup = true;
                    break;
                }

                // ES DESCUENTO DEBE APLICARSE PRIMERO
                if (promoResItem.Promotion._promotion_type_id == 6)
                {
                    await ApplyDiscount(saleOrder, promoResItem);
                    UpdateTotals();

                    ShowPromoPopup = true;
                    break;
                }
            }
        }

        //No se muestra Popup si no hay elemento que elegir
        if(!ShowPromoPopup)
        {
            return new List<string>();
        }

        bool ShowPromoPopupLevel2 = false;

        var view = new PromocionesViewer(saleOrder);
        view.ItemsData = AppliedPromotionResults;
        view.OrderLines = OrderLines;
        view.saleOrderPromotions = saleOrderPromotions;
        //dawait view.AutoApplyPromotion();
        await view.ApplyPromosOnList();

        ShowPromoPopupLevel2 = view.BenefitsForShow;
        
        //Se guardan las promociones que no son manuales
        await SavePromotions(true, false);

        //if (!ShowPromoPopupLevel2) return new List<string>();

        var popup = new Popup
        {
            Content = view,
            BackgroundColor = Colors.Black.WithAlpha(0.4f), // fondo semi-transparente
            CanBeDismissedByTappingOutsideOfPopup = false,
            Padding = new Thickness(0),
            Margin = new Thickness(0)
        };

        view.ClosePopupAction = (promo) => PopupExtensions.ClosePopupAsync(Application.Current.Windows[0].Page, promo);

        var result = await PopupExtensions.ShowPopupAsync<PromoResultPopup>(App.Current.Windows[0].Page, popup, new PopupOptions
        {
            Shape = new RoundRectangle
            {
                CornerRadius = new CornerRadius(0),
                Stroke = Colors.Gray,
                StrokeThickness = 0.1,                
            },
            Shadow = new Shadow
            {
                Brush = Brush.Black,
                Offset = new Point(5, 5),
                Opacity = 0.5f,
                Radius = 0
            },
        });

        if (result.Result != null && result.Result is PromoResultPopup selected)
        {            
            if(selected.ActionResult == 1 || selected.ActionResult == 2) //Aplicar - Aplicar y continuar
            {
                var toRemove = OrderLines
                .Where(x => x.is_gift && x.is_manual)
                .ToList();

                foreach (var item in toRemove)
                {
                    OrderLines.Remove(item);
                }

                //Solo se hace el proceso para regalos manuales
                foreach (var giftLine in selected.manualGifts)
                {
                    giftLine._order_id = CurrentSaleOrder.id;
                    await new SaleOrderLineDb(App.Session.odooConnection.DbNameSqlite).InsertAsync(giftLine);
                    OrderLines.Add(giftLine);
                }

                var db = new SaleOrderLineDb(App.Session.odooConnection.DbNameSqlite);

                // índice para evitar búsquedas repetidas
                var lineIndex = OrderLines
                    .GroupBy(x => (x.product_id, x.sequence))
                    .ToDictionary(g => g.Key, g => g.First());

                var updates = new List<sale_order_line>();

                foreach (var benefit in selected.benefits)
                {
                    foreach (var promoItem in benefit.Items.Where(x =>
                        x.Promotion._promotion_type_id == 2 &&
                        x.Promotion._selection_type_id == 2))
                    {
                        foreach (var rule in promoItem.RuleSet)
                        {
                            foreach (var data in rule.ProductSequenceApplyList)
                            {
                                if (!lineIndex.TryGetValue((data.product_id, data.sequence), out var line))
                                    continue;

                                // promotion_ids (int[])
                                var promoList = line.promotion_ids?.ToList() ?? new List<int>();
                                if (!promoList.Contains(promoItem.Promotion.id))
                                    promoList.Add(promoItem.Promotion.id);
                                line.promotion_ids = promoList.ToArray();

                                // rule_ids (int[])
                                var ruleList = line.rule_ids?.ToList() ?? new List<int>();
                                if (!ruleList.Contains(rule.id))
                                    ruleList.Add(rule.id);
                                line.rule_ids = ruleList.ToArray();

                                updates.Add(line);
                            }
                        }
                    }
                }

                // ejecutar updates (puedes paralelizar si quieres)
                foreach (var line in updates.Distinct())
                {
                    await db.UpdateAsync(line);
                }

                //Almacenar ahora información de bonificados manuales
                await SavePromotions(false, true);
            }

            if(selected.ActionResult == 1 || selected.ActionResult == 0)
            {
                if(selected.ActionResult == 0)
                {
                    await Toast.Make("Promociones manuales no aplicadas.").Show();
                }

                await Navigation.PopModalAsync();
            }
        }

        return resultData;
    }

    private async Task ApplyDiscount(sale_order saleOrder, PromotionEvalItem promoResItem)
    {
        PromotionEngineRunner promotionEngineRunner = new PromotionEngineRunner();

        foreach (var ruleMatch in promoResItem.RuleSet)
        {
            if (ruleMatch.IsDiscount)
            {
                int maxProductTarget = ruleMatch.ProductTmplIdMaxTotal;
                if ( ruleMatch.variable == "qty_product_unts")
                {
                    maxProductTarget = ruleMatch.ProductTmplIdMaxQty;                    
                }

                int[] listIdsProd = JsonConvert.DeserializeObject<int[]>(ruleMatch.ProductTmplIds);
                                
                bool existedBefore = listIdsProd.Contains(maxProductTarget);
                                
                var cleanedList = listIdsProd.Where(id => id != maxProductTarget);
                                
                listIdsProd = (new int[] { maxProductTarget })
                                .Concat(cleanedList)
                                .ToArray();
                
                if (!existedBefore)
                {
                    Debug.WriteLine(
                        $"[Promotions] maxProductTarget ({maxProductTarget}) no existía en ProductTmplIds: {ruleMatch.ProductTmplIds}. Fue agregado manualmente."
                    );
                }

                foreach (var productTarget in listIdsProd)
                {
                    if (!await promotionEngineRunner.CanApplyPromotion(saleOrder, promoResItem, saleOrderPromotions))
                    {
                        Debug.WriteLine($"{promoResItem.Promotion.name} ya ha sido aplicado maximo de veces - Crud-ApplyDiscount");
                        return;
                    }

                    double discountPercentage = ruleMatch.discount;
                    int productTemplateId = ruleMatch.ProductTmplId;
                    var orderLines = saleOrder.order_line;

                    var productDb = new ProductProductDb(App.Session.odooConnection.DbNameSqlite);

                    //var productTarget = ruleMatch.ProductId; //await productDb.GetByProductTemplate(productTemplateId);

                    var lineToDiscount = orderLines
                            .Select(line => line.Count > 2 ? line[2] as sale_order_line : null)
                            .FirstOrDefault(l => l != null && l.product_tmpl_id == productTarget);

                    if (lineToDiscount != null)
                    {
                        List<PromotionEvalItem> listPromotionData = new List<PromotionEvalItem>();

                        listPromotionData = lineToDiscount.promotionDataList;
                        
                        //!string.IsNullOrEmpty(lineToDiscount.promotion_data) ?
                        //            Newtonsoft.Json.JsonConvert.DeserializeObject<List<PromotionEvalItem>>(lineToDiscount.promotion_data) :
                        //            new List<PromotionEvalItem>();

                        //existingPromos = Newtonsoft.Json.JsonConvert.DeserializeObject<List<PromotionEvalItemV2>>(lineToDiscount.promotion_data ?? "[]");
                        //sino existe promoResItem dentro de la lista

                        if (listPromotionData.Exists(p => p.Promotion.id == promoResItem.Promotion.id))
                        {
                            Debug.WriteLine($"Descuento de promoción ya ha sido aplicado anteriormente");
                            //continue;
                        }

                        listPromotionData.Add(promoResItem);

                        //if (lineToDiscount.promotion_data != Newtonsoft.Json.JsonConvert.SerializeObject(new List<PromotionEvalItemV2> { promoResItem }))
                        //{
                        if (lineToDiscount.discount == 0)
                        {
                            decimal originalPrice = lineToDiscount.price_unit;
                            decimal virtual_price_no_tax = lineToDiscount.virtual_price_no_tax;
                            decimal discountAmount = (virtual_price_no_tax * lineToDiscount.product_uom_qty_real) * (decimal)(discountPercentage / 100);
                            lineToDiscount.discount = (decimal)discountPercentage;
                            lineToDiscount.amount_discount = discountAmount;
                            lineToDiscount.price_subtotal = (virtual_price_no_tax * lineToDiscount.product_uom_qty_real) - discountAmount;
                            lineToDiscount.price_tax = (lineToDiscount.price_subtotal * lineToDiscount.virtual_iva_percentage) / 100;
                            lineToDiscount.price_total = lineToDiscount.price_subtotal + lineToDiscount.price_tax;
                            lineToDiscount.virtual_line_subtotal = virtual_price_no_tax * lineToDiscount.product_uom_qty_real;
                            lineToDiscount.promotion_data = Newtonsoft.Json.JsonConvert.SerializeObject(listPromotionData);
                            await promotionEngineRunner.AddApplyPromotion(saleOrder, promoResItem, 1, saleOrderPromotions);
                            DMSA.Models.Odoo.Promotions.Tools.SetPromotionData(lineToDiscount, 
                                new List<PromotionEvalItem> { promoResItem });

                            
                            lineToDiscount.origin_gift_line_ids_offline =
                                    Newtonsoft.Json.JsonConvert.SerializeObject(
                                        ruleMatch.ProductSequenceApplyList
                                    );

                            lineToDiscount.origin_gift_line_ids_offline =
                                    Newtonsoft.Json.JsonConvert.SerializeObject(
                                        listPromotionData
                                            .Where(x => x.RuleSet != null)
                                            .SelectMany(x => x.RuleSet)
                                            .Where(r => r.ProductSequenceApplyList != null)
                                            .SelectMany(r => r.ProductSequenceApplyList)
                                            .Distinct()
                                            .ToList()
                                    );
                        }

                        Debug.WriteLine($"Descuento aplicado: {discountPercentage}% al producto ID {productTemplateId}");                       

                        //}
                        //else
                        //{
                        //    Debug.WriteLine($"Descuento de promoción ya ha sido aplicado");
                        //}
                    }
                }
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
            await SaveOrder();
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
            await Navigation.PopModalAsync();
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
                //CurrentSaleOrderLine = null;
                //ProductEditing = null;
                //product_uom_qty_real = 0;
                //product_uom_qty = 0;
                //OrderLinesCl.SelectedItem = null;

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
                //Aqui buscamos las promociones relacionadas y se le cambia la cantidad
                PromotionEngineRunner promotionEngineRunner = new PromotionEngineRunner();
                await promotionEngineRunner.ResetManualGiftBenefitSoft(CurrentSaleOrder, saleOrderPromotions);

                UpdateOrderLine(CurrentSaleOrderLine, product_item);
            }
            
            CurrentSaleOrderLine = null;
            ProductEditing = null;
            product_uom_qty_real = 0;
            product_uom_qty = 0;

            //((CrudViewModel)BindingContext).UpdateTotals();
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