
using DMSA.Models.Odoo.Native;

namespace DMOrders.Services.Database.Sqlite
{
    public class StockQuantDb : SqliteDbBase<DMSA.Models.Odoo.Native.stock_quant>
    {
        public StockQuantDb(string _DatabaseFilename) : base(_DatabaseFilename)
        {
            Task.Run(async () =>
            {
                await InitializeAsync();
            });
        }

        public async Task InitializeAsync()
        {
            await Init();
            await Database.ExecuteAsync("CREATE INDEX IF NOT EXISTS idx_stock_quant_product_id ON stock_quant(_product_id)");
            await Database.ExecuteAsync("CREATE INDEX IF NOT EXISTS idx_stock_quant_warehouse_id ON stock_quant(_warehouse_id)");
            await Database.ExecuteAsync("CREATE INDEX IF NOT EXISTS idx_stock_quant_tracking ON stock_quant(tracking)");
            await Database.ExecuteAsync("CREATE INDEX IF NOT EXISTS idx_stock_quant_on_hand ON stock_quant(on_hand)");
            await Database.ExecuteAsync("CREATE INDEX IF NOT EXISTS idx_stock_quant_in_date ON stock_quant(in_date)");
        }

        public async Task<DMSA.Models.Odoo.Native.stock_quant> GetItem(int id)
        {
            return await GetItemAsync(x => x.id == id);
        }

        public async Task<List<DMSA.Models.Odoo.Native.stock_quant>> GetItemsAsync(int product_id)
        {
            return await GetItemsAsync(x => x._product_id == product_id);
        }
    }
}
