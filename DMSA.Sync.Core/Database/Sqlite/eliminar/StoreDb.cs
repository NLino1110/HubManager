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
    public class StoreDb
    {
        SQLiteAsyncConnection Database;

        public StoreDb()
        {

        }

        public async Task<int>  GetCount()
        {
            await Init();
            return (await Database.Table<res_store>().ToListAsync()).Count;
        }

        public async Task<List<res_store>> GetItemsAsync()
        {
            await Init();
            return await Database.Table<res_store>().ToListAsync();
        }

        public async Task<res_store> GetItem(int id)
        {
            await Init();
            return await Database.Table<res_store>().Where(x=>x.id == id).FirstOrDefaultAsync();
        }

        public async Task<int> InsertAsync(res_store item)
        {
            await Init();
            await Database.InsertAsync(item);
            return 0;
        }

        public async Task<int> InsertBatchAsync(res_store[] items)
        {
            await Init();
            await Database.InsertAllAsync(items, "OR REPLACE",true);            
            return 0;
        }

        public async Task<int> Truncate()
        {
            await Init();

            return await Database.DeleteAllAsync<res_store>();
        }

        async Task Init()
        {
            if (Database is not null)
                return;

            Database = new SQLiteAsyncConnection(Constants.DatabasePath, Constants.Flags);
            var result = await Database.CreateTableAsync<res_store>();
        }    
    }
}
