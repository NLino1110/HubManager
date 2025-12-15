using DMSA.Models.Odoo.DMOrders.promotions;

namespace DMSA.Sync.Core.Database.Sqlite.Benefits
{
    public class PosTarjetasCanalDb : SqliteDbBase<PosTarjetasCanal>
    {

        public PosTarjetasCanalDb(string _DatabaseFilename) : base(_DatabaseFilename)
        {

        }


        public async Task<List<PosTarjetasCanal>> GetItemsAsync()
        {
            await Init();
            return await Database.Table<PosTarjetasCanal>().ToListAsync();
        }

        public async Task<PosTarjetasCanal> GetItem(int id)
        {
            await Init();
            return await Database.Table<PosTarjetasCanal>().Where(x=>x.id == id).FirstOrDefaultAsync();
        }

    }
}
