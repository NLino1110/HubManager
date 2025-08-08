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
    public class ProductBrandDb
    {
        SQLiteAsyncConnection Database;

        public ProductBrandDb()
        {

        }

        public async Task<int>  GetCount()
        {
            await Init();
            return (await Database.Table<product_brand>().ToListAsync()).Count;
        }

        public async Task<List<product_brand>> GetItemsAsync()
        {
            await Init();
            return await Database.Table<product_brand>().ToListAsync();
        }

        public async Task<product_brand> GetItem(int id)
        {
            await Init();
            return await Database.Table<product_brand>().Where(x=>x.id == id).FirstOrDefaultAsync();
        }

        public async Task<int> InsertAsync(product_brand item)
        {
            await Init();
            await Database.InsertAsync(item);
            return 0;
        }

        public async Task<int> InsertBatchAsync(product_brand[] items)
        {
            await Init();
            await Database.InsertAllAsync(items, "OR REPLACE",true);            
            return 0;
        }

        public async Task<int> Truncate()
        {
            await Init();

            return await Database.DeleteAllAsync<product_brand>();
        }

        async Task Init()
        {
            if (Database is not null)
                return;

            Database = new SQLiteAsyncConnection(Constants.DatabasePath, Constants.Flags);
            var result = await Database.CreateTableAsync<product_brand>();
        }    
    }
}
