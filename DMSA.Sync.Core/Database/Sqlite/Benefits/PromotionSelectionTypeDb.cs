using DMSA.Models.Odoo.DMOrders.promotions;
using DMSA.Sync.Core.Database.Sqlite;

namespace DMSA.Sync.Core.Database.Sqlite.Benefits
{
    public class PromotionSelectionTypeDb : SqliteDbBase<PromotionSelectionType>
    {
        public PromotionSelectionTypeDb(string _DatabaseFilename) : base(_DatabaseFilename)
        {

        }

        public async Task<List<PromotionSelectionType>> GetItemsAsync()
        {
            await Init();
            return await Database.Table<PromotionSelectionType>().ToListAsync();
        }

        public async Task<PromotionSelectionType> GetItem(int id)
        {
            await Init();
            return await Database.Table<PromotionSelectionType>().Where(x=>x.Id== id).FirstOrDefaultAsync();
        }

    }
}
