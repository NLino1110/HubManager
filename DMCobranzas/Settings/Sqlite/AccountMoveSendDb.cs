using DMCobranzas.Models;
using DMSA.Models.Odoo.DMCobranzas;
using DMSA.Models.Odoo.Native;
using Microsoft.Data.Sqlite;
using SQLite;

namespace DMCobranzas.Settings.Sqlite
{
    public class AccountMoveSendDb
    {
        SQLiteAsyncConnection Database;
        
        public AccountMoveSendDb()
        {

        }

        public async Task<int> GetCount()
        {
            return await Database.Table<account_move_send>().CountAsync();
        }

        public async Task<int> Truncate()
        {
            await Init();
            //Database.Table<account_move>().Delete();
            //Database.DeleteAll<account_move>();
            return await Database.DeleteAllAsync<account_move_send>();
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

        public async Task<account_move_send> GetItem(int id)
        {
            await Init();
            return await Database.Table<account_move_send>().Where(i => i.id == id).FirstOrDefaultAsync();
        }

        public async Task<int> InsertAsync(account_move_send item)
        {
            await Init();

            await Database.InsertAsync(item);

            return 0;
        }

        public async Task<int> InsertBatchAsync(account_move_send[] items)
        {
            await Init();

            await Database.InsertAllAsync(items, "OR REPLACE");
            
            return 0;
        }

        public async Task<int> UpdateAsync(account_move_send item)
        {
            await Init();
            return await Database.UpdateAsync(item);
        }

        async Task Init()
        {
            if (Database is not null)
                return;

            Database = new SQLiteAsyncConnection(Constants.DatabasePath, Constants.Flags);
            var result = await Database.CreateTableAsync<account_move_send>();
        }
    
    }
}
