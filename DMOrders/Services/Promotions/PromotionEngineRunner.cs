using DMOrders.Services.Database.Sqlite;
using DMSA.Models.Odoo.DMOrders.promotions.abstractCustom;
using DMSA.Models.Odoo.Native;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMOrders.Services.Promotions
{
    public class PromotionEngineRunner
    {
        public string GetRelatedProductTmplIds(
            PromotionEvalItemV2 promotionEvalItem)
        {
            List<int> allProductTmplIds = new();
            if (promotionEvalItem.RuleSet != null && promotionEvalItem.RuleSet.Count > 0)
            {
                foreach (var ruleMatch in promotionEvalItem.RuleSet)
                {
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
                }
            }
            // Eliminamos duplicados y usamos orden opcional
            var finalIds = allProductTmplIds.Distinct().ToList();
            // Construimos el string final con formato de array
            string fullProductTmplIds = $"[{string.Join(",", finalIds)}]";
            return fullProductTmplIds;
        }

        public async Task<bool> CanApplyPromotion(
            sale_order order, 
            PromotionEvalItemV2 promotionEvalItem, 
            List<SaleOrderPromotions> saleOrderPromotions)
        {
            bool exists = saleOrderPromotions.Any(x =>
                x.order_id == order.id &&
                x.promotion_id == promotionEvalItem.Promotion.id &&
                x.promotion_centers == promotionEvalItem.PricelistId
                );

            if (exists)
            {
                var existingPromos = saleOrderPromotions.Where(x => x.order_id == order.id &&
                x.promotion_id == promotionEvalItem.Promotion.id &&
                x.promotion_centers == promotionEvalItem.PricelistId).ToList();
                
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
                        //Se deben +/- item.assigned_gifts = promotionEvalItem.MaxAllowedGifts;
                        //promo.assigned_gifts++;
                    }
                }
            }
            else
            {

                //List<int> allProductTmplIds = new();

                //if (promotionEvalItem.RuleSet != null && promotionEvalItem.RuleSet.Count > 0)
                //{
                //    foreach (var ruleMatch in promotionEvalItem.RuleSet)
                //    {
                //        string raw = ruleMatch.ProductTmplIds;

                //        if (!string.IsNullOrWhiteSpace(raw))
                //        {
                //            // Limpia: quita corchetes
                //            string cleaned = raw.Replace("[", "").Replace("]", "").Trim();

                //            if (!string.IsNullOrWhiteSpace(cleaned))
                //            {
                //                var ids = cleaned
                //                    .Split(',', StringSplitOptions.RemoveEmptyEntries)
                //                    .Select(x => int.Parse(x.Trim()));

                //                allProductTmplIds.AddRange(ids);
                //            }
                //        }
                //    }
                //}

                //// Eliminamos duplicados y usamos orden opcional
                //var finalIds = allProductTmplIds.Distinct().ToList();

                //// Construimos el string final con formato de array
                //string fullProductTmplIds = $"[{string.Join(",", finalIds)}]";

                string fullProductTmplIds = GetRelatedProductTmplIds(promotionEvalItem);

                var newItem = new SaleOrderPromotions
                {
                    order_id = order.id,
                    promotion_id = promotionEvalItem.Promotion.id,
                    promotion_type_id = promotionEvalItem.Promotion._promotion_type_id,
                    promotion_selection_type_id = promotionEvalItem.Promotion._selection_type_id,
                    promotion_centers = promotionEvalItem.PricelistId,
                    times_inv = promotionEvalItem.TotalTimesAllowed,
                    times_inv_applied = 0,
                    applied = false,
                    related_product_tmpl_ids = fullProductTmplIds
                };

                saleOrderPromotions.Add(newItem);
            }
            return true;
        }

        public async Task<bool> AddApplyPromotion(sale_order order, 
            PromotionEvalItemV2 promotionEvalItem, 
            int times_inv, 
            List<SaleOrderPromotions> saleOrderPromotions)
        {
            //saleOrderPromotions != null && saleOrderPromotions.Count > 0

            var existingPromos = saleOrderPromotions.Where(x => x.order_id == order.id &&
                x.promotion_id == promotionEvalItem.Promotion.id &&
                x.promotion_centers == promotionEvalItem.PricelistId).ToList();

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
            PromotionEvalItemV2 promotionEvalItem,            
            List<SaleOrderPromotions> saleOrderPromotions)
        {
            var existingPromos = saleOrderPromotions.Where(x => x.order_id == order.id &&
                x.promotion_id == promotionEvalItem.Promotion.id &&
                x.promotion_centers == promotionEvalItem.PricelistId).ToList();

            string fullProductTmplIds = GetRelatedProductTmplIds(promotionEvalItem);

            if (existingPromos != null && existingPromos.Count > 0)
            {
                foreach (var item in existingPromos)
                {
                    item.gifts_for_remove = 0;
                    //item.gifts_for_remove = item.max_gifts - promotionEvalItem.MaxAllowedGifts;
                    if (item.assigned_gifts > promotionEvalItem.MaxAllowedGifts)
                    {
                        item.gifts_for_remove = item.assigned_gifts - promotionEvalItem.MaxAllowedGifts;
                    }
                    //item.gifts_for_remove = item.max_gifts - item.assigned_gifts;
                    promotionEvalItem.GiftsForRemove = item.gifts_for_remove;
                    item.max_gifts = promotionEvalItem.MaxAllowedGifts;
                    item.related_product_tmpl_ids = fullProductTmplIds;
                    //item.assigned_gifts = promotionEvalItem.MaxAllowedGifts;
                }
            }

            return true;
        }

        public async Task<bool> ResetManualGiftBenefitSoft(
            sale_order order,
            //PromotionEvalItemV2 promotionEvalItem,
            List<SaleOrderPromotions> saleOrderPromotions
            )
        {
            //Se planea utilizar related_product_tmpl_ids para identificar los productos por los cuales
            // se aplicó el beneficio de regalo manual

            //saleOrderPromotions[0].related_product_tmpl_ids

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
            //Por ahora la idea es solo resetear los beneficios aplicados de tipo regalo manual

            //for ( var i = 0; i < saleOrderPromotions.Count; i++)
            //{
            //    var item = saleOrderPromotions[i];
            //    saleOrderPromotions.Remove(item);
            //}

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
            PromotionEvalItemV2 promotionEvalItem,
            List<SaleOrderPromotions> saleOrderPromotions
            )
        {
            var existingBenefit = saleOrderPromotions.Where(x => x.order_id == order.id &&
                x.promotion_id == promotionEvalItem.Promotion.id &&
                x.promotion_centers == promotionEvalItem.PricelistId).FirstOrDefault();

            if (existingBenefit != null)
            {
                return existingBenefit;                
            }

            return null;
        }

        public async Task<SaleOrderPromotions> AddGitfs(
            sale_order order,
            PromotionEvalItemV2 promotionEvalItem,
            List<SaleOrderPromotions> saleOrderPromotions,
            int giftsToAdd
            )
        {
            var existingPromos = saleOrderPromotions.Where(x => x.order_id == order.id &&
                x.promotion_id == promotionEvalItem.Promotion.id &&
                x.promotion_centers == promotionEvalItem.PricelistId).FirstOrDefault();

            if (existingPromos != null)
            {
                existingPromos.assigned_gifts += giftsToAdd;
                return existingPromos;
            }

            return null;
        }

        //public async Task<bool> CanApplyPromotion(sale_order order, PromotionEvalItem benefit)
        //{
        //    var saleOrderPromotion = new SaleOrderPromotionsDb(App.Session.odooConnection.DbNameSqlite);
        //    var existingPromos = await saleOrderPromotion.GetItemsByPromoEval(order, benefit);

        //    if (existingPromos != null && existingPromos.Count > 0)
        //    {
        //        foreach(var promo in existingPromos)
        //        {
        //            Debug.WriteLine($"Promoción existente: ID {promo.promotion_id}, Aplicada: {promo.applied}");
        //            if (promo.applied == true || promo.times_inv == promo.times_inv_applied)
        //            {
        //                Debug.WriteLine($"Descuento de promoción ya ha sido aplicado");
        //                return false;
        //            }
        //        }
        //    }
        //    else
        //    {
        //        await saleOrderPromotion.InsertOrUpdate(order, benefit);                
        //    }
        //    return true;
        //}

        //public async Task<bool> AddApplyPromotion(sale_order order, PromotionEvalItem benefit, int times)
        //{
        //    var saleOrderPromotion = new SaleOrderPromotionsDb(App.Session.odooConnection.DbNameSqlite);
        //    var existingPromos = await saleOrderPromotion.AddApply(order, benefit, times);

        //    if (existingPromos != null && existingPromos.Count > 0)
        //    {                
        //        return false;
        //    }

        //    return true;
        //}

        //public async Task<bool> SubstractApplyPromotion(sale_order order, PromotionEvalItem benefit, int times)
        //{
        //    var saleOrderPromotion = new SaleOrderPromotionsDb(App.Session.odooConnection.DbNameSqlite);
        //    var existingPromos = await saleOrderPromotion.AddApply(order, benefit, times);

        //    if (existingPromos != null && existingPromos.Count > 0)
        //    {
        //        return false;
        //    }

        //    return true;
        //}
    }
}
