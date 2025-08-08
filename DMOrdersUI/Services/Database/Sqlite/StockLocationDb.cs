using DMSA.Models.Odoo.Native;
using Microsoft.Data.Sqlite;
using SQLite;
using SQLiteNetExtensions.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMOrdersUI.Services.Database.Sqlite
{
    public class StockLocationDb
    {
        SQLiteAsyncConnection Database;

        public StockLocationDb()
        {

        }

        public async Task<int>  GetCount()
        {
            await Init();
            return (await Database.Table<stock_location>().ToListAsync()).Count;
        }

        [Obsolete]
        public async Task<List<stock_location>> GetItemsAsync()
        {
            await Init();
            return await Database.Table<stock_location>()
                .Take(100)
                .ToListAsync();
        }

        public async Task<List<stock_location>> GetItemsAsync(int id)
        {
            await Init();
            return await Database.Table<stock_location>()
                .Where(x => x.id == id)
                .ToListAsync();
        }

        public async Task<stock_location> GetItem(int id)
        {
            await Init();
            return await Database.Table<stock_location>().Where(x=>x.id == id).FirstOrDefaultAsync();
        }

        public async Task<int> InsertAsync(stock_location item)
        {
            await Init();
            await Database.InsertAsync(item);
            return 0;
        }

        public async Task<int> InsertBatchAsync(stock_location[] items)
        {
            await Init();
            await Database.InsertAllAsync(items, "OR REPLACE", true);            
            return 0;
        }

        public async Task<int> Truncate()
        {
            await Init();
            return await Database.DeleteAllAsync<stock_location>();
        }

        async Task Init()
        {
            if (Database is not null)
                return;

            Database = new SQLiteAsyncConnection(Constants.DatabasePath, Constants.Flags);
            var result = await Database.CreateTableAsync<stock_location>();
        }    
    }
}
