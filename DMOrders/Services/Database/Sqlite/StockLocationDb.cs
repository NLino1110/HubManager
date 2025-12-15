using DMSA.Models.Odoo.Native;
using SQLite;

namespace DMOrders.Services.Database.Sqlite
{
    public class StockLocationDb : SqliteDbBase<stock_location>
    {
        private bool _initialized = false;

        public StockLocationDb(string databaseFilename) : base(databaseFilename)
        {
        }

        public async Task<stock_location?> GetItem(int id)
        {
            await Init();
            return await Database.Table<stock_location>()
                .Where(x => x.id == id)
                .FirstOrDefaultAsync();
        }
    }
}

