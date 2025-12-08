using DMSA.Models.Odoo.DMCobranzas;
using SQLite;

namespace DMSA.Sync.Core.Database.Sqlite.Payments
{
    public class AccountPaymentInvoiceLineDb : SqliteDbBase<AccountPaymentInvoiceLine>
    {
        public AccountPaymentInvoiceLineDb(string _DatabaseFilename) : base(_DatabaseFilename)
        {

        }

        public async Task<List<AccountPaymentInvoiceLine>> GetItemsAsync(AccountPayment parent)
        {
            await Init();
            //return Database.GetAllWithChildren<AccountPaymentInvoiceLine>();
            return (await Database.Table<AccountPaymentInvoiceLine>().ToListAsync()).Where(pl=>pl.parent_payment_id == parent.id).ToList();
        }

        public async Task<int> DeleteItemOfParent(AccountPayment parent)
        {
            await Init();
            int count = 0;
            //await Init();
            var resultItems = (await Database.Table<AccountPaymentInvoiceLine>().ToListAsync()).Where(i => i.parent_payment_id == parent.id);
            foreach (var item in resultItems)
            {
                count++;
                await Database.DeleteAsync(item);
            }

            return count;
        }
    }
}
