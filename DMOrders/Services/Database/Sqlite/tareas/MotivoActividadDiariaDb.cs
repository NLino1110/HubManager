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
    public class MotivoActividadDiariaDb
    {
        SQLiteAsyncConnection Database;

        public MotivoActividadDiariaDb()
        {

        }

        public async Task<int>  GetCount()
        {
            await Init();
            return (await Database.Table<MotivoActividadDiaria>().ToListAsync()).Count;
        }

        public async Task<List<MotivoActividadDiaria>> GetItemsAsync()
        {
            await Init();
            return await Database.Table<MotivoActividadDiaria>().ToListAsync();
        }

        public async Task<MotivoActividadDiaria> GetItem(int id)
        {
            await Init();
            return await Database.Table<MotivoActividadDiaria>().Where(x=>x.id == id).FirstOrDefaultAsync();
        }

        public async Task<int> InsertAsync(MotivoActividadDiaria item)
        {
            await Init();
            await Database.InsertAsync(item);
            return 0;
        }

        public async Task<int> InsertBatchAsync(MotivoActividadDiaria[] items)
        {
            await Init();
            await Database.InsertAllAsync(items, "OR REPLACE",true);            
            return 0;
        }

        public async Task<int> Truncate()
        {
            await Init();

            return await Database.DeleteAllAsync<MotivoActividadDiaria>();
        }

        async Task Init()
        {
            if (Database is not null)
                return;

            Database = new SQLiteAsyncConnection(Constants.DatabasePath, Constants.Flags);
            var result = await Database.CreateTableAsync<MotivoActividadDiaria>();
        }    
    }
}
