using DMSA.Models.Odoo.DMOrders.tareas;

namespace DMSA.Sync.Core.Database.Sqlite.tareas
{
    public class MotivoActividadDiariaDb : SqliteDbBase<MotivoActividadDiaria>
    {   
        public MotivoActividadDiariaDb(string _DatabaseFilename) : base(_DatabaseFilename)
        {

        }

        public async Task<List<MotivoActividadDiaria>> GetItemsAsync()
        {
            await Init();
            return await Database.Table<MotivoActividadDiaria>().ToListAsync();
        }

        public async Task<MotivoActividadDiaria> GetItem(int id)
        {
            await Init();
            return await Database.Table<MotivoActividadDiaria>().Where(x=>x.id == id).FirstOrDefaultAsync();
        } 
    }
}
