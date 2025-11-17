using DMSA.Models.Odoo.DMOrders.promotions;

namespace DMOrders.Services.Database.Sqlite
{
    public class PromotionProductDetailDb : SqliteDbBase<PromotionProductDetail>
    {


        public PromotionProductDetailDb(string _DatabaseFilename) : base(_DatabaseFilename)
        {

        }

        public async Task<List<PromotionProductDetail>> GetItemsAsync()
        {
            await Init();
            return await Database.Table<PromotionProductDetail>().ToListAsync();
        }

        public async Task<PromotionProductDetail> GetItem(int id)
        {
            await Init();
            return await Database.Table<PromotionProductDetail>().Where(x=>x.id == id).FirstOrDefaultAsync();
        }

        public async Task<List<PromotionProductDetail>> GetItemsByParent(int parentId)
        {
            await Init();
            return await Database.Table<PromotionProductDetail>().Where(x => x._parent_id == parentId).ToListAsync();
        }

        public async Task<List<PromotionProductDetail>> GetItemsByPromo(int promoId)
        {
            await Init();
            return await Database.Table<PromotionProductDetail>().Where(x => x._promo_id == promoId).ToListAsync();
        }
    }
}
