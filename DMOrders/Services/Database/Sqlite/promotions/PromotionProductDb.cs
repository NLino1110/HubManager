using DMSA.Models.Odoo.DMOrders.promotions;

namespace DMOrders.Services.Database.Sqlite
{
    public class PromotionProductDb : SqliteDbBase<PromotionProduct>
    {        
        public PromotionProductDb(string _DatabaseFilename) : base(_DatabaseFilename)
        {

        }

        public async Task<List<PromotionProduct>> GetItemsAsync()
        {
            await Init();
            return await Database.Table<PromotionProduct>().ToListAsync();
        }

        public async Task<PromotionProduct> GetItem(int id)
        {
            await Init();
            return await Database.Table<PromotionProduct>().Where(x=>x.id == id).FirstOrDefaultAsync();
        }        

        public async Task<List<PromotionProduct>> GetItemsByPromo(int promoId)
        {
            await Init();
            return await Database.Table<PromotionProduct>().Where(x => x._promo_id == promoId).ToListAsync();
        }
    }
}
