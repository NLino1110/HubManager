using DMSA.Models.Odoo.DMOrders.promotions.abstractCustom;
using DMSA.Models.Odoo.Native;

namespace DMSA.Sync.Core.Database.Sqlite.Benefits
{
    public class SaleOrderPromotionsDb : SqliteDbBase<SaleOrderPromotions>
    {

        public SaleOrderPromotionsDb(string _DatabaseFilename) : base(_DatabaseFilename)
        {

        }

        protected override async Task OnAfterInit()
        {
            await Database.RunInTransactionAsync(tran =>
            {
                tran.Execute("CREATE INDEX IF NOT EXISTS idx_sale_order_promotion_id ON sale_order_promotion(id)");
                tran.Execute("CREATE INDEX IF NOT EXISTS idx_sale_order_promotion_order_id ON sale_order_promotion(order_id)");
                tran.Execute("CREATE INDEX IF NOT EXISTS idx_sale_order_promotion_promotion_id ON sale_order_promotion(promotion_id)");
            });
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

        public async Task<List<SaleOrderPromotions>> GetItemsByPromoEval(sale_order order, PromotionEvalItem promotionEvalItem)
        {
            await Init();
            return await Database.Table<SaleOrderPromotions>().Where(x => x.order_id == order.id
            && x.promotion_id == promotionEvalItem.Promotion.id
            && x.promotion_centers == promotionEvalItem.PricelistId
            ).ToListAsync();
        }

        public async Task<List<SaleOrderPromotions>> InsertOrUpdate(sale_order order, PromotionEvalItem promotionEvalItem)
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
                    promotion_selection_type_id = promotionEvalItem.Promotion._selection_type_id,
                    promotion_type_id = promotionEvalItem.Promotion._promotion_type_id,
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
                    PromotionEvalItem promotionEvalItem,
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
