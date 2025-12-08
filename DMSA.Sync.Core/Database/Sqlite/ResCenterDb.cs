using DMSA.Models.Odoo.Native;

namespace DMSA.Sync.Core.Database.Sqlite
{
    public class ResCenterDb : SqliteDbBase<res_center>
    {
        public ResCenterDb(string _DatabaseFilename) : base(_DatabaseFilename)
        {

        }

        public async Task<List<res_center>> GetItemsAsync()
        {
            await Init();
            return await Database.Table<res_center>().ToListAsync();
        }

        public async Task<res_center> GetItem(int id)
        {
            await Init();
            return await Database.Table<res_center>().Where(x=>x.id == id).FirstOrDefaultAsync();
        }  
    }
}
