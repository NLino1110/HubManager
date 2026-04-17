
namespace DMSA.Sync.Core.Database.Sqlite
{
    public class ProductLineaDb : SqliteDbBase<DMSA.Models.Odoo.Native.product_linea>
    {
        public ProductLineaDb(string _DatabaseFilename) : base(_DatabaseFilename)
        {

        }

        public async Task<DMSA.Models.Odoo.Native.product_linea> GetItem(int id)
        {
            return await GetItemAsync(x => x.id == id);
        }

        public async Task<List<DMSA.Models.Odoo.Native.product_linea>> GetItemsAsync(int[] ids)
        {
            if (ids == null || ids.Length == 0)
                return new List<DMSA.Models.Odoo.Native.product_linea>();
            var ids_list = ids.ToList();
            return await GetItemsAsync(x => ids_list.Contains(x.id));
        }
    }
}
