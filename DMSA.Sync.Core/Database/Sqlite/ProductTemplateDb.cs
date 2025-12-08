using DMSA.Models.Odoo.Native;

namespace DMSA.Sync.Core.Database.Sqlite
{
    public class ProductTemplateDb : SqliteDbBase<product_template>
    {
        public ProductTemplateDb(string _DatabaseFilename) : base(_DatabaseFilename)
        {

        }

        public async Task<List<product_template>> GetItemsAsync()
        {
            await Init();
            return await Database.Table<product_template>().Take(50).ToListAsync();
        }

        public async Task<product_template> GetItem(int id)
        {
            await Init();
            return await Database.Table<product_template>().Where(x=>x.id == id).FirstOrDefaultAsync();
        }   
    }
}
