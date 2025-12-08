using DMSA.Models.Odoo.Native;

namespace DMSA.Sync.Core.Database.Sqlite
{
    public class CompanyDb : SqliteDbBase<res_company>
    {
        public CompanyDb(string _DatabaseFilename) : base(_DatabaseFilename)
        {

        }

        public async Task<List<res_company>> GetItemsAsync()
        {
            await Init();
            return await Database.Table<res_company>().ToListAsync();
            //return Database.Table<account_journal>().ToList();
        }

        public async Task<res_company> GetItem(int id)
        {
            await Init();
            return await Database.Table<res_company>().Where(i => i.id == id).FirstOrDefaultAsync();
        }
    }
}
