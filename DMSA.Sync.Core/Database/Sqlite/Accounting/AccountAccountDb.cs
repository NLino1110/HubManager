using DMSA.Models.Odoo.Accounting;
using DMSA.Models.Odoo.Modules.Accounting;
using DMSA.Sync.Core.Database.Sqlite;

namespace DMSA.Sync.Core
{
    public class AccountAccountDb : SqliteDbBase<AccountAccount>
    {
        public AccountAccountDb(string _DatabaseFilename) : base(_DatabaseFilename)
        {

        }

        public async Task<AccountAccount> GetItem(int id)
        {
            await Init();
            return await Database.Table<AccountAccount>().Where(x=>x.id == id).FirstOrDefaultAsync();
        }  
    }
}
