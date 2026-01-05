using DMSA.Models.Odoo.Accounting;
using DMSA.Models.Odoo.DebitCollection;
using System.Globalization;

namespace DMSA.Sync.Core.Database.Sqlite.DebitCollection
{
    public class MultipleCobrosInvoiceLineAiDb : SqliteDbBase<MultipleCobrosInvoiceLineAi>
    {
        public MultipleCobrosInvoiceLineAiDb(string _DatabaseFilename) : base(_DatabaseFilename)
        {
            
        }

        public async Task<int> DeleteItemOfParent(MultipleCobrosInvoiceLine parent)
        {
            await Init();
            int count = 0;
            var resultItems = (await Database.Table<MultipleCobrosInvoiceLineAi>().ToListAsync()).Where(i => i.multiple_cobros_invoice_line_id == parent.Id);
            foreach (var item in resultItems)
            {
                count++;
                await Database.DeleteAsync(item);
            }

            return count;
        }
    }
}
