using DMSA.Models.Odoo.DMOrders.promotions;

namespace DMSA.Sync.Core.Database.Sqlite.Benefits
{
    public class PosPaymentMethodDb : SqliteDbBase<PosPaymentMethod>
    {        
        public PosPaymentMethodDb(string _DatabaseFilename) : base(_DatabaseFilename)
        {

        }

        public async Task<List<PosPaymentMethod>> GetItemsAsync()
        {
            await Init();
            return await Database.Table<PosPaymentMethod>().ToListAsync();
        }

        public async Task<PosPaymentMethod> GetItem(int id)
        {
            await Init();
            return await Database.Table<PosPaymentMethod>().Where(x=>x.Id == id).FirstOrDefaultAsync();
        } 
    }
}
