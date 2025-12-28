
using CommunityToolkit.Maui;
using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Extensions;
using CommunityToolkit.Maui.Views;
using DMOrders.Controls.Tools;
using DMOrders.Pages.Fragments.Orders.modals;
using DMOrders.Services.Promotions;
using DMOrders.Shared;
using DMSA.Models.Odoo.Abstract;
using DMSA.Models.Odoo.DMOrders.promotions;
//using DMSA.Models.Odoo.DMOrders.promotions.@abstract;
using DMSA.Models.Odoo.DMOrders.promotions.abstractCustom;
using DMSA.Models.Odoo.Native;
using DMSA.Models.Odoo.Sales;
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

    public string PartnerDisplayAddress =>
        CurrentSaleOrder?.partner_display_address
        ?? CurrentPartner?.street
        ?? string.Empty;

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

    private ObservableCollection<PromotionEvalResultV2> AppliedPromotionResults;
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
        bool RequiredPreloadData = false;

        ResPartnerDb resPartnerDb = new ResPartnerDb(App.Session.odooConnection.DbNameSqlite);

        if (CurrentPartner != null)
        {
            //DESDE LISTA DE CLIENTES PARA AGREGAR NUEVA ORDEN
            Title = CurrentPartner.name;
            if (CurrentSaleOrder == null)
            {
                Title += " [*]";
                btnSend.IsVisible = false;

                //Se muestra por defecto la búsqueda de productos
                SearchProductView.IsVisible = true;
            }
            else
            {
                Title += " []";
                btnSend.IsVisible = true;
            }

            ((CrudViewModel)this.BindingContext)._CurrentPartner = CurrentPartner;
            ((CrudViewModel)this.BindingContext).CurrentCompany = CurrentCompany;
            ((CrudViewModel)this.BindingContext).CurrentSaleOrder = CurrentSaleOrder;

            await ((CrudViewModel)this.BindingContext).LoadData();

            saleOrderPromotions = ((CrudViewModel)this.BindingContext).saleOrderPromotions;
        }
        else
        {

            CurrentPartner = await resPartnerDb.GetItemsAsync(CurrentCompany.id, CurrentSaleOrder._partner_id);
            if (CurrentPartner != null)
            {
                Title = CurrentPartner.name;
            }

            if (CurrentSaleOrder != null)
            {
                Title += " [edición]";
                RequiredPreloadData = true;
            }

            ((CrudViewModel)this.BindingContext).CurrentCompany = CurrentCompany;
            ((CrudViewModel)this.BindingContext).CurrentSaleOrder = CurrentSaleOrder;

            await ((CrudViewModel)this.BindingContext).LoadData();

            saleOrderPromotions = ((CrudViewModel)this.BindingContext).saleOrderPromotions;

            LockEdition = CurrentSaleOrder.is_synchronized;
            OnPropertyChanged(nameof(LockEdition));
        }

        var PriceListDb = new ProductPricelistDb(App.Session.odooConnection.DbNameSqlite);
        CurrentPriceList = await PriceListDb.GetItem(CurrentPartner._product_pricelist_id);

        if (CurrentPriceList == null || CurrentPartner._product_pricelist_id == 0)
        {
            await DisplayAlert("Alerta", "El cliente no tiene lista de precio asignada, no se puede continuar", "Aceptar");
            await Navigation.PopModalAsync();
        }

        PartnerAddress = new List<res_partner>();
        PartnerAddress = await resPartnerDb.GetItemsAsync(x => x._parent_id == CurrentPartner.id);
        if (PartnerAddress != null && PartnerAddress.Count > 0)
        {
            //PartnerAddress.Add(CurrentPartner);            
        }
        else
        {
            PartnerAddress = new List<res_partner>();
        }

        PartnerAddress.Add(CurrentPartner);
        ddfAddress.ItemsSource = PartnerAddress;
        ddfAddress.ItemDisplayBinding = new Binding("display_full_address");
        ddfAddress.SelectedItem = PartnerAddress[0];

        if (RequiredPreloadData)
        {
            for(var i=0; i< PartnerAddress.Count ; i++)
            {
                if(PartnerAddress[i].id == CurrentSaleOrder._partner_invoice_id)
                {
                    ddfAddress.SelectedItem = PartnerAddress[i];
                    break;
                }
            }
        }

        ((CrudViewModel)this.BindingContext).CurrentPriceList = CurrentPriceList;
        SearchProductView.CurrentPriceList = CurrentPriceList;
        OnPropertyChanged(nameof(PriceListDisplayName));
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
            if(LockEdition)
            {
                await Navigation.PopModalAsync();
            }

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
        sale_order targetOrder = await SaveOrder();

        if (targetOrder != null)
        {
            await CleanPromotionStatusV2(targetOrder);

            var applyPromo = await ApplyPromo(targetOrder);

            if (applyPromo.Count > 0)
            {
                //Se guarda después de la aplicación de promociones
                targetOrder = await SaveOrder();
            }
        }

        //await Navigation.PopAsync();
        await Navigation.PopModalAsync();
        //SendBackButtonPressed();
    }

    private async void ButtonPromo_Clicked(object sender, EventArgs e)
    {
        var leave = await DisplayAlert("Atención", "Se guardarán los cambios antes de aplicar las promociones. ¿Desea continuar?", "Si", "No");

        if (!leave)
        {
            return;
        }
                
        //saleOrderPromotions?.Clear();

        sale_order targetOrder = await SaveOrder();

        if (targetOrder != null)
        {
            await CleanPromotionStatusV2(targetOrder);

            var applyPromo = await ApplyPromo(targetOrder);

            if (applyPromo.Count > 0)
            {

            }
        }
    }

    private async Task CleanPromotionStatusV2(sale_order saleOrder)
    {
        //saleOrderPromotions?.Clear();
        for(var i=0; i < saleOrderPromotions.Count(); i++)
        {
            var promo = saleOrderPromotions[i];
            //Se excluyen de la elmininacion los bonificados/manuales
            // - deben mantenerse en memoria
            if (promo.promotion_type_id != 2 && promo.promotion_selection_type_id != 2)
            {
                saleOrderPromotions.Remove(promo);
            }
        }

        //var view = new PromocionesViewer(saleOrder);
        //view.ItemsData = AppliedPromotionResults;
        var OrderLines = ((CrudViewModel)this.BindingContext).OrderLines;
        //var saleOrderPromotions = ((CrudViewModel)this.BindingContext).saleOrderPromotions;

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
            ((CrudViewModel)BindingContext).UpdateOrderLine(line, product_item);
        }
    }
    
    public static string ObtenerIniciales(string nombreCompleto)
    {
        var partes = nombreCompleto
            .Trim()
            .Split(' ', StringSplitOptions.RemoveEmptyEntries);

        if (partes.Length == 0)
            return "";

        string inicialApellido = partes[0][0].ToString();
        string inicialNombre = partes.Length > 2 ? partes[2][0].ToString() : partes[1][0].ToString();

        return (inicialApellido + inicialNombre).ToUpper();
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
        var viewModel = (CrudViewModel)this.BindingContext;
        var orderLines = viewModel.OrderLines;
        var orderPromotions = viewModel.saleOrderPromotions;
        var saleOrderDb = new SaleOrderDb(App.Session.odooConnection.DbNameSqlite);
        var saleOrderLineDb = new SaleOrderLineDb(App.Session.odooConnection.DbNameSqlite);
        var saleOrderPromoDb = new SaleOrderPromotionsDb(App.Session.odooConnection.DbNameSqlite);

        sale_order targetOrder;

        bool isNew = CurrentSaleOrder == null;

        int warehouseId = 0;
        int partner_invoice_id = ((res_partner)ddfAddress.SelectedItem).id;

        StockWareHouseDb stockWareHouseDb = new StockWareHouseDb(App.Session.odooConnection.DbNameSqlite);
        var warehouseList = await stockWareHouseDb.GetByResCenter(App.Session.res_center.id);
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
                _center_id = App.Session.res_center.id,
                _warehouse_id = warehouseId,
                sale_channel = App.Session.odooConnection.sale_channel_default,
                id_referencia = new_id_referencia,
                _pricelist_id = CurrentPriceList.id,
                amount_total = viewModel.Total,
                amount_tax = viewModel.Impuesto,
                amount_untaxed = viewModel.Subtotal,
                state = "draft",
                partner_display_name = CurrentPartner?.name,
                partner_display_address = CurrentPartner?.street,
                partner_display_status = (CurrentPartner != null ? (CurrentPartner.active ? "Activo" : "Inactivo") : string.Empty),
                partner_sale_id = App.Session.CurrentUserFront.partner_id,
                _partner_invoice_id = partner_invoice_id,
                note2 = viewModel.Note2
            };

            if (await saleOrderDb.InsertAsync(targetOrder) <= 0)
            {
                await Toast.Make("Error al crear la orden").Show();
                return null;
            }

            CurrentSaleOrder = targetOrder;
            viewModel.CurrentSaleOrder = targetOrder;
        }
        else
        {
            targetOrder = CurrentSaleOrder;
            targetOrder.write_date = DateTime.Now;
            targetOrder._center_id = App.Session.res_center.id;
            targetOrder._warehouse_id = warehouseId;
            targetOrder.sale_channel = App.Session.odooConnection.sale_channel_default;
            targetOrder._partner_invoice_id = partner_invoice_id;            
            targetOrder._pricelist_id = CurrentPriceList.id;
            targetOrder.amount_total = viewModel.Total;
            targetOrder.amount_tax = viewModel.Impuesto;
            targetOrder.amount_untaxed = viewModel.Subtotal;
            targetOrder.note2 = viewModel.Note2;

            if (await saleOrderDb.UpdateAsync(targetOrder) <= 0)
            {
                await Toast.Make("Error al actualizar la orden").Show();
                return null;
            }

            // Eliminar líneas anteriores antes de insertar las nuevas
            await saleOrderLineDb.DeleteItemOfParent(targetOrder);
            await saleOrderPromoDb.DeleteItemOfParent(targetOrder);
        }

        int ordinal = 1;
        
        targetOrder.order_line = new List<OrderLineWrapper>();

        // Asignar el ID de la orden a las líneas y guardar
        foreach (var orderLine in orderLines)
        {
            orderLine._order_id = targetOrder.id;
            orderLine.ordinal = ordinal;
            await saleOrderLineDb.InsertAsync(orderLine);

            //Datos referenciales
            targetOrder.order_line.Add(new OrderLineWrapper(orderLine));
            
            ordinal++;
        }

        // Guardar promociones aplicadas
        foreach (var orderPromo in orderPromotions)
        {
            //orderPromo._sale_order_id = targetOrder.id;            
            await saleOrderPromoDb.InsertAsync(orderPromo);
        }

        try
        {
            var mainPage = (MainPageTab)App.Current.MainPage;
            mainPage.SelectTab("Pedidos");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error al cambiar a la pestaña Pedidos: {ex.Message}");
        }

        await Toast.Make(isNew ? "Orden creada" : "Orden actualizada").Show();        
        return targetOrder;
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

                // es DESCUENTO DEBE APLICARSE PRIMERO
                if (promoResItem.Promotion._promotion_type_id == 6)
                {
                    await ApplyDiscountV2(saleOrder, promoResItem);
                    ((CrudViewModel)BindingContext).UpdateTotals();
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
        view.OrderLines = ((CrudViewModel)this.BindingContext).OrderLines;
        view.saleOrderPromotions = ((CrudViewModel)this.BindingContext).saleOrderPromotions;
        await view.ApplyPromosOnList();

        ShowPromoPopupLevel2 = view.BenefitsForShow;

        if (!ShowPromoPopupLevel2) return new List<string>();

        var popup = new Popup
        {
            Content = view,
            BackgroundColor = Colors.Black.WithAlpha(0.4f), // fondo semi-transparente
            CanBeDismissedByTappingOutsideOfPopup = false,
            Padding = new Thickness(0),
            Margin = new Thickness(0)
        };

        view.ClosePopupAction = (promo) => PopupExtensions.ClosePopupAsync(Application.Current.Windows[0].Page, promo);

        var result = await PopupExtensions.ShowPopupAsync<List<PromotionBenefit>>(App.Current.Windows[0].Page, popup, new PopupOptions
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

        if (result is List<PromotionBenefit> selected)
        {
            // Usar la promoción seleccionada
            Debug.WriteLine(selected);
        }

        if (result.Result != null)
        {
            
        }

        return resultData;
    }

    private async Task ApplyDiscountV2(sale_order saleOrder, PromotionEvalItemV2 promoResItem)
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

                    double discountPercentage = ruleMatch.Discount;
                    int productTemplateId = ruleMatch.ProductTmplId;
                    var orderLines = saleOrder.order_line;

                    var productDb = new ProductProductDb(App.Session.odooConnection.DbNameSqlite);

                    //var productTarget = ruleMatch.ProductId; //await productDb.GetByProductTemplate(productTemplateId);

                    var lineToDiscount = orderLines
                            .Select(line => line.Count > 2 ? line[2] as sale_order_line : null)
                            .FirstOrDefault(l => l != null && l.product_tmpl_id == productTarget);

                    if (lineToDiscount != null)
                    {
                        List<PromotionEvalItemV2> listPromotionData = new List<PromotionEvalItemV2>();

                        listPromotionData = !string.IsNullOrEmpty(lineToDiscount.promotion_data) ?
                                    Newtonsoft.Json.JsonConvert.DeserializeObject<List<PromotionEvalItemV2>>(lineToDiscount.promotion_data) :
                                    new List<PromotionEvalItemV2>();

                        //existingPromos = Newtonsoft.Json.JsonConvert.DeserializeObject<List<PromotionEvalItemV2>>(lineToDiscount.promotion_data ?? "[]");
                        //sino existe promoResItem dentro de la lista

                        if (listPromotionData.Exists(p => p.Promotion.id == promoResItem.Promotion.id))
                        {
                            Debug.WriteLine($"Descuento de promoción ya ha sido aplicado anteriormente");
                            continue;
                        }

                        listPromotionData.Add(promoResItem);

                        //if (lineToDiscount.promotion_data != Newtonsoft.Json.JsonConvert.SerializeObject(new List<PromotionEvalItemV2> { promoResItem }))
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

                            Debug.WriteLine($"Descuento aplicado: {discountPercentage}% al producto ID {productTemplateId}");
                        }
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

            AppliedPromotionResults ??= new ObservableCollection<PromotionEvalResultV2>();
            AppliedPromotionResults.Clear();

            string dbNameSqlite = App.Session.odooConnection.DbNameSqlite;


            var repo = new PromotionRepository();            
            // 2️⃣ Crear el motor de promociones
            var engine = new PromotionEngineLite(repo);

            //AppliedPromotionResults = await engine.EvaluatePromotionsV2(
            //    saleOrder: saleOrder
            //);

            AppliedPromotionResults = await engine.EvaluatePromotionsV3(
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
        var leave = await DisplayAlert("Enviar", "¿Desea enviar esta orden al ERP? Los cambios realizados serán almacenados.", "Si", "No");

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

        ServerPusher serverPusher = new ServerPusher();

        var orderLinesList = ((CrudViewModel)this.BindingContext).OrderLines.ToList();
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
            await DisplayAlert("Envío de datos", "Envío correcto", "Aceptar");
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
        ((CrudViewModel)this.BindingContext).RemoveOrderLine((sale_order_line)obj); 
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
        ProductEditing.uom_display = SaleOrderLine.uom_category_display;

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

    private async void ApplyValueChanges(object sender, EventArgs e)
    {
        if (CurrentSaleOrderLine != null)
        {
            if (product_uom_qty == 0 || product_uom_qty_real == 0)
            {   
                await DisplayAlert("Alerta", "La cantidad no puede ser 0.", "Aceptar");
                return;
            }

            if (product_uom_qty > product_uom_qty_real)
            {
                //CurrentSaleOrderLine = null;
                //ProductEditing = null;
                //product_uom_qty_real = 0;
                //product_uom_qty = 0;
                //OrderLinesCl.SelectedItem = null;

                await DisplayAlert("Alerta", "La cantidad solicitada no puede ser mayor a la cantidad real.", "Aceptar");
                return;
            }

            if ((decimal)ProductEditing.cantidad_disponible < product_uom_qty)
            {
                CurrentSaleOrderLine = null;
                ProductEditing = null;
                product_uom_qty_real = 0;
                product_uom_qty = 0;                
                OrderLinesCl.SelectedItem = null;

                await DisplayAlert("Alerta", "La cantidad solicitada no puede ser mayor a la disponible en inventario.", "Aceptar");
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

            //////////////////////////////
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
                ((CrudViewModel)BindingContext).UpdateOrderLineRefresh(CurrentSaleOrderLine, product_item);
            }
            else
            {
                //Aqui buscamos las promociones relacionadas y se le cambia la cantidad
                PromotionEngineRunner promotionEngineRunner = new PromotionEngineRunner();
                await promotionEngineRunner.ResetManualGiftBenefitSoft(CurrentSaleOrder, saleOrderPromotions);

                ((CrudViewModel)BindingContext).UpdateOrderLine(CurrentSaleOrderLine, product_item);
            }
            
            //////////////////////////////

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
}