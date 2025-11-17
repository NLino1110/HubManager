
namespace DMOrders.Services.Database.Sqlite
{
    public class ProductCategoriaDb : SqliteDbBase<DMSA.Models.Odoo.Native.product_categoria>
    {
        public ProductCategoriaDb(string _DatabaseFilename) : base(_DatabaseFilename)
        {

        }

        public async Task<DMSA.Models.Odoo.Native.product_categoria> GetItem(int id)
        {
            return await GetItemAsync(x => x.id == id);
        }

        public async Task<List<DMSA.Models.Odoo.Native.product_categoria>> GetItemsAsync(int[] ids)
        {
            if (ids == null || ids.Length == 0)
                return new List<DMSA.Models.Odoo.Native.product_categoria>();

            return await GetItemsAsync(x => ids.Contains(x.id));
        }
    }
}
