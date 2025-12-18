using DMSA.Models.Odoo.Accounting;
using DMSA.Models.Odoo.Modules.Accounting;
using DMSA.Sync.Core.Database.Sqlite;

namespace DMSA.Sync.Core
{
    public class TypeParentNcDb : SqliteDbBase<TypeParentNc>
    {
        public TypeParentNcDb(string _DatabaseFilename) : base(_DatabaseFilename)
        {

        }

        public async Task<TypeParentNc> GetItem(int id)
        {
            await Init();
            return await Database.Table<TypeParentNc>().Where(x=>x.id == id).FirstOrDefaultAsync();
        }  
    }
}
