using DMSA.Models.Odoo.Accounting;
using SQLite;

namespace DMSA.Sync.Core.Database.Sqlite.Payments
{
    [Obsolete]
    public class InboundPaymentMethodDb : SqliteDbBase<inbound_payment_method>
    {

        public InboundPaymentMethodDb(string _DatabaseFilename) : base(_DatabaseFilename)
        {

        }

        public async Task<List<inbound_payment_method>> GetItemsByParentAsync(int parent_id)
        {
            await Init();
            return await Database.Table<inbound_payment_method>().Where(x=>x.parent_id == parent_id).ToListAsync();
        }    
    }
}
