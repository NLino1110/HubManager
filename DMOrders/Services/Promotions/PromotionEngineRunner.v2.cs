using DMSA.Models.Odoo.DMOrders.promotions.abstractCustom;
using DMSA.Models.Odoo.Native;
using System.Diagnostics;

namespace DMOrders.Services.Promotions
{
    public partial class PromotionEngineRunner
    {
        public string GetRelatedProductTmplIds(PromoRuleItem ruleMatch)
        {
            List<int> allProductTmplIds = new();
                            
            string raw = ruleMatch.ProductTmplIds;
            if (!string.IsNullOrWhiteSpace(raw))
            {
                // Limpia: quita corchetes
                string cleaned = raw.Replace("[", "").Replace("]", "").Trim();
                if (!string.IsNullOrWhiteSpace(cleaned))
                {
                    var ids = cleaned
                        .Split(',', StringSplitOptions.RemoveEmptyEntries)
                        .Select(x => int.Parse(x.Trim()));
                    allProductTmplIds.AddRange(ids);
                }
            }
            
            var finalIds = allProductTmplIds.Distinct().ToList();
            
            string fullProductTmplIds = $"[{string.Join(",", finalIds)}]";
            return fullProductTmplIds;
        }

        /// <summary>
        /// Indica si la promo/regla aún puede aplicarse al pedido.
        /// ANTES: saleOrderPromotions.Any(...) sin null-check → NRE si la lista era null.
        /// DESPUÉS: retorna false si order / promoRuleItem / lista son null; ignora ítems null.
        /// </summary>
        public async Task<bool> CanApplyPromotion(
            sale_order order,
            PromoRuleItem promoRuleItem, 
            List<SaleOrderPromotions> saleOrderPromotions)
        {
            if (promoRuleItem == null || order == null) return false;
            if (saleOrderPromotions == null) return false;

            bool exists = saleOrderPromotions.Any(x =>
                x != null &&
                x.order_id == order.id &&
                x.promotion_id == promoRuleItem.promo_id &&
                x.promotion_centers == promoRuleItem.promo_pricelist_id &&
                x.rule_id == promoRuleItem.id
                );

            if (exists)
            {
                var existingPromos = saleOrderPromotions.Where(x => x.order_id == order.id &&
                x.promotion_id == promoRuleItem.promo_id &&
                x.promotion_centers == promoRuleItem.promo_pricelist_id &&
                x.rule_id == promoRuleItem.id).ToList();
                
                if (existingPromos == null || existingPromos.Count == 0)
                {
                    return true;
                }

                foreach (var promo in existingPromos)
                {
                    Debug.WriteLine($"Promoción existente: ID {promo.promotion_id}, Aplicada: {promo.applied}");
                    if (promo.applied == true || promo.times_inv == promo.times_inv_applied)
                    {
                        Debug.WriteLine($"Descuento de promoción ya ha sido aplicado");
                        return false;
                    }
                    else
                    {
                        
                    }
                }
            }
            else
            {
                string fullProductTmplIds = GetRelatedProductTmplIds(promoRuleItem);

                var newItem = new SaleOrderPromotions
                {
                    order_id = order.id,
                    promotion_id = promoRuleItem.promo_id,
                    rule_id = promoRuleItem.id,
                    promotion_type_id = promoRuleItem.promotion_type_id,
                    promotion_selection_type_id = promoRuleItem.selection_type_id,
                    promotion_centers = promoRuleItem.promo_pricelist_id,
                    times_inv = promoRuleItem.TotalTimesAllowed,
                    times_inv_applied = 0,
                    applied = false,
                    related_product_tmpl_ids = fullProductTmplIds
                };

                saleOrderPromotions.Add(newItem);
            }
            return true;
        }

        public async Task<bool> AddApplyPromotion(sale_order order,
            PromoRuleItem promoRuleItem, 
            int times_inv, 
            List<SaleOrderPromotions> saleOrderPromotions)
        {
            if (promoRuleItem == null)
                return false;
            //saleOrderPromotions != null && saleOrderPromotions.Count > 0

            var existingPromos = saleOrderPromotions.Where(x => x.order_id == order.id &&
                x.promotion_id == promoRuleItem.promo_id &&
                x.promotion_centers == promoRuleItem.promo_pricelist_id &&
                x.rule_id == promoRuleItem.id).ToList();

            if (existingPromos != null && existingPromos.Count > 0)
            {
                foreach (var item in existingPromos)
                {
                    var newValue = item.times_inv_applied + times_inv;

                    if (newValue > item.times_inv)
                    {
                        continue;
                    }

                    if(newValue < 0)
                    {
                        newValue = 0;
                    }

                    item.times_inv_applied = newValue;

                    item.applied = item.times_inv_applied >= item.times_inv;
                    //item.max_gifts = promotionEvalItem.MaxAllowedGifts;
                    //item.assigned_gifts = promotionEvalItem.MaxAllowedGifts;
                }
            }

            return true;
        }

        public async Task<bool> UpdateApplyPromotion(sale_order order,
            PromoRuleItem promoRuleItem,            
            List<SaleOrderPromotions> saleOrderPromotions)
        {
            if (promoRuleItem == null) return false;

            var existingPromos = saleOrderPromotions.Where(
                x => x.order_id == order.id &&
                x.promotion_id == promoRuleItem.promo_id &&
                x.promotion_centers == promoRuleItem.promo_pricelist_id &&
                x.rule_id == promoRuleItem.id).ToList();

            string fullProductTmplIds = GetRelatedProductTmplIds(promoRuleItem);

            if (existingPromos != null && existingPromos.Count > 0)
            {
                foreach (var item in existingPromos)
                {
                    item.gifts_for_remove = 0;
                    
                    //Aquí se evalúa si es que se deben eliminar items
                    if (item.assigned_gifts > promoRuleItem.MaxAllowedGifts)
                    {
                        item.gifts_for_remove = item.assigned_gifts - promoRuleItem.MaxAllowedGifts;
                    }
                    
                    promoRuleItem.GiftsForRemove = item.gifts_for_remove;
                    item.max_gifts = promoRuleItem.MaxAllowedGifts;
                    item.related_product_tmpl_ids = fullProductTmplIds;
                    //item.assigned_gifts = promotionEvalItem.MaxAllowedGifts;
                }
            }

            return true;
        }

        public async Task<bool> ResetManualGiftBenefitSoft(
            sale_order order,            
            List<SaleOrderPromotions> saleOrderPromotions
            )
        {
            var existingPromos = saleOrderPromotions.Where(x => x.order_id == order.id &&
                x.promotion_type_id == 2 &&
                x.promotion_selection_type_id == 2).ToList();

            if (existingPromos != null && existingPromos.Count > 0)
            {
                foreach (var item in existingPromos)
                {
                    item.times_inv_applied = 0;
                    item.applied = false;
                }
            }

            return true;
        }

        public async Task<bool> ResetManualGiftBenefit(
            sale_order order,
            List<SaleOrderPromotions> saleOrderPromotions
            )
        {

            var existingPromos = saleOrderPromotions.Where(x => x.order_id == order.id &&
                x.promotion_type_id == 2 &&
                x.promotion_selection_type_id == 2).ToList();

            if (existingPromos != null && existingPromos.Count > 0)
            {
                foreach (var item in existingPromos)
                {
                    //item.times_inv = 0;
                    item.times_inv_applied = 0;
                    //item.max_gifts = 0;
                    item.assigned_gifts = 0;
                    item.applied = false;
                }
            }

            return true;
        }

        public async Task<SaleOrderPromotions> GetDataBenefit(
            sale_order order,
            PromoRuleItem promoRuleItem,
            List<SaleOrderPromotions> saleOrderPromotions
            )
        {

            if (promoRuleItem == null) return null;

            var existingBenefit = saleOrderPromotions.Where(x => x.order_id == order.id &&
                x.promotion_id == promoRuleItem.promo_id &&
                x.promotion_centers == promoRuleItem.promo_pricelist_id &&
                x.rule_id == promoRuleItem.id).FirstOrDefault();

            if (existingBenefit != null)
            {
                return existingBenefit;                
            }

            return null;
        }

        //public async Task<SaleOrderPromotions> AddGitfs(
        //    sale_order order,
        //    PromotionEvalItem promotionEvalItem,
        //    List<SaleOrderPromotions> saleOrderPromotions,
        //    int giftsToAdd
        //    )
        //{
        //    var existingPromos = saleOrderPromotions.Where(x => x.order_id == order.id &&
        //        x.promotion_id == promotionEvalItem.Promotion.id &&
        //        x.promotion_centers == promotionEvalItem.PricelistId).FirstOrDefault();

        //    if (existingPromos != null)
        //    {
        //        existingPromos.assigned_gifts += giftsToAdd;
        //        return existingPromos;
        //    }

        //    return null;
        //}
    }
}
