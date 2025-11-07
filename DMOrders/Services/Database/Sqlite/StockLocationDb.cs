using DMSA.Models.Odoo.Native;
using SQLite;

namespace DMOrders.Services.Database.Sqlite
{
    public class StockLocationDb : SqliteDbBase<stock_location>
    {
        public StockLocationDb(string _DatabaseFilename) : base(_DatabaseFilename)
        {

        }

        public async Task<List<stock_location>> GetItemsAsync(int id)
        {
            await Init();
            return await Database.Table<stock_location>()
                .Where(x => x.id == id)
                .ToListAsync();
        }

        public async Task<stock_location> GetItem(int id)
        {
            await Init();
            return await Database.Table<stock_location>().Where(x=>x.id == id).FirstOrDefaultAsync();
        }  
    }
}
