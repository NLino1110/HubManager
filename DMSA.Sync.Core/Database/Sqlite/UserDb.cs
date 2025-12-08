using DMSA.Models.Odoo.Native;

namespace DMSA.Sync.Core.Database.Sqlite
{
    public class UserDb : SqliteDbBase<res_user>
    {
        public UserDb(string _DatabaseFilename) : base(_DatabaseFilename)
        {

        }

        public async Task<res_user> GetItemsAsync(int company_id, int user_id)
        {
            await Init();
            return await Database.Table<res_user>().Where(x => x._company_id == company_id &&
            x.id == user_id).FirstOrDefaultAsync();
        }

        public async Task<List<res_user>> GetItemsAsync()
        {
            await Init();
            return await Database.Table<res_user>().ToListAsync();
        }

        public async Task<res_user> GetItem(int id)
        {
            await Init();
            return await Database.Table<res_user>().Where(x => x.id == id).FirstOrDefaultAsync();
        }
    }
}
