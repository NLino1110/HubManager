using DMSA.Models.Odoo.DMOrders.promotions;
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
    public class PosTarjetasCanalDb
    {
        SQLiteAsyncConnection Database;

        public PosTarjetasCanalDb()
        {

        }

        public async Task<int>  GetCount()
        {
            await Init();
            return (await Database.Table<PosTarjetasCanal>().ToListAsync()).Count;
        }

        public async Task<List<PosTarjetasCanal>> GetItemsAsync()
        {
            await Init();
            return await Database.Table<PosTarjetasCanal>().ToListAsync();
        }

        public async Task<PosTarjetasCanal> GetItem(int id)
        {
            await Init();
            return await Database.Table<PosTarjetasCanal>().Where(x=>x.id == id).FirstOrDefaultAsync();
        }

        public async Task<int> InsertAsync(LoyaltyFiltersDetail item)
        {
            await Init();
            await Database.InsertAsync(item);
            return 0;
        }

        public async Task<int> InsertBatchAsync(LoyaltyFiltersDetail[] items)
        {
            await Init();
            await Database.InsertAllAsync(items, "OR REPLACE",true);            
            return 0;
        }

        public async Task<int> Truncate()
        {
            await Init();

            return await Database.DeleteAllAsync<PosTarjetasCanal>();
        }

        async Task Init()
        {
            if (Database is not null)
                return;

            Database = new SQLiteAsyncConnection(Constants.DatabasePath, Constants.Flags);
            var result = await Database.CreateTableAsync<PosTarjetasCanal>();
        }    
    }
}
