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
    public class ProjectTaskDb
    {
        SQLiteAsyncConnection Database;

        public ProjectTaskDb()
        {

        }

        public async Task<int>  GetCount()
        {
            await Init();
            return (await Database.Table<ProjectTask>().ToListAsync()).Count;
        }

        public async Task<List<ProjectTask>> GetItemsAsync()
        {
            await Init();
            return await Database.Table<ProjectTask>().ToListAsync();
        }

        public async Task<List<ProjectTask>> GetItemsAsync(int company_id, bool sync_status)
        {
            await Init();
            return await Database.Table<ProjectTask>()
                .Where(x => x.company_id == company_id && x.is_synchronized == sync_status)
                .ToListAsync();
        }

        public async Task<List<ProjectTask>> GetItemsAsync(int company_id, int int_status, DateTime? dateStart, DateTime? dateEnd)
        {
            await Init();
            var query = Database.Table<ProjectTask>().Where(x => x.company_id == company_id);
            //int_status == -1 TODOS
            //int_status == 0 NO SINCRONIZADOS
            //int_status == 1 SINCRONIZADOS
            if (int_status == 0)
            {
                // NO SINCRONIZADOS
                query = query.Where(x => !x.is_synchronized);
            }
            else if (int_status == 1)
            {
                // SINCRONIZADOS
                query = query.Where(x => x.is_synchronized);
            }

            if (dateStart.HasValue && dateEnd.HasValue)
            {
                query = query.Where(x => x.date_assign >= dateStart && x.date_assign <= dateEnd);
            }

            return await query.ToListAsync();
        }

        public async Task<List<ProjectTask>> GetItemByNameAsync(int company_id, string name)
        {
            await Init();
            return await Database.Table<ProjectTask>().Where(x=>x.name == name && x.company_id == company_id).ToListAsync();
        }

        public async Task<ProjectTask> GetItem(int id)
        {
            await Init();
            return await Database.Table<ProjectTask>().Where(x=>x.id == id).FirstOrDefaultAsync();
        }

        public async Task<int> InsertAsync(ProjectTask item)
        {
            await Init();
            await Database.InsertAsync(item);
            return 0;
        }

        public async Task<int> InsertBatchAsync(ProjectTask[] items)
        {
            await Init();
            await Database.InsertAllAsync(items, "OR REPLACE",true);            
            return 0;
        }

        public async Task<int> UpdateAsync(ProjectTask item)
        {
            await Init();
            return await Database.UpdateAsync(item);
        }

        public async Task<int> Truncate()
        {
            await Init();

            return await Database.DeleteAllAsync<ProjectTask>();
        }

        async Task Init()
        {
            if (Database is not null)
                return;

            Database = new SQLiteAsyncConnection(Constants.DatabasePath, Constants.Flags);
            var result = await Database.CreateTableAsync<ProjectTask>();
        }    
    }
}
