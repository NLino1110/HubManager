using DMSA.Models.Odoo.Modules.Accounting;
using DMSA.Sync.Core.Database.Sqlite;

namespace DMSA.Sync.Core
{
    public class AccountTaxDb : SqliteDbBase<AccountTax>
    {
        public AccountTaxDb(string _DatabaseFilename) : base(_DatabaseFilename)
        {

        }

        public async Task<AccountTax> GetItem(int id)
        {
            await Init();
            return await Database.Table<AccountTax>().Where(x=>x.id == id).FirstOrDefaultAsync();
        }  
    }
}
