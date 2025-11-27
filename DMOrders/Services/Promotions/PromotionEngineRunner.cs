using DMOrders.Services.Database.Sqlite;
using DMSA.Models.Odoo.DMOrders.promotions.@abstract;
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
        public async Task<bool> CanApplyPromotion(sale_order order, PromotionEvalItem benefit)
        {
            var saleOrderPromotion = new SaleOrderPromotionsDb(App.Session.odooConnection.DbNameSqlite);
            var existingPromos = await saleOrderPromotion.GetItemsByPromoEval(order, benefit);

            if (existingPromos != null && existingPromos.Count > 0)
            {
                foreach(var promo in existingPromos)
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
                await saleOrderPromotion.InsertOrUpdate(order, benefit);                
            }
            return true;
        }

        public async Task<bool> AddApplyPromotion(sale_order order, PromotionEvalItem benefit, int times)
        {
            var saleOrderPromotion = new SaleOrderPromotionsDb(App.Session.odooConnection.DbNameSqlite);
            var existingPromos = await saleOrderPromotion.AddApply(order, benefit, times);

            if (existingPromos != null && existingPromos.Count > 0)
            {                
                return false;
            }
            
            return true;
        }

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
