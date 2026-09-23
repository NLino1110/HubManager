using DMSA.Models.Odoo.Security;

namespace DMSA.Sync.Core.Database.Sqlite
{
    public class UserAccessDb : SqliteDbBase<user_access>
    {
        public UserAccessDb(string _DatabaseFilename) : base(_DatabaseFilename)
        {

        }

        protected override async Task OnAfterInit()
        {
            try
            {
                await Database.ExecuteAsync(
                    "ALTER TABLE user_access ADD COLUMN is_mobile_app_admin INTEGER NOT NULL DEFAULT 0");
            }
            catch
            {
                // Columna ya existe en tablets con BD previa.
            }
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

        public async Task<user_access> FixMissingCurrentUser()
        {
            await Init();            
            var userFound = await GetItemAsync(u => u.uid == Constants.Session.CurrentUserFront.uid);

            if (userFound == null)
            {
                userFound = new user_access()
                {
                    uid = Constants.Session.CurrentUserFront.uid,
                    name = Constants.Session.CurrentUserFront.nombres,
                    username = Constants.Session.CurrentUserFront.username,
                    pwd = Constants.Session.CurrentUserFront.password
                };

                await InsertAsync(userFound);
            }

            return userFound;
        }
    }
}
