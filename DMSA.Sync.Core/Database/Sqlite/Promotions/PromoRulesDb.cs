using DMSA.Models.Odoo.DMOrders.promotions;

namespace DMSA.Sync.Core.Database.Sqlite.Benefits
{
    public class PromoRulesDb : SqliteDbBase<PromoRules>
    {

        public PromoRulesDb(string _DatabaseFilename) : base(_DatabaseFilename)
        {

        }

        protected override async Task OnAfterInit()
        {
            await Database.RunInTransactionAsync(tran =>
            {
                tran.Execute("CREATE INDEX IF NOT EXISTS idx_promo_rule_promo_id ON promo_rules(_promo_id)");                
            });
        }

        public async Task<List<PromoRules>> GetItemsAsync()
        {
            await Init();
            return await Database.Table<PromoRules>().ToListAsync();
        }

        public async Task<PromoRules> GetItem(int id)
        {
            await Init();
            return await Database.Table<PromoRules>().Where(x=>x.id == id).FirstOrDefaultAsync();
        }

        public async Task<List<PromoRules>> GetItemsByParent(int parentId)
        {
            await Init();
            return await Database.Table<PromoRules>().Where(x=>x._promo_id == parentId).ToListAsync();
        }
    }
}
