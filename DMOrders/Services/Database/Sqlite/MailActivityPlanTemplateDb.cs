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
    public class MailActivityPlanTemplateDb
    {
        SQLiteAsyncConnection Database;

        public MailActivityPlanTemplateDb()
        {

        }

        public async Task<int> GetCount()
        {
            return (await Database.Table<MailActivityPlanTemplate>().ToListAsync()).Count;
        }

        public async Task<List<MailActivityPlanTemplate>> GetItemsAsync()
        {
            await Init();
            return await Database.Table<MailActivityPlanTemplate>().ToListAsync();
            //return Database.Table<account_journal>().ToList();
        }

        public async Task<MailActivityPlanTemplate> GetItem(MailActivityPlan id)
        {
            await Init();
            return await Database.Table<MailActivityPlanTemplate>().Where(i => i.plan_id == id).FirstOrDefaultAsync();
        }

        public async Task<int> InsertAsync(MailActivityPlanTemplate item)
        {
            await Init();

            await Database.InsertOrReplaceAsync(item);

            return 0;
        }

        public async Task<int> InsertBatchAsync(MailActivityPlanTemplate[] items)
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
            var result = await Database.CreateTableAsync<MailActivityPlanTemplate>();
        }

        public async Task<int> Truncate()
        {
            await Init();            
            return await Database.DeleteAllAsync<MailActivityPlanTemplate>();
        }
    }
}
