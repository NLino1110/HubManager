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
    public class ProductTemplateDb
    {
        SQLiteAsyncConnection Database;

        public ProductTemplateDb()
        {

        }

        public async Task<int>  GetCount()
        {
            await Init();
            return (await Database.Table<product_template>().ToListAsync()).Count;
        }

        public async Task<List<product_template>> GetItemsAsync()
        {
            await Init();
            return await Database.Table<product_template>().Take(50).ToListAsync();
        }

        public async Task<product_template> GetItem(int id)
        {
            await Init();
            return await Database.Table<product_template>().Where(x=>x.id == id).FirstOrDefaultAsync();
        }

        public async Task<int> InsertAsync(product_template item)
        {
            await Init();
            await Database.InsertAsync(item);
            return 0;
        }

        public async Task<int> InsertBatchAsync(product_template[] items)
        {
            await Init();
            await Database.InsertAllAsync(items, "OR REPLACE", true);            
            return 0;
        }

        public async Task<int> Truncate()
        {
            await Init();

            return await Database.DeleteAllAsync<product_template>();
        }

        async Task Init()
        {
            if (Database is not null)
                return;

            Database = new SQLiteAsyncConnection(Constants.DatabasePath, Constants.Flags);
            var result = await Database.CreateTableAsync<product_template>();
        }    
    }
}
