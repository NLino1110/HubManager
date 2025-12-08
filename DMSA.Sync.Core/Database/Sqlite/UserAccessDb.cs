using DMSA.Models.Odoo.DMApps;

namespace DMSA.Sync.Core.Database.Sqlite
{
    public class UserAccessDb : SqliteDbBase<user_access>
    {
        public UserAccessDb(string _DatabaseFilename) : base(_DatabaseFilename)
        {

        }

        public async Task<List<user_access>> GetItemsAsync()
        {
            await Init();
            return await Database.Table<user_access>().ToListAsync();
        }

        public async Task<user_access> GetItemAsync(int id)
        {
            await Init();
            return await Database.Table<user_access>().Where(i => i.uid == id).FirstOrDefaultAsync();
        }
    
    }
}
