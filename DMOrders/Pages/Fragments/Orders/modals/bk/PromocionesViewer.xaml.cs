using CommunityToolkit.Maui.Alerts;
using DMOrders.Services.Promotions;
using DMSA.Models.Odoo.DMOrders.promotions.abstractCustom;
using DMSA.Models.Odoo.Native;
using DMSA.Sync.Core.Database.Sqlite;
using Newtonsoft.Json;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;

namespace DMOrders.Pages.Fragments.Orders.modals;

public partial class PromocionesViewer : ContentView, INotifyPropertyChanged
{
    private int GlobalTotalManualGiftsAllowed = 0;
    private int GlobalTotalManualGiftsApplied = 0;
    public int GlobalTotalManualGiftsForRemove = 0;
    public int GlobalTotalManualGiftsRemoved = 0;
    
    public bool RequiredRemoveItems
    {
        get
        {
            return GlobalTotalManualGiftsForRemove > GlobalTotalManualGiftsRemoved;
        }
    }

    public int TotalGiftsForRemove
    {
        get
        {            
            return GlobalTotalManualGiftsForRemove;
        }
    }

    public int TotalGiftsRemoved
    {
        get
        {
            return GlobalTotalManualGiftsRemoved;
        }
    }

    private List<sale_order_line> realApplied { get; set; }

    ProductProductDb productDb { get; set; }

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

    public bool BenefitsForShow { get; set; }

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

            OnPropertyChanged(nameof(ItemsData));
            OnPropertyChanged(nameof(ItemsDataBenefits));
            OnPropertyChanged(nameof(ComputeTotal));
            OnPropertyChanged(nameof(ComputeTotalQty));
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
            promoGiftsFiltered = _promoGifts;
            OnPropertyChanged(nameof(promoGifts));
            OnPropertyChanged(nameof(promoGiftsFiltered));
        }
    }

    private ObservableCollection<product_product> _promoGifts;

    public ObservableCollection<product_product> promoGiftsFiltered { get; set; }

    public ObservableCollection<PromoRuleMatch> promoDiscounts { get; set; }

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

    private bool _editQty;
    public bool EditQty
    {
        get => _editQty;
        set
        {
            _editQty = value;
            OnPropertyChanged(nameof(EditQty));            
        }
    }

    public int PromosCount
    {
        get => ItemsData?.Count ?? 0;
    }

    public async Task ApplyPromosOnList()
    {
        bool existPromosForEval = false;

        foreach (var promo in _itemsFullPromos)
        {
            if (promo.Items != null)
            {
                foreach (var benefit in promo.Items)
                {
                    //Se agregan los beneficios automáticos para cargar sus regalos si es que es tipo Bonificado == 2
                    if (benefit.Promotion._promotion_type_id == 2 && benefit.Promotion._selection_type_id == 1)
                    {
                        await AddAutoGiftsAsync(benefit);
                    }

                    //NxN se aplica automáticamente
                    if (benefit.Promotion._promotion_type_id == 4 && benefit.Promotion._selection_type_id == 1)
                    {
                        existPromosForEval = true;
                        await AddGiftNxn(benefit);
                    }

                    if (benefit?.Promotion?.id == null)
                        continue;

                    _ItemsDataBenefits.Add(benefit);

                    if (!_ItemsDataBenefits.Any(x => x.Promotion?.id == benefit.Promotion.id))
                    {
                        //Quizas se deban acumular los qty * numero de apariciones de la promoción                            
                        benefit.FoundTimesApplies = 1;

                        foreach (var rule in benefit.RuleSet)
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
                        if (totalTimesFound > existing.TotalTimesAllowed)
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

        if (existPromosForEval && !_promoGiftsAuto.Any()) //(_promoGiftsAuto == null || _promoGiftsAuto.Count == 0))
            BenefitsForShow = false;
        else
        {
            //Se recalculan los regalos asignados automáticamente
            
            foreach (var promo in _itemsFullPromos)
            {
                if (promo.Items != null)
                {
                    foreach (var benefit in promo.Items)
                    {
                        //Bonificado / Manual
                        if(benefit.Promotion._promotion_type_id == 2 && benefit.Promotion._selection_type_id == 2)
                        {                            
                            GlobalTotalManualGiftsAllowed += benefit.MaxAllowedGifts;                            
                            PromotionEngineRunner promotionEngineRunner = new PromotionEngineRunner();
                            await promotionEngineRunner.UpdateApplyPromotion(SaleOrder, benefit, saleOrderPromotions);

                            GlobalTotalManualGiftsForRemove += benefit.GiftsForRemove;
                        }

                        //Bonificado / Automático
                        if (benefit.Promotion._promotion_type_id == 2 && benefit.Promotion._selection_type_id == 1)
                        {
                            
                        }

                        //NxN
                        if (benefit.Promotion._promotion_type_id == 4 && benefit.Promotion._selection_type_id == 1)
                        {
                            int totalGifts = 0;
                            foreach (var giftItem in _promoGiftsAuto)
                            {
                                Debug.WriteLine($"Este es el regalo {giftItem.qty_gift}");
                                if (giftItem.promotionEvalItem != null && giftItem.promotionEvalItem.Promotion.id == benefit.Promotion.id)
                                {
                                    totalGifts += giftItem.qty_gift;
                                }
                            }

                            benefit.MaxAllowedGifts = totalGifts; 
                        }
                    }
                }
            }

            OnPropertyChanged(nameof(ItemsData));
            OnPropertyChanged(nameof(ItemsDataBenefits));
            OnPropertyChanged(nameof(TotalGiftsForRemove));
            OnPropertyChanged(nameof(TotalGiftsRemoved));
            OnPropertyChanged(nameof(RequiredRemoveItems));
        }

        AutoselectManualPromotion();
    }    

    public PromocionesViewer(sale_order SaleOrderParam)
	{
		InitializeComponent();
        BindingContext = this;
        SaleOrder = SaleOrderParam;
        productDb = new ProductProductDb(App.Session.odooConnection.DbNameSqlite);
        realApplied = new List<sale_order_line>();
        BenefitsForShow = true;
    }

    private void AutoselectManualPromotion()
    {
        var benefit = _itemsFullPromos?
            .Where(p => p.Items != null)
            .SelectMany(p => p.Items)
            .FirstOrDefault(b =>
                b.Promotion._promotion_type_id == 2 &&
                b.Promotion._selection_type_id == 2);

        if (benefit != null)
        {
            ListaDetallesPromocion.SelectedItem = benefit;
        }
    }

    //private void AutoselectManualPromotionFull()
    //{
    //    foreach (var promo in _itemsFullPromos)
    //    {
    //        if (promo.Items != null)
    //        {
    //            foreach (var benefit in promo.Items)
    //            {
    //                //Bonificado / Manual
    //                if (benefit.Promotion._promotion_type_id == 2 && benefit.Promotion._selection_type_id == 2)
    //                {
    //                    ListaDetallesPromocion.SelectedItem = benefit.Promotion;
    //                    return;
    //                }
    //            }
    //        }
    //    }
    //}

    private async Task AddAutoGiftsAsync(PromotionEvalItemV2 benefit)
    {
        PromotionEngineRunner promotionEngineRunner = new PromotionEngineRunner();

        foreach (var ruleMatch in benefit.RuleSet)
        {
            int productIdCompare = ruleMatch.ProductIdOrigin;

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

            var productGift = await productDb.GetByProductTemplate(productIdCompare, SaleOrder._pricelist_id);            
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
                                lineObject.amount_discount = lineObject.product_uom_qty_real * lineObject.virtual_price_no_tax;
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

                        if (lineObject.product_id == ruleMatch.ProductIdOrigin)
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

                            DMSA.Models.Odoo.Promotions.Tools.SetPromotionData(lineObject, listPromotionData);
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
                        sequence = ordinal,
                        product_id = productGift.id,
                        product_tmpl_id = productGift._product_tmpl_id,
                        product_display = productGift.name,
                        product_code = productGift.code,
                        qty_to_deliver = qty_assign,
                        product_uom_qty_real = qty_assign,
                        product_uom_qty = qty_assign,
                        uom_category_display = productGift.uom_sale_display,
                        price_subtotal = 0,
                        _virtual_price_no_tax = (decimal) productGift.list_price,
                        amount_discount =(decimal) (qty_assign * productGift.list_price),
                        discount = 100,
                        price_tax = 0,
                        price_total = 0,
                        is_gift = true,
                        product_id_origin = ruleMatch.ProductIdOrigin,
                        promotion_data = Newtonsoft.Json.JsonConvert.SerializeObject(
                            new List<PromotionEvalItemV2> { productGift.promotionEvalItem }
                        )
                    };

                    DMSA.Models.Odoo.Promotions.Tools.SetPromotionDataGift(line, new List<PromotionEvalItemV2> { productGift.promotionEvalItem });

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
        if (e.CurrentSelection?.Count > 0)
        {
            IsLoading = true;
            var selected = (PromotionEvalItemV2)e.CurrentSelection[0];
            await HandlePromotionSelection(selected);
            IsLoading = false;
        }
    }

    //private async Task HandlePromotionSelection_OLD(PromotionEvalItemV2 selectedPromoEvalItem)
    //{
    //    EditQty = false;

    //    realApplied.Clear();

    //    if (promoGifts != null)
    //        promoGifts.Clear();
    //    else
    //        promoGifts = new ObservableCollection<product_product>();

    //    if (selectedPromoEvalItem == null)
    //        return;

    //    this.selectedPromoEvalItem = selectedPromoEvalItem;

    //    var productDb = new ProductProductDb(App.Session.odooConnection.DbNameSqlite);

    //    if (selectedPromoEvalItem.Promotion._promotion_type_id == 2) // regalo
    //    {
    //        BorderBenefits.IsVisible = true;
    //        BorderDiscount.IsVisible = false;

    //        foreach (var itemResult in _itemsFullPromos)
    //        {
    //            foreach (var itemEval in itemResult.Items)
    //            {
    //                if (itemEval.Promotion.id != selectedPromoEvalItem.Promotion.id)
    //                    continue;

    //                if (itemEval.Promotion._selection_type_id == 1)
    //                {
    //                    foreach (var ruleEval in itemEval.RuleSet)
    //                    {
    //                        int productIdCompare = ruleEval.ProductIdOrigin;
    //                        var productGift = await productDb
    //                            .GetByProductTemplate(productIdCompare, SaleOrder._pricelist_id);

    //                        if (productGift != null)
    //                        {
    //                            productGift.qty_gift = 0;
    //                            productGift.promotionEvalItem = itemEval;
    //                            productGift.allow_add_gift = false;

    //                            if (!_promoGifts.Any(x => x?.default_code == productGift.default_code))
    //                                promoGifts.Add(productGift);
    //                        }
    //                    }
    //                }

    //                if (itemEval.Promotion._selection_type_id == 2)
    //                {
    //                    EditQty = true;
    //                    var listIdsProd = itemEval.Promotion._product_details_promotion_ids
    //                        .Select(x => x._product_id)
    //                        .ToList();

    //                    var productGift = await productDb
    //                        .GetByProductsTemplate(listIdsProd.ToArray(), SaleOrder._pricelist_id);

    //                    if (productGift == null)
    //                        continue;

    //                    foreach (var prod in productGift)
    //                    {
    //                        int qty_gift_eval = 0;

    //                        for (var i = 0; i < saleOrderPromotions.Count(); i++)
    //                        {
    //                            if (saleOrderPromotions[i].promotion_id != itemEval.Promotion.id)
    //                                continue;

    //                            foreach (var lineWrapper in SaleOrder.order_line)
    //                            {
    //                                var line = (sale_order_line)lineWrapper[2];

    //                                var listPromotionData = !string.IsNullOrEmpty(line.promotion_data)
    //                                    ? JsonConvert.DeserializeObject<List<PromotionEvalItemV2>>(line.promotion_data)
    //                                    : new List<PromotionEvalItemV2>();

    //                                Debug.WriteLine($"{line.is_gift}");
    //                                Debug.WriteLine($"{line.product_id} - {prod.id}");
    //                                Debug.WriteLine($"{itemEval.Promotion.id}");

    //                                if (line.is_gift && line.product_id == prod.id &&                                        
    //                                    listPromotionData.Any(x => x.Promotion.id == itemEval.Promotion.id))
    //                                {
    //                                    qty_gift_eval += (int)line.product_uom_qty_real;
    //                                    GlobalTotalManualGiftsApplied += qty_gift_eval;
    //                                    realApplied.Add(line);

    //                                    OnPropertyChanged(nameof(ComputeTotal));
    //                                    OnPropertyChanged(nameof(ComputeTotalQty));
    //                                }
    //                            }
    //                        }

    //                        prod.qty_gift = qty_gift_eval;

    //                        prod.qty_gift_virtual = prod.qty_gift;

    //                        prod.promotionEvalItem = itemEval;
    //                        prod.allow_add_gift = true;

    //                        if (!_promoGifts.Any(x => x?.default_code == prod.default_code))
    //                            promoGifts.Add(prod);
    //                    }
    //                }
    //            }
    //        }

    //        OnPropertyChanged(nameof(promoGifts));
    //    }

    //    if (selectedPromoEvalItem.Promotion._promotion_type_id == 6) // Descuento
    //    {
    //        BorderBenefits.IsVisible = false;
    //        BorderDiscount.IsVisible = true;

    //        Debug.WriteLine("Promocion de descuento seleccionada!");

    //        promoDiscounts = new ObservableCollection<PromoRuleMatch>();

    //        foreach (var itemResult in _itemsFullPromos)
    //        {
    //            foreach (var itemEval in itemResult.Items)
    //            {
    //                if (itemEval.Promotion.id != selectedPromoEvalItem.Promotion.id)
    //                    continue;

    //                foreach (var ruleEval in itemEval.RuleSet)
    //                {
    //                    promoDiscounts.Add(ruleEval);
    //                }
    //            }
    //        }

    //        OnPropertyChanged(nameof(promoDiscounts));

    //        if (selectedPromoEvalItem.Promotion._selection_type_id == 1)
    //        {

    //        }
    //    }
    //}


    private async Task HandlePromotionSelection(PromotionEvalItemV2 selectedPromoEvalItem)
    {
        EditQty = false;

        realApplied?.Clear();

        promoGifts ??= new ObservableCollection<product_product>();
        promoGifts.Clear();

        if (selectedPromoEvalItem == null)
            return;

        this.selectedPromoEvalItem = selectedPromoEvalItem;

        var productDb = new ProductProductDb(App.Session.odooConnection.DbNameSqlite);

        //Filtrar promos una sola vez
        var promoItems = _itemsFullPromos
            .SelectMany(x => x.Items)
            .Where(x => x.Promotion.id == selectedPromoEvalItem.Promotion.id)
            .ToList();

        if (selectedPromoEvalItem.Promotion._promotion_type_id == 2) // REGALO
        {
            BorderBenefits.IsVisible = true;
            BorderDiscount.IsVisible = false;

            await HandleGiftPromotion(promoItems, productDb);
        }
        else if (selectedPromoEvalItem.Promotion._promotion_type_id == 6) // DESCUENTO
        {
            HandleDiscountPromotion(promoItems);
        }
    }



    private async Task HandleGiftPromotion(List<PromotionEvalItemV2> promoItems, ProductProductDb productDb)
    {
        var existingCodes = new HashSet<string>();
        var promotionCache = new Dictionary<string, List<PromotionEvalItemV2>>();

        var relevantLines = SaleOrder.order_line
            .Select(x => (sale_order_line)x[2])
            .Where(l => l.is_gift)
            .ToList();

        foreach (var itemEval in promoItems)
        {

            if (itemEval.Promotion._selection_type_id == 1)
            {
                var tasks = itemEval.RuleSet.Select(async ruleEval =>
                {
                    var product = await productDb
                        .GetByProductTemplate(ruleEval.ProductIdOrigin, SaleOrder._pricelist_id);

                    if (product == null) return null;

                    product.qty_gift = 0;
                    product.promotionEvalItem = itemEval;
                    product.allow_add_gift = false;

                    return product;
                });

                var results = await Task.WhenAll(tasks);

                foreach (var product in results.Where(p => p != null))
                {
                    if (existingCodes.Add(product.default_code))
                        promoGifts.Add(product);
                }
            }

      
            if (itemEval.Promotion._selection_type_id == 2)
            {
                EditQty = true;

                var listIds = itemEval.Promotion._product_details_promotion_ids
                    .Select(x => x._product_id)
                    .ToArray();

                var products = await productDb
                    .GetByProductsTemplate(listIds, SaleOrder._pricelist_id);

                if (products == null) continue;

                foreach (var prod in products)
                {
                    int qty = 0;

                    foreach (var line in relevantLines)
                    {
                        if (line.product_id != prod.id)
                            continue;

                        var promoData = GetPromotionData(line.promotion_data, promotionCache);

                        if (promoData.Any(x => x.Promotion.id == itemEval.Promotion.id))
                        {
                            qty += (int)line.product_uom_qty_real;

                            GlobalTotalManualGiftsApplied += (int)line.product_uom_qty_real;

                            if (!realApplied.Contains(line))
                                realApplied.Add(line);
                        }
                    }

                    prod.qty_gift = qty;
                    prod.qty_gift_virtual = qty;
                    prod.promotionEvalItem = itemEval;
                    prod.allow_add_gift = true;

                    if (existingCodes.Add(prod.default_code))
                        promoGifts.Add(prod);
                }
            }
        }

        OnPropertyChanged(nameof(promoGifts));
        OnPropertyChanged(nameof(ComputeTotal));
        OnPropertyChanged(nameof(ComputeTotalQty));
    }

    private void HandleDiscountPromotion(List<PromotionEvalItemV2> promoItems)
    {
        BorderBenefits.IsVisible = false;
        BorderDiscount.IsVisible = true;

        promoDiscounts = new ObservableCollection<PromoRuleMatch>(
            promoItems.SelectMany(x => x.RuleSet)
        );

        OnPropertyChanged(nameof(promoDiscounts));
    }

    private List<PromotionEvalItemV2> GetPromotionData(
    string json,
    Dictionary<string, List<PromotionEvalItemV2>> cache)
    {
        if (string.IsNullOrEmpty(json))
            return new List<PromotionEvalItemV2>();

        if (!cache.TryGetValue(json, out var result))
        {
            result = JsonConvert.DeserializeObject<List<PromotionEvalItemV2>>(json);
            cache[json] = result;
        }

        return result;
    }


    private async void AddGiftAndSave(object sender, EventArgs e)
    {
        bool ShouldSaveToo = false;
        
        Button button = (Button)sender;
        product_product product = (product_product)button.BindingContext;

        //product.qty_gift++;
        product.qty_gift_virtual++;
        await ChangeQtyEvent(product, false);

        //_ = AddGiftIsolated(product, ShouldSaveToo, selectedPromoEvalItem);

        //OnPropertyChanged(nameof(ComputeTotal));
        //OnPropertyChanged(nameof(ComputeTotalQty));
    }
        
    private async void SubstractGift(object sender, EventArgs e)
    {
        bool ShouldSaveToo = false;

        Button button = (Button)sender;
        product_product product = (product_product)button.BindingContext;

        //product.qty_gift++;
        product.qty_gift_virtual--;

        await ChangeQtyEvent(product, false);

        //_ = SubstractGiftIsolated(product, ShouldSaveToo, selectedPromoEvalItem);
        //OnPropertyChanged(nameof(ComputeTotal));
        //OnPropertyChanged(nameof(ComputeTotalQty));
    }


    private async void AddDiscount(object sender, EventArgs e)
    {
        bool ShouldSaveToo = false;
        Button button = (Button)sender;
        var product = (PromoRuleMatch)button.BindingContext;
        Debug.WriteLine(product);
    }

    private async void SubstractDiscount(object sender, EventArgs e)
    {
        bool ShouldSaveToo = false;

        Button button = (Button)sender;
        var product = (PromoRuleMatch)button.BindingContext;
        Debug.WriteLine(product);
    }


    //////[Obsolete("Ya no se usara")]
    //////private async Task AddGiftIsolated(product_product product, bool ShouldSaveToo, PromotionEvalItemV2 benefit)
    //////{
    //////    PromotionEngineRunner promotionEngineRunner = new PromotionEngineRunner();

    //////    if (!await promotionEngineRunner.CanApplyPromotion(SaleOrder, benefit, saleOrderPromotions))
    //////    {
    //////        Debug.WriteLine($"{benefit.Promotion.name} ya ha sido aplicado maximo de veces - AddGiftIsolated");
    //////        await Toast.Make($"{benefit.Promotion.name} ya ha sido aplicado maximo de veces - AddGiftIsolated").Show();
    //////        return;
    //////    }

    //////    var dataBenefitFound = await promotionEngineRunner.GetDataBenefit(SaleOrder, benefit, saleOrderPromotions);

    //////    sale_order_line saleOrderLineOrigin = new sale_order_line();

    //////    var saleOrderLineDb = new SaleOrderLineDb(App.Session.odooConnection.DbNameSqlite);

    //////    bool productExistsInOrder = false;

    //////    //Se busca linea de origen de promocion aplicada
    //////    foreach (var itemLineOrigin in SaleOrder.order_line)
    //////    {
    //////        if (itemLineOrigin[2] != null)
    //////        {
    //////            saleOrderLineOrigin = (sale_order_line)itemLineOrigin[2];

    //////            foreach (var ruleMatch in benefit.RuleSet)
    //////            {
    //////                int[] listIdsProd = JsonConvert.DeserializeObject<int[]>(ruleMatch.ProductTmplIds);

    //////                foreach (var productIdCompare in listIdsProd)
    //////                {
    //////                    if (productIdCompare == saleOrderLineOrigin.product_tmpl_id && !saleOrderLineOrigin.is_gift)
    //////                    {
    //////                        productExistsInOrder = true;
    //////                        //if (saleOrderLineOrigin.max_gifts != benefit.MaxAllowedGifts)
    //////                        //{
    //////                        //saleOrderLineOrigin.max_gifts = benefit.MaxAllowedGifts;

    //////                        if(dataBenefitFound.max_gifts != benefit.MaxAllowedGifts)
    //////                            dataBenefitFound.max_gifts = benefit.MaxAllowedGifts;

    //////                        saleOrderLineOrigin.promotion_data = JsonConvert.SerializeObject(
    //////                                    new List<PromotionEvalItemV2> { product.promotionEvalItem }
    //////                                );

    //////                        DMSA.Models.Odoo.Promotions.Tools.SetPromotionData(saleOrderLineOrigin, new List<PromotionEvalItemV2> { product.promotionEvalItem });
    //////                        //}
    //////                        break;
    //////                    }
    //////                }
    //////            }

    //////            if (productExistsInOrder) break;
    //////        }
    //////    }

    //////    //if (saleOrderLineOrigin.assigned_gifts >= saleOrderLineOrigin.max_gifts)
    //////    if (dataBenefitFound.assigned_gifts >= dataBenefitFound.max_gifts)
    //////    {
    //////        await Application.Current.Windows[0].Page.DisplayAlert("Información", "Cantidad máxima alcanzada en pedido.", "OK");
    //////        return;
    //////    }

    //////    //Se busca linea de regalo si es que ya existe
    //////    foreach (var itemLine in SaleOrder.order_line)
    //////    {
    //////        if (itemLine[2] != null)
    //////        {
    //////            var lineObject = (sale_order_line)itemLine[2];
    //////            if (lineObject.product_id == product.id && lineObject.is_gift) // && lineObject.product_id_origin == saleOrderLineOrigin.product_id)
    //////            {
    //////                //saleOrderLineOrigin.assigned_gifts++;
    //////                dataBenefitFound.assigned_gifts++;

    //////                product.qty_gift++;

    //////                lineObject.product_uom_qty_real++;
    //////                lineObject.product_uom_qty = lineObject.product_uom_qty_real;
    //////                lineObject.virtual_line_subtotal = lineObject.product_uom_qty_real * lineObject.virtual_price_no_tax;
    //////                lineObject.amount_discount = lineObject.product_uom_qty_real * lineObject.virtual_price_no_tax;
    //////                GlobalTotalManualGiftsApplied++;

    //////                product.qty_gift_virtual = product.qty_gift;

    //////                //if (saleOrderLineOrigin.assigned_gifts >= saleOrderLineOrigin.max_gifts)
    //////                if (dataBenefitFound.assigned_gifts >= dataBenefitFound.max_gifts)
    //////                {
    //////                    //En el momento en que se ha completado el maximo de regalos, se registra la aplicación de la promoción
    //////                    await promotionEngineRunner.AddApplyPromotion(SaleOrder, benefit, 1, saleOrderPromotions);
    //////                }

    //////                if (ShouldSaveToo)
    //////                    await saleOrderLineDb.UpdateAsync(lineObject);
    //////                return;
    //////                //}
    //////            }
    //////        }
    //////    }

    //////    //saleOrderLineOrigin.assigned_gifts++;
    //////    dataBenefitFound.assigned_gifts++;

    //////    product.qty_gift++;
    //////    product.qty_gift_virtual = product.qty_gift;

    //////    GlobalTotalManualGiftsApplied++;

    //////    //if (saleOrderLineOrigin.assigned_gifts >= saleOrderLineOrigin.max_gifts)
    //////    if(dataBenefitFound.assigned_gifts >= dataBenefitFound.max_gifts)
    //////    {
    //////        //En el momento en que se ha completado el maximo de regalos, se registra la aplicación de la promoción
    //////        await promotionEngineRunner.AddApplyPromotion(SaleOrder, benefit, 1, saleOrderPromotions);
    //////    }

    //////    int ordinal = 0;

    //////    var line = new sale_order_line
    //////    {
    //////        _order_id = SaleOrder.id,
    //////        sequence = ordinal,
    //////        product_id = product.id,
    //////        product_display = product.name,
    //////        product_code = product.code,
    //////        qty_to_deliver = 1,
    //////        product_uom_qty_real = 1,
    //////        product_uom_qty = 1,
    //////        uom_category_display = product.uom_sale_display,
    //////        price_subtotal = 0,
    //////        virtual_line_subtotal = (decimal)(1 * product.list_price),
    //////        amount_discount = (decimal)(1 * product.list_price),
    //////        discount = 100,
    //////        price_tax = 0,
    //////        price_total = 0,
    //////        is_gift = true,
    //////        is_manual = true,
    //////        virtual_price_no_tax = (decimal) product.list_price,
    //////        product_tmpl_id = product._product_tmpl_id,
    //////        product_id_origin = saleOrderLineOrigin.product_id,
    //////        promotion_data = Newtonsoft.Json.JsonConvert.SerializeObject(
    //////            new List<PromotionEvalItemV2> { product.promotionEvalItem }
    //////        )
    //////    };

    //////    DMSA.Models.Odoo.Promotions.Tools.SetPromotionDataGift(line, new List<PromotionEvalItemV2> { product.promotionEvalItem });

    //////    if (ShouldSaveToo)
    //////        await saleOrderLineDb.InsertAsyncAutoOrdinal(line);

    //////    SaleOrder.order_line.Add(new OrderLineWrapper(line));

    //////    OrderLines.Add(line);

    //////    realApplied.Add(line);
    //////}

    //////[Obsolete("Ya no se usara")]
    //////private async Task SubstractGiftIsolated(product_product product, bool ShouldSaveToo, PromotionEvalItemV2 benefit)
    //////{
    //////    //Evaluar si llega a 0 para ya no poder restar más
    //////    //if (saleOrderLineOrigin.assigned_gifts >= saleOrderLineOrigin.max_gifts)
    //////    if (product.qty_gift == 0)
    //////    {
    //////        //await Application.Current.Windows[0].Page.DisplayAlert("Información", "Cantidad mínima alcanzada en pedido.", "OK");
    //////        await Toast.Make("Cantidad mínima alcanzada en pedido.").Show();
    //////        return;
    //////    }

    //////    PromotionEngineRunner promotionEngineRunner = new PromotionEngineRunner();

    //////    var dataBenefitFound = await promotionEngineRunner.GetDataBenefit(SaleOrder, benefit, saleOrderPromotions);

    //////    sale_order_line saleOrderLineOrigin = new sale_order_line();

    //////    var saleOrderLineDb = new SaleOrderLineDb(App.Session.odooConnection.DbNameSqlite);

    //////    bool productExistsInOrder = false;

    //////    //Se busca linea de origen de promoción aplicada
    //////    foreach (var itemLineOrigin in SaleOrder.order_line)
    //////    {
    //////        if (itemLineOrigin[2] != null)
    //////        {
    //////            saleOrderLineOrigin = (sale_order_line)itemLineOrigin[2];

    //////            foreach (var ruleMatch in benefit.RuleSet)
    //////            {
    //////                int[] listIdsProd = Newtonsoft.Json.JsonConvert.DeserializeObject<int[]>(ruleMatch.ProductTmplIds);

    //////                foreach (var productIdCompare in listIdsProd)
    //////                {
    //////                    if (productIdCompare == saleOrderLineOrigin.product_tmpl_id && !saleOrderLineOrigin.is_gift)
    //////                    {
    //////                        productExistsInOrder = true;

    //////                        if (dataBenefitFound.max_gifts != benefit.MaxAllowedGifts)
    //////                            dataBenefitFound.max_gifts = benefit.MaxAllowedGifts;

    //////                        //if (saleOrderLineOrigin.max_gifts != benefit.MaxAllowedGifts)
    //////                        //{
    //////                        //saleOrderLineOrigin.max_gifts = benefit.MaxAllowedGifts;
    //////                        saleOrderLineOrigin.promotion_data = Newtonsoft.Json.JsonConvert.SerializeObject(
    //////                                    new List<PromotionEvalItemV2> { product.promotionEvalItem }
    //////                                );
    //////                        DMSA.Models.Odoo.Promotions.Tools.SetPromotionData(saleOrderLineOrigin, new List<PromotionEvalItemV2> { product.promotionEvalItem });
    //////                        //}
    //////                        break;
    //////                    }
    //////                }
    //////            }

    //////            if (productExistsInOrder) break;
    //////        }
    //////    }

    //////    bool ShouldBeRemoved = false;

    //////    //Se busca linea de regalo si es que ya existe
    //////    foreach (var itemLine in SaleOrder.order_line)
    //////    {
    //////        if (itemLine[2] != null)
    //////        {
    //////            var lineObject = (sale_order_line)itemLine[2];
    //////            if (lineObject.product_id == product.id && lineObject.is_gift) // && lineObject.product_id_origin == saleOrderLineOrigin.product_id)
    //////            {
    //////                //saleOrderLineOrigin.assigned_gifts--;
    //////                dataBenefitFound.assigned_gifts--;

    //////                product.qty_gift--;                    
    //////                lineObject.product_uom_qty_real--;
    //////                lineObject.product_uom_qty = lineObject.product_uom_qty_real;

    //////                product.qty_gift_virtual = product.qty_gift;

    //////                GlobalTotalManualGiftsApplied--;

    //////                if (GlobalTotalManualGiftsRemoved < GlobalTotalManualGiftsForRemove)
    //////                {
    //////                    GlobalTotalManualGiftsRemoved++;
    //////                    OnPropertyChanged(nameof(TotalGiftsForRemove));
    //////                    OnPropertyChanged(nameof(TotalGiftsRemoved));
    //////                }

    //////                //if (saleOrderLineOrigin.max_gifts >= saleOrderLineOrigin.assigned_gifts)
    //////                if (dataBenefitFound.max_gifts >= dataBenefitFound.assigned_gifts)
    //////                {
    //////                    //En el momento en que se ha completado el maximo de regalos, se registra la aplicación de la promoción
    //////                    await promotionEngineRunner.AddApplyPromotion(SaleOrder, benefit, -1, saleOrderPromotions);
    //////                }

    //////                if (ShouldSaveToo)
    //////                    await saleOrderLineDb.UpdateAsync(lineObject);

    //////                if (product.qty_gift == 0)
    //////                {
    //////                    ShouldBeRemoved = true;

    //////                    SaleOrder.order_line.Remove(itemLine);
    //////                    OrderLines.Remove(lineObject);
    //////                    realApplied.Remove(lineObject);
    //////                }
    //////                return;                    
    //////            }
    //////        }
    //////    }
    //////}

    //////private async Task<bool> UpdateGiftIsolated(
    //////    product_product product,
    //////    bool ShouldSaveToo,
    //////    PromotionEvalItemV2 benefit)
    //////{
    //////    int targetQty = product.qty_gift_virtual;
    //////    int currentQty = product.qty_gift;

    //////    if (targetQty < 0)
    //////    {
    //////        await Toast.Make("Cantidad inválida.").Show();
    //////        return false;
    //////    }

    //////    PromotionEngineRunner promotionEngineRunner = new PromotionEngineRunner();

    //////    if (!await promotionEngineRunner.CanApplyPromotion(SaleOrder, benefit, saleOrderPromotions))
    //////    {
    //////        //Debug.WriteLine($"{benefit.Promotion.name} ya ha sido aplicado maximo de veces - AddGiftIsolated");
    //////        //await Toast.Make($"{benefit.Promotion.name} ya ha sido aplicado maximo de veces - AddGiftIsolated").Show();
    //////        //return false;
    //////    }

    //////    var dataBenefitFound = await promotionEngineRunner
    //////        .GetDataBenefit(SaleOrder, benefit, saleOrderPromotions);

    //////    if (dataBenefitFound == null)
    //////        return false;

    //////    if (dataBenefitFound.max_gifts != benefit.MaxAllowedGifts)
    //////        dataBenefitFound.max_gifts = benefit.MaxAllowedGifts;

    //////    var totalAssignedPreview = _promoGifts.Sum(x => x.qty_gift_virtual);

    //////    if (totalAssignedPreview > dataBenefitFound.max_gifts)
    //////    {
    //////        await Application.Current.Windows[0]
    //////            .Page.DisplayAlert("Información", "Cantidad máxima alcanzada en pedido.", "OK");
    //////        return false;
    //////    }

    //////    var saleOrderLineDb = new SaleOrderLineDb(App.Session.odooConnection.DbNameSqlite);

    //////    sale_order_line saleOrderLineOrigin = null;

    //////    // Buscar línea origen
    //////    foreach (var item in SaleOrder.order_line)
    //////    {
    //////        if (item[2] == null) continue;

    //////        var line = (sale_order_line)item[2];

    //////        if (!line.is_gift)
    //////        {
    //////            foreach (var ruleMatch in benefit.RuleSet)
    //////            {
    //////                int[] ids = Newtonsoft.Json.JsonConvert
    //////                    .DeserializeObject<int[]>(ruleMatch.ProductTmplIds);

    //////                if (ids.Contains(line.product_tmpl_id))
    //////                {
    //////                    saleOrderLineOrigin = line;
    //////                    break;
    //////                }
    //////            }
    //////        }

    //////        if (saleOrderLineOrigin != null)
    //////            break;
    //////    }

    //////    if (saleOrderLineOrigin == null)
    //////        return false;

    //////    // Buscar línea gift existente
    //////    sale_order_line giftLine = null;

    //////    foreach (var item in SaleOrder.order_line)
    //////    {
    //////        if (item[2] == null) continue;

    //////        var line = (sale_order_line)item[2];

    //////        if (line.product_id == product.id && line.is_gift)
    //////        {
    //////            giftLine = line;
    //////            break;
    //////        }
    //////    }

    //////    int delta = targetQty - currentQty;

    //////    // =========================================================
    //////    // ESCENARIO 1: ELIMINAR COMPLETAMENTE
    //////    // =========================================================
    //////    if (targetQty == 0)
    //////    {
    //////        if (giftLine != null)
    //////        {
    //////            dataBenefitFound.assigned_gifts -= currentQty;
    //////            GlobalTotalManualGiftsApplied -= currentQty;

    //////            SaleOrder.order_line.Remove(
    //////                SaleOrder.order_line.First(x => x[2] == giftLine)
    //////            );

    //////            OrderLines.Remove(giftLine);
    //////            realApplied.Remove(giftLine);

    //////            if (ShouldSaveToo)
    //////                await saleOrderLineDb.DeleteAsync(giftLine);
    //////        }

    //////        product.qty_gift = 0;
    //////        product.qty_gift_virtual = 0;

    //////        await promotionEngineRunner
    //////            .AddApplyPromotion(SaleOrder, benefit, -1, saleOrderPromotions);

    //////        return true;
    //////    }

    //////    // =========================================================
    //////    // ESCENARIO 2: CREAR NUEVA LINEA
    //////    // =========================================================
    //////    if (giftLine == null)
    //////    {
    //////        giftLine = new sale_order_line
    //////        {
    //////            _order_id = SaleOrder.id,
    //////            product_id = product.id,
    //////            product_display = product.name,
    //////            product_code = product.code,
    //////            product_uom_qty_real = targetQty,
    //////            product_uom_qty = targetQty,
    //////            virtual_price_no_tax = (decimal)product.list_price,
    //////            virtual_line_subtotal = targetQty * (decimal)product.list_price,
    //////            amount_discount = targetQty * (decimal)product.list_price,
    //////            discount = 100,
    //////            is_gift = true,
    //////            is_manual = true,
    //////            product_tmpl_id = product._product_tmpl_id,
    //////            product_id_origin = saleOrderLineOrigin.product_id
    //////        };

    //////        if (ShouldSaveToo)
    //////            await saleOrderLineDb.InsertAsyncAutoOrdinal(giftLine);

    //////        SaleOrder.order_line.Add(new OrderLineWrapper(giftLine));
    //////        OrderLines.Add(giftLine);
    //////        realApplied.Add(giftLine);
    //////    }
    //////    else
    //////    {
    //////        giftLine.product_uom_qty_real = targetQty;
    //////        giftLine.product_uom_qty = targetQty;
    //////        giftLine.virtual_line_subtotal =
    //////            targetQty * giftLine.virtual_price_no_tax;
    //////        giftLine.amount_discount =
    //////            targetQty * giftLine.virtual_price_no_tax;

    //////        if (ShouldSaveToo)
    //////            await saleOrderLineDb.UpdateAsync(giftLine);
    //////    }

    //////    // =========================================================
    //////    // ACTUALIZAR CONTADORES
    //////    // =========================================================

    //////    var totalAssignedPreviewFinal = _promoGifts.Sum(x => x.qty_gift_virtual);

    //////    dataBenefitFound.assigned_gifts = totalAssignedPreviewFinal;
    //////    GlobalTotalManualGiftsApplied += delta;

    //////    if (dataBenefitFound.assigned_gifts >= dataBenefitFound.max_gifts)
    //////    {
    //////        await promotionEngineRunner
    //////            .AddApplyPromotion(SaleOrder, benefit, 1, saleOrderPromotions);
    //////    }

    //////    product.qty_gift = targetQty;
    //////    product.qty_gift_virtual = targetQty;


    //////    var listPromotionData = new List<PromotionEvalItemV2>();

    //////    listPromotionData = !string.IsNullOrEmpty(giftLine.promotion_data) ?
    //////        JsonConvert.DeserializeObject<List<PromotionEvalItemV2>>(giftLine.promotion_data) :
    //////        new List<PromotionEvalItemV2>();

    //////    listPromotionData.Add(product.promotionEvalItem);

    //////    giftLine.promotion_data = JsonConvert.SerializeObject(
    //////            listPromotionData
    //////        );

    //////    DMSA.Models.Odoo.Promotions.Tools.SetPromotionData(giftLine, listPromotionData);


    //////    return true;
    //////}

    private async Task<bool> UpdateGiftIsolated(
    product_product product,
    bool ShouldSaveToo,
    PromotionEvalItemV2 benefit)
    {
        int targetQty = product.qty_gift_virtual;
        int currentQty = product.qty_gift;

        if (targetQty < 0)
        {
            await Toast.Make("Cantidad inválida.").Show();
            return false;
        }

        var promotionEngineRunner = new PromotionEngineRunner();

        if (!await promotionEngineRunner.CanApplyPromotion(SaleOrder, benefit, saleOrderPromotions))
        {
            // lógica intacta (comentado)
        }

        var dataBenefitFound = await promotionEngineRunner
            .GetDataBenefit(SaleOrder, benefit, saleOrderPromotions);

        if (dataBenefitFound == null)
            return false;

        if (dataBenefitFound.max_gifts != benefit.MaxAllowedGifts)
            dataBenefitFound.max_gifts = benefit.MaxAllowedGifts;

        //cachear sum
        var totalAssignedPreview = _promoGifts.Sum(x => x.qty_gift_virtual);

        if (totalAssignedPreview > dataBenefitFound.max_gifts)
        {
            await Application.Current.Windows[0]
                .Page.DisplayAlert("Información", "Cantidad máxima alcanzada en pedido.", "OK");
            return false;
        }

        var saleOrderLineDb = new SaleOrderLineDb(App.Session.odooConnection.DbNameSqlite);

        //cachear líneas
        var orderLines = SaleOrder.order_line
            .Where(x => x[2] != null)
            .Select(x => (sale_order_line)x[2])
            .ToList();

        sale_order_line saleOrderLineOrigin = null;

        //cache JSON de reglas (CRÍTICO)
        var ruleCache = new Dictionary<string, int[]>();

        foreach (var line in orderLines)
        {
            if (line.is_gift) continue;

            foreach (var ruleMatch in benefit.RuleSet)
            {
                if (!ruleCache.TryGetValue(ruleMatch.ProductTmplIds, out var ids))
                {
                    ids = JsonConvert.DeserializeObject<int[]>(ruleMatch.ProductTmplIds);
                    ruleCache[ruleMatch.ProductTmplIds] = ids;
                }

                if (ids.Contains(line.product_tmpl_id))
                {
                    saleOrderLineOrigin = line;
                    break;
                }
            }

            if (saleOrderLineOrigin != null)
                break;
        }

        if (saleOrderLineOrigin == null)
            return false;

        // buscar gift line optimizado
        var giftLine = orderLines
            .FirstOrDefault(line => line.product_id == product.id && line.is_gift);

        int delta = targetQty - currentQty;

        // =========================================================
        // ESCENARIO 1: ELIMINAR
        // =========================================================
        if (targetQty == 0)
        {
            if (giftLine != null)
            {
                dataBenefitFound.assigned_gifts -= currentQty;
                GlobalTotalManualGiftsApplied -= currentQty;

                var wrapper = SaleOrder.order_line.First(x => x[2] == giftLine);

                SaleOrder.order_line.Remove(wrapper);
                OrderLines.Remove(giftLine);
                realApplied.Remove(giftLine);

                if (ShouldSaveToo)
                    await saleOrderLineDb.DeleteAsync(giftLine);
            }

            product.qty_gift = 0;
            product.qty_gift_virtual = 0;

            await promotionEngineRunner
                .AddApplyPromotion(SaleOrder, benefit, -1, saleOrderPromotions);

            return true;
        }

        // =========================================================
        // ESCENARIO 2: CREAR / ACTUALIZAR
        // =========================================================
        if (giftLine == null)
        {
            giftLine = new sale_order_line
            {
                _order_id = SaleOrder.id,
                product_id = product.id,
                product_display = product.name,
                product_code = product.code,
                product_uom_qty_real = targetQty,
                product_uom_qty = targetQty,
                virtual_price_no_tax = (decimal)product.list_price,
                virtual_line_subtotal = targetQty * (decimal)product.list_price,
                amount_discount = targetQty * (decimal)product.list_price,
                discount = 100,
                is_gift = true,
                is_manual = true,
                product_tmpl_id = product._product_tmpl_id,
                product_id_origin = saleOrderLineOrigin.product_id
            };

            if (ShouldSaveToo)
                await saleOrderLineDb.InsertAsyncAutoOrdinal(giftLine);

            SaleOrder.order_line.Add(new OrderLineWrapper(giftLine));
            OrderLines.Add(giftLine);
            realApplied.Add(giftLine);
        }
        else
        {
            var price = giftLine.virtual_price_no_tax;

            giftLine.product_uom_qty_real = targetQty;
            giftLine.product_uom_qty = targetQty;
            giftLine.virtual_line_subtotal = targetQty * price;
            giftLine.amount_discount = targetQty * price;

            if (ShouldSaveToo)
                await saleOrderLineDb.UpdateAsync(giftLine);
        }

        // =========================================================
        // ACTUALIZAR CONTADORES
        // =========================================================

        var totalAssignedPreviewFinal = _promoGifts.Sum(x => x.qty_gift_virtual);

        dataBenefitFound.assigned_gifts = totalAssignedPreviewFinal;
        GlobalTotalManualGiftsApplied += delta;

        if (dataBenefitFound.assigned_gifts >= dataBenefitFound.max_gifts)
        {
            await promotionEngineRunner
                .AddApplyPromotion(SaleOrder, benefit, 1, saleOrderPromotions);
        }

        product.qty_gift = targetQty;
        product.qty_gift_virtual = targetQty;

        //cache JSON promotion_data
        List<PromotionEvalItemV2> listPromotionData;

        if (!string.IsNullOrEmpty(giftLine.promotion_data))
        {
            listPromotionData = JsonConvert
                .DeserializeObject<List<PromotionEvalItemV2>>(giftLine.promotion_data);
        }
        else
        {
            listPromotionData = new List<PromotionEvalItemV2>();
        }

        listPromotionData.Add(product.promotionEvalItem);

        giftLine.promotion_data = JsonConvert.SerializeObject(listPromotionData);

        DMSA.Models.Odoo.Promotions.Tools.SetPromotionData(giftLine, listPromotionData);

        return true;
    }


    private async Task AddGiftNxn(PromotionEvalItemV2 benefit)
    {
        //SE COMPARA CON EL MISMO PRODUCTO ORIGEN PORQUE ES NXN
        PromotionEngineRunner promotionEngineRunner = new PromotionEngineRunner();

        var productDb = new ProductProductDb(App.Session.odooConnection.DbNameSqlite);

        foreach (var ruleMatch in benefit.RuleSet)
        {
            int[] listIdsProd = Newtonsoft.Json.JsonConvert.DeserializeObject<int[]>(ruleMatch.ProductTmplIds);

            foreach(var productIdCompare in listIdsProd)
            {
                if (benefit.Promotion._selection_type_id == 1)
                {
                    //Aqui debe ser solo un producto
                    var listDetailProd = benefit.Promotion._product_details_promotion_ids
                                        .Where(x => x._promo_id == 0 && x._bonus_id > 0 && x._bonus_id == ruleMatch.id).ToList();

                    //var productGift = await productDb.GetByProductsTemplate(listIdsProd.ToArray());

                    if (listDetailProd != null && listDetailProd.Count > 0)
                    {

                        if (listDetailProd.Count > 1)
                        {
                            Debug.WriteLine($"Advertencia: Más de un producto de regalo encontrado para la regla {ruleMatch.id} de la promoción {benefit.Promotion.name}. Se tomará el primero.");
                        }
                    }
                }

                var productGift = await productDb.GetByProductTemplate(productIdCompare, SaleOrder._pricelist_id);
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

                    if (!await promotionEngineRunner.CanApplyPromotion(SaleOrder, benefit, saleOrderPromotions))
                    {
                        Debug.WriteLine($"{benefit.Promotion.name} ya ha sido aplicado maximo de veces - AddAutoGiftsAsync");
                        return;
                    }

                    //}

                    int line_qty = 0;

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
                                    await promotionEngineRunner.AddApplyPromotion(SaleOrder, benefit, 1, saleOrderPromotions);
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

                            if (lineObject.product_tmpl_id == productIdCompare)
                            {
                                var old_promotion_data = lineObject.promotion_data;
                                var listPromotionData = new List<PromotionEvalItemV2>();

                                listPromotionData = !string.IsNullOrEmpty(lineObject.promotion_data) ?
                                    Newtonsoft.Json.JsonConvert.DeserializeObject<List<PromotionEvalItemV2>>(lineObject.promotion_data) :
                                    new List<PromotionEvalItemV2>();

                                listPromotionData.Add(productGift.promotionEvalItem);

                                lineObject.promotion_data = Newtonsoft.Json.JsonConvert.SerializeObject(
                                        listPromotionData
                                    );

                                DMSA.Models.Odoo.Promotions.Tools.SetPromotionDataGift(lineObject, listPromotionData);

                                line_qty = (int) lineObject.product_uom_qty_real;
                            }
                        }
                    }

                    bool ShouldSaveToo = false;
                    int ordinal = 0;

                    if (!productExistsInOrder)
                    {
                        //Proceso de reconversión de qty basado en N x N
                        int qty_assign = line_qty / ruleMatch.value; //ruleMatch.AllowedGifts;

                        if (qty_assign == 0) continue;

                        productGift.qty_gift = qty_assign;
                        Debug.WriteLine($"Cargado regalo automático para promoción {benefit.Promotion.name}: {productGift.name}");
                        _promoGiftsAuto.Add(productGift);

                        var line = new sale_order_line
                        {
                            _order_id = SaleOrder.id,
                            sequence = ordinal,
                            product_id = productGift.id,
                            product_display = productGift.name,
                            product_code = productGift.code,
                            qty_to_deliver = qty_assign,
                            product_uom_qty_real = qty_assign,
                            product_uom_qty = qty_assign,
                            uom_category_display = productGift.uom_sale_display,
                            price_subtotal = 0,
                            virtual_line_subtotal = (decimal)(qty_assign * productGift.list_price),
                            amount_discount = (decimal)(qty_assign * productGift.list_price),
                            discount = 100,
                            price_tax = 0,
                            price_total = 0,
                            is_gift = true,
                            is_manual = false,
                            product_tmpl_id = productGift._product_tmpl_id,
                            _virtual_price_no_tax = (decimal)productGift.list_price,
                            product_id_origin = ruleMatch.ProductIdOrigin,
                            promotion_data = Newtonsoft.Json.JsonConvert.SerializeObject(
                                new List<PromotionEvalItemV2> { productGift.promotionEvalItem }
                            )
                        };

                        DMSA.Models.Odoo.Promotions.Tools.SetPromotionData(line, new List<PromotionEvalItemV2> { productGift.promotionEvalItem });

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
    }

    //public decimal ComputeTotal
    //{
    //    get
    //    {
    //        decimal tmp_Subtotal = 0;
    //        if(realApplied!=null && realApplied.Count>0)
    //        {
    //            foreach (var applied in realApplied)
    //            {                    
    //                tmp_Subtotal += (decimal) (applied.product_uom_qty_real * applied.virtual_price_no_tax);
    //            }
    //        }
    //        return tmp_Subtotal;
    //    }
    //}

    public decimal ComputeTotal
    {
        get
        {
            if (realApplied == null) return 0m;

            decimal total = 0m;
            foreach (var a in realApplied)
            {
                total += a.product_uom_qty_real * (decimal)a.virtual_price_no_tax;
            }
            return total;
        }
    }

    //public decimal ComputeTotalQty
    //{
    //    get
    //    {
    //        decimal tmp_SubtotalQty = 0;
    //        if (realApplied != null && realApplied.Count > 0)
    //        {
    //            foreach (var applied in realApplied)
    //            {
    //                tmp_SubtotalQty += (decimal)(applied.product_uom_qty_real);
    //            }
    //        }
    //        return tmp_SubtotalQty;
    //    }
    //}

    public decimal ComputeTotalQty
    {
        get
        {
            if (realApplied == null) return 0m;

            decimal total = 0m;
            foreach (var a in realApplied)
            {
                total += (decimal)a.product_uom_qty_real;
            }
            return total;
        }
    }

    private void FiltroArtPromo_TextChanged(object sender, TextChangedEventArgs e)
    {
        //var vm = BindingContext as MyViewModel;
        //vm?.ApplyFilter(e.NewTextValue);
        if (e.NewTextValue.Length < 3 && !e.NewTextValue.Trim().Equals(""))
        {            
            return;
        }
        ApplyFilterGifts(e.NewTextValue);
    }

    public void ApplyFilterGifts(string text)
    {
        if(promoGifts == null || promoGifts.Count == 0)
        {
            promoGiftsFiltered = new ObservableCollection<product_product>();
            OnPropertyChanged(nameof(promoGiftsFiltered));
            return;
        }

        if (string.IsNullOrWhiteSpace(text))
        {
            promoGiftsFiltered = new ObservableCollection<product_product>(promoGifts);
        }
        else
        {
            promoGiftsFiltered = new ObservableCollection<product_product>(
                promoGifts.Where(x => x.display_name.Contains(text, StringComparison.OrdinalIgnoreCase))
            );
        }

        OnPropertyChanged(nameof(promoGiftsFiltered));
    }


    public event PropertyChangedEventHandler PropertyChanged;
    private void OnPropertyChanged(string property) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));

    public Action<List<PromotionEvalResultV2>> ClosePopupAction { get; set; }
    public ObservableCollection<sale_order_line> OrderLines { get; internal set; }

    private async void OnCloseButtonClicked(object sender, EventArgs e)
    {
        //if (GlobalTotalManualGiftsRemoved < GlobalTotalManualGiftsForRemove)
        //{
        //    await App.Current.Windows[0].Page.DisplayAlert($"No se puede continuar", $"Se requiere eliminar regalos manuales {GlobalTotalManualGiftsRemoved}/{GlobalTotalManualGiftsForRemove}","Ok");
        //    return;
        //}

        if(GlobalTotalManualGiftsApplied < GlobalTotalManualGiftsAllowed)
        {            
            var leave = await App.Current.Windows[0].Page.DisplayAlert($"¿Desea continuar?", $"No se han aplicado todos los {GlobalTotalManualGiftsAllowed} regalos de los bonificados manuales", "Si", "No");

            if (!leave)
            {
                return;
            }
        }

        ClosePopupAction?.Invoke(ItemsData.ToList());
    }

    private async void Qty_Entry_Completed(object sender, EventArgs e)
    {
        bool ShouldSaveToo = false;
        Entry entry = (Entry)sender;
        product_product product = (product_product)entry.BindingContext;

        if (product.qty_gift_virtual < 0)
        {
            product.qty_gift_virtual = product.qty_gift;
            return;
        }

        int previousQty = product.qty_gift; // respaldo real
        int delta = product.qty_gift_virtual - product.qty_gift;

        bool success = await UpdateGiftIsolated(product, ShouldSaveToo, selectedPromoEvalItem);

        if (!success)
        {
            // revertir valor visual
            product.qty_gift_virtual = previousQty;
            OnPropertyChanged(nameof(product.qty_gift_virtual));
        }

        OnPropertyChanged(nameof(ComputeTotal));
        OnPropertyChanged(nameof(ComputeTotalQty));
    }

    private async void Disc_Entry_Completed(object sender, EventArgs e)
    {
        bool ShouldSaveToo = false;
        Button viewObject = (Button) sender;
        PromoRuleMatch ruleMatch = (PromoRuleMatch) viewObject.BindingContext;
        await ApplyDiscountRule(SaleOrder, ruleMatch);
        await ChangeDiscountEvent(ruleMatch, false);
    }

    private async Task ApplyDiscountRule(sale_order saleOrder, PromoRuleMatch ruleMatch)
    {
        PromotionEngineRunner promotionEngineRunner = new PromotionEngineRunner();
               
        if (ruleMatch.IsDiscount)
        {
            int maxProductTarget = ruleMatch.ProductTmplIdMaxTotal;
            if (ruleMatch.variable == "qty_product_unts")
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
                    List<PromotionEvalItemV2> listPromotionData = new List<PromotionEvalItemV2>();

                    listPromotionData = !string.IsNullOrEmpty(lineToDiscount.promotion_data) ?
                                Newtonsoft.Json.JsonConvert.DeserializeObject<List<PromotionEvalItemV2>>(lineToDiscount.promotion_data) :
                                new List<PromotionEvalItemV2>();
                        
                    decimal originalPrice = lineToDiscount.price_unit;
                    decimal virtual_price_no_tax = lineToDiscount.virtual_price_no_tax;

                    decimal discountAmount = (virtual_price_no_tax * lineToDiscount.product_uom_qty_real) * (decimal)(discountPercentage / 100);
                    lineToDiscount.discount = (decimal)discountPercentage;
                    lineToDiscount.amount_discount = discountAmount;

                    lineToDiscount.price_subtotal = (virtual_price_no_tax * lineToDiscount.product_uom_qty_real) - discountAmount;
                    lineToDiscount.price_tax = (lineToDiscount.price_subtotal * lineToDiscount.virtual_iva_percentage) / 100;
                    lineToDiscount.price_total = lineToDiscount.price_subtotal + lineToDiscount.price_tax;

                    lineToDiscount.virtual_line_subtotal = virtual_price_no_tax * lineToDiscount.product_uom_qty_real;

                    foreach(var promoItem in listPromotionData)
                    {
                        foreach(var rulesInside in promoItem.RuleSet)
                        {
                            if(rulesInside.id == ruleMatch.id)
                            {
                                rulesInside.discount = ruleMatch.discount;
                            }
                        }
                    }

                    lineToDiscount.promotion_data = Newtonsoft.Json.JsonConvert.SerializeObject(listPromotionData);

                    //await promotionEngineRunner.AddApplyPromotion(saleOrder, promoResItem, 1, saleOrderPromotions);

                    Debug.WriteLine($"Descuento aplicado: {discountPercentage}% al producto ID {productTemplateId}");
                        
                }
            }
            
        }
    }

    private async Task ChangeDiscountEvent(PromoRuleMatch rule, bool ShouldSaveToo)
    {
        if (rule.discount > rule.discount_base || rule.discount < 2)
        {
            rule.discount = rule.discount_base;
            await Toast.Make("El descuento no puede ser mayor a " + rule.discount_base + "% ni menos del 2%").Show();
            
            OnPropertyChanged(nameof(promoDiscounts)); 
            return;
        }
        else
        {
            await Toast.Make("El descuento del " + rule.discount + "% aplicado!").Show();
        }
    }

    private async Task ChangeQtyEvent(product_product product, bool ShouldSaveToo)
    {
        if (product.qty_gift_virtual < 0)
        {
            product.qty_gift_virtual = product.qty_gift;
            return;
        }

        int previousQty = product.qty_gift; // respaldo real
        int delta = product.qty_gift_virtual - product.qty_gift;

        bool success = await UpdateGiftIsolated(product, ShouldSaveToo, selectedPromoEvalItem);

        if (!success)
        {
            // revertir valor visual
            product.qty_gift_virtual = previousQty;
            OnPropertyChanged(nameof(product.qty_gift_virtual));
        }

        OnPropertyChanged(nameof(ComputeTotal));
        OnPropertyChanged(nameof(ComputeTotalQty));
    }
}