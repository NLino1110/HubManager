using DMSA.Models.Odoo.Native;
using SQLite;

namespace DMSA.Sync.Core.Database.Sqlite.Payments
{
    public class AccountJournalDb : SqliteDbBase<DMSA.Models.Odoo.Native.account_journal>
    {
        public AccountJournalDb(string _DatabaseFilename) : base(_DatabaseFilename)
        {

        }

        public async Task<List<account_journal>> GetItemsAsync()
        {
            await Init();
            return await Database.Table<account_journal>().ToListAsync();
            //return Database.Table<account_journal>().ToList();
        }
    }
}
