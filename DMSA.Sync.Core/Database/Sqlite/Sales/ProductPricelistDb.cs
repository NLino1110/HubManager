
using DMSA.Models.Odoo.Sales;

namespace DMSA.Sync.Core.Database.Sqlite.Sales
{
    public class ProductPricelistDb : SqliteDbBase<product_pricelist>
    {
        public ProductPricelistDb(string _DatabaseFilename) : base(_DatabaseFilename)
        {

        }

        public async Task<product_pricelist> GetItem(int id)
        {
            return await GetItemAsync(x => x.id == id);
        }

        public async Task<List<product_pricelist>> GetItemsAsync(int[] ids)
        {
            if (ids == null || ids.Length == 0)
                return new List<product_pricelist>();
            var ids_list = ids.ToList();
            return await GetItemsAsync(x => ids_list.Contains(x.id));
        }

        public async Task<List<product_pricelist>> GetItemsByStatus(bool Active)
        {
            return await GetItemsAsync(x => x.active == Active);
        }
    }
}
