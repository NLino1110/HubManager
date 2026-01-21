using DMSA.Models.Odoo.Accounting;
using DMSA.Models.Odoo.DMCobranzas;
using DMSA.Models.Odoo.Native;
using SQLite;

namespace DMSA.Sync.Core.Database.Sqlite.Payments
{
    public class BankDb : SqliteDbBase<ResBank>
    {
        public BankDb(string _DatabaseFilename) : base(_DatabaseFilename)
        {

        }

        public async Task<List<ResBank>> GetItemsAsync()
        {
            await Init();
            return await Database.Table<ResBank>().ToListAsync();
        }
    }
}
