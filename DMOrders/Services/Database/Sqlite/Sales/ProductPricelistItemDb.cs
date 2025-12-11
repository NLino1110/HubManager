
using DMSA.Models.Odoo.Sales;

namespace DMOrders.Services.Database.Sqlite
{
    public class ProductPricelistItemDb : SqliteDbBase<product_pricelist_item>
    {
        public ProductPricelistItemDb(string _DatabaseFilename) : base(_DatabaseFilename)
        {
            Task.Run(async () =>
            {
                await InitializeAsync();
            });
        }

        public async Task InitializeAsync()
        {
            await Init();
            await Database.ExecuteAsync("CREATE INDEX IF NOT EXISTS idx_product_pricelist_item__pricelist_id ON product_pricelist_item(_pricelist_id)");
            await Database.ExecuteAsync("CREATE INDEX IF NOT EXISTS idx_product_pricelist_item__product_tmpl_id ON product_pricelist_item(_product_tmpl_id)");            
        }

        public async Task<product_pricelist_item> GetItem(int id)
        {
            return await GetItemAsync(x => x.id == id);
        }

        public async Task<List<product_pricelist_item>> GetItemsAsync(int[] ids)
        {
            if (ids == null || ids.Length == 0)
                return new List<product_pricelist_item>();

            return await GetItemsAsync(x => ids.Contains(x.id));
        }
    }
}
