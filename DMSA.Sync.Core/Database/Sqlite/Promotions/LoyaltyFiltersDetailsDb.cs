using DMSA.Models.Odoo.DMOrders.promotions;

namespace DMSA.Sync.Core.Database.Sqlite.Benefits
{
    public class LoyaltyFiltersDetailsDb : SqliteDbBase<LoyaltyFiltersDetail>
    {
        public LoyaltyFiltersDetailsDb(string _DatabaseFilename) : base(_DatabaseFilename)
        {

        }

        public async Task<List<LoyaltyFiltersDetail>> GetItemsAsync()
        {
            await Init();
            return await Database.Table<LoyaltyFiltersDetail>().ToListAsync();
        }

        public async Task<LoyaltyFiltersDetail> GetItem(int id)
        {
            await Init();
            return await Database.Table<LoyaltyFiltersDetail>().Where(x=>x.id == id).FirstOrDefaultAsync();
        } 
    }
}
