using DMSA.Models.Odoo.DMCobranzas;
using DMSA.Models.Odoo.DMOrders.promotions;
using DMSA.Models.Odoo.DMOrders.tareas;
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
    public class AccountAnalyticLineDb
    {
        SQLiteAsyncConnection Database;

        public AccountAnalyticLineDb()
        {

        }

        public async Task<int>  GetCount()
        {
            await Init();
            return (await Database.Table<AccountAnalyticLine>().ToListAsync()).Count;
        }

        public async Task<List<AccountAnalyticLine>> GetItemsAsync(ProjectTask parent)
        {
            await Init();
            return await Database.Table<AccountAnalyticLine>().Where(x=> x.project_id == parent.project_id && x.task_id == parent.id).ToListAsync();
        }

        public async Task<List<AccountAnalyticLine>> GetItemsAsync()
        {
            await Init();
            return await Database.Table<AccountAnalyticLine>().ToListAsync();
        }

        public async Task<List<AccountAnalyticLine>> GetItemByNameAsync(string name)
        {
            await Init();
            return await Database.Table<AccountAnalyticLine>().Where(x=>x.name == name).ToListAsync();
        }

        public async Task<AccountAnalyticLine> GetItem(int id)
        {
            await Init();
            return await Database.Table<AccountAnalyticLine>().Where(x=>x.id == id).FirstOrDefaultAsync();
        }

        public async Task<int> InsertAsync(AccountAnalyticLine item)
        {
            await Init();
            await Database.InsertAsync(item);
            return 0;
        }

        public async Task<int> InsertBatchAsync(AccountAnalyticLine[] items)
        {
            await Init();
            await Database.InsertAllAsync(items, "OR REPLACE",true);            
            return 0;
        }

        public async Task<int> UpdateAsync(AccountAnalyticLine item)
        {
            await Init();
            return await Database.UpdateAsync(item);
        }

        public async Task<int> Truncate()
        {
            await Init();

            return await Database.DeleteAllAsync<AccountAnalyticLine>();
        }

        async Task Init()
        {
            if (Database is not null)
                return;

            Database = new SQLiteAsyncConnection(Constants.DatabasePath, Constants.Flags);
            var result = await Database.CreateTableAsync<AccountAnalyticLine>();
        }    
    }
}
