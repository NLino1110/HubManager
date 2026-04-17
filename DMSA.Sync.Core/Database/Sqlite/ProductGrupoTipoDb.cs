
namespace DMSA.Sync.Core.Database.Sqlite
{
    public class ProductGrupoTipoDb : SqliteDbBase<DMSA.Models.Odoo.Native.product_grupo_tipo>
    {
        public ProductGrupoTipoDb(string _DatabaseFilename) : base(_DatabaseFilename)
        {

        }

        public async Task<DMSA.Models.Odoo.Native.product_grupo_tipo> GetItem(int id)
        {
            return await GetItemAsync(x => x.id == id);
        }

        public async Task<List<DMSA.Models.Odoo.Native.product_grupo_tipo>> GetItemsAsync(int[] ids)
        {
            if (ids == null || ids.Length == 0)
                return new List<DMSA.Models.Odoo.Native.product_grupo_tipo>();

            var idsList = ids.ToList();
            return await GetItemsAsync(x => idsList.Contains(x.id));
        }
    }
}
