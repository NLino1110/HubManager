using CommunityToolkit.Maui.Alerts;
using DMOrders.Services.Promotions;
using DMSA.Models.Odoo.DMOrders.promotions.abstractCustom;
using DMSA.Models.Odoo.Native;
using DMSA.Models.Odoo.Sales.promotions.abstractCustom;
using DMSA.Sync.Core.Database.Sqlite;
using Microsoft.Maui.Controls.Shapes;
using Newtonsoft.Json;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;

namespace DMOrders.Pages.Fragments.Orders.modals;

public partial class PromocionesViewer : ContentView
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
    private List<sale_order_line> wholeRealApplied { get; set; }
    private List<OrderLineWrapper> SaleOrdersLinesTmp { get; set; }

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
    private PromotionEvalItem selectedPromoEvalItem { get; set; }
    public ObservableCollection<PromotionEvalResult> ItemsData
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
                _ItemsDataBenefits = new ObservableCollection<PromotionEvalItem>();
                _promoGiftsAuto = new ObservableCollection<product_product>();
                //_ItemsDataBenefits = new ObservableCollection<PromotionBenefit>();
            }

            OnPropertyChanged(nameof(ItemsData));
            OnPropertyChanged(nameof(ItemsDataBenefits));
            OnPropertyChanged(nameof(ComputeTotal));
            OnPropertyChanged(nameof(ComputeTotalQty));
        }
    }


    private ObservableCollection<PromotionEvalResult> _itemsFullPromos;

    public ObservableCollection<PromotionEvalItem> ItemsDataBenefits
    {
        get => _ItemsDataBenefits;
        set
        {
            _ItemsDataBenefits = value;
            OnPropertyChanged(nameof(ItemsDataBenefits));
        }
    }

    private ObservableCollection<PromotionEvalItem> _ItemsDataBenefits;

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

                    if(benefit.Promotion._promotion_type_id == 2 && benefit.Promotion._selection_type_id == 2)
                    {
                        
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
        wholeRealApplied = new List<sale_order_line>();
        SaleOrdersLinesTmp = SaleOrder.order_line;

        CachedPreselected();

        if(App.Session.odooConnection.IsTestMode)
        {
            btnApplyAndContinue.IsVisible = true;
        }

        BenefitsForShow = true;
    }

    private void CachedPreselected()
    {
        var giftLineForAdd = SaleOrdersLinesTmp
            .Select(x => (sale_order_line)x[2])
            .Where(l => l.is_gift && l.is_manual)
            .ToList();

        if (giftLineForAdd.Any())
        {
            foreach (var item in giftLineForAdd)
            {
                wholeRealApplied.Add(item);
            }
        }
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

    private async Task AddAutoGiftsAsync(PromotionEvalItem benefit)
    {
        var promotionEngineRunner = new PromotionEngineRunner();

        // Cache líneas
        var orderLines = SaleOrder.order_line
            .Where(x => x[2] != null)
            .Select(x => (sale_order_line)x[2])
            .ToList();

        // HashSet para evitar Any()
        var existingCodes = new HashSet<string>(
            _promoGiftsAuto.Where(x => x != null).Select(x => x.default_code)
        );

        // Cache JSON
        var jsonCache = new Dictionary<string, List<PromotionEvalItem>>();

        // Cache CanApplyPromotion (evita repetir)
        bool? canApplyPromotionCache = null;


        var allProductIds = new HashSet<int>();

        foreach (var ruleMatch in benefit.RuleSet)
        {
            int productIdCompare = ruleMatch.ProductIdOrigin;

            if (productIdCompare == 0)
                productIdCompare = ruleMatch.ProductTmplId;

            if (benefit.Promotion._selection_type_id == 1)
            {
                var listDetailProd = benefit.Promotion._product_details_promotion_ids
                    .Where(x => x._promo_id == 0 && x._bonus_id > 0 && x._bonus_id == ruleMatch.id)
                    .ToList();

                if (listDetailProd.Count > 0)
                    productIdCompare = listDetailProd[0]._product_id;
            }

            allProductIds.Add(productIdCompare);
        }

        var products = await productDb.GetByProductsTemplate(allProductIds.ToArray(), SaleOrder._pricelist_id);
        var productDict = products
            .Where(p => p != null)
            .GroupBy(p => p._product_tmpl_id)
            .ToDictionary(g => g.Key, g => g.First());

        foreach (var ruleMatch in benefit.RuleSet)
        {
            int productIdCompare = ruleMatch.ProductIdOrigin;

            if (productIdCompare == 0)
                productIdCompare = ruleMatch.ProductTmplId;

            if (benefit.Promotion._selection_type_id == 1)
            {
                var listDetailProd = benefit.Promotion._product_details_promotion_ids
                    .Where(x => x._promo_id == 0 && x._bonus_id > 0 && x._bonus_id == ruleMatch.id)
                    .ToList();

                if (listDetailProd.Count > 0)
                {
                    productIdCompare = listDetailProd[0]._product_id;

                    if (listDetailProd.Count > 1)
                    {
                        Debug.WriteLine($"Advertencia: múltiples regalos en regla {ruleMatch.id}");
                    }
                }
            }

            if (!productDict.TryGetValue(productIdCompare, out var productGift))
                continue;

            //if (productGift == null)
                //continue;

            productGift.promotionEvalItem = benefit;
            productGift.qty_gift = ruleMatch.AllowedGifts;

            bool productExistsInOrder = existingCodes.Contains(productGift.default_code);

            // CanApplyPromotion cacheado
            if (canApplyPromotionCache == null)
            {
                canApplyPromotionCache = await promotionEngineRunner
                    .CanApplyPromotion(SaleOrder, benefit, saleOrderPromotions);
            }

            if (!canApplyPromotionCache.Value)
            {
                Debug.WriteLine($"{benefit.Promotion.name} max aplicado");
                return;
            }

            Debug.WriteLine($"Auto gift: {productGift.name}");
            _promoGiftsAuto.Add(productGift);
            existingCodes.Add(productGift.default_code);

            foreach (var lineObject in orderLines)
            {
                //Caso gift existente
                if (lineObject.product_id == productGift.id && lineObject.is_gift)
                {
                    if (benefit.Promotion._selection_type_id == 1)
                    {
                        lineObject.product_uom_qty_real++;
                        lineObject.product_uom_qty = lineObject.product_uom_qty_real;
                        lineObject.amount_discount =
                            lineObject.product_uom_qty_real * lineObject.virtual_price_no_tax;

                        await promotionEngineRunner
                            .AddApplyPromotion(SaleOrder, benefit, 1, saleOrderPromotions);
                    }
                    else
                    {
                        return;
                    }
                }

                //Línea origen
                if (lineObject.product_id == ruleMatch.ProductIdOrigin)
                {
                    List<PromotionEvalItem> listPromotionData;

                    if (!string.IsNullOrEmpty(lineObject.promotion_data))
                    {
                        if (!jsonCache.TryGetValue(lineObject.promotion_data, out listPromotionData))
                        {
                            listPromotionData = JsonConvert
                                .DeserializeObject<List<PromotionEvalItem>>(lineObject.promotion_data);

                            jsonCache[lineObject.promotion_data] = listPromotionData;
                        }
                    }
                    else
                    {
                        listPromotionData = new List<PromotionEvalItem>();
                    }

                    listPromotionData.Add(productGift.promotionEvalItem);

                    lineObject.promotion_data = JsonConvert.SerializeObject(listPromotionData);

                    DMSA.Models.Odoo.Promotions.Tools.SetPromotionDataGift(lineObject, listPromotionData);
                }
            }

            if (!productExistsInOrder)
            {
                int qty_assign = ruleMatch.AllowedGifts;

                if (benefit.Promotion._promotion_type_id == 4)
                    qty_assign = benefit.MaxAllowedGifts;

                var line = new sale_order_line
                {
                    _order_id = SaleOrder.id,
                    sequence = 0,
                    product_id = productGift.id,
                    product_tmpl_id = productGift._product_tmpl_id,
                    product_display = productGift.name,
                    product_code = productGift.code,
                    qty_to_deliver = qty_assign,
                    product_uom_qty_real = qty_assign,
                    product_uom_qty = qty_assign,
                    uom_category_display = productGift.uom_sale_display,
                    price_subtotal = 0,
                    _virtual_price_no_tax = (decimal)productGift.list_price,
                    amount_discount = qty_assign * (decimal)productGift.list_price,
                    discount = 100,
                    price_tax = 0,
                    price_total = 0,
                    is_gift = true,
                    product_id_origin = ruleMatch.ProductIdOrigin,
                    promotion_data = JsonConvert.SerializeObject(
                        new List<PromotionEvalItem> { productGift.promotionEvalItem }
                    )
                };

                //DMSA.Models.Odoo.Promotions.Tools.SetPromotionDataGift(line,new List<PromotionEvalItemV2> { productGift.promotionEvalItem });
                DMSA.Models.Odoo.Promotions.Tools.SetPromotionData(line, new List<PromotionEvalItem> { productGift.promotionEvalItem });

                SaleOrder.order_line.Add(new OrderLineWrapper(line));
                OrderLines.Add(line);

                await promotionEngineRunner
                    .AddApplyPromotion(SaleOrder, benefit, 1, saleOrderPromotions);
            }
        }
    }

    private async void detail_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection?.Count > 0)
        {
            IsLoading = true;
            var selected = (PromotionEvalItem)e.CurrentSelection[0];
            await HandlePromotionSelection(selected);

            await Task.Delay(400);
            IsLoading = false;
        }
    }

    private async Task HandlePromotionSelection(PromotionEvalItem selectedPromoEvalItem)
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

    private async Task HandleGiftPromotion(List<PromotionEvalItem> promoItems, ProductProductDb productDb)
    {
        var existingCodes = new HashSet<string>();
        var promotionCache = new Dictionary<string, List<PromotionEvalItem>>();

        //var relevantLines = SaleOrder.order_line
        //    .Select(x => (sale_order_line)x[2])
        //    .Where(l => l.is_gift)
        //    .ToList();

        var relevantLines = SaleOrdersLinesTmp
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

    private void HandleDiscountPromotion(List<PromotionEvalItem> promoItems)
    {
        BorderBenefits.IsVisible = false;
        BorderDiscount.IsVisible = true;

        promoDiscounts = new ObservableCollection<PromoRuleMatch>(
            promoItems.SelectMany(x => x.RuleSet)
        );

        foreach(var promoItem in promoDiscounts)
        {
            foreach(var sequenceData in promoItem.ProductSequenceApplyList)
            {
                var order_line_match = OrderLines.Where(x => x.product_id == sequenceData.product_id
                    && x.sequence == sequenceData.sequence).FirstOrDefault();

                if (order_line_match != null)
                {
                    promoItem.discount = (int)order_line_match.discount;
                }
            }            
        }        

        OnPropertyChanged(nameof(promoDiscounts));
    }

    private List<PromotionEvalItem> GetPromotionData(
    string json,
    Dictionary<string, List<PromotionEvalItem>> cache)
    {
        if (string.IsNullOrEmpty(json))
            return new List<PromotionEvalItem>();

        if (!cache.TryGetValue(json, out var result))
        {
            result = JsonConvert.DeserializeObject<List<PromotionEvalItem>>(json);
            cache[json] = result;
        }

        return result;
    }

    private readonly SemaphoreSlim _btnLock = new(1, 1);

    private async void AddGiftAndSave(object sender, EventArgs e)
    {
        if (sender is not Button button)
            return;

        if (button.BindingContext is not product_product product)
            return;

        await _btnLock.WaitAsync();
        button.IsEnabled = false;

        try
        {
            bool ShouldSaveToo = false;
            IsLoading = true;

            product.qty_gift_virtual++;
            await ChangeQtyEvent(product, ShouldSaveToo);

            IsLoading = false;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error AddGiftAndSave: {ex}");
        }
        finally
        {
            _btnLock.Release();
            button.IsEnabled = true;
        }
    }

    private async void SubstractGift(object sender, EventArgs e)
    {
        if (sender is not Button button)
            return;

        if (button.BindingContext is not product_product product)
            return;

        await _btnLock.WaitAsync();
        button.IsEnabled = false;
        try
        {
            bool ShouldSaveToo = false;

            if (product.qty_gift_virtual <= 0)
                return;

            product.qty_gift_virtual--;

            await ChangeQtyEvent(product, ShouldSaveToo);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error SubstractGift: {ex}");
        }
        finally
        {
            _btnLock.Release();
            button.IsEnabled = true;
        }
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
    private readonly SemaphoreSlim _giftLock = new(1, 1);

    private async Task<bool> UpdateGiftIsolated(product_product product, bool ShouldSaveToo, PromotionEvalItem benefit)
    {
        await _giftLock.WaitAsync();

        bool is_manual = false;
        
        try
        {
            if( benefit.Promotion._promotion_type_id == 2 && benefit.Promotion._selection_type_id == 2)
            {
                is_manual = true;
            }

            int targetQty = product.qty_gift_virtual;
            int currentQty = product.qty_gift;

            if (targetQty < 0)
            {
                await Toast.Make("Cantidad inválida.").Show();
                return false;
            }

            var promotionEngineRunner = new PromotionEngineRunner();

            await promotionEngineRunner.CanApplyPromotion(SaleOrder, benefit, saleOrderPromotions);

            var dataBenefitFound = await promotionEngineRunner
                .GetDataBenefit(SaleOrder, benefit, saleOrderPromotions);

            if (dataBenefitFound == null)
                return false;

            if (dataBenefitFound.max_gifts != benefit.MaxAllowedGifts)
                dataBenefitFound.max_gifts = benefit.MaxAllowedGifts;

            // SNAPSHOT SEGURO
            var promoSnapshot = _promoGifts?.ToList() ?? new List<product_product>();
            var totalAssignedPreview = promoSnapshot.Sum(x => x.qty_gift_virtual);

            if (totalAssignedPreview > dataBenefitFound.max_gifts)
            {
                await MainThread.InvokeOnMainThreadAsync(async () =>
                {
                    var page = Application.Current?.MainPage;
                    if (page != null)
                        await page.DisplayAlert("Información", "Cantidad máxima alcanzada en pedido.", "OK");
                });

                return false;
            }

            var saleOrderLineDb = new SaleOrderLineDb(App.Session.odooConnection.DbNameSqlite);

            // SNAPSHOT ORDER LINES
            var orderLines = new List<sale_order_line>();

            var orderLineSnapshot = SaleOrder?.order_line?.ToList() ?? new List<OrderLineWrapper>();

            foreach (var x in orderLineSnapshot)
            {
                if (x == null)
                    continue;

                if (x.Count > 2 && x[2] is sale_order_line line)
                    orderLines.Add(line);
            }

            sale_order_line saleOrderLineOrigin = null;

            // SNAPSHOT RULESET
            var ruleSetSnapshot = benefit.RuleSet?.ToList() ?? new List<PromoRuleMatch>();
            var ruleCache = new Dictionary<string, int[]>();

            foreach (var line in orderLines)
            {
                if (line.is_gift) continue;

                foreach (var ruleMatch in ruleSetSnapshot)
                {
                    if (!ruleCache.TryGetValue(ruleMatch.ProductTmplIds, out var ids))
                    {
                        try
                        {
                            ids = JsonConvert.DeserializeObject<int[]>(ruleMatch.ProductTmplIds);
                        }
                        catch
                        {
                            continue;
                        }

                        ruleCache[ruleMatch.ProductTmplIds] = ids;
                    }

                    if (ids != null && ids.Contains(line.product_tmpl_id))
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

            var giftLine = orderLines
                .FirstOrDefault(line => line.product_id == product.id && line.is_gift);

            int delta = targetQty - currentQty;

            // =========================================================
            // ELIMINAR
            // =========================================================
            if (targetQty == 0)
            {
                if (giftLine != null)
                {
                    dataBenefitFound.assigned_gifts -= currentQty;
                    GlobalTotalManualGiftsApplied -= currentQty;

                    // eliminar seguro
                    var wrapperSnapshot = SaleOrdersLinesTmp.ToList();

                    for (int i = 0; i < wrapperSnapshot.Count; i++)
                    {
                        var x = wrapperSnapshot[i];

                        if (x == null)
                            continue;

                        if (x.Count > 2 && ReferenceEquals(x[2], giftLine))
                        {
                            await MainThread.InvokeOnMainThreadAsync(() =>
                            {
                                SaleOrdersLinesTmp.Remove(x);
                            });
                            break;
                        }
                    }

                    await MainThread.InvokeOnMainThreadAsync(() =>
                    {
                        //OrderLines.Remove(giftLine);
                        realApplied.Remove(giftLine);
                        wholeRealApplied.Remove(giftLine);
                    });

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
            // CREAR / ACTUALIZAR
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
                    is_manual = is_manual,
                    product_tmpl_id = product._product_tmpl_id,
                    product_id_origin = saleOrderLineOrigin.product_id
                };

                if (ShouldSaveToo)
                    await saleOrderLineDb.InsertAsyncAutoOrdinal(giftLine);

                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    SaleOrdersLinesTmp.Add(new OrderLineWrapper(giftLine));
                    //SaleOrder.order_line.Add(new OrderLineWrapper(giftLine));
                    //OrderLines.Add(giftLine);
                    realApplied.Add(giftLine);
                    wholeRealApplied.Add(giftLine);
                });

                DMSA.Models.Odoo.Promotions.Tools.SetPromotionDataGift(giftLine, new List<PromotionEvalItem> { benefit });

                var originProducts = benefit.RuleSet[0].ProductSequenceApplyList;

                //Obtenemos la linea de origen lineaOrigen
                //var lineaOrigen = orderLines.FirstOrDefault(l => l.product_id == giftLine.product_id_origin____ && !l.is_gift);
    
                //if (lineaOrigen == null)
                //    lineaOrigen = saleOrderLineOrigin;

                //DMSA.Models.Odoo.Promotions.Tools.SetPromotionData(lineaOrigen, new List<PromotionEvalItem> { benefit });


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
            // CONTADORES
            // =========================================================

            var promoSnapshotFinal = _promoGifts?.ToList() ?? new List<product_product>();
            var totalAssignedPreviewFinal = promoSnapshotFinal.Sum(x => x.qty_gift_virtual);

            dataBenefitFound.assigned_gifts = totalAssignedPreviewFinal;
            GlobalTotalManualGiftsApplied += delta;

            if (dataBenefitFound.assigned_gifts >= dataBenefitFound.max_gifts)
            {
                await promotionEngineRunner
                    .AddApplyPromotion(SaleOrder, benefit, 1, saleOrderPromotions);
            }

            product.qty_gift = targetQty;
            product.qty_gift_virtual = targetQty;

            // =========================================================
            // JSON SEGURO
            // =========================================================
            List<PromotionEvalItem> listPromotionData;

            try
            {
                //listPromotionData = !string.IsNullOrEmpty(giftLine.promotion_data)
                //    ? JsonConvert.DeserializeObject<List<PromotionEvalItem>>(giftLine.promotion_data)
                //    : new List<PromotionEvalItem>();

                listPromotionData = giftLine.promotionDataList;
            }
            catch
            {
                listPromotionData = new List<PromotionEvalItem>();
            }

            listPromotionData.Add(product.promotionEvalItem);

            giftLine.promotion_data = JsonConvert.SerializeObject(listPromotionData);
                        
            //DMSA.Models.Odoo.Promotions.Tools.SetPromotionDataGift(giftLine, listPromotionData, ruleMatch);

            return true;
        }
        finally
        {
            _giftLock.Release();
        }
    }


    private async Task AddGiftNxn(PromotionEvalItem benefit)
    {
        var promotionEngineRunner = new PromotionEngineRunner();
        var productDb = new ProductProductDb(App.Session.odooConnection.DbNameSqlite);

        var orderLines = SaleOrder.order_line
            .Where(x => x[2] != null)
            .Select(x => (sale_order_line)x[2])
            .ToList();

        var existingCodes = new HashSet<string>(
            _promoGiftsAuto.Where(x => x != null).Select(x => x.default_code)
        );

        var jsonCache = new Dictionary<string, List<PromotionEvalItem>>();

        bool? canApplyPromotionCache = null;

        var allProductIds = new HashSet<int>();

        var tmplIdsCache = new Dictionary<string, int[]>();

        foreach (var ruleMatch in benefit.RuleSet)
        {
            if (!tmplIdsCache.TryGetValue(ruleMatch.ProductTmplIds, out var listIds))
            {
                listIds = JsonConvert.DeserializeObject<int[]>(ruleMatch.ProductTmplIds);
                tmplIdsCache[ruleMatch.ProductTmplIds] = listIds;
            }

            foreach (var id in listIds)
                allProductIds.Add(id);
        }

        var products = await productDb
            .GetByProductsTemplate(allProductIds.ToArray(), SaleOrder._pricelist_id);

        var productDict = products
            .Where(p => p != null)
            .GroupBy(p => p._product_tmpl_id)
            .ToDictionary(g => g.Key, g => g.First());

        // =========================================================
        // LOOP PRINCIPAL
        // =========================================================
        foreach (var ruleMatch in benefit.RuleSet)
        {
            if (!tmplIdsCache.TryGetValue(ruleMatch.ProductTmplIds, out var listIdsProd))
            {
                listIdsProd = JsonConvert.DeserializeObject<int[]>(ruleMatch.ProductTmplIds);
                tmplIdsCache[ruleMatch.ProductTmplIds] = listIdsProd;
            }

            foreach (var productIdCompare in listIdsProd)
            {
                if (!productDict.TryGetValue(productIdCompare, out var productGift))
                    continue;

                productGift.promotionEvalItem = benefit;
                productGift.qty_gift = ruleMatch.AllowedGifts;

                bool productExistsInOrder = existingCodes.Contains(productGift.default_code);

                // Cache CanApplyPromotion
                if (canApplyPromotionCache == null)
                {
                    canApplyPromotionCache = await promotionEngineRunner
                        .CanApplyPromotion(SaleOrder, benefit, saleOrderPromotions);
                }

                if (!canApplyPromotionCache.Value)
                {
                    Debug.WriteLine($"{benefit.Promotion.name} max aplicado");
                    return;
                }

                int line_qty = 0;

                foreach (var lineObject in orderLines)
                {
                    //Gift existente
                    if (lineObject.product_id == productGift.id && lineObject.is_gift)
                    {
                        if (benefit.Promotion._selection_type_id == 1)
                        {
                            await promotionEngineRunner
                                .AddApplyPromotion(SaleOrder, benefit, 1, saleOrderPromotions);
                        }
                        else
                        {
                            return;
                        }
                    }

                    // Línea origen
                    if (lineObject.product_tmpl_id == productIdCompare)
                    {
                        List<PromotionEvalItem> listPromotionData;

                        if (!string.IsNullOrEmpty(lineObject.promotion_data))
                        {
                            if (!jsonCache.TryGetValue(lineObject.promotion_data, out listPromotionData))
                            {
                                listPromotionData = JsonConvert
                                    .DeserializeObject<List<PromotionEvalItem>>(lineObject.promotion_data);

                                jsonCache[lineObject.promotion_data] = listPromotionData;
                            }
                        }
                        else
                        {
                            listPromotionData = new List<PromotionEvalItem>();
                        }

                        listPromotionData.Add(productGift.promotionEvalItem);

                        lineObject.promotion_data = JsonConvert.SerializeObject(listPromotionData);

                        //DMSA.Models.Odoo.Promotions.Tools.SetPromotionDataGift(lineObject, listPromotionData);
                        DMSA.Models.Odoo.Promotions.Tools.SetPromotionData(lineObject, listPromotionData); 

                        line_qty = (int)lineObject.product_uom_qty_real;
                    }
                }

                if (!productExistsInOrder)
                {
                    int qty_assign = line_qty / ruleMatch.value;

                    if (qty_assign == 0)
                        continue;

                    productGift.qty_gift = qty_assign;

                    Debug.WriteLine($"Auto gift NxN: {productGift.name}");
                    _promoGiftsAuto.Add(productGift);
                    existingCodes.Add(productGift.default_code);

                    var line = new sale_order_line
                    {
                        _order_id = SaleOrder.id,
                        sequence = 0,
                        product_id = productGift.id,
                        product_display = productGift.name,
                        product_code = productGift.code,
                        qty_to_deliver = qty_assign,
                        product_uom_qty_real = qty_assign,
                        product_uom_qty = qty_assign,
                        uom_category_display = productGift.uom_sale_display,
                        price_subtotal = 0,
                        virtual_line_subtotal = qty_assign * (decimal)productGift.list_price,
                        amount_discount = qty_assign * (decimal)productGift.list_price,
                        discount = 100,
                        price_tax = 0,
                        price_total = 0,
                        is_gift = true,
                        is_manual = false,
                        product_tmpl_id = productGift._product_tmpl_id,
                        _virtual_price_no_tax = (decimal)productGift.list_price,
                        product_id_origin = ruleMatch.ProductIdOrigin,
                        promotion_data = JsonConvert.SerializeObject(
                            new List<PromotionEvalItem> { productGift.promotionEvalItem }
                        )
                    };

                    DMSA.Models.Odoo.Promotions.Tools.SetPromotionDataGift(line,
                        new List<PromotionEvalItem> { productGift.promotionEvalItem });

                    SaleOrder.order_line.Add(new OrderLineWrapper(line));
                    OrderLines.Add(line);

                    await promotionEngineRunner
                        .AddApplyPromotion(SaleOrder, benefit, 1, saleOrderPromotions);
                }
            }
        }
    }

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

    public Action<PromoResultPopup> ClosePopupAction { get; set; }
    public ObservableCollection<sale_order_line> OrderLines { get; internal set; }

    private async void OnApplyButtonClicked(object sender, EventArgs e)
    {
        if(GlobalTotalManualGiftsApplied < GlobalTotalManualGiftsAllowed)
        {            
            var leave = await App.Current.Windows[0].Page.DisplayAlert($"¿Desea continuar?", $"No se han aplicado todos los {GlobalTotalManualGiftsAllowed} regalos de los bonificados manuales", "Si", "No");

            if (!leave)
            {
                return;
            }
        }

        var resultPopup = new PromoResultPopup();
        resultPopup.benefits = ItemsData.ToList();
        resultPopup.manualGifts = wholeRealApplied?.ToList() ?? new List<sale_order_line>();
        resultPopup.ActionResult = 1;
        ClosePopupAction?.Invoke(resultPopup);
    }

    private async void OnApplyAndContinueButtonClicked(object sender, EventArgs e)
    {
        if (GlobalTotalManualGiftsApplied < GlobalTotalManualGiftsAllowed)
        {
            var leave = await App.Current.Windows[0].Page.DisplayAlert($"¿Desea continuar?", $"No se han aplicado todos los {GlobalTotalManualGiftsAllowed} regalos de los bonificados manuales", "Si", "No");

            if (!leave)
            {
                return;
            }
        }

        var resultPopup = new PromoResultPopup();
        resultPopup.benefits = ItemsData.ToList();
        resultPopup.manualGifts = wholeRealApplied?.ToList() ?? new List<sale_order_line>();
        resultPopup.ActionResult = 2;
        ClosePopupAction?.Invoke(resultPopup);
    }


    private async void OnCloseButtonClicked(object sender, EventArgs e)
    {
        var resultPopup = new PromoResultPopup();
        resultPopup.benefits = ItemsData.ToList();
        resultPopup.manualGifts = wholeRealApplied?.ToList() ?? new List<sale_order_line>();
        resultPopup.ActionResult = 0;
        ClosePopupAction?.Invoke(resultPopup);
    }

    private readonly Dictionary<Entry, CancellationTokenSource> _entryTokens = new();

    private async void Qty_Entry_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (IsLoading)
            return;

        if (sender is not Entry entry)
            return;

        try
        {
            bool ShouldSaveToo = false;

            if (_entryTokens.TryGetValue(entry, out var existingCts))
            {
                existingCts.Cancel();
                existingCts.Dispose();
            }

            var cts = new CancellationTokenSource();
            _entryTokens[entry] = cts;
            var token = cts.Token;

            await Task.Delay(400, token);

            if (token.IsCancellationRequested)
                return;

            if (entry.BindingContext is not product_product product)
                return;

            await ChangeQtyEvent(product, ShouldSaveToo);
        }
        catch (TaskCanceledException)
        {
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error Qty_Entry_TextChanged: {ex}");
        }
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

                var lineToDiscount = orderLines
                        .Select(line => line.Count > 2 ? line[2] as sale_order_line : null)
                        .FirstOrDefault(l => l != null && l.product_tmpl_id == productTarget);

                if (lineToDiscount != null)
                {
                    List<PromotionEvalItem> listPromotionData = lineToDiscount.promotionDataList;

                    //List<PromotionEvalItem> listPromotionData = new List<PromotionEvalItem>();

                    //listPromotionData = !string.IsNullOrEmpty(lineToDiscount.promotion_data) ?
                    //            Newtonsoft.Json.JsonConvert.DeserializeObject<List<PromotionEvalItem>>(lineToDiscount.promotion_data) :
                    //            new List<PromotionEvalItem>();

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
                                break;
                            }
                        }
                    }

                    lineToDiscount.promotion_data = JsonConvert.SerializeObject(listPromotionData);

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

        int previousQty = product.qty_gift;
        int delta = product.qty_gift_virtual - product.qty_gift;

        Debug.WriteLine("ChangeQtyEvent - before UpdateGiftIsolated");

        bool success = await UpdateGiftIsolated(product, ShouldSaveToo, selectedPromoEvalItem);

        Debug.WriteLine("ChangeQtyEvent - pass UpdateGiftIsolated");

        if (!success)
        {
            product.qty_gift_virtual = previousQty;
            OnPropertyChanged(nameof(product.qty_gift_virtual));
        }

        OnPropertyChanged(nameof(ComputeTotal));
        OnPropertyChanged(nameof(ComputeTotalQty));
    }
}