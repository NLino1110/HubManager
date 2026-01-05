using DMSA.Models.Odoo.Accounting;
using DMSA.Models.Odoo.DebitCollection;
using System.Globalization;

namespace DMSA.Sync.Core.Database.Sqlite.DebitCollection
{
    public class MultipleCobrosInvoiceLineDb : SqliteDbBase<MultipleCobrosInvoiceLine>
    {
        public MultipleCobrosInvoiceLineDb(string _DatabaseFilename) : base(_DatabaseFilename)
        {
            
        }

        public async Task<int> DeleteItemOfParent(MultipleCobrosInvoice parent)
        {
            await Init();
            int count = 0;            
            var resultItems = (await Database.Table<MultipleCobrosInvoiceLine>().ToListAsync()).Where(i => i.MultipleCobrosInvoiceId == parent.id);
            foreach (var item in resultItems)
            {
                count++;
                await Database.DeleteAsync(item);
            }

            return count;
        }
    }
}
