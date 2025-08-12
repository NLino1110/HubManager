
namespace DMOrders.Services.Database.Sqlite
{
    public class StockQuantDb : SqliteDbBase<DMSA.Models.Odoo.Native.stock_quant>
    {
        public async Task<DMSA.Models.Odoo.Native.stock_quant> GetItem(int id)
        {
            return await GetItemAsync(x => x.id == id);
        }

        public async Task<List<DMSA.Models.Odoo.Native.stock_quant>> GetItemsAsync(int id)
        {
            return await GetItemsAsync(x => x.id == id);
        }
    }
}
