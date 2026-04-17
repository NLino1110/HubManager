using DMSA.Models.Odoo.Accounting;

namespace DMSA.Sync.Core.Database.Sqlite.Accounting
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
