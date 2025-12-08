
namespace DMSA.Sync.Core.Database.Sqlite
{
    public class ProductSubcategoriaDb : SqliteDbBase<DMSA.Models.Odoo.Native.product_subcategoria>
    {
        public ProductSubcategoriaDb(string _DatabaseFilename) : base(_DatabaseFilename)
        {

        }

        public async Task<DMSA.Models.Odoo.Native.product_subcategoria> GetItem(int id)
        {
            return await GetItemAsync(x => x.id == id);
        }

        public async Task<List<DMSA.Models.Odoo.Native.product_subcategoria>> GetItemsAsync(int[] ids)
        {
            if (ids == null || ids.Length == 0)
                return new List<DMSA.Models.Odoo.Native.product_subcategoria>();

            return await GetItemsAsync(x => ids.Contains(x.id));
        }
    }
}
