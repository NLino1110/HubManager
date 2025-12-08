using DMSA.Models.Odoo.Native;

namespace DMSA.Sync.Core.Database.Sqlite
{
    public class UomUomDb : SqliteDbBase<uom_uom>
    {
        public UomUomDb(string _DatabaseFilename) : base(_DatabaseFilename)
        {

        }

        public async Task<List<uom_uom>> GetItemsAsync()
        {
            await Init();
            return await Database.Table<uom_uom>().ToListAsync();
        }

        public async Task<uom_uom> GetItem(int id)
        {
            await Init();
            return await Database.Table<uom_uom>().Where(x=>x.id == id).FirstOrDefaultAsync();
        } 
    }
}
