using DMSA.Models.Odoo.DMCobranzas;
using SQLite;

namespace DMSA.Sync.Core.Database.Sqlite.Payments
{
    public class AccountPaymentDb : SqliteDbBase<AccountPayment>
    {   
        public AccountPaymentDb(string _DatabaseFilename) : base(_DatabaseFilename)
        {

        }

        public async Task<List<AccountPayment>> GetByParent(int parent_id)
        {
            await Init();
            return await Database.Table<AccountPayment>().Where(i => i.parent_id == parent_id).ToListAsync();
        }

        public async Task<int> DeleteItemOfParent(AccountPaymentHeader parent)
        {
            await Init();
            int count = 0;
            //await Init();
            
            var resultItems = (await Database.Table<AccountPayment>().ToListAsync()).Where(i => i.parent_id == parent.id);

            foreach (var item in resultItems)
            {
                count++;
                await Database.DeleteAsync(item);
            }

            return count;
        }
    }
}
