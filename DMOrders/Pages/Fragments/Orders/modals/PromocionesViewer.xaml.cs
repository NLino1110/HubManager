using CommunityToolkit.Maui.Alerts;
using DMOrders.Services.Database.Sqlite;
using DMOrders.Services.Promotions;
using DMSA.Models.Odoo.DMOrders.promotions;
using DMSA.Models.Odoo.DMOrders.promotions.abstractCustom;
using DMSA.Models.Odoo.Native;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;

namespace DMOrders.Pages.Fragments.Orders.modals;

public partial class PromocionesViewer : ContentView, INotifyPropertyChanged
{
    public List<SaleOrderPromotions> saleOrderPromotions { get; set; }

    public ObservableCollection<product_product> promoGiftsAuto
    {
        get => _promoGiftsAuto;
        set
        {
            _promoGiftsAuto = value;
            OnPropertyChanged(nameof(promoGiftsAuto));
        }
    }

    private ObservableCollection<product_product> _promoGiftsAuto;

    private sale_order SaleOrder { get; set; }
    private PromotionEvalItemV2 selectedPromoEvalItem { get; set; }
    public ObservableCollection<PromotionEvalResultV2> ItemsData
    {
        get => _itemsFullPromos;
        set
        {
            _itemsFullPromos = value;

            if (_ItemsDataBenefits != null)
            {
                _ItemsDataBenefits.Clear();
                _promoGiftsAuto.Clear();
            }
            else
            {
                _ItemsDataBenefits = new ObservableCollection<PromotionEvalItemV2>();
                _promoGiftsAuto = new ObservableCollection<product_product>();
                //_ItemsDataBenefits = new ObservableCollection<PromotionBenefit>();
            }

            foreach (var promo in _itemsFullPromos)
            {
                if (promo.Items != null)
                {
                    foreach (var benefit in promo.Items)
                    {
                        //Se agregan los beneficios automáticos para cargar sus regalos si es que es tipo Bonificado == 2
                        if (benefit.Promotion._promotion_type_id == 2 && benefit.Promotion._selection_type_id == 1)
                        {
                            _ = AddAutoGiftsAsync(benefit);
                        }

                        //NxN se aplica automáticamente
                        if (benefit.Promotion._promotion_type_id == 4 && benefit.Promotion._selection_type_id == 1)
                        {
                            _ = AddAutoGiftsAsync(benefit);
                        }

                        if (benefit?.Promotion?.id == null)
                            continue;

                        _ItemsDataBenefits.Add(benefit);

                        if (!_ItemsDataBenefits.Any(x => x.Promotion?.id == benefit.Promotion.id))
                        {
                            //Quizas se deban acumular los qty * numero de apariciones de la promoción                            
                            benefit.FoundTimesApplies = 1;
                            
                            foreach(var rule in benefit.RuleSet)
                            {                                                                   
                                benefit.MaxAllowedGifts += rule.AllowedGifts;                                
                            }

                            //Original
                            //benefit.MaxAllowedGifts = benefit.AllowedGifts;

                            //_ItemsDataBenefits.Add(benefit);
                        }
                        else
                        {
                            var existing = _ItemsDataBenefits.First(x => x.Promotion?.id == benefit.Promotion.id);

                            // Acumulas el qty
                            //existing.RuleSet.qty += benefit.RuleSet.qty;

                            // Si TotalTimesAllowed también debe acumularse:
                            var totalTimesFound = existing.FoundTimesApplies + 1;
                            if(totalTimesFound > existing.TotalTimesAllowed)
                            {
                                //No se puede acumular más veces de las permitidas
                                Debug.WriteLine($"Promoción {existing.Promotion.name} ya ha alcanzado el máximo de aplicaciones permitidas.");
                                continue;
                            }

                            existing.FoundTimesApplies = totalTimesFound;

                            //TODO: AQUI SUCEDE ALGO CRITICO
                            foreach (var ruleMatch in existing.RuleSet)
                            {
                                //existing.MaxAllowedGifts += existing.FoundTimesApplies * ruleMatch.AllowedGifts;
                            }

                            //foreach (var rule in existing.RuleSet)
                            //{
                            //    existing.MaxAllowedGifts += rule.AllowedGifts;
                            //}
                        }
                    }
                }
            }

            OnPropertyChanged(nameof(ItemsData));
            OnPropertyChanged(nameof(ItemsDataBenefits));
        }
    }

    private ObservableCollection<PromotionEvalResultV2> _itemsFullPromos;

    public ObservableCollection<PromotionEvalItemV2> ItemsDataBenefits
    {
        get => _ItemsDataBenefits;
        set
        {
            _ItemsDataBenefits = value;
            OnPropertyChanged(nameof(ItemsDataBenefits));
        }
    }

    private ObservableCollection<PromotionEvalItemV2> _ItemsDataBenefits;

    public ObservableCollection<product_product> promoGifts
    {
        get => _promoGifts;
        set
        {
            _promoGifts = value;
            OnPropertyChanged(nameof(promoGifts));
        }
    }

    private ObservableCollection<product_product> _promoGifts;

    private bool _isLoading;
    public bool IsLoading
    {
        get => _isLoading;
        set
        {
            _isLoading = value;
            OnPropertyChanged(nameof(IsLoading));
            Debug.WriteLine("_isLoading");
            Debug.WriteLine(_isLoading);
        }
    }

    public int PromosCount
    {
        get => ItemsData?.Count ?? 0;
    }

    public PromocionesViewer(sale_order SaleOrderParam)
	{
		InitializeComponent();
        BindingContext = this;
        SaleOrder = SaleOrderParam;
        //LoadDataByTimer();
    }

    private async Task AddAutoGiftsAsync(PromotionEvalItemV2 benefit)
    {
        PromotionEngineRunner promotionEngineRunner = new PromotionEngineRunner();

        var productDb = new ProductProductDb(App.Session.odooConnection.DbNameSqlite);

        foreach (var ruleMatch in benefit.RuleSet)
        {
            int productIdCompare = ruleMatch.ProductId;

            //if (benefit.Promotion._selection_type_id == 1)
            if (productIdCompare == 0)
                productIdCompare = ruleMatch.ProductTmplId;

            if (benefit.Promotion._selection_type_id == 1)
            {
                //Aqui debe ser solo un producto
                var listDetailProd = benefit.Promotion._product_details_promotion_ids
                                    .Where(x => x._promo_id == 0 && x._bonus_id > 0 && x._bonus_id == ruleMatch.id).ToList();

                //var productGift = await productDb.GetByProductsTemplate(listIdsProd.ToArray());
                                
                if (listDetailProd != null && listDetailProd.Count > 0)
                {
                    productIdCompare = listDetailProd[0]._product_id;

                    if (listDetailProd.Count > 1)
                    {
                        Debug.WriteLine($"Advertencia: Más de un producto de regalo encontrado para la regla {ruleMatch.id} de la promoción {benefit.Promotion.name}. Se tomará el primero.");
                    }
                }
            }            

            var productGift = await productDb.GetByProductTemplate(productIdCompare);            
            productGift.promotionEvalItem = benefit;
            productGift.qty_gift = ruleMatch.AllowedGifts;

            if (productGift != null)
            {
                bool productExistsInOrder = false;
                //Se busca producto dentro de la orden para evitar duplicados 
                // cuando no se debe agregar más de una vez y poder sumar cantidades en caso de que se requiera
                if (_promoGiftsAuto.Any(x => x?.default_code == productGift.default_code))
                {
                    productExistsInOrder = true;
                }

                //Aquì debe sumar si es que encuentra el producto
                //if (benefit.Promotion._selection_type_id == 1)
                //{

                //}

                //if (!productExistsInOrder)
                //{
                    if (!await promotionEngineRunner.CanApplyPromotion(SaleOrder, benefit, saleOrderPromotions))
                    {
                        Debug.WriteLine($"{benefit.Promotion.name} ya ha sido aplicado maximo de veces - AddAutoGiftsAsync");
                        return;
                    }
                    
                    Debug.WriteLine($"Cargado regalo automático para promoción {benefit.Promotion.name}: {productGift.name}");
                    _promoGiftsAuto.Add(productGift);
                //}
             
                foreach (var itemLine in SaleOrder.order_line)
                {
                    if (itemLine[2] != null)
                    {
                        var lineObject = (sale_order_line)itemLine[2];
                        if (lineObject.product_id == productGift.id && lineObject.is_gift) //&& lineObject.product_id_origin == benefit.ProductId
                        {
                            //Si es que es regalo automático, se suma la cantidad si es que no ha llegado al máximo permitido
                            if (benefit.Promotion._selection_type_id == 1)
                            {
                                //if (lineObject.product_uom_qty_real >= productGift.qty_gift)
                                //if (benefit.MaxAllowedGifts >= productGift.qty_gift)
                                //{
                                    //return;
                                //}
                                //else
                                //{
                                    lineObject.product_uom_qty_real++;
                                    lineObject.product_uom_qty = lineObject.product_uom_qty_real;
                                    await promotionEngineRunner.AddApplyPromotion(SaleOrder, benefit, 1, saleOrderPromotions);
                                //}
                            }
                            else
                            {
                                //Si es de algun otro tipo
                                // return porque no debe acumular *por ahora*
                                return;
                            }                            
                        }

                        //Se busca la linea de origen de la promoción aplicada
                        // para agregar la información de la promoción en el campo promotion_data
                        // pero si ya tiene datos debe leerlos y agregar el nuevo

                        if (lineObject.product_id == ruleMatch.ProductId)
                        {
                            var old_promotion_data = lineObject.promotion_data;
                            var listPromotionData = new List<PromotionEvalItemV2>();

                            listPromotionData = !string.IsNullOrEmpty(lineObject.promotion_data) ?
                                Newtonsoft.Json.JsonConvert.DeserializeObject<List<PromotionEvalItemV2>>(lineObject.promotion_data) :
                                new List<PromotionEvalItemV2>();

                            //foreach (var promotion in lineObject.promotion_data != null ? 
                            //    Newtonsoft.Json.JsonConvert.DeserializeObject<List<PromotionEvalItemV2>>(lineObject.promotion_data) : 
                            //    new List<PromotionEvalItemV2>())
                            //{
                            //    listPromotionData.Add(promotion);
                            //}

                            listPromotionData.Add(productGift.promotionEvalItem);

                            lineObject.promotion_data = Newtonsoft.Json.JsonConvert.SerializeObject(
                                    listPromotionData
                                );
                        }
                    }
                }

                bool ShouldSaveToo = false;
                int ordinal = 0;

                //if (productGift.id != ruleMatch.ProductId)
                //{
                //    continue;
                //}

                if (!productExistsInOrder)
                {
                    int qty_assign = ruleMatch.AllowedGifts;

                    if (benefit.Promotion._promotion_type_id == 4)
                    {
                        qty_assign = benefit.MaxAllowedGifts;
                    }

                    var line = new sale_order_line
                    {
                        _order_id = SaleOrder.id,
                        ordinal = ordinal,
                        product_id = productGift.id,
                        product_display = productGift.name,
                        product_code = productGift.code,
                        qty_to_deliver = qty_assign,
                        product_uom_qty_real = qty_assign,
                        product_uom_qty = qty_assign,
                        uom_category_display = "UND",
                        price_subtotal = 0,
                        discount = 100,
                        price_tax = 0,
                        price_total = 0,
                        is_gift = true,
                        product_id_origin = ruleMatch.ProductId,
                        promotion_data = Newtonsoft.Json.JsonConvert.SerializeObject(
                            new List<PromotionEvalItemV2> { productGift.promotionEvalItem }
                        )
                    };

                    if (ShouldSaveToo)
                    {
                        var saleOrderLineDb = new SaleOrderLineDb(App.Session.odooConnection.DbNameSqlite);
                        await saleOrderLineDb.InsertAsyncAutoOrdinal(line);
                    }

                    SaleOrder.order_line.Add(new OrderLineWrapper(line));
                    OrderLines.Add(line);

                    await promotionEngineRunner.AddApplyPromotion(SaleOrder, benefit, 1, saleOrderPromotions);
                }
                
            }
        }
    }

    private async void detail_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (promoGifts != null)
            promoGifts.Clear();
        else
            promoGifts = new ObservableCollection<product_product>();

        if (e.CurrentSelection != null && e.CurrentSelection.Count > 0)
        {
            selectedPromoEvalItem = (PromotionEvalItemV2) e.CurrentSelection[0];
            
            var productDb = new ProductProductDb(App.Session.odooConnection.DbNameSqlite);
            
            if (selectedPromoEvalItem.Promotion._promotion_type_id == 2) // es regalo
            {
                foreach(var itemResult in _itemsFullPromos)                
                {
                    foreach(var itemEval in itemResult.Items)
                    {
                        if (itemEval.Promotion.id != selectedPromoEvalItem.Promotion.id)
                            continue;

                        if(itemEval.Promotion._selection_type_id == 1)
                        {
                            //Agregar automáticamente todos los regalos permitidos
                            foreach (var ruleEval in itemEval.RuleSet)
                            {
                                int productIdCompare = ruleEval.ProductId;
                                var productGift = await productDb.GetByProductTemplate(productIdCompare);
                                //int qty_gift = 0;
                                //qty_gift = itemEval.AllowedGifts;

                                if (productGift != null)
                                {
                                    productGift.qty_gift = 0;
                                    productGift.promotionEvalItem = itemEval;
                                    productGift.allow_add_gift = false;
                                    if (!_promoGifts.Any(x => x?.default_code == productGift.default_code))
                                        promoGifts.Add(productGift);
                                }
                            }
                        }
                                                
                        if(itemEval.Promotion._selection_type_id == 2)
                        {
                            foreach (var ruleEval in itemEval.RuleSet)
                            {
                                //Si es que es manual se maneja distinto la extracción de los productos
                                //var listIdsProd = itemEval.Promotion._product_details_promotion_ids
                                //.Where(x => x._promo_id == 0 && x._bonus_id > 0)
                                //.Select(x => x._product_id).ToList();

                                var listIdsProd = itemEval.Promotion._product_details_promotion_ids                                
                                    .Select(x => x._product_id).ToList();

                                var productGift = await productDb.GetByProductsTemplate(listIdsProd.ToArray());

                                if (productGift != null && productGift.Count > 0)
                                {
                                    foreach (var prod in productGift)
                                    {
                                        prod.qty_gift = 0;
                                        prod.promotionEvalItem = itemEval;
                                        prod.allow_add_gift = true;

                                        if (!_promoGifts.Any(x => x?.default_code == prod.default_code))
                                            promoGifts.Add(prod);
                                    }
                                }
                            }
                        }
                    }
                }

                OnPropertyChanged(nameof(promoGifts));
            }

            if (selectedPromoEvalItem.Promotion._promotion_type_id == 4) // es NXN
            {
                
            }

            if (selectedPromoEvalItem.Promotion._promotion_type_id == 6) // es descuento
            {
                
            }

        }
    }

    private async Task LoadDetailInfo(PromotionBenefit promotionBenefit)
    {        
        
    }

    //public void LoadDataByTimer()
    //{
    //    var timer = Dispatcher.CreateTimer();
    //    timer.Interval = TimeSpan.FromMilliseconds(300);
    //    timer.IsRepeating = false;

    //    timer.Tick += async (s, e) =>
    //    {
    //        try
    //        {
    //            await EvalPromotions();
    //        }
    //        catch (Exception ex)
    //        {
    //            Debug.WriteLine($"Error en LoadData: {ex}");
    //        }
    //        finally
    //        {
    //            timer.Stop();
    //        }
    //    };

    //    timer.Start();
    //}

    //public async Task EvalPromotions()
    //{
    //    try
    //    {
    //        IsLoading = true;

    //        ItemsData ??= new ObservableCollection<PromotionBenefit>();
    //        ItemsData.Clear();

    //        string DbNameSqlite = App.Session.odooConnection.DbNameSqlite;

    //        PromotionBenefitDb dataDb = new PromotionBenefitDb(DbNameSqlite);
    //        var items = await dataDb.GetItemsAsync("authorized");

    //        //foreach (var it in items)
    //        //    ItemsData.Add(it);

    //        Debug.WriteLine($"Promociones cargadas: {ItemsData.Count}");
    //    }
    //    catch (Exception ex)
    //    {
    //        Debug.WriteLine($"Error cargando promociones: {ex}");
    //    }
    //    finally
    //    {
    //        IsLoading = false;

    //        //IsFinishedAnalysis = true;
    //    }
    //}

    [Obsolete]
    private async void _obsolete_AddGift(object sender, EventArgs e)
    {        
        //Debug.WriteLine(sender);
        Button button = (Button) sender;
        product_product product = (product_product)button.BindingContext;

        var saleOrderLineDb = new SaleOrderLineDb(App.Session.odooConnection.DbNameSqlite);

        foreach (var itemLine in SaleOrder.order_line)
        {
            if (itemLine[2] != null)
            {
                var lineObject = (sale_order_line)itemLine[2];
                if( lineObject.product_id == product.id )
                {
                    if(lineObject.product_uom_qty_real == product.qty_gift)
                    {
                        await Application.Current.Windows[0].Page.DisplayAlert("Información", "Cantidad máxima alcanzada en pedido.", "OK");
                        return;
                    }
                    else
                    {
                        lineObject.product_uom_qty_real++;
                        lineObject.product_uom_qty = lineObject.product_uom_qty_real;
                        await saleOrderLineDb.UpdateAsync(lineObject);
                        return;
                    }
                }
            }
        }

        int ordinal = 0;

        var line = new sale_order_line
        {
            _order_id = SaleOrder.id,
            ordinal = ordinal,
            product_id = product.id,
            product_display = product.name,
            product_code = product.code,
            qty_to_deliver = 1,
            product_uom_qty_real = 1,
            product_uom_qty = 1,
            uom_category_display = "UND",
            price_subtotal = 0,
            discount = 100,
            price_tax = 0,
            price_total = 0,
            is_gift = true,
            promotion_data = Newtonsoft.Json.JsonConvert.SerializeObject(
                new List<PromotionEvalItemV2> { product.promotionEvalItem }
            )
        };

        await saleOrderLineDb.InsertAsyncAutoOrdinal(line);
        SaleOrder.order_line.Add(new OrderLineWrapper(line));

        OrderLines.Add(line);
    }

    private async void AddGiftAndSave(object sender, EventArgs e)
    {
        bool ShouldSaveToo = false;
                
        Button button = (Button)sender;
        product_product product = (product_product)button.BindingContext;

        _ = AddGiftIsolated(product, ShouldSaveToo, selectedPromoEvalItem);

        ////sale_order_line saleOrderLineOrigin = new sale_order_line();

        ////var saleOrderLineDb = new SaleOrderLineDb(App.Session.odooConnection.DbNameSqlite);

        //////Se busca linea de origen de promocion aplicada
        ////foreach (var itemLineOrigin in SaleOrder.order_line)
        ////{
        ////    if (itemLineOrigin[2] != null)
        ////    {
        ////        saleOrderLineOrigin = (sale_order_line)itemLineOrigin[2];

        ////        if (selectedPromoEvalItem.ProductId == saleOrderLineOrigin.product_tmpl_id && !saleOrderLineOrigin.is_gift)
        ////        {                    
        ////            if (saleOrderLineOrigin.max_gifts != selectedPromoEvalItem.TotalTimesAllowed)
        ////                saleOrderLineOrigin.max_gifts = selectedPromoEvalItem.TotalTimesAllowed;
        ////            break;
        ////        }
        ////    }
        ////}

        ////if (saleOrderLineOrigin.assigned_gifts >= saleOrderLineOrigin.max_gifts)
        ////{
        ////    await Application.Current.Windows[0].Page.DisplayAlert("Información", "Cantidad máxima alcanzada en pedido.", "OK");
        ////    return;
        ////}

        //////Se busca linea de regalo si es que ya existe
        ////foreach (var itemLine in SaleOrder.order_line)
        ////{
        ////    if (itemLine[2] != null)
        ////    {
        ////        var lineObject = (sale_order_line)itemLine[2];
        ////        if (lineObject.product_id == product.id) // && lineObject.product_id_origin == saleOrderLineOrigin.product_id)
        ////        {
        ////            saleOrderLineOrigin.assigned_gifts++;
        ////            product.qty_gift++;

        ////            //var productFromGift = promoGifts.FirstOrDefault(x => x.id == product.id);

        ////            //if (productFromGift != null)
        ////            //{
        ////            //    productFromGift.qty_gift++;
        ////            //    OnPropertyChanged(nameof(promoGifts)); 
        ////            //}

        ////            //foreach(var giftItem in promoGifts)
        ////            //{
        ////            //    if(giftItem.id == product.id)
        ////            //    {
        ////            //        giftItem.qty_gift++;
        ////            //        OnPropertyChanged(nameof(promoGifts));
        ////            //        break;
        ////            //    }
        ////            //}

        ////            lineObject.product_uom_qty_real++;
        ////            lineObject.product_uom_qty = lineObject.product_uom_qty_real;
        ////            if(ShouldSaveToo)
        ////                await saleOrderLineDb.UpdateAsync(lineObject);
        ////            return;
        ////            //}
        ////        }
        ////    }
        ////}

        ////saleOrderLineOrigin.assigned_gifts++;
        ////product.qty_gift++;

        ////int ordinal = 0;

        ////var line = new sale_order_line
        ////{
        ////    _order_id = SaleOrder.id,
        ////    ordinal = ordinal,
        ////    product_id = product.id,
        ////    product_display = product.name,
        ////    product_code = product.code,
        ////    qty_to_deliver = 1,
        ////    product_uom_qty_real = 1,
        ////    product_uom_qty = 1,
        ////    uom_category_display = "UND",
        ////    price_subtotal = 0,
        ////    discount = 100,
        ////    price_tax = 0,
        ////    price_total = 0,
        ////    is_gift = true,
        ////    product_id_origin = saleOrderLineOrigin.product_id,
        ////    promotion_data = Newtonsoft.Json.JsonConvert.SerializeObject(product.promotionEvalItem)
        ////};

        ////if (ShouldSaveToo)
        ////    await saleOrderLineDb.InsertAsyncAutoOrdinal(line);

        ////SaleOrder.order_line.Add(new OrderLineWrapper(line));

        ////OrderLines.Add(line);
    }

    private async Task AddGiftIsolated(product_product product, bool ShouldSaveToo, PromotionEvalItemV2 benefit)
    {
        PromotionEngineRunner promotionEngineRunner = new PromotionEngineRunner();

        if (!await promotionEngineRunner.CanApplyPromotion(SaleOrder, benefit, saleOrderPromotions))
        {
            Debug.WriteLine($"{benefit.Promotion.name} ya ha sido aplicado maximo de veces - AddGiftIsolated");
            await Toast.Make($"{benefit.Promotion.name} ya ha sido aplicado maximo de veces - AddGiftIsolated").Show();
            return;
        }

        sale_order_line saleOrderLineOrigin = new sale_order_line();

        var saleOrderLineDb = new SaleOrderLineDb(App.Session.odooConnection.DbNameSqlite);

        bool productExistsInOrder = false;
        //Se busca linea de origen de promocion aplicada
        foreach (var itemLineOrigin in SaleOrder.order_line)
        {
            if (itemLineOrigin[2] != null)
            {
                saleOrderLineOrigin = (sale_order_line)itemLineOrigin[2];

                foreach (var ruleItem in benefit.RuleSet)
                {
                    if (ruleItem.ProductTmplId == saleOrderLineOrigin.product_tmpl_id && !saleOrderLineOrigin.is_gift)
                    {
                        productExistsInOrder = true;
                        if (saleOrderLineOrigin.max_gifts != benefit.MaxAllowedGifts)
                        {
                            saleOrderLineOrigin.max_gifts = benefit.MaxAllowedGifts;
                            saleOrderLineOrigin.promotion_data = Newtonsoft.Json.JsonConvert.SerializeObject(
                                    new List<PromotionEvalItemV2> { product.promotionEvalItem }
                                );
                        }
                        break;
                    }
                }

                if (productExistsInOrder) break;
            }
        }

        if (saleOrderLineOrigin.assigned_gifts >= saleOrderLineOrigin.max_gifts)
        {
            await Application.Current.Windows[0].Page.DisplayAlert("Información", "Cantidad máxima alcanzada en pedido.", "OK");
            return;
        }

        //Se busca linea de regalo si es que ya existe
        foreach (var itemLine in SaleOrder.order_line)
        {
            if (itemLine[2] != null)
            {
                var lineObject = (sale_order_line)itemLine[2];
                if (lineObject.product_id == product.id && lineObject.is_gift) // && lineObject.product_id_origin == saleOrderLineOrigin.product_id)
                {
                    saleOrderLineOrigin.assigned_gifts++;
                    product.qty_gift++;

                    lineObject.product_uom_qty_real++;
                    lineObject.product_uom_qty = lineObject.product_uom_qty_real;

                    if (saleOrderLineOrigin.assigned_gifts >= saleOrderLineOrigin.max_gifts)
                    {
                        //En el momento en que se ha completado el maximo de regalos, se registra la aplicación de la promoción
                        await promotionEngineRunner.AddApplyPromotion(SaleOrder, benefit, 1, saleOrderPromotions);
                    }

                    if (ShouldSaveToo)
                        await saleOrderLineDb.UpdateAsync(lineObject);
                    return;
                    //}
                }
            }
        }

        
        saleOrderLineOrigin.assigned_gifts++;
        product.qty_gift++;

        if (saleOrderLineOrigin.assigned_gifts >= saleOrderLineOrigin.max_gifts)
        {
            //En el momento en que se ha completado el maximo de regalos, se registra la aplicación de la promoción
            await promotionEngineRunner.AddApplyPromotion(SaleOrder, benefit, 1, saleOrderPromotions);
        }

        int ordinal = 0;

        var line = new sale_order_line
        {
            _order_id = SaleOrder.id,
            ordinal = ordinal,
            product_id = product.id,
            product_display = product.name,
            product_code = product.code,
            qty_to_deliver = 1,
            product_uom_qty_real = 1,
            product_uom_qty = 1,
            uom_category_display = "UND",
            price_subtotal = 0,
            discount = 100,
            price_tax = 0,
            price_total = 0,
            is_gift = true,
            product_id_origin = saleOrderLineOrigin.product_id,
            promotion_data = Newtonsoft.Json.JsonConvert.SerializeObject(
                new List<PromotionEvalItemV2> { product.promotionEvalItem }
            )
        };

        if (ShouldSaveToo)
            await saleOrderLineDb.InsertAsyncAutoOrdinal(line);

        SaleOrder.order_line.Add(new OrderLineWrapper(line));

        OrderLines.Add(line);
    }

    [Obsolete]
    private async Task AddGiftNxN(product_product product, bool ShouldSaveToo, PromotionEvalItemV2 benefit)
    {
        PromotionEngineRunner promotionEngineRunner = new PromotionEngineRunner();

        if (!await promotionEngineRunner.CanApplyPromotion(SaleOrder, benefit, saleOrderPromotions))
        {
            Debug.WriteLine($"{benefit.Promotion.name} ya ha sido aplicado maximo de veces - AddGiftNxN");
            return;
        }

        //## Buscamos dentro de SaleOrder.order_line si es que existe el producto

        foreach (var itemLine in SaleOrder.order_line)
        {
            if (itemLine[2] != null)
            {
                var lineObject = (sale_order_line) itemLine[2];
                if (lineObject.product_id == product.id && lineObject.is_gift) //&& lineObject.product_id_origin == benefit.ProductId
                {
                    //return;
                    //if (lineObject.product_uom_qty_real >= product.qty_gift)
                    //{
                    //    return;
                    //}
                    //else
                    //{
                    //    lineObject.product_uom_qty_real++;
                    //    lineObject.product_uom_qty = lineObject.product_uom_qty_real;
                    //}
                }

                //Se busca la linea de origen de la promoción aplicada
                foreach(var ruleItem in benefit.RuleSet)
                {
                    if (lineObject.product_id == ruleItem.ProductId)
                    {
                        lineObject.promotion_data = Newtonsoft.Json.JsonConvert.SerializeObject(
                                new List<PromotionEvalItemV2> { product.promotionEvalItem }
                            );
                    }
                }                
            }
        }

        int ordinal = 0;

        foreach (var ruleItem in benefit.RuleSet)
        {
            if (product.id != ruleItem.ProductId)
            {
                continue;
            }

            var line = new sale_order_line
            {
                _order_id = SaleOrder.id,
                ordinal = ordinal,
                product_id = product.id,
                product_display = product.name,
                product_code = product.code,
                qty_to_deliver = benefit.MaxAllowedGifts, // benefit.RuleSet.qty,
                product_uom_qty_real = benefit.MaxAllowedGifts, //benefit.RuleSet.qty,
                product_uom_qty = benefit.MaxAllowedGifts, //benefit.RuleSet.qty,
                uom_category_display = "UND",
                price_subtotal = 0,
                discount = 100,
                price_tax = 0,
                price_total = 0,
                is_gift = true,
                product_id_origin = ruleItem.ProductId,
                promotion_data = Newtonsoft.Json.JsonConvert.SerializeObject(
                new List<PromotionEvalItemV2> { product.promotionEvalItem }
            )
            };

            if (ShouldSaveToo)
            {
                var saleOrderLineDb = new SaleOrderLineDb(App.Session.odooConnection.DbNameSqlite);
                await saleOrderLineDb.InsertAsyncAutoOrdinal(line);
            }

            SaleOrder.order_line.Add(new OrderLineWrapper(line));

            OrderLines.Add(line);

            await promotionEngineRunner.AddApplyPromotion(SaleOrder, benefit, 1, saleOrderPromotions);
        }
    }

    public event PropertyChangedEventHandler PropertyChanged;
    private void OnPropertyChanged(string property) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));

    public Action<List<PromotionEvalResultV2>> ClosePopupAction { get; set; }
    public ObservableCollection<sale_order_line> OrderLines { get; internal set; }

    private void OnCloseButtonClicked(object sender, EventArgs e)
    {
        ClosePopupAction?.Invoke(ItemsData.ToList());
    }   
}