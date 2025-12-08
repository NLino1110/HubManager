using DMSA.Models.Odoo.DMCobranzas;
using SQLite;

namespace DMSA.Sync.Core.Database.Sqlite.Payments
{
    public class AccountTypeModuleDb : SqliteDbBase<AccountTypeModule>
    {        
        public AccountTypeModuleDb(string _DatabaseFilename) : base(_DatabaseFilename)
        {

        }

        public async Task<List<AccountTypeModule>> GetItemsAsync()
        {
            await Init();
            return await Database.Table<AccountTypeModule>().ToListAsync();
            //return Database.Table<account_journal>().ToList();
        }

        public async Task<AccountTypeModule> GetByNameItem(string name_doc)
        {
            await Init();
            return await Database.Table<AccountTypeModule>().Where(i => i.name == name_doc).FirstOrDefaultAsync();
        }    
    }
}
