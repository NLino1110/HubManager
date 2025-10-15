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
    public class PromotionSelectionTypeDb
    {
        SQLiteAsyncConnection Database;

        public PromotionSelectionTypeDb()
        {

        }

        public async Task<int>  GetCount()
        {
            await Init();
            return (await Database.Table<PromotionSelectionType>().ToListAsync()).Count;
        }

        public async Task<List<PromotionSelectionType>> GetItemsAsync()
        {
            await Init();
            return await Database.Table<PromotionSelectionType>().ToListAsync();
        }

        public async Task<PromotionSelectionType> GetItem(int id)
        {
            await Init();
            return await Database.Table<PromotionSelectionType>().Where(x=>x.Id== id).FirstOrDefaultAsync();
        }

        public async Task<int> InsertAsync(PromotionSelectionType item)
        {
            await Init();
            await Database.InsertAsync(item);
            return 0;
        }

        public async Task<int> InsertBatchAsync(PromotionSelectionType[] items)
        {
            await Init();
            await Database.InsertAllAsync(items, "OR REPLACE",true);            
            return 0;
        }

        public async Task<int> Truncate()
        {
            await Init();

            return await Database.DeleteAllAsync<PromotionSelectionType>();
        }

        async Task Init()
        {
            if (Database is not null)
                return;

            Database = new SQLiteAsyncConnection(Constants.DatabasePath, Constants.Flags);
            var result = await Database.CreateTableAsync<PromotionSelectionType>();
        }    
    }
}
