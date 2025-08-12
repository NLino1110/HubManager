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
    public class MacroProductSubCategoryDb
    {
        SQLiteAsyncConnection Database;

        public MacroProductSubCategoryDb()
        {

        }

        public async Task<int>  GetCount()
        {
            await Init();
            return (await Database.Table<macro_product_sub_category>().ToListAsync()).Count;
        }

        public async Task<List<macro_product_sub_category>> GetItemsAsync()
        {
            await Init();
            return await Database.Table<macro_product_sub_category>().ToListAsync();
        }

        public async Task<macro_product_sub_category> GetItem(int id)
        {
            await Init();
            return await Database.Table<macro_product_sub_category>().Where(x=>x.id == id).FirstOrDefaultAsync();
        }

        public async Task<int> InsertAsync(macro_product_sub_category item)
        {
            await Init();
            await Database.InsertAsync(item);
            return 0;
        }

        public async Task<int> InsertBatchAsync(macro_product_sub_category[] items)
        {
            await Init();
            await Database.InsertAllAsync(items, "OR REPLACE",true);            
            return 0;
        }

        public async Task<int> Truncate()
        {
            await Init();

            return await Database.DeleteAllAsync<macro_product_sub_category>();
        }

        async Task Init()
        {
            if (Database is not null)
                return;

            Database = new SQLiteAsyncConnection(Constants.DatabasePath, Constants.Flags);
            var result = await Database.CreateTableAsync<macro_product_sub_category>();
        }    
    }
}
