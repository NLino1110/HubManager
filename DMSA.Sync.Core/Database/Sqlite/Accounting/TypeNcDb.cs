using DMSA.Models.Odoo.Accounting;
using DMSA.Sync.Core.Database.Sqlite;

namespace DMSA.Sync.Core.Database.Sqlite.Accounting
{
    public class TypeNcDb : SqliteDbBase<TypeNc>
    {
        public TypeNcDb(string _DatabaseFilename) : base(_DatabaseFilename)
        {

        }

        public async Task<TypeNc> GetItem(int id)
        {
            await Init();
            return await Database.Table<TypeNc>().Where(x=>x.id == id).FirstOrDefaultAsync();
        }

        public async Task<int[]> GetAllAccountIds()
        {
            await Init();
            var query = await Database.Table<TypeNc>().ToListAsync();
            return query.Select(x => x._account_id).ToArray();
        }
    }
}
