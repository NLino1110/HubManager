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
    public class ProductMarcaDb
    {
        SQLiteAsyncConnection Database;

        public ProductMarcaDb()
        {

        }

        public async Task<int>  GetCount()
        {
            await Init();
            return (await Database.Table<product_marca>().ToListAsync()).Count;
        }

        public async Task<List<product_marca>> GetItemsAsync()
        {
            await Init();
            return await Database.Table<product_marca>().ToListAsync();
        }

        public async Task<product_marca> GetItem(int id)
        {
            await Init();
            return await Database.Table<product_marca>().Where(x=>x.id == id).FirstOrDefaultAsync();
        }

        public async Task<int> InsertAsync(product_marca item)
        {
            await Init();
            await Database.InsertAsync(item);
            return 0;
        }

        public async Task<int> InsertBatchAsync(product_marca[] items)
        {
            await Init();
            await Database.InsertAllAsync(items, "OR REPLACE",true);            
            return 0;
        }

        public async Task<int> Truncate()
        {
            await Init();

            return await Database.DeleteAllAsync<product_marca>();
        }

        async Task Init()
        {
            if (Database is not null)
                return;

            Database = new SQLiteAsyncConnection(Constants.DatabasePath, Constants.Flags);
            var result = await Database.CreateTableAsync<product_marca>();
        }    
    }
}
