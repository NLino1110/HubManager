using DMSA.Models.Odoo.DMOrders.promotions;

namespace DMSA.Sync.Core.Database.Sqlite.Benefits
{
    public class LoyaltyFiltersDb : SqliteDbBase<LoyaltyFilters>
    {

        public LoyaltyFiltersDb(string _DatabaseFilename) : base(_DatabaseFilename)
        {

        }

        public async Task<List<LoyaltyFilters>> GetItemsAsync()
        {
            await Init();
            return await Database.Table<LoyaltyFilters>().ToListAsync();
        }

        public async Task<LoyaltyFilters> GetItem(int id)
        {
            await Init();
            return await Database.Table<LoyaltyFilters>().Where(x=>x.id == id).FirstOrDefaultAsync();
        }   
    }
}
