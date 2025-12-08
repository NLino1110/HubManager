using DMSA.Models.Odoo.DMCobranzas;
using DMSA.Models.Odoo.Native;
using SQLite;

namespace DMSA.Sync.Core.Database.Sqlite.Payments
{
    public class BankDb : SqliteDbBase<Bank_Id>
    {
        public BankDb(string _DatabaseFilename) : base(_DatabaseFilename)
        {

        }

        public async Task<List<Bank_Id>> GetItemsAsync()
        {
            await Init();
            return await Database.Table<Bank_Id>().ToListAsync();
        }
    }
}
