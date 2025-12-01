
using CommunityToolkit.Maui;
using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Extensions;
using CommunityToolkit.Maui.Views;
using DMOrders.Controls.Tools;
using DMOrders.Pages.Fragments.Orders.modals;
using DMOrders.Services.Database.Sqlite;
using DMOrders.Services.Promotions;
using DMOrders.Services.Update.Pusher;
using DMOrders.Shared;
using DMSA.Models.Odoo.Abstract;
using DMSA.Models.Odoo.DMOrders.promotions;
//using DMSA.Models.Odoo.DMOrders.promotions.@abstract;
using DMSA.Models.Odoo.DMOrders.promotions.abstractCustom;
using DMSA.Models.Odoo.Native;
using DMSA.Models.Odoo.Sales;
using Microsoft.Maui.Controls.Shapes;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace DMOrders.Pages.Fragments.Orders;

[XamlCompilation(XamlCompilationOptions.Compile)]
public partial class Crud : ContentPage, IBackButtonHandler
{    
    private Entry _activeEntry;
    public res_company CurrentCompany { get; set; }
    public res_partner _CurrentPartner { get; set; }
    public sale_order _CurrentSaleOrder { get; set; }
    public product_pricelist CurrentPriceList { get; set; }
    public ICommand EditCommand { get; set; }
    public ICommand DeleteCommand { get; set; }
    public product_product _ProductEditing { get; set; }    
    public List<SaleOrderPromotions> saleOrderPromotions { get; set; }
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
        if( CurrentPartner != null )
        {
            //DESDE LISTA DE CLIENTES PARA AGREGAR NUEVA ORDEN
            Title = CurrentPartner.name;
            if(CurrentSaleOrder == null)
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
            ResPartnerDb resPartnerDb = new ResPartnerDb(App.Session.odooConnection.DbNameSqlite);
            CurrentPartner = await resPartnerDb.GetItemsAsync(CurrentCompany.id , CurrentSaleOrder._partner_id);
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

            saleOrderPromotions = ((CrudViewModel)this.BindingContext).saleOrderPromotions;
        }

        var PriceListDb = new ProductPricelistDb(App.Session.odooConnection.DbNameSqlite);
        CurrentPriceList = await PriceListDb.GetItem(CurrentPartner._product_pricelist_id);

        if (CurrentPriceList == null || CurrentPartner._product_pricelist_id == 0)
        {
            await DisplayAlert("Alerta", "El cliente no tiene lista de precio asignada, no se puede continuar", "Aceptar");
            await Navigation.PopModalAsync();
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
            //var applyPromo = await ApplyPromo(targetOrder);

            //if (applyPromo.Count > 0)
            //{

            //}
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
            await CleanPromotionStatus(targetOrder);

            var applyPromo = await ApplyPromo(targetOrder);

            if (applyPromo.Count > 0)
            {

            }
        }
    }

    private async Task CleanPromotionStatus(sale_order saleOrder)
    {
        saleOrderPromotions?.Clear();

        //var view = new PromocionesViewer(saleOrder);
        //view.ItemsData = AppliedPromotionResults;
        var OrderLines = ((CrudViewModel)this.BindingContext).OrderLines;
        //var saleOrderPromotions = ((CrudViewModel)this.BindingContext).saleOrderPromotions;

        for (int i = 0; i < OrderLines.Count; i++)
        {
            if(OrderLines[i].is_gift )
            {
                OrderLines.RemoveAt(i);
                continue;
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

        StockWareHouseDb stockWareHouseDb = new StockWareHouseDb(App.Session.odooConnection.DbNameSqlite);
        var warehouseList = await stockWareHouseDb.GetByResCenter(App.Session.res_center.id);
        if (warehouseList != null && warehouseList.Count > 0)
        {
            warehouseId = warehouseList[0].id;
        }

        if (isNew)
        {
            targetOrder = new sale_order
            {
                _partner_id = _CurrentPartner.id,
                _company_id = CurrentCompany.id,
                date_order = DateTime.Now,
                _center_id = App.Session.res_center.id,
                _warehouse_id = warehouseId,
                sale_channel = App.Session.odooConnection.sale_channel_default,
                id_referencia = "M001-RC29102025",
                _pricelist_id = CurrentPriceList.id,
                amount_total = viewModel.Total,
                amount_tax = viewModel.Impuesto,
                amount_untaxed = viewModel.Subtotal,
                state = "draft"
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
            targetOrder.id_referencia = "M001-RC29102025";
            targetOrder._pricelist_id = CurrentPriceList.id;
            targetOrder.amount_total = viewModel.Total;
            targetOrder.amount_tax = viewModel.Impuesto;
            targetOrder.amount_untaxed = viewModel.Subtotal;

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

        await Toast.Make(isNew ? "Orden creada" : "Orden actualizada").Show();        
        return targetOrder;
    }

    private async Task<List<string>> ApplyPromo(sale_order saleOrder)
    {
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
                if(promoResItem.Promotion._promotion_type_id == 2) //REGALO
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
                    await ApplyDiscount(saleOrder, promoResItem);
                    ((CrudViewModel)BindingContext).UpdateTotals();
                }
            }
        }

        //No se muestra Popup si no hay elemento que elegir
        if(!ShowPromoPopup)
        {
            return new List<string>();
        }

        var view = new PromocionesViewer(saleOrder);
        view.ItemsData = AppliedPromotionResults;
        view.OrderLines = ((CrudViewModel)this.BindingContext).OrderLines;
        view.saleOrderPromotions = ((CrudViewModel)this.BindingContext).saleOrderPromotions;

        var popup = new Popup
        {
            Content = view,
            BackgroundColor = Colors.Black.WithAlpha(0.4f), // fondo semi-transparente
            CanBeDismissedByTappingOutsideOfPopup = true,
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

        return new List<string>();
    }

    private async Task ApplyNxN(sale_order saleOrder, PromotionEvalItemV2 promoResItem)
    {
        throw new NotImplementedException();
    }

    private async Task ApplyDiscount(sale_order saleOrder, PromotionEvalItemV2 promoResItem)
    {
        PromotionEngineRunner promotionEngineRunner = new PromotionEngineRunner();

        
        foreach(var rule in promoResItem.RuleSet)
        {
            if(rule.IsDiscount)
            {
                if (!await promotionEngineRunner.CanApplyPromotion(saleOrder, promoResItem, saleOrderPromotions))
                {
                    Debug.WriteLine($"{promoResItem.Promotion.name} ya ha sido aplicado maximo de veces - Crud-ApplyDiscount");
                    return;
                }

                double discountPercentage = rule.Discount;
                int productTemplateId = rule.ProductTmplId;
                var orderLines = saleOrder.order_line;

                var productDb = new ProductProductDb(App.Session.odooConnection.DbNameSqlite);

                var productTarget = rule.ProductId; //await productDb.GetByProductTemplate(productTemplateId);

                var lineToDiscount = orderLines
                        .Select(line => line.Count > 2 ? line[2] as sale_order_line : null)
                        .FirstOrDefault(l => l != null && l.product_id == productTarget);

                if (lineToDiscount != null)
                {
                    List<PromotionEvalItemV2> listPromotionData = new List<PromotionEvalItemV2>();

                    listPromotionData = !string.IsNullOrEmpty(lineToDiscount.promotion_data) ?
                                Newtonsoft.Json.JsonConvert.DeserializeObject<List<PromotionEvalItemV2>>(lineToDiscount.promotion_data) :
                                new List<PromotionEvalItemV2>();

                    //existingPromos = Newtonsoft.Json.JsonConvert.DeserializeObject<List<PromotionEvalItemV2>>(lineToDiscount.promotion_data ?? "[]");
                    //sino existe promoResItem dentro de la lista

                    if(listPromotionData.Exists(p => p.Promotion.id == promoResItem.Promotion.id))
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

            AppliedPromotionResults = await engine.EvaluatePromotionsV2(
                saleOrder: saleOrder
            );

            //var databaseLines = new SaleOrderLineDb(dbNameSqlite);
            //var _order_lines = await databaseLines.GetItemsByParent(saleOrder);

            //var productDb = new ProductProductDb(dbNameSqlite);

            // 3️⃣ Iterar productos de la orden
            //foreach (var line in _order_lines)
            //{
            //    var product_tmpl_id = line.product_tmpl_id;

            //    if (product_tmpl_id == 0)
            //    {                    
            //        var product_template_id = await productDb.GetItem(line.product_id);
            //        product_tmpl_id = product_template_id._product_tmpl_id;
            //    }

            //    var qty = (int)line.product_uom_qty;
            //    var partner = saleOrder._partner_id;
            //    var company_id = saleOrder._company_id;
            //    totalProductAmount = line.price_total;

            //    // 4️⃣ Evaluar promociones
            //    var result = await engine.EvaluatePromotions(
            //        product_tmpl_id: product_tmpl_id,
            //        orderLine: line,
            //        qty: qty,
            //        totalProductAmount: totalProductAmount,
            //        totalOrder: totalOrder,
            //        companyId: company_id,
            //        pricelist_id: CurrentPriceList.id
            //    );

            //    if (result.Best != null)
            //    {
            //        //Debug.WriteLine($"Promo aplicada: {result.Best.Promotion.Name} ({result.Best.Discount}%) al producto {product.name}");
            //        // Opcional: agregar a tu lista de promociones aplicadas
            //        var benefit = (await repo.Search(company_id, DateTime.UtcNow))
            //                          .FirstOrDefault(p => p.id == result.Best.Promotion.id);

            //        if (benefit != null)
            //            AppliedPromotionResults.Add(result);

            //        Debug.WriteLine("Aplicar la promocion automatica");
            //        foreach(var item in result.Items)
            //        {
            //            //Tipo automatico + bonificado
            //            if (item.Promotion._selection_type_id == 1 && item.Promotion._promotion_type_id == 2)
            //            {
            //                Debug.WriteLine(item.ProductTmplId);
            //            }
            //        }                       
            //    }
            //}

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
            _activeEntry = EntryCantidadFinal;
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
            if ((decimal)ProductEditing.qty_available < product_uom_qty)
            {
                CurrentSaleOrderLine = null;
                ProductEditing = null;
                product_uom_qty_real = 0;
                product_uom_qty = 0;                
                OrderLinesCl.SelectedItem = null;

                await DisplayAlert("Alerta", "La cantidad solicitada no puede ser mayor a la disponible en inventario.", "Aceptar");
                return;
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
            ((CrudViewModel)BindingContext).UpdateOrderLine(CurrentSaleOrderLine, product_item);
            //////////////////////////////

            CurrentSaleOrderLine = null;
            ProductEditing = null;
            product_uom_qty_real = 0;
            product_uom_qty = 0;

            //((CrudViewModel)BindingContext).UpdateTotals();
            OrderLinesCl.SelectedItem = null;
        }
    }
}