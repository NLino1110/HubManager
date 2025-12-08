using DMSA.Models.Odoo.DMCobranzas;
using DMSA.Models.Odoo.Native;
using SQLite;

namespace DMSA.Sync.Core.Database.Sqlite.Payments
{
    public class AccountMoveSendDb : SqliteDbBase<account_move_send>
    {
        public AccountMoveSendDb(string _DatabaseFilename) : base(_DatabaseFilename)
        {

        }

        public async Task<int> DeleteItemOfParent(AccountMoveSendHeader parent)
        {
            await Init();
            int count = 0;
            //await Init();

            var resultItems = (await Database.Table<account_move_send>().ToListAsync()).Where(i => i.parent_id == parent.id);

            foreach (var item in resultItems)
            {
                count++;
                await Database.DeleteAsync(item);
            }

            return count;
        }

        public async Task<List<account_move_send>> GetByParent(int parent_id)
        {
            await Init();
            return await Database.Table<account_move_send>().Where(i => i.parent_id == parent_id).ToListAsync();
        }

        public async Task<List<account_move_send>> GetItemsAsync()
        {
            await Init();
            return await Database.Table<account_move_send>().ToListAsync();
            //return Database.Table<account_journal>().ToList();
        }

        public async Task<List<account_move_send>> GetItemsAsync(int company_id, DateTime dateIni, DateTime dateEnd, int user_id)
        {
            await Init();
            return await Database.Table<account_move_send>().ToListAsync();
            //return Database.Table<account_journal>().ToList();
        }

        public async Task<List<account_move_send>> GetItemsByPartnerForPaymentAsync(res_partner res_Partner)
        {
            await Init();
            return await Database.Table<account_move_send>().Where(x=>
            x.partner_id == res_Partner.id).ToListAsync();
            //return Database.Table<account_journal>().ToList();
        }    
    }
}
