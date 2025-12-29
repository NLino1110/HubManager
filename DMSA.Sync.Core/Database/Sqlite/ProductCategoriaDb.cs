using DMSA.Models.Odoo.General.Responses;
using DMSA.Models.Odoo.Native;

namespace DMSA.Sync.Core.Database.Sqlite
{
    public class ProductCategoriaDb : SqliteDbBase<product_categoria>
    {
        public ProductCategoriaDb(string _DatabaseFilename) : base(_DatabaseFilename)
        {

        }

        public async Task<product_categoria> GetItem(int id)
        {
            return await GetItemAsync(x => x.id == id);
        }

        public async Task<List<product_categoria>> GetItemsAsync(int[] ids)
        {
            if (ids == null || ids.Length == 0)
                return new List<product_categoria>();

            return await GetItemsAsync(x => ids.Contains(x.id));
        }

        public async Task<List<product_categoria>> GetItemsAsync(string ByName)
        {
            string[] excludedIds = new string[] { "6", "14", "19", "20", "15", "29" };
            return await GetItemsAsync(x => !excludedIds.Contains(x.clave_externa) && x.name.ToLower().Contains(ByName.ToLower()));
        }
    }
}
