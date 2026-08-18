using DMOrders.Services.Promotions;
using DMSA.Models.Odoo.DMOrders.promotions.abstractCustom;
using DMSA.Models.Odoo.Native;
using DMSA.Models.Odoo.Promotions;
using DMSA.Sync.Core.Database.Sqlite;
using DMSA.Sync.Core.Update.Pusher;
using Newtonsoft.Json;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace DMOrders.Pages.Fragments.Orders.modals;

public partial class PromocionesViewer
{
    public async Task PrepareList(PromotionEvalItem benefit)
    {
        _ItemsDataBenefits.Add(benefit);

        string promo_type_name = "";

        if(benefit.Promotion._promotion_type_id == 1)
        {
            promo_type_name = "BONIFICACION PARCIAL";
        }
        if (benefit.Promotion._promotion_type_id == 2)
        {
            promo_type_name = "BONIFICACION";
        }
        if (benefit.Promotion._promotion_type_id == 3)
        {
            promo_type_name = "NXN";
        }
        if (benefit.Promotion._promotion_type_id == 6)
        {
            promo_type_name = "DESCUENTOS";
        }
    }

    public async Task PrepareListRules(PromoRuleItem promoRuleItem)
    {
        string promo_type_name = "";

        if (promoRuleItem.promotion_type_id == 1)
        {
            promo_type_name = "BONIFICACION PARCIAL";
        }
        if (promoRuleItem.promotion_type_id == 2)
        {
            promo_type_name = "BONIFICACION";
        }
        if (promoRuleItem.promotion_type_id == 3)
        {
            promo_type_name = "NXN";
        }
        if (promoRuleItem.promotion_type_id == 6)
        {
            promo_type_name = "DESCUENTOS";
        }
        
        _ItemsDataBenefitsRules.Add(promoRuleItem);        
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
                    foreach(var rule in benefit.RuleSet)
                    {
                        PromoRuleItem promoRuleItem = Tools.FromBenefitRule(benefit, rule, SaleOrder);

                        //Se agregan los beneficios automáticos para cargar sus regalos si es que es tipo Bonificado == 2
                        if (benefit.Promotion._promotion_type_id == 2 && benefit.Promotion._selection_type_id == 1)
                        {
                            await AddAutoGiftsAsync(promoRuleItem);
                            //await HandleGiftAutoPromotion(promo.Items, productDb);
                        }

                        if (benefit.Promotion._promotion_type_id == 2 && benefit.Promotion._selection_type_id == 2)
                        {

                        }

                        //NxN se aplica automáticamente
                        if (benefit.Promotion._promotion_type_id == 4 && benefit.Promotion._selection_type_id == 1)
                        {
                            existPromosForEval = true;
                            await AddGiftNxn(promoRuleItem);
                        }

                        if (benefit?.Promotion?.id == null)
                            continue;

                        //_ItemsDataBenefits.Add(benefit);

                        await PrepareList(benefit);
                        await PrepareListRules(promoRuleItem);

                        if (!_ItemsDataBenefits.Any(x => x.Promotion?.id == benefit.Promotion.id))
                        {
                            //Quizas se deban acumular los qty * numero de apariciones de la promoción                            
                            benefit.FoundTimesApplies = 1;

                            //foreach (var rule in benefit.RuleSet)
                            //{
                                benefit.MaxAllowedGifts += rule.AllowedGifts;
                            //}

                            //Original
                            //benefit.MaxAllowedGifts = benefit.AllowedGifts;

                            //_ItemsDataBenefits.Add(benefit);
                        }
                        else
                        {
                            var existing = _ItemsDataBenefits.First(x => x.Promotion?.id == benefit.Promotion.id);

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
                        }

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
                        if (benefit.Promotion._promotion_type_id == 2 && benefit.Promotion._selection_type_id == 2)
                        {
                            GlobalTotalManualGiftsAllowed += benefit.MaxAllowedGifts;
                            PromotionEngineRunner promotionEngineRunner = new PromotionEngineRunner();

                            foreach (var rule in benefit.RuleSet)
                            {
                                PromoRuleItem promoRuleItem = Tools.FromBenefitRule(benefit, rule, SaleOrder);
                                await promotionEngineRunner.UpdateApplyPromotion(SaleOrder, promoRuleItem, saleOrderPromotions);
                            }

                            GlobalTotalManualGiftsForRemove += benefit.GiftsForRemove;
                        }

                        //Bonificado / Automático
                        if (benefit.Promotion._promotion_type_id == 2 && benefit.Promotion._selection_type_id == 1)
                        {

                        }

                        //NxN
                        if (benefit.Promotion._promotion_type_id == 4 && benefit.Promotion._selection_type_id == 1)
                        {
                            // El regalo auto se marca con promoRuleItem; promotionEvalItem queda null
                            // y hacía que el total mostrado cayera a 0.
                            int totalGifts = 0;
                            foreach (var giftItem in _promoGiftsAuto)
                            {
                                if (giftItem == null)
                                    continue;

                                int giftPromoId = giftItem.promoRuleItem?.promo_id
                                    ?? giftItem.promotionEvalItem?.Promotion?.id
                                    ?? 0;

                                if (giftPromoId != benefit.Promotion.id)
                                    continue;

                                Debug.WriteLine($"Este es el regalo {giftItem.qty_gift}");
                                totalGifts += giftItem.qty_gift;
                            }

                            if (totalGifts > 0)
                                benefit.MaxAllowedGifts = totalGifts;
                        }
                    }
                }
            }

            OnPropertyChanged(nameof(ItemsData));
            OnPropertyChanged(nameof(ItemsDataBenefits));
            OnPropertyChanged(nameof(ItemsDataBenefitsRules));            
            OnPropertyChanged(nameof(TotalGiftsForRemove));
            OnPropertyChanged(nameof(TotalGiftsRemoved));
            OnPropertyChanged(nameof(RequiredRemoveItems));
        }

        //AutoselectManualPromotion();
        AutoselectManualPromotionRule();
    }

    public List<PromoRuleItem> ListFromPromo(PromotionEvalItem benefit)
    {
        string promo_type_name = "";

        if (benefit.Promotion._promotion_type_id == 1)
        {
            promo_type_name = "BONIFICACION PARCIAL";
        }
        if (benefit.Promotion._promotion_type_id == 2)
        {
            promo_type_name = "BONIFICACION";
        }
        if (benefit.Promotion._promotion_type_id == 3)
        {
            promo_type_name = "NXN";
        }
        if (benefit.Promotion._promotion_type_id == 6)
        {
            promo_type_name = "DESCUENTOS";
        }
        List <PromoRuleItem> resultRuleItems = new List<PromoRuleItem>();

        foreach (var rule in benefit.RuleSet)
        {
            var promoRuleItemAdd = Tools.FromBenefitRule(benefit, rule, SaleOrder);
            _ItemsDataBenefitsRules.Add(promoRuleItemAdd);
        }

        return resultRuleItems;
    }


    private List<PromotionEvalItem> __GetPromotionData(string json,Dictionary<string, List<PromotionEvalItem>> cache)
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

    private List<PromoRuleItem> GetPromotionData(string json, Dictionary<string, List<PromoRuleItem>> cache)
    {
        if (string.IsNullOrEmpty(json))
            return new List<PromoRuleItem>();

        if (!cache.TryGetValue(json, out var result))
        {
            result = JsonConvert.DeserializeObject<List<PromoRuleItem>>(json);
            cache[json] = result;
        }

        return result;
    }

    /// <summary>
    /// Regalo manual (ManualGiftsSeparateLinePerPromo): identifica línea por promo_id.
    /// </summary>
    private static bool GiftLineBelongsToPromo(sale_order_line? line, int promoId)
    {
        if (line == null || !line.is_gift)
            return false;

        if (line.promotion_ids != null && line.promotion_ids.Contains(promoId))
            return true;

        if (string.IsNullOrWhiteSpace(line.promotion_data))
            return false;

        try
        {
            var rules = JsonConvert.DeserializeObject<List<PromoRuleItem>>(line.promotion_data);
            return rules?.Any(r => r != null && r.promo_id == promoId) == true;
        }
        catch
        {
            return false;
        }
    }

    private static sale_order_line? FindGiftLineForPromo(
        IEnumerable<sale_order_line> orderLines,
        int productId,
        int promoId)
    {
        return orderLines.FirstOrDefault(l =>
            l != null
            && l.is_gift
            && l.product_id == productId
            && GiftLineBelongsToPromo(l, promoId));
    }

    private static int SumGiftQtyForPromo(IEnumerable<sale_order_line> orderLines, int promoId)
    {
        return orderLines
            .Where(l => l != null && l.is_gift && GiftLineBelongsToPromo(l, promoId))
            .Sum(l => (int)l.product_uom_qty_real);
    }

    /// <summary>
    /// Regalo automático (AutoGiftsSeparateLinePerPromo): identifica línea por (promo_id, rule_id).
    /// Con el flag apagado la clave es solo el código, replicando la fusión anterior.
    /// </summary>
    private static string AutoGiftKey(string? defaultCode, int promoId, int ruleId)
    {
        return AutoGiftsSeparateLinePerPromo
            ? $"{defaultCode}|{promoId}|{ruleId}"
            : defaultCode ?? string.Empty;
    }

    private static string AutoGiftKey(product_product? product)
    {
        if (product == null)
            return string.Empty;

        return AutoGiftKey(
            product.default_code,
            product.promoRuleItem?.promo_id ?? 0,
            product.promoRuleItem?.id ?? 0);
    }

    private static bool GiftLineBelongsToPromoRule(sale_order_line? line, int promoId, int ruleId)
    {
        if (line == null || !line.is_gift)
            return false;

        if (!string.IsNullOrWhiteSpace(line.promotion_data))
        {
            try
            {
                var rules = JsonConvert.DeserializeObject<List<PromoRuleItem>>(line.promotion_data);

                if (rules != null && rules.Count > 0)
                    return rules.Any(r => r != null && r.promo_id == promoId && r.id == ruleId);
            }
            catch
            {
                // promotion_data puede no ser PromoRuleItem; se evalúan los arrays.
            }
        }

        return line.promotion_ids != null
            && line.rule_ids != null
            && line.promotion_ids.Contains(promoId)
            && line.rule_ids.Contains(ruleId);
    }

    /// <summary>
    /// True si la línea regalo corresponde a esta misma promo/regla automática y por tanto
    /// puede acumular cantidad. Los regalos manuales nunca entran aquí.
    /// </summary>
    private static bool AutoGiftLineMatchesRule(sale_order_line? line, PromoRuleItem promoRuleItem)
    {
        if (line == null || promoRuleItem == null)
            return false;

        if (!AutoGiftsSeparateLinePerPromo)
            return true;

        if (line.is_manual)
            return false;

        return GiftLineBelongsToPromoRule(line, promoRuleItem.promo_id, promoRuleItem.id);
    }
}
