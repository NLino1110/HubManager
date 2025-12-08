using DMSA.Models.Odoo.Native;

namespace DMSA.Sync.Core.Database.Sqlite
{
    public class StockWareHouseDb : SqliteDbBase<stock_warehouse>
    {
        public StockWareHouseDb(string _DatabaseFilename) : base(_DatabaseFilename)
        {
        }

        public async Task<stock_warehouse> GetItem(int id)
        {
            await Init();
            return await Database.Table<stock_warehouse>().Where(x => x.id == id).FirstOrDefaultAsync();
        }

        public async Task<List<stock_warehouse>> GetItemsAsync(int company_id)
        {
            await Init();
            return await Database.Table<stock_warehouse>()
                .Where(x => x._company_id == company_id)
                .ToListAsync();
        }

        internal async Task<List<stock_warehouse>> GetByResCenter(int id)
        {
            await Init();
            return await Database.Table<stock_warehouse>()
                .Where(x => x._center_id == id)
                .ToListAsync();
        }
    }
}
