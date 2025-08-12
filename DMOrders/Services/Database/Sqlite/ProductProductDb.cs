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
    public class ProductProductDb
    {
        SQLiteAsyncConnection Database;

        public ProductProductDb()
        {

        }

        public async Task<int>  GetCount()
        {
            await Init();
            return (await Database.Table<product_product>().ToListAsync()).Count;
        }

        [Obsolete]
        public async Task<List<product_product>> GetItemsAsync()
        {
            await Init();
            return await Database.Table<product_product>()
                .Take(100)
                .ToListAsync();
        }

        public async Task<List<product_product>> GetItemsAsync(string code)
        {
            await Init();
            return await Database.Table<product_product>()
                .Where(x => !x.image_256.Contains("false"))
                //.Where(x=>x.code.Contains(code))
                .ToListAsync();
        }

        public async Task<product_product> GetItem(int id)
        {
            await Init();
            return await Database.Table<product_product>().Where(x=>x.id == id).FirstOrDefaultAsync();
        }

        public async Task<int> InsertAsync(product_product item)
        {
            await Init();
            await Database.InsertAsync(item);
            return 0;
        }

        public async Task<int> InsertBatchAsync(product_product[] items)
        {
            await Init();
            await Database.InsertAllAsync(items, "OR REPLACE", true);            
            return 0;
        }

        public async Task<int> Truncate()
        {
            await Init();

            return await Database.DeleteAllAsync<product_product>();
        }

        async Task Init()
        {
            if (Database is not null)
                return;

            Database = new SQLiteAsyncConnection(Constants.DatabasePath, Constants.Flags);
            var result = await Database.CreateTableAsync<product_product>();
        }    
    }
}
