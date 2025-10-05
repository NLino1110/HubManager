using DMSA.Models.Odoo.DMOrders;
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
    public class PlanningSlotDb
    {
        SQLiteAsyncConnection Database;

        public PlanningSlotDb()
        {

        }

        public async Task<int> GetCount()
        {
            return (await Database.Table<PlanningSlot>().ToListAsync()).Count;
        }

        public async Task<List<PlanningSlot>> GetItemsAsync()
        {
            await Init();
            return await Database.Table<PlanningSlot>().ToListAsync();
            //return Database.Table<account_journal>().ToList();
        }

        public async Task<PlanningSlot> GetItem(int id)
        {
            await Init();
            return await Database.Table<PlanningSlot>().Where(i => i.id == id).FirstOrDefaultAsync();
        }

        public async Task<int> InsertAsync(PlanningSlot item)
        {
            await Init();

            await Database.InsertOrReplaceAsync(item);

            return 0;
        }

        public async Task<int> InsertBatchAsync(PlanningSlot[] items)
        {
            await Init();
            await Database.InsertAllAsync(items, "OR REPLACE");
            return 0;
        }

        async Task Init()
        {
            if (Database is not null)
                return;

            Database = new SQLiteAsyncConnection(Constants.DatabasePath);
            var result = await Database.CreateTableAsync<PlanningSlot>();
        }

        public async Task<int> Truncate()
        {
            await Init();            
            return await Database.DeleteAllAsync<PlanningSlot>();
        }
    }
}
