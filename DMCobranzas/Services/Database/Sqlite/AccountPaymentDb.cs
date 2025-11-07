using DMCobranzas.Models;
using DMCobranzas.Settings;
using DMSA.Models.Odoo.DMCobranzas;
using DMSA.Models.Odoo.Native;
using Microsoft.Data.Sqlite;
using SQLite;
using SQLiteNetExtensions.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMCobranzas.Services.Database.Sqlite
{
    public class AccountPaymentDb
    {   
        SQLiteAsyncConnection Database;

        public AccountPaymentDb()
        {

        }

        public async Task<int> GetCount()
        {
            return (await Database.Table<AccountPayment>().ToListAsync()).Count;
        }

        public async Task<List<AccountPayment>> GetItemsAsync()
        {
            await Init();
            return await Database.Table<AccountPayment>().ToListAsync();
            //return Database.Table<account_journal>().ToList();
        }

        public async Task<List<AccountPayment>> GetByParent(int parent_id)
        {
            await Init();
            return await Database.Table<AccountPayment>().Where(i => i.parent_id == parent_id).ToListAsync();
        }

        public async Task<int> InsertAsync(AccountPayment item)
        {
            await Init();
            await Database.InsertAsync(item);
            return 0;
        }

        public async Task<int> InsertBatchAsync(AccountPayment[] items)
        {
            await Init();
            await Database.InsertAllAsync(items, "OR REPLACE");
            return 0;
        }

        public async Task<int> UpdateAsync(AccountPayment item)
        {
            await Init();
            return await Database.UpdateAsync(item);
        }

        public async Task<int> DeleteItemAsync(AccountPayment item)
        {
            //await Init();
            return await Database.DeleteAsync(item);
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

        async Task Init()
        {
            if (Database is not null)
                return;

            Database = new SQLiteAsyncConnection(Constants.DatabasePath);
            var result = await Database.CreateTableAsync<AccountPayment>();
        }
    }
}
