
using DMSA.Models.Odoo.Accounting;

namespace DMSA.Sync.Core.Database.Sqlite
{
    public class CalificacionCrediticiaDb : SqliteDbBase<calificacion_crediticia>
    {
        public CalificacionCrediticiaDb(string _DatabaseFilename) : base(_DatabaseFilename)
        {

        }

        public async Task<calificacion_crediticia> GetItem(int id)
        {
            return await GetItemAsync(x => x.id == id);
        }

        public async Task<List<calificacion_crediticia>> GetItemsAsync(int[] ids)
        {
            if (ids == null || ids.Length == 0)
                return new List<calificacion_crediticia>();

            return await GetItemsAsync(x => ids.Contains(x.id));
        }
    }
}
