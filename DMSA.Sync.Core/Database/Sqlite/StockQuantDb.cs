namespace DMSA.Sync.Core.Database.Sqlite
{
    public class StockQuantDb : SqliteDbBase<DMSA.Models.Odoo.Native.stock_quant>
    {
        public StockQuantDb(string _DatabaseFilename) : base(_DatabaseFilename)
        {

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
