using CommunityToolkit.Maui;
using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Extensions;
using CommunityToolkit.Maui.Views;
using DMOrders.Models;
using DMOrders.Pages.Fragments.Orders.modals;
using DMOrders.Services.Promotions;
using DMSA.Models.Odoo.Abstract;
using DMSA.Models.Odoo.DMOrders.promotions;
using DMSA.Models.Odoo.DMOrders.promotions.abstractCustom;
using DMSA.Models.Odoo.Native;
using DMSA.Models.Odoo.Promotions;
using DMSA.Models.Odoo.Sales;
using DMSA.Models.Odoo.Sales.promotions.abstractCustom;
using DMSA.Sync.Core;
using DMSA.Sync.Core.Database.Sqlite;
using DMSA.Sync.Core.Database.Sqlite.Benefits;
using DMSA.Sync.Core.Database.Sqlite.Sales;
using Microsoft.Maui.Controls.Shapes;
using Newtonsoft.Json;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows.Input;

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
                    var ruleItem = Tools.FromBenefitRule(promoResItem, ruleMatch, CurrentSaleOrder);

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
                        if (!await promotionEngineRunner.CanApplyPromotion(saleOrder, ruleItem, saleOrderPromotions))
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
                                lineToDiscount.promotion_data = Newtonsoft.Json.JsonConvert.SerializeObject(new List<PromoRuleItem>() { ruleItem });
                                await promotionEngineRunner.AddApplyPromotion(saleOrder, ruleItem, 1, saleOrderPromotions);
                                DMSA.Models.Odoo.Promotions.Tools.SetPromotionData(lineToDiscount,
                                    new List<PromoRuleItem> { ruleItem });


                                lineToDiscount.origin_gift_line_ids_offline =
                                        Newtonsoft.Json.JsonConvert.SerializeObject(
                                            ruleMatch.ProductSequenceApplyList
                                        );

                                //lineToDiscount.origin_gift_line_ids_offline =
                                //        Newtonsoft.Json.JsonConvert.SerializeObject(
                                //            listPromotionData
                                //                .Where(x => x.RuleSet != null)
                                //                .SelectMany(x => x.RuleSet)
                                //                .Where(r => r.ProductSequenceApplyList != null)
                                //                .SelectMany(r => r.ProductSequenceApplyList)
                                //                .Distinct()
                                //                .ToList()
                                //        );
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
                                        
                    var ruleItem = Tools.FromBenefitRule(promoResItem, ruleMatch, CurrentSaleOrder);

                    foreach (var productTarget in listIdsProd)
                    {
                        if (!await promotionEngineRunner.CanApplyPromotion(saleOrder, ruleItem, saleOrderPromotions))
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
                            //List<PromotionEvalItem> listPromotionData = new List<PromotionEvalItem>();

                            //listPromotionData = lineToDiscount.promotionDataList;
                            var promotionRules = lineToDiscount.promotionRules;
                            if (promotionRules.Exists(p => p.promo_id == ruleItem.promo_id))
                            {
                                Debug.WriteLine($"Descuento de promoción ya ha sido aplicado anteriormente");                                
                            }

                            //listPromotionData.Add(promoResItem);
                            promotionRules.Add(ruleItem);

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
                                lineToDiscount.promotion_data = Newtonsoft.Json.JsonConvert.SerializeObject(new List<PromoRuleItem>() { ruleItem });
                                await promotionEngineRunner.AddApplyPromotion(saleOrder, ruleItem, 1, saleOrderPromotions);
                                DMSA.Models.Odoo.Promotions.Tools.SetPromotionData(lineToDiscount,
                                    new List<PromoRuleItem> { ruleItem });


                                lineToDiscount.origin_gift_line_ids_offline =
                                        Newtonsoft.Json.JsonConvert.SerializeObject(
                                            ruleMatch.ProductSequenceApplyList
                                        );

                                //lineToDiscount.origin_gift_line_ids_offline =
                                //        Newtonsoft.Json.JsonConvert.SerializeObject(
                                //            promotionRules                                                
                                //                .Where(r => r.ProductSequenceApplyList != null)
                                //                .SelectMany(r => r.ProductSequenceApplyList)
                                //                .Distinct()
                                //                .ToList()
                                //        );
                            }
                            Debug.WriteLine($"Descuento aplicado: {discountPercentage}% al producto ID {productTemplateId}");
                        }
                    }
                }
            }
        }

        private async Task<List<string>> ApplyPromo(sale_order saleOrder)
        {
            List<string> resultData = new List<string>();

            await EvalPromotions(saleOrder);

            bool ShowPromoPopup = false;

            if (AppliedPromotionResults.Count == 0)
            {
                await Toast.Make("No hay promociones aplicables").Show();
                return new List<string>();
            }

            foreach (var promoResult in AppliedPromotionResults)
            {
                foreach (var promoResItem in promoResult.Items)
                {
                    resultData.Add(promoResItem.Promotion.name);

                    if (promoResItem.Promotion._promotion_type_id == 2) //REGALO
                    {
                        ShowPromoPopup = true;
                        break;
                    }

                    if (promoResItem.Promotion._promotion_type_id == 4) // es NXN
                    {
                        ShowPromoPopup = true;
                        break;
                    }

                    // ES DESCUENTO DEBE APLICARSE PRIMERO
                    if (promoResItem.Promotion._promotion_type_id == 6)
                    {
                        //await ApplyDiscount(saleOrder, promoResItem);
                        //UpdateTotals();

                        await PrepareDiscount(saleOrder, promoResItem);

                        ShowPromoPopup = true;
                        break;
                    }
                }
            }

            //No se muestra Popup si no hay elemento que elegir
            if (!ShowPromoPopup)
            {
                return new List<string>();
            }

            bool ShowPromoPopupLevel2 = false;

            var view = new PromocionesViewer(saleOrder);
            view.ItemsData = AppliedPromotionResults;
            view.OrderLines = OrderLines;
            view.saleOrderPromotions = saleOrderPromotions;
            //dawait view.AutoApplyPromotion();
            await view.ApplyPromosOnList();

            ShowPromoPopupLevel2 = view.BenefitsForShow;

            //Se guardan las promociones que no son manuales
            await SavePromotions(true, false);

            //if (!ShowPromoPopupLevel2) return new List<string>();

            var popup = new Popup
            {
                Content = view,
                BackgroundColor = Colors.Black.WithAlpha(0.4f), // fondo semi-transparente
                CanBeDismissedByTappingOutsideOfPopup = false,
                Padding = new Thickness(0),
                Margin = new Thickness(0)
            };

            view.ClosePopupAction = (promo) => PopupExtensions.ClosePopupAsync(Application.Current.Windows[0].Page, promo);

            var result = await PopupExtensions.ShowPopupAsync<PromoResultPopup>(App.Current.Windows[0].Page, popup, new PopupOptions
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

            if (result.Result != null && result.Result is PromoResultPopup selected)
            {
                if (selected.ActionResult == 1 || selected.ActionResult == 2) //Aplicar - Aplicar y continuar
                {
                    var toRemove = OrderLines
                    .Where(x => x.is_gift && x.is_manual)
                    .ToList();

                    foreach (var item in toRemove)
                    {
                        Debug.WriteLine("ELIMINAR LINEA ===========================================");
                        Debug.WriteLine(item);
                        Debug.WriteLine(item.sequence);
                        Debug.WriteLine(item.product_id);
                        OrderLines.Remove(item);
                    }

                    //Solo se hace el proceso para regalos manuales
                    foreach (var giftLine in selected.manualGifts)
                    {
                        giftLine._order_id = CurrentSaleOrder.id;
                        await new SaleOrderLineDb(App.Session.odooConnection.DbNameSqlite).InsertAsync(giftLine);
                        OrderLines.Add(giftLine);
                    }

                    var db = new SaleOrderLineDb(App.Session.odooConnection.DbNameSqlite);

                    // índice para evitar búsquedas repetidas
                    var lineIndex = OrderLines
                        .GroupBy(x => (x.product_id, x.sequence))
                        .ToDictionary(g => g.Key, g => g.First());

                    var updates = new List<sale_order_line>();

                    foreach (var benefit in selected.benefits)
                    {
                        foreach (var promoItem in benefit.Items.Where(x =>
                            x.Promotion._promotion_type_id == 2 &&
                            x.Promotion._selection_type_id == 2))
                        {
                            foreach (var rule in promoItem.RuleSet)
                            {
                                foreach (var data in rule.ProductSequenceApplyList)
                                {
                                    if (!lineIndex.TryGetValue((data.product_id, data.sequence), out var line))
                                        continue;

                                    // promotion_ids (int[])
                                    var promoList = line.promotion_ids?.ToList() ?? new List<int>();
                                    if (!promoList.Contains(promoItem.Promotion.id))
                                        promoList.Add(promoItem.Promotion.id);
                                    line.promotion_ids = promoList.ToArray();

                                    // rule_ids (int[])
                                    var ruleList = line.rule_ids?.ToList() ?? new List<int>();
                                    if (!ruleList.Contains(rule.id))
                                        ruleList.Add(rule.id);
                                    line.rule_ids = ruleList.ToArray();

                                    updates.Add(line);
                                }
                            }
                        }
                    }

                    // ejecutar updates (puedes paralelizar si quieres)
                    foreach (var line in updates.Distinct())
                    {
                        await db.UpdateAsync(line);
                    }

                    //Almacenar ahora información de bonificados manuales
                    await SavePromotions(false, true);
                }

                if (selected.ActionResult == 1 || selected.ActionResult == 0)
                {
                    if (selected.ActionResult == 0)
                    {
                        await Toast.Make("Promociones manuales no aplicadas.").Show();
                    }

                    await Navigation.PopModalAsync(false);
                }
            }

            return resultData;
        }
    }
}
