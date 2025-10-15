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
    public class PromotionEvalItemDb
    {
        SQLiteAsyncConnection Database;

        public PromotionEvalItemDb()
        {

        }

        public async Task<int>  GetCount()
        {
            await Init();
            return (await Database.Table<PromotionEvalItem>().ToListAsync()).Count;
        }

        public async Task<List<PromotionEvalItem>> GetItemsAsync()
        {
            await Init();
            return await Database.Table<PromotionEvalItem>().ToListAsync();
        }

        public async Task<PromotionEvalItem> GetItem(PromotionHeader id)
        {
            await Init();
            return await Database.Table<PromotionEvalItem>().Where(x=>x.Promotion == id).FirstOrDefaultAsync();
        }

        public async Task<int> InsertAsync(PromotionEvalItem item)
        {
            await Init();
            await Database.InsertAsync(item);
            return 0;
        }

        public async Task<int> InsertBatchAsync(PromotionEvalItem[] items)
        {
            await Init();
            await Database.InsertAllAsync(items, "OR REPLACE",true);            
            return 0;
        }

        public async Task<int> Truncate()
        {
            await Init();

            return await Database.DeleteAllAsync<PromotionEvalItem>();
        }

        async Task Init()
        {
            if (Database is not null)
                return;

            Database = new SQLiteAsyncConnection(Constants.DatabasePath, Constants.Flags);
            var result = await Database.CreateTableAsync<PromotionEvalItem>();
        }    
    }
}
