using DMSA.Models.Odoo.DMOrders.promotions.abstractCustom;
using DMSA.Models.Odoo.Native;
using DMSA.Sync.Core.Database.Sqlite;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace DMOrders.Pages.Fragments.Orders.modals
{
    public partial class PromocionesViewer
    {
        private async Task HandleGiftPromotion(List<PromotionEvalItem> promoItems, ProductProductDb productDb)
        {
            var existingCodes = new HashSet<string>();
            var promotionCache = new Dictionary<string, List<PromotionEvalItem>>();

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

            foreach (var promoItem in promoDiscounts)
            {
                foreach (var sequenceData in promoItem.ProductSequenceApplyList)
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
    }
}
