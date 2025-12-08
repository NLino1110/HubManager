using DMSA.Models.Odoo.DMOrders.promotions;

namespace DMSA.Sync.Core.Database.Sqlite.Benefits
{
    public class PromoCentersDb : SqliteDbBase<PromoCenters>
    {   

        public PromoCentersDb(string _DatabaseFilename) : base(_DatabaseFilename)
        {

        }

        public async Task<List<PromoCenters>> GetItemsAsync()
        {
            await Init();
            return await Database.Table<PromoCenters>().ToListAsync();
        }

        public async Task<PromoCenters> GetItem(int id)
        {
            await Init();
            return await Database.Table<PromoCenters>().Where(x=>x.id == id).FirstOrDefaultAsync();
        }

        public async Task<List<PromoCenters>> GetItemsByParent(int parentId)
        {
            await Init();
            return await Database.Table<PromoCenters>().Where(x => x._promo_id == parentId).ToListAsync();
        }
    }
}
