using System.Globalization;
using DMOrders.Models;
using DMOrders.Services.Promotions;
using DMSA.Models.Odoo.Abstract;
using DMSA.Models.Odoo.DMOrders.promotions;
using DMSA.Models.Odoo.DMOrders.promotions.abstractCustom;
using DMSA.Models.Odoo.Native;
using DMSA.Models.Odoo.Sales;
using DMSA.Sync.Core;
using DMSA.Sync.Core.Database.Sqlite;
using DMSA.Sync.Core.Database.Sqlite.Benefits;
using DMSA.Sync.Core.Database.Sqlite.Sales;
using Newtonsoft.Json;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows.Input;
using System.Text.RegularExpressions;
using CommunityToolkit.Maui.Alerts;

namespace DMOrders.Pages.Fragments.Orders
{
    public partial class Crud
    {
        private async Task ApplyDiscount(sale_order saleOrder, PromotionEvalItem promoResItem)
        {
            PromotionEngineRunner promotionEngineRunner = new PromotionEngineRunner();

            foreach (var ruleMatch in promoResItem.RuleSet)
            {
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
                        if (!await promotionEngineRunner.CanApplyPromotion(saleOrder, promoResItem, saleOrderPromotions))
                        {
                            Debug.WriteLine($"{promoResItem.Promotion.name} ya ha sido aplicado maximo de veces - Crud-ApplyDiscount");
                            return;
                        }

                        double discountPercentage = ruleMatch.discount;
                        int productTemplateId = ruleMatch.ProductTmplId;
                        var orderLines = saleOrder.order_line;

                        var productDb = new ProductProductDb(App.Session.odooConnection.DbNameSqlite);

                        var lineToDiscount = orderLines
                                .Select(line => line.Count > 2 ? line[2] as sale_order_line : null)
                                .FirstOrDefault(l => l != null && l.product_tmpl_id == productTarget);

                        if (lineToDiscount != null)
                        {
                            List<PromotionEvalItem> listPromotionData = new List<PromotionEvalItem>();

                            listPromotionData = lineToDiscount.promotionDataList;

                            if (listPromotionData.Exists(p => p.Promotion.id == promoResItem.Promotion.id))
                            {
                                Debug.WriteLine($"Descuento de promoción ya ha sido aplicado anteriormente");
                                //continue;
                            }

                            listPromotionData.Add(promoResItem);

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
                        }
                    }
                }
            }
        }

        private async Task PrepareDiscount(sale_order saleOrder, PromotionEvalItem promoResItem)
        {
            PromotionEngineRunner promotionEngineRunner = new PromotionEngineRunner();

            foreach (var ruleMatch in promoResItem.RuleSet)
            {
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
                        if (!await promotionEngineRunner.CanApplyPromotion(saleOrder, promoResItem, saleOrderPromotions))
                        {
                            Debug.WriteLine($"{promoResItem.Promotion.name} ya ha sido aplicado maximo de veces - Crud-ApplyDiscount");
                            return;
                        }

                        //double discountPercentage = ruleMatch.discount;
                        double discountPercentage = 0;
                        //int productTemplateId = ruleMatch.ProductTmplId;
                        int productTemplateId = productTarget;
                        var orderLines = saleOrder.order_line;

                        var productDb = new ProductProductDb(App.Session.odooConnection.DbNameSqlite);

                        var lineToDiscount = orderLines
                                .Select(line => line.Count > 2 ? line[2] as sale_order_line : null)
                                .FirstOrDefault(l => l != null && l.product_tmpl_id == productTarget);

                        if (lineToDiscount != null)
                        {
                            List<PromotionEvalItem> listPromotionData = new List<PromotionEvalItem>();

                            listPromotionData = lineToDiscount.promotionDataList;

                            if (listPromotionData.Exists(p => p.Promotion.id == promoResItem.Promotion.id))
                            {
                                Debug.WriteLine($"Descuento de promoción ya ha sido aplicado anteriormente");                                
                            }

                            listPromotionData.Add(promoResItem);

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
                        }
                    }
                }
            }
        }
    }
}
