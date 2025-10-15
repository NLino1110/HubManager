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
    public class PromotionProductDetailDb
    {
        SQLiteAsyncConnection Database;

        public PromotionProductDetailDb()
        {

        }

        public async Task<int>  GetCount()
        {
            await Init();
            return (await Database.Table<PromotionProductDetail>().ToListAsync()).Count;
        }

        public async Task<List<PromotionProductDetail>> GetItemsAsync()
        {
            await Init();
            return await Database.Table<PromotionProductDetail>().ToListAsync();
        }

        public async Task<PromotionProductDetail> GetItem(int id)
        {
            await Init();
            return await Database.Table<PromotionProductDetail>().Where(x=>x.id == id).FirstOrDefaultAsync();
        }

        public async Task<int> InsertAsync(PromotionProductDetail item)
        {
            await Init();
            await Database.InsertAsync(item);
            return 0;
        }

        public async Task<int> InsertBatchAsync(PromotionProductDetail[] items)
        {
            await Init();
            await Database.InsertAllAsync(items, "OR REPLACE",true);            
            return 0;
        }

        public async Task<int> Truncate()
        {
            await Init();

            return await Database.DeleteAllAsync<PromotionProductDetail>();
        }

        async Task Init()
        {
            if (Database is not null)
                return;

            Database = new SQLiteAsyncConnection(Constants.DatabasePath, Constants.Flags);
            var result = await Database.CreateTableAsync<PromotionProductDetail>();
        }    
    }
}
