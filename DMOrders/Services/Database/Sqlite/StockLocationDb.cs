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

        /// <summary>
        /// Warmup para inicializar SQLite antes que el usuario lo use.
        /// Esto elimina el retraso en la primera consulta.
        /// </summary>
        public async Task Warmup()
        {
            if (_initialized)
                return;

            await Init();

            // Ejecuta una consulta mínima para "despertar" SQLite
            try
            {
                await Database.Table<stock_location>().FirstOrDefaultAsync();
            }
            catch { }

            _initialized = true;
        }

        public async Task<List<stock_location>> GetItemsAsync(int id)
        {
            await Init();
            return await Database.Table<stock_location>()
                .Where(x => x.id == id)
                .ToListAsync();
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

