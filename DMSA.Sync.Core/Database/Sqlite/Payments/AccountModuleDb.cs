using DMSA.Models.Odoo.DMCobranzas;
using SQLite;

namespace DMSA.Sync.Core.Database.Sqlite.Payments
{
    public class AccountModuleDb : SqliteDbBase<AccountModule>
    {
        
        public AccountModuleDb(string _DatabaseFilename) : base(_DatabaseFilename)
        {

        }
               

        //public async Task<List<AccountModule>> GetItemsAsync(int company_id, int partner_id, int limit)
        //{
        //    await Init();
        //    return await Database.Table<AccountModule>().Where(i => i._company_id == company_id &&
        //    i._partner_id == partner_id).Take(limit).ToListAsync();
        //}

        public async Task<List<AccountModule>> GetItemsAsync()
        {
            await Init();
            return await Database.Table<AccountModule>().ToListAsync();
            //return Database.Table<account_journal>().ToList();
        }
    }
}
