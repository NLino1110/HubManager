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
        public async Task<bool> CanApplyPromotion(
            sale_order order, 
            PromotionEvalItemV2 promotionEvalItem, 
            List<SaleOrderPromotions> saleOrderPromotions)
        {
            if (saleOrderPromotions != null && saleOrderPromotions.Count > 0)
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
                }
            }
            else
            {
                var newItem = new SaleOrderPromotions
                {
                    order_id = order.id,
                    promotion_id = promotionEvalItem.Promotion.id,
                    promotion_type_id = promotionEvalItem.Promotion._promotion_type_id,
                    promotion_selection_type_id = promotionEvalItem.Promotion._selection_type_id,
                    promotion_centers = promotionEvalItem.PricelistId,
                    times_inv = promotionEvalItem.TotalTimesAllowed,
                    times_inv_applied = 0,
                    applied = false
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
                }
            }

            return true;
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
