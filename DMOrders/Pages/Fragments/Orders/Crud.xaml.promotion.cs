using CommunityToolkit.Maui;
using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Extensions;
using CommunityToolkit.Maui.Views;
using DMOrders.Controls.Tools;
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
                        var linesToDiscount = DMSA.Models.Odoo.Promotions.Tools.ResolveDiscountTargetLines(
                            saleOrder.order_line,
                            productTarget,
                            ruleMatch.ProductSequenceApplyList);

                        bool promotionRegistered = false;
                        foreach (var lineToDiscount in linesToDiscount)
                        {
                            List<PromotionEvalItem> listPromotionData = lineToDiscount.promotionDataList;

                            if (listPromotionData.Exists(p => p.Promotion.id == promoResItem.Promotion.id))
                            {
                                Debug.WriteLine($"Descuento de promoción ya ha sido aplicado anteriormente");
                            }

                            listPromotionData.Add(promoResItem);

                            if (lineToDiscount.discount != 0)
                                continue;

                            decimal virtual_price_no_tax = lineToDiscount.virtual_price_no_tax;
                            decimal discountAmount = (virtual_price_no_tax * lineToDiscount.product_uom_qty_real) * (decimal)(discountPercentage / 100);
                            lineToDiscount.discount = (decimal)discountPercentage;
                            lineToDiscount.amount_discount = discountAmount;
                            lineToDiscount.price_subtotal = (virtual_price_no_tax * lineToDiscount.product_uom_qty_real) - discountAmount;
                            lineToDiscount.price_tax = (lineToDiscount.price_subtotal * lineToDiscount.virtual_iva_percentage) / 100;
                            lineToDiscount.price_total = lineToDiscount.price_subtotal + lineToDiscount.price_tax;
                            lineToDiscount.virtual_line_subtotal = virtual_price_no_tax * lineToDiscount.product_uom_qty_real;
                            lineToDiscount.promotion_data = Newtonsoft.Json.JsonConvert.SerializeObject(new List<PromoRuleItem>() { ruleItem });

                            if (!promotionRegistered)
                            {
                                await promotionEngineRunner.AddApplyPromotion(saleOrder, ruleItem, 1, saleOrderPromotions);
                                promotionRegistered = true;
                            }

                            DMSA.Models.Odoo.Promotions.Tools.SetPromotionData(lineToDiscount,
                                new List<PromoRuleItem> { ruleItem });

                            lineToDiscount.origin_gift_line_ids_offline =
                                    Newtonsoft.Json.JsonConvert.SerializeObject(
                                        ruleMatch.ProductSequenceApplyList
                                    );

                            Debug.WriteLine(
                                $"Descuento aplicado: {discountPercentage}% " +
                                $"product_id={lineToDiscount.product_id} seq={lineToDiscount.sequence} tmpl={productTarget}");
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Prepara descuentos (tipo promo 6) antes de abrir el modal.
        ///
        /// ANTES:
        /// - Accedía a promotionRules / ProductTmplIds / order_line sin null-checks.
        /// - Reprocesaba líneas que ya tenían descuento (flujo "No eliminar").
        ///
        /// ERROR: NullReferenceException en
        ///   var promotionRules = lineToDiscount.promotionRules;
        ///   (o en Exists/Add si la lista venía null / ítems null).
        ///
        /// DESPUÉS / POR QUÉ:
        /// - Guards de null; ProductTmplIds seguro; skip si discount &gt; 0.
        /// - Si falla, ApplyPromo captura y sigue abriendo el modal.
        /// </summary>
        private async Task PrepareDiscount(sale_order saleOrder, PromotionEvalItem promoResItem)
        {
            if (saleOrder == null || promoResItem?.RuleSet == null || promoResItem.Promotion == null)
                return;

            saleOrderPromotions ??= new List<SaleOrderPromotions>();
            PromotionEngineRunner promotionEngineRunner = new PromotionEngineRunner();

            foreach (var ruleMatch in promoResItem.RuleSet)
            {
                if (ruleMatch == null || !ruleMatch.IsDiscount)
                    continue;

                int maxProductTarget = ruleMatch.ProductTmplIdMaxTotal;
                if (ruleMatch.variable == "qty_product_unts")
                {
                    maxProductTarget = ruleMatch.ProductTmplIdMaxQty;
                }

                int[] listIdsProd = string.IsNullOrWhiteSpace(ruleMatch.ProductTmplIds)
                    ? Array.Empty<int>()
                    : (JsonConvert.DeserializeObject<int[]>(ruleMatch.ProductTmplIds) ?? Array.Empty<int>());

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

                if (CurrentSaleOrder == null)
                {
                    Debug.WriteLine("PrepareDiscount: CurrentSaleOrder es null.");
                    return;
                }

                var ruleItem = Tools.FromBenefitRule(promoResItem, ruleMatch, CurrentSaleOrder);
                if (ruleItem == null)
                    continue;

                foreach (var productTarget in listIdsProd)
                {
                    if (!await promotionEngineRunner.CanApplyPromotion(saleOrder, ruleItem, saleOrderPromotions))
                    {
                        Debug.WriteLine($"{promoResItem.Promotion?.name} ya ha sido aplicado maximo de veces - Crud-ApplyDiscount");
                        return;
                    }

                    double discountPercentage = 0;
                    if (saleOrder.order_line == null)
                        continue;

                    var linesToDiscount = DMSA.Models.Odoo.Promotions.Tools.ResolveDiscountTargetLines(
                        saleOrder.order_line,
                        productTarget,
                        ruleMatch.ProductSequenceApplyList);

                    bool promotionRegistered = false;
                    foreach (var lineToDiscount in linesToDiscount)
                    {
                        // Ya tiene descuento: no reprocesar (caso "No eliminar" / promos previas).
                        if (lineToDiscount.discount > 0)
                        {
                            Debug.WriteLine(
                                $"PrepareDiscount: línea {lineToDiscount.product_id} seq={lineToDiscount.sequence} ya tiene descuento, se omite.");
                            continue;
                        }

                        List<PromoRuleItem> promotionRules;
                        try
                        {
                            promotionRules = lineToDiscount.promotionRules?
                                .Where(p => p != null)
                                .ToList()
                                ?? new List<PromoRuleItem>();
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine($"PrepareDiscount promotionRules: {ex}");
                            promotionRules = new List<PromoRuleItem>();
                        }

                        if (promotionRules.Exists(p => p.promo_id == ruleItem.promo_id))
                        {
                            Debug.WriteLine($"Descuento de promoción ya ha sido aplicado anteriormente");
                        }

                        promotionRules.Add(ruleItem);

                        decimal virtual_price_no_tax = lineToDiscount.virtual_price_no_tax;
                        decimal discountAmount = (virtual_price_no_tax * lineToDiscount.product_uom_qty_real) * (decimal)(discountPercentage / 100);
                        lineToDiscount.discount = (decimal)discountPercentage;
                        lineToDiscount.amount_discount = discountAmount;
                        lineToDiscount.price_subtotal = (virtual_price_no_tax * lineToDiscount.product_uom_qty_real) - discountAmount;
                        lineToDiscount.price_tax = (lineToDiscount.price_subtotal * lineToDiscount.virtual_iva_percentage) / 100;
                        lineToDiscount.price_total = lineToDiscount.price_subtotal + lineToDiscount.price_tax;
                        lineToDiscount.virtual_line_subtotal = virtual_price_no_tax * lineToDiscount.product_uom_qty_real;
                        lineToDiscount.promotion_data = Newtonsoft.Json.JsonConvert.SerializeObject(new List<PromoRuleItem>() { ruleItem });

                        if (!promotionRegistered)
                        {
                            await promotionEngineRunner.AddApplyPromotion(saleOrder, ruleItem, 1, saleOrderPromotions);
                            promotionRegistered = true;
                        }

                        DMSA.Models.Odoo.Promotions.Tools.SetPromotionData(lineToDiscount,
                            new List<PromoRuleItem> { ruleItem });

                        lineToDiscount.origin_gift_line_ids_offline =
                                Newtonsoft.Json.JsonConvert.SerializeObject(
                                    ruleMatch.ProductSequenceApplyList ?? new List<OriginPromoOrderLine>()
                                );

                        Debug.WriteLine(
                            $"PrepareDiscount: {discountPercentage}% " +
                            $"product_id={lineToDiscount.product_id} seq={lineToDiscount.sequence} tmpl={productTarget}");
                    }
                }
            }
        }

        /// <summary>
        /// Evalúa y muestra el modal de promociones; aplica resultado del popup.
        ///
        /// ANTES:
        /// - Tras Aplicar/Salir hacía Navigation.PopModalAsync del CRUD.
        /// - ButtonSave_Clicked también hacía PopModal en finally → doble pop.
        /// - Acceso a App.Current.Windows[0].Page / manualGifts / benefits sin null-checks.
        ///
        /// ERROR: cierre de app o NRE intermitente en WinUI.
        ///
        /// DESPUÉS: no hace PopModal (lo hace el caller); null-safe; PrepareDiscount
        /// envuelto en try para no bloquear la apertura del modal.
        /// </summary>
        private async Task<List<string>> ApplyPromo(sale_order saleOrder)
        {
            List<string> resultData = new List<string>();

            await EvalPromotions(saleOrder);

            bool ShowPromoPopup = false;
            AppliedPromotionResults ??= new ObservableCollection<PromotionEvalResult>();

            if (AppliedPromotionResults.Count == 0)
            {
                await UITools.HideLoadingPopup();
                await Toast.Make("No hay promociones aplicables").Show();
                return new List<string>();
            }

            foreach (var promoResult in AppliedPromotionResults)
            {
                if (promoResult?.Items == null)
                    continue;

                foreach (var promoResItem in promoResult.Items)
                {
                    if (promoResItem?.Promotion == null)
                        continue;

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

                    // Tipo 6 (descuento): preparar antes del modal.
                    // ANTES: si PrepareDiscount lanzaba NRE, no se abría el modal.
                    // DESPUÉS: se registra el error y se continúa al popup.
                    if (promoResItem.Promotion._promotion_type_id == 6)
                    {
                        try
                        {
                            await PrepareDiscount(saleOrder, promoResItem);
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine($"PrepareDiscount error (se continúa al modal): {ex}");
                        }

                        ShowPromoPopup = true;
                        break;
                    }
                }
            }

            //No se muestra Popup si no hay elemento que elegir
            if (!ShowPromoPopup)
            {
                await UITools.HideLoadingPopup();
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

            // Grid con data lista → quitar "Calculando promociones..." (no esperar al ShowPopup).
            bool gridHasData =
                (view.ItemsDataBenefitsRules?.Count ?? 0) > 0
                || (view.ItemsDataBenefits?.Count ?? 0) > 0
                || (AppliedPromotionResults?.Count ?? 0) > 0;

            if (gridHasData)
                await UITools.HideLoadingPopup();

            //if (!ShowPromoPopupLevel2) return new List<string>();

            var popup = new Popup
            {
                Content = view,
                BackgroundColor = Colors.Black.WithAlpha(0.4f), // fondo semi-transparente
                CanBeDismissedByTappingOutsideOfPopup = false,
                Padding = new Thickness(0),
                Margin = new Thickness(0)
            };

            view.ClosePopupAction = (promo) =>
            {
                _ = ClosePromoPopupSafeAsync(promo);
            };

            // Si el grid ya se pintó / abrió, asegurar que el loading no quede encima.
            void OnPromoViewLoaded(object sender, EventArgs e)
            {
                view.Loaded -= OnPromoViewLoaded;
                _ = UITools.HideLoadingPopup();
            }
            view.Loaded += OnPromoViewLoaded;

            var hostPage = Application.Current?.Windows?.FirstOrDefault()?.Page;
            if (hostPage == null)
            {
                await UITools.HideLoadingPopup();
                Debug.WriteLine("ApplyPromo: no hay Page para mostrar el popup de promociones.");
                return resultData;
            }

            // Por si aún seguía visible (sin data en colecciones pero igual se abre modal).
            await UITools.HideLoadingPopup();
            await Task.Yield();

            var result = await PopupExtensions.ShowPopupAsync<PromoResultPopup>(hostPage, popup, new PopupOptions
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

            if (result?.Result is PromoResultPopup selected)
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
                    foreach (var giftLine in selected.manualGifts ?? Enumerable.Empty<sale_order_line>())
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

                    foreach (var benefit in selected.benefits ?? Enumerable.Empty<PromotionEvalResult>())
                    {
                        if (benefit?.Items == null)
                            continue;

                        foreach (var promoItem in benefit.Items.Where(x =>
                            x?.Promotion != null &&
                            x.Promotion._promotion_type_id == 2 &&
                            x.Promotion._selection_type_id == 2))
                        {
                            if (promoItem.RuleSet == null)
                                continue;

                            foreach (var rule in promoItem.RuleSet)
                            {
                                if (rule?.ProductSequenceApplyList == null)
                                    continue;

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

                if (selected.ActionResult == 0)
                {
                    await Toast.Make("Promociones manuales no aplicadas.").Show();
                }

                // ANTES: PopModal aquí + PopModal en ButtonSave_Clicked.finally → crash WinUI
                //        ("Object reference" / UnhandledException / Debugger.Break).
                // DESPUÉS: el caller (ButtonSave) es el único que cierra el CRUD.
            }

            return resultData;
        }

        /// <summary>
        /// Cierra el popup de promos de forma segura.
        /// ANTES: ClosePopupAction llamaba ClosePopupAsync sin await → excepciones no observadas.
        /// DESPUÉS: Task fire-and-forget con try/catch y Page null-check.
        /// </summary>
        private static async Task ClosePromoPopupSafeAsync(PromoResultPopup promo)
        {
            try
            {
                var page = Application.Current?.Windows?.FirstOrDefault()?.Page;
                if (page == null)
                    return;

                await PopupExtensions.ClosePopupAsync(page, promo);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"ClosePromoPopupSafeAsync: {ex}");
            }
        }
    }
}
