using DMSA.Models.Odoo.Native;
using Microsoft.Data.Sqlite;
using SQLite;
using SQLiteNetExtensions.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMOrders.Services.Database.Sqlite
{
    public class AccountJournalDb
    {
        SQLiteAsyncConnection Database;

        public AccountJournalDb()
        {

        }

        public async Task<int> GetCount()
        {
            return (await Database.Table<account_journal>().ToListAsync()).Count;
        }

        public async Task<List<account_journal>> GetItemsAsync()
        {
            await Init();
            return await Database.Table<account_journal>().ToListAsync();
            //return Database.Table<account_journal>().ToList();
        }

        public async Task<account_journal> GetItem(int id)
        {
            await Init();
            return await Database.Table<account_journal>().Where(i => i.id == id).FirstOrDefaultAsync();
        }

        public async Task<int> InsertAsync(account_journal item)
        {
            await Init();

            await Database.InsertOrReplaceAsync(item);

            return 0;
        }

        public async Task<int> InsertBatchAsync(account_journal[] items)
        {
            await Init();
            await Database.InsertAllAsync(items, "OR REPLACE");
            return 0;
        }

        async Task Init()
        {
            if (Database is not null)
                return;

            Database = new SQLiteAsyncConnection(Constants.DatabasePath);
            var result = await Database.CreateTableAsync<account_journal>();
        }

        public async Task<int> Truncate()
        {
            await Init();            
            return await Database.DeleteAllAsync<account_journal>();
        }
    }
}
