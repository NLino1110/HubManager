using DMOrders.Services.Database.Sqlite;
using DMOrders.Services.Promotions;
using DMSA.Models.Odoo.DMOrders.promotions;
using DMSA.Models.Odoo.DMOrders.promotions.@abstract;
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

                        if (!_ItemsDataBenefits.Any(x => x.Promotion?.id == benefit.Promotion.id))
                        {
                            //Quizas se deban acumular los qty * numero de apariciones de la promoción                            
                            benefit.FoundTimesApplies = 1;
                            benefit.MaxAllowedGifts = benefit.AllowedGifts;
                            _ItemsDataBenefits.Add(benefit);
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
                            existing.MaxAllowedGifts = existing.FoundTimesApplies * existing.AllowedGifts;
                        }
                    }
                }
            }

            OnPropertyChanged(nameof(ItemsData));
            OnPropertyChanged(nameof(ItemsDataBenefits));
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

    private async Task AddAutoGiftsAsync(PromotionEvalItem benefit)
    {
        var productDb = new ProductProductDb(App.Session.odooConnection.DbNameSqlite);

        int productIdCompare = benefit.RuleSet._product_id;

        if (productIdCompare == 0)
            productIdCompare = benefit.ProductTmplId;

        var productGift = await productDb.GetByProductTemplate(productIdCompare);

        if (productGift != null)
        {
            if (!_promoGiftsAuto.Any(x => x?.default_code == productGift.default_code))
            {
                productGift.qty_gift = 0;
                productGift.promotionEvalItem = benefit;
                productGift.qty_gift = benefit.RuleSet.value;
                
                Debug.WriteLine($"Cargado regalo automático para promoción {benefit.Promotion.name}: {productGift.name}");
                _promoGiftsAuto.Add(productGift);

                _ = AddGiftNxN(productGift, false, benefit);

                //_ = AddGiftIsolated(productGift, false, benefit); 
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
            selectedPromoEvalItem = (PromotionEvalItem) e.CurrentSelection[0];
            
            var productDb = new ProductProductDb(App.Session.odooConnection.DbNameSqlite);
            
            if (selectedPromoEvalItem.PromotionTypeId == 2) // es regalo
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
                            int productIdCompare = itemEval.RuleSet._product_id;
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
                                                
                        if(itemEval.Promotion._selection_type_id == 2)
                        {
                            //Si es que es manual se maneja distinto la extracción de los productos
                            var listIdsProd = itemEval.Promotion._product_details_promotion_ids
                                .Where(x=>x._promo_id == 0 && x._bonus_id > 0)
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

                OnPropertyChanged(nameof(promoGifts));
            }

            if (selectedPromoEvalItem.PromotionTypeId == 4) // es NXN
            {
                
            }

            if (selectedPromoEvalItem.PromotionTypeId == 6) // es descuento
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
                new List<PromotionEvalItem> { product.promotionEvalItem }
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

    private async Task AddGiftIsolated(product_product product, bool ShouldSaveToo, PromotionEvalItem benefit)
    {
        PromotionEngineRunner promotionEngineRunner = new PromotionEngineRunner();

        if (!await promotionEngineRunner.CanApplyPromotion(SaleOrder, benefit, saleOrderPromotions))
        {
            Debug.WriteLine($"{benefit.Promotion.name} ya ha sido aplicado maximo de veces - AddGiftIsolated");
            return;
        }

        sale_order_line saleOrderLineOrigin = new sale_order_line();

        var saleOrderLineDb = new SaleOrderLineDb(App.Session.odooConnection.DbNameSqlite);

        //Se busca linea de origen de promocion aplicada
        foreach (var itemLineOrigin in SaleOrder.order_line)
        {
            if (itemLineOrigin[2] != null)
            {
                saleOrderLineOrigin = (sale_order_line)itemLineOrigin[2];

                if (benefit.ProductTmplId == saleOrderLineOrigin.product_tmpl_id && !saleOrderLineOrigin.is_gift)
                {
                    if (saleOrderLineOrigin.max_gifts != benefit.MaxAllowedGifts)
                    {
                        saleOrderLineOrigin.max_gifts = benefit.MaxAllowedGifts;
                        saleOrderLineOrigin.promotion_data = Newtonsoft.Json.JsonConvert.SerializeObject(
                                new List<PromotionEvalItem> { product.promotionEvalItem }
                            );
                    }
                    break;
                }
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
                if (lineObject.product_id == product.id) // && lineObject.product_id_origin == saleOrderLineOrigin.product_id)
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
                new List<PromotionEvalItem> { product.promotionEvalItem }
            )
        };

        if (ShouldSaveToo)
            await saleOrderLineDb.InsertAsyncAutoOrdinal(line);

        SaleOrder.order_line.Add(new OrderLineWrapper(line));

        OrderLines.Add(line);
    }


    private async Task AddGiftNxN(product_product product, bool ShouldSaveToo, PromotionEvalItem benefit)
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
                    return;
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
                if (lineObject.product_id == benefit.ProductId)
                {
                    lineObject.promotion_data = Newtonsoft.Json.JsonConvert.SerializeObject(
                            new List<PromotionEvalItem> { product.promotionEvalItem }
                        );
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
            qty_to_deliver = benefit.MaxAllowedGifts , // benefit.RuleSet.qty,
            product_uom_qty_real = benefit.MaxAllowedGifts, //benefit.RuleSet.qty,
            product_uom_qty = benefit.MaxAllowedGifts, //benefit.RuleSet.qty,
            uom_category_display = "UND",
            price_subtotal = 0,
            discount = 100,
            price_tax = 0,
            price_total = 0,
            is_gift = true,
            product_id_origin = benefit.ProductId,
            promotion_data = Newtonsoft.Json.JsonConvert.SerializeObject(
                new List<PromotionEvalItem> { product.promotionEvalItem }
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

    public event PropertyChangedEventHandler PropertyChanged;
    private void OnPropertyChanged(string property) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));

    public Action<List<PromotionEvalResult>> ClosePopupAction { get; set; }
    public ObservableCollection<sale_order_line> OrderLines { get; internal set; }

    private void OnCloseButtonClicked(object sender, EventArgs e)
    {
        ClosePopupAction?.Invoke(ItemsData.ToList());
    }   
}