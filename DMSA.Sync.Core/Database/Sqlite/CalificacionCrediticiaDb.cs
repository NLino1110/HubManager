
namespace DMSA.Sync.Core.Database.Sqlite
{
    public class CalificacionCrediticiaDb : SqliteDbBase<DMSA.Models.Odoo.Native.calificacion_crediticia>
    {
        public CalificacionCrediticiaDb(string _DatabaseFilename) : base(_DatabaseFilename)
        {

        }

        public async Task<DMSA.Models.Odoo.Native.calificacion_crediticia> GetItem(int id)
        {
            return await GetItemAsync(x => x.id == id);
        }

        public async Task<List<DMSA.Models.Odoo.Native.calificacion_crediticia>> GetItemsAsync(int[] ids)
        {
            if (ids == null || ids.Length == 0)
                return new List<DMSA.Models.Odoo.Native.calificacion_crediticia>();

            return await GetItemsAsync(x => ids.Contains(x.id));
        }
    }
}
