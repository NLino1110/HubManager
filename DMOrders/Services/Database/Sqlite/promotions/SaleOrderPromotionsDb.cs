using DMSA.Models.Odoo.DMOrders.promotions;
using DMSA.Models.Odoo.DMOrders.promotions.abstractCustom;
using DMSA.Models.Odoo.Native;
using Microsoft.Data.Sqlite;
using SQLite;
using SQLiteNetExtensions.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMOrders.Services.Database.Sqlite
{
    public class SaleOrderPromotionsDb : SqliteDbBase<SaleOrderPromotions>
    {

        public SaleOrderPromotionsDb(string _DatabaseFilename) : base(_DatabaseFilename)
        {

        }
        
        public async Task<List<SaleOrderPromotions>> GetItemsByPromotion(int parentId)
        {
            await Init();
            return await Database.Table<SaleOrderPromotions>().Where(x=>x.promotion_id == parentId).ToListAsync();
        }

        public async Task<List<SaleOrderPromotions>> GetItemsByOrder(int orderId)
        {
            await Init();
            return await Database.Table<SaleOrderPromotions>().Where(x => x.order_id == orderId).ToListAsync();
        }

        public async Task<List<SaleOrderPromotions>> GetItemsByIds(int orderId, int promotion_id, int center_id)
        {
            await Init();
            return await Database.Table<SaleOrderPromotions>().Where(x => x.order_id == orderId 
            && x.promotion_id == promotion_id
            && x.promotion_centers == center_id
            ).ToListAsync();
        }

        public async Task<List<SaleOrderPromotions>> GetItemsByPromoEval(sale_order order, PromotionEvalItemV2 promotionEvalItem)
        {
            await Init();
            return await Database.Table<SaleOrderPromotions>().Where(x => x.order_id == order.id
            && x.promotion_id == promotionEvalItem.Promotion.id
            && x.promotion_centers == promotionEvalItem.PricelistId
            ).ToListAsync();
        }

        public async Task<List<SaleOrderPromotions>> InsertOrUpdate(sale_order order, PromotionEvalItemV2 promotionEvalItem)
        {
            await Init();
            var existingItems = await GetItemsByIds(order.id, promotionEvalItem.Promotion.id, promotionEvalItem.PricelistId);
            if (existingItems != null && existingItems.Count > 0)
            {
                return existingItems;
            }
            else
            {
                var newItem = new SaleOrderPromotions
                {
                    order_id = order.id,
                    promotion_id = promotionEvalItem.Promotion.id,
                    promotion_centers = promotionEvalItem.PricelistId,
                    times_inv = promotionEvalItem.TotalTimesAllowed,
                    times_inv_applied = 1,
                    applied = false
                };
                await Database.InsertAsync(newItem);
                return new List<SaleOrderPromotions> { newItem };
            }
        }

        public async Task<List<SaleOrderPromotions>> AddApply(
                    sale_order order,
                    PromotionEvalItemV2 promotionEvalItem,
                    int times_inv)
        {
            await Init();
            var existingItems = await GetItemsByIds(order.id, promotionEvalItem.Promotion.id, promotionEvalItem.PricelistId);
            if (existingItems != null && existingItems.Count > 0)
            {
                foreach (var item in existingItems)
                {
                    var newValue = item.times_inv_applied + times_inv;

                    if (newValue > item.times_inv)
                    {                        
                        continue;
                    }

                    item.times_inv_applied = newValue;

                    item.applied = item.times_inv_applied >= item.times_inv;
                    await Database.UpdateAsync(item);
                }

                return existingItems;
            }

            return new List<SaleOrderPromotions>();
        }

        public async Task<int> DeleteItemOfParent(sale_order parent)
        {
            await Init();
            int count = 0;
            
            var resultItems = (await Database.Table<SaleOrderPromotions>().ToListAsync()).Where(i => i.order_id == parent.id);

            foreach (var item in resultItems)
            {
                count++;
                await Database.DeleteAsync(item);
            }

            return count;
        }
    }
}
