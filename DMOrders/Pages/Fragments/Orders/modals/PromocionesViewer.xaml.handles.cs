using DMSA.Models.Odoo.DMOrders.promotions.abstractCustom;
using DMSA.Models.Odoo.Native;
using DMSA.Sync.Core.Database.Sqlite;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace DMOrders.Pages.Fragments.Orders.modals;

public partial class PromocionesViewer
{
    //private async Task HandleGiftPromotion(List<PromotionEvalItem> promoItems, ProductProductDb productDb)
    //{
    //    var existingCodes = new HashSet<string>();
    //    var promotionCache = new Dictionary<string, List<PromotionEvalItem>>();

    //    var relevantLines = SaleOrdersLinesTmp
    //        .Select(x => (sale_order_line)x[2])
    //        .Where(l => l.is_gift)
    //        .ToList();

    //    foreach (var itemEval in promoItems)
    //    {
    //        if (itemEval.Promotion._selection_type_id == 1)
    //        {
    //            var tasks = itemEval.RuleSet.Select(async ruleEval =>
    //            {
    //                var product = await productDb
    //                    .GetByProductTemplate(ruleEval.ProductIdOrigin, SaleOrder._pricelist_id);

    //                if (product == null) return null;

    //                product.qty_gift = 0;
    //                product.promotionEvalItem = itemEval;
    //                product.allow_add_gift = false;

    //                return product;
    //            });

    //            var results = await Task.WhenAll(tasks);

    //            foreach (var product in results.Where(p => p != null))
    //            {
    //                if (existingCodes.Add(product.default_code))
    //                    promoGifts.Add(product);
    //            }
    //        }


    //        if (itemEval.Promotion._selection_type_id == 2)
    //        {
    //            EditQty = true;

    //            var listIds = itemEval.Promotion._product_details_promotion_ids
    //                .Select(x => x._product_id)
    //                .ToArray();

    //            var products = await productDb
    //                .GetByProductsTemplate(listIds, SaleOrder._pricelist_id);

    //            if (products == null) continue;

    //            foreach (var prod in products)
    //            {
    //                int qty = 0;

    //                foreach (var line in relevantLines)
    //                {
    //                    if (line.product_id != prod.id)
    //                        continue;

    //                    var promoData = GetPromotionData(line.promotion_data, promotionCache);

    //                    if (promoData.Any(x => x.Promotion.id == itemEval.Promotion.id))
    //                    {
    //                        qty += (int)line.product_uom_qty_real;

    //                        GlobalTotalManualGiftsApplied += (int)line.product_uom_qty_real;

    //                        if (!realApplied.Contains(line))
    //                            realApplied.Add(line);
    //                    }
    //                }

    //                prod.qty_gift = qty;

    //                prod.qty_gift_virtual = qty;

    //                prod.promotionEvalItem = itemEval;
    //                prod.allow_add_gift = true;

    //                if (existingCodes.Add(prod.default_code))
    //                    promoGifts.Add(prod);
    //            }
    //        }
    //    }

    //    OnPropertyChanged(nameof(promoGifts));
    //    OnPropertyChanged(nameof(ComputeTotal));
    //    OnPropertyChanged(nameof(ComputeTotalQty));
    //}


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
            PanelSeleccionArticulos.IsVisible = true;
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
        var promotionCache = new Dictionary<string, List<PromoRuleItem>>();

        // HashSet para O(1)
        var realAppliedSet = new HashSet<sale_order_line>(realApplied);

        var relevantLines = SaleOrdersLinesTmp
            .Select(x => (sale_order_line)x[2])
            .Where(l => l.is_gift)
            .ToList();

        // =========================
        // PREPROCESAR LÍNEAS (CLAVE)
        // =========================
        var linesByProductId = new Dictionary<int, List<(sale_order_line line, HashSet<int> promoIds)>>();

        foreach (var line in relevantLines)
        {
            var promoData = GetPromotionData(line.promotion_data, promotionCache);

            // Convertir a HashSet UNA sola vez
            var promoIds = new HashSet<int>();
            foreach (var p in promoData)
                promoIds.Add(p.promo_id);

            if (!linesByProductId.TryGetValue(line.product_id, out var list))
            {
                list = new List<(sale_order_line, HashSet<int>)>();
                linesByProductId[line.product_id] = list;
            }

            list.Add((line, promoIds));
        }

        // =========================
        // PRELOAD PRODUCTOS
        // =========================
        var allTemplateIds = new HashSet<int>();

        foreach (var p in promoItems)
        {
            if (p.Promotion._selection_type_id == 1)
            {
                foreach (var r in p.RuleSet)
                    allTemplateIds.Add(r.ProductIdOrigin);
            }
            else if (p.Promotion._selection_type_id == 2)
            {
                foreach (var d in p.Promotion._product_details_promotion_ids)
                    allTemplateIds.Add(d._product_id);
            }
        }

        var allProducts = await productDb
            .GetByProductsTemplate(allTemplateIds.ToArray(), SaleOrder._pricelist_id);

        var productsByTemplate = new Dictionary<int, product_product>(allProducts.Count);
        foreach (var p in allProducts)
            productsByTemplate[p._product_tmpl_id] = p;

        // =========================
        // LOOP PRINCIPAL OPTIMIZADO
        // =========================
        foreach (var itemEval in promoItems)
        {
            var promotionId = itemEval.Promotion.id;
            var selectionType = itemEval.Promotion._selection_type_id;

            // =====================
            // TIPO 1
            // =====================
            if (selectionType == 1)
            {
                foreach (var ruleEval in itemEval.RuleSet)
                {
                    if (!productsByTemplate.TryGetValue(ruleEval.ProductIdOrigin, out var product))
                        continue;

                    product.qty_gift = 0;
                    product.promotionEvalItem = itemEval;
                    product.allow_add_gift = false;

                    if (existingCodes.Add(product.default_code))
                        promoGifts.Add(product);
                }
            }
            // =====================
            // TIPO 2
            // =====================
            else if (selectionType == 2)
            {
                var swTotal = Stopwatch.StartNew();
                long timeLookup = 0;
                long timeInnerLoop = 0;

                EditQty = true;

                // Recalcular aplicados de esta selección (evitar acumular al reabrir/cambiar promo)
                int appliedForThisPromo = 0;

                foreach (var detail in itemEval.Promotion._product_details_promotion_ids)
                {
                    var swLookup = Stopwatch.StartNew();

                    if (!productsByTemplate.TryGetValue(detail._product_id, out var prod))
                        continue;

                    swLookup.Stop();
                    timeLookup += swLookup.ElapsedTicks;

                    int qty = 0;

                    if (linesByProductId.TryGetValue(prod.id, out var lines))
                    {
                        var swInner = Stopwatch.StartNew();

                        foreach (var (line, promoIds) in lines)
                        {
                            if (!promoIds.Contains(promotionId))
                                continue;

                            int lineQty = (int)line.product_uom_qty_real;

                            qty += lineQty;
                            appliedForThisPromo += lineQty;

                            if (realAppliedSet.Add(line))
                                realApplied.Add(line);
                        }

                        swInner.Stop();
                        timeInnerLoop += swInner.ElapsedTicks;
                    }

                    prod.qty_gift = qty;
                    prod.qty_gift_virtual = qty;
                    prod.promotionEvalItem = itemEval;
                    prod.allow_add_gift = true;

                    if (existingCodes.Add(prod.default_code))
                        promoGifts.Add(prod);
                }

                GlobalTotalManualGiftsApplied = appliedForThisPromo;

                swTotal.Stop();

                Debug.WriteLine($" TOTAL: {swTotal.ElapsedMilliseconds} ms");
                Debug.WriteLine($" Lookup: {TimeSpan.FromTicks(timeLookup).TotalMilliseconds} ms");
                Debug.WriteLine($" InnerLoop: {TimeSpan.FromTicks(timeInnerLoop).TotalMilliseconds} ms");
            }
        }

        OnPropertyChanged(nameof(promoGifts));
        OnPropertyChanged(nameof(ComputeTotal));
        OnPropertyChanged(nameof(ComputeTotalQty));
    }

    private void HandleDiscountPromotion(List<PromotionEvalItem> promoItems)
    {
        PanelSeleccionArticulos.IsVisible = false;
        BorderBenefits.IsVisible = false;
        BorderDiscount.IsVisible = true;

        promoDiscounts = new ObservableCollection<PromoRuleMatch>(
            promoItems.SelectMany(x => x.RuleSet)
        );

        foreach (var promoItem in promoDiscounts)
        {
            if (promoItem?.ProductSequenceApplyList == null)
                continue;

            int loadedDiscount = 0;
            foreach (var sequenceData in promoItem.ProductSequenceApplyList)
            {
                var order_line_match = OrderLines.FirstOrDefault(x =>
                    x.product_id == sequenceData.product_id && x.sequence == sequenceData.sequence);

                if (order_line_match != null && order_line_match.discount > loadedDiscount)
                    loadedDiscount = (int)order_line_match.discount;
            }

            if (loadedDiscount > 0)
                promoItem.discount = loadedDiscount;
        }

        OnPropertyChanged(nameof(promoDiscounts));
    }


    private async Task HandleGiftAutoPromotion(List<PromotionEvalItem> promoItems, ProductProductDb productDb)
    {
        var existingCodes = new HashSet<string>();

        int[] product_ids = promoItems.SelectMany(item => item.RuleSet.Select(rule => rule.ProductIdOrigin)).ToArray();

        var products = await productDb.GetByProductsIds(product_ids, SaleOrder._pricelist_id);

        foreach (var itemEval in promoItems)
        {
            if (itemEval.Promotion._selection_type_id == 1)
            {
                var tasks = itemEval.RuleSet.Select(async ruleEval =>
                {
                    var product = products.FirstOrDefault(p => p.id == ruleEval.ProductIdOrigin);

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
        }

        //OnPropertyChanged(nameof(promoGifts));
        //OnPropertyChanged(nameof(ComputeTotal));
        //OnPropertyChanged(nameof(ComputeTotalQty));
    }

}
