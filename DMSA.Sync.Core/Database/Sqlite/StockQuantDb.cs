using DMSA.Models.Odoo.Inventory;

namespace DMSA.Sync.Core.Database.Sqlite
{
    public class StockQuantDb : SqliteDbBase<stock_quant>
    {
        public StockQuantDb(string _DatabaseFilename) : base(_DatabaseFilename)
        {
            
        }

        protected override async Task OnAfterInit()
        {
            await Database.RunInTransactionAsync(tran =>
            {
                tran.Execute("CREATE INDEX IF NOT EXISTS idx_stock_quant_product_id ON stock_quant(_product_id)");
                tran.Execute("CREATE INDEX IF NOT EXISTS idx_stock_quant_warehouse_id ON stock_quant(_warehouse_id)");
                tran.Execute("CREATE INDEX IF NOT EXISTS idx_stock_quant_tracking ON stock_quant(tracking)");
                tran.Execute("CREATE INDEX IF NOT EXISTS idx_stock_quant_on_hand ON stock_quant(on_hand)");
                tran.Execute("CREATE INDEX IF NOT EXISTS idx_stock_quant_in_date ON stock_quant(in_date)");
            });
        }

        public async Task<stock_quant> GetItem(int id)
        {
            return await GetItemAsync(x => x.id == id);
        }

        public async Task<List<stock_quant>> GetItemsAsync(int product_id)
        {
            return await GetItemsAsync(x => x._product_id == product_id);
        }
    }
}
