using DMSA.Models.Odoo.Accounting;
using DMSA.Models.Odoo.Native;
using SQLite;

namespace DMSA.Sync.Core.Database.Sqlite.Payments
{
    public class CreditNoteRequestDb : SqliteDbBase<credit_note_request>
    {
        public CreditNoteRequestDb(string _DatabaseFilename) : base(_DatabaseFilename)
        {

        }

        public async Task<int> DeleteItemOfParent(CreditNoteRequestGroup parent)
        {
            await Init();
            int count = 0;
            //await Init();

            var resultItems = (await Database.Table<credit_note_request>().ToListAsync()).Where(i => i.parent_id == parent.id);

            foreach (var item in resultItems)
            {
                count++;
                await Database.DeleteAsync(item);
            }

            return count;
        }

        public async Task<List<credit_note_request>> GetByParent(int parent_id)
        {
            await Init();
            return await Database.Table<credit_note_request>().Where(i => i.parent_id == parent_id).ToListAsync();
        }

        public async Task<List<credit_note_request>> GetItemsAsync()
        {
            await Init();
            return await Database.Table<credit_note_request>().ToListAsync();
            //return Database.Table<account_journal>().ToList();
        }

        public async Task<List<credit_note_request>> GetItemsAsync(int company_id, DateTime dateIni, DateTime dateEnd, int user_id)
        {
            await Init();
            return await Database.Table<credit_note_request>().ToListAsync();
            //return Database.Table<account_journal>().ToList();
        }

        public async Task<List<credit_note_request>> GetItemsByPartnerForPaymentAsync(res_partner res_Partner)
        {
            await Init();
            return await Database.Table<credit_note_request>().Where(x=>
            x.partner_id == res_Partner.id).ToListAsync();
            //return Database.Table<account_journal>().ToList();
        }    
    }
}
