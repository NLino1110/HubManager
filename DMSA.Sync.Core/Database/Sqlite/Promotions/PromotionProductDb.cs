using DMSA.Models.Odoo.DMOrders.promotions;

namespace DMSA.Sync.Core.Database.Sqlite.Benefits
{
    public class PromotionProductDb : SqliteDbBase<PromotionProduct>
    {        
        public PromotionProductDb(string _DatabaseFilename) : base(_DatabaseFilename)
        {

        }

        protected override async Task OnAfterInit()
        {
            await Database.RunInTransactionAsync(tran =>
            {
                tran.Execute("CREATE INDEX IF NOT EXISTS idx_promotion_product_promo_id ON promotion_product(_promo_id)");
            });
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
            return await Database.Table<PromotionProduct>().Where(x => x._promo_id == promoId && x._bonus_id == 0).ToListAsync();
        }
    }
}
