using CommunityToolkit.Maui.Alerts;
using DMOrders.Services.Promotions;
using DMSA.Models.Odoo.DMOrders.promotions.abstractCustom;
using DMSA.Models.Odoo.Native;
using DMSA.Models.Odoo.Sales.promotions.abstractCustom;
using DMSA.Sync.Core.Database.Sqlite;
using Newtonsoft.Json;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace DMOrders.Pages.Fragments.Orders.modals;

public partial class PromocionesViewer
{
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

                    decimal originalPrice = lineToDiscount.price_unit;
                    decimal virtual_price_no_tax = lineToDiscount.virtual_price_no_tax;

                    decimal discountAmount = (virtual_price_no_tax * lineToDiscount.product_uom_qty_real) * (decimal)(discountPercentage / 100);
                    lineToDiscount.discount = (decimal)discountPercentage;
                    lineToDiscount.amount_discount = discountAmount;

                    lineToDiscount.price_subtotal = (virtual_price_no_tax * lineToDiscount.product_uom_qty_real) - discountAmount;
                    lineToDiscount.price_tax = (lineToDiscount.price_subtotal * lineToDiscount.virtual_iva_percentage) / 100;
                    lineToDiscount.price_total = lineToDiscount.price_subtotal + lineToDiscount.price_tax;

                    lineToDiscount.virtual_line_subtotal = virtual_price_no_tax * lineToDiscount.product_uom_qty_real;

                    foreach (var promoItem in listPromotionData)
                    {
                        foreach (var rulesInside in promoItem.RuleSet)
                        {
                            if (rulesInside.id == ruleMatch.id)
                            {
                                rulesInside.discount = ruleMatch.discount;
                                break;
                            }
                        }
                    }

                    lineToDiscount.promotion_data = JsonConvert.SerializeObject(listPromotionData);
                    Debug.WriteLine($"Descuento aplicado: {discountPercentage}% al producto ID {productTemplateId}");
                }
            }
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
                    virtual_line_subtotal = (decimal)productGift.list_price * qty_assign,
                    amount_discount = qty_assign * (decimal)productGift.list_price,
                    discount = 100,
                    price_tax = 0,
                    price_total = 0,
                    is_gift = true,
                    product_id_origin = ruleMatch.ProductIdOrigin,
                    promotion_data = JsonConvert.SerializeObject(
                        new List<PromotionEvalItem> { productGift.promotionEvalItem }
                    ),
                    //origin_gift_line_ids_offline = "[]"
                };

                DMSA.Models.Odoo.Promotions.Tools.SetPromotionData(line, new List<PromotionEvalItem> { productGift.promotionEvalItem });
                DMSA.Models.Odoo.Promotions.Tools.SetPromotionDataGift(line, new List<PromotionEvalItem> { productGift.promotionEvalItem });

                SaleOrder.order_line.Add(new OrderLineWrapper(line));
                OrderLines.Add(line);

                await promotionEngineRunner
                    .AddApplyPromotion(SaleOrder, benefit, 1, saleOrderPromotions);
            }
        }
    }

    private readonly SemaphoreSlim _giftLock = new(1, 1);

    private async Task<bool> UpdateGiftIsolated(product_product product, 
        bool ShouldSaveToo, 
        PromotionEvalItem benefit)
    {
        await _giftLock.WaitAsync();

        bool is_manual = false;

        try
        {
            if (benefit.Promotion._promotion_type_id == 2 && benefit.Promotion._selection_type_id == 2)
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
                        await page.DisplayAlertAsync("Información", "Cantidad máxima alcanzada en pedido.", "OK");
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
                        _virtual_price_no_tax = (decimal)productGift.list_price,
                        virtual_line_subtotal = qty_assign * (decimal)productGift.list_price,
                        amount_discount = qty_assign * (decimal)productGift.list_price,
                        discount = 100,
                        price_tax = 0,
                        price_total = 0,
                        is_gift = true,
                        is_manual = false,
                        product_tmpl_id = productGift._product_tmpl_id,
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

}

