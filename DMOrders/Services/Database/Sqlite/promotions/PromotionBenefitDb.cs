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
    public class PromotionBenefitDb
    {
        SQLiteAsyncConnection Database;

        public PromotionBenefitDb()
        {

        }

        public async Task<int>  GetCount()
        {
            await Init();
            return (await Database.Table<PromotionBenefit>().ToListAsync()).Count;
        }

        public async Task<List<PromotionBenefit>> GetItemsAsync()
        {
            await Init();
            return await Database.Table<PromotionBenefit>().ToListAsync();
        }

        public async Task<PromotionBenefit> GetItem(int id)
        {
            await Init();
            return await Database.Table<PromotionBenefit>().Where(x=>x.id == id).FirstOrDefaultAsync();
        }

        public async Task<int> InsertAsync(PromotionBenefit item)
        {
            await Init();
            await Database.InsertAsync(item);
            return 0;
        }

        public async Task<int> InsertBatchAsync(PromotionBenefit[] items)
        {
            await Init();
            await Database.InsertAllAsync(items, "OR REPLACE",true);            
            return 0;
        }

        public async Task<int> Truncate()
        {
            await Init();

            return await Database.DeleteAllAsync<PromotionBenefit>();
        }

        async Task Init()
        {
            if (Database is not null)
                return;

            Database = new SQLiteAsyncConnection(Constants.DatabasePath, Constants.Flags);
            var result = await Database.CreateTableAsync<PromotionBenefit>();
        }    
    }
}
