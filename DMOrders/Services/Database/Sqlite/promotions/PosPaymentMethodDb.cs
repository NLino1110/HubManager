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
    public class PosPaymentMethodDb
    {
        SQLiteAsyncConnection Database;

        public PosPaymentMethodDb()
        {

        }

        public async Task<int>  GetCount()
        {
            await Init();
            return (await Database.Table<PosPaymentMethod>().ToListAsync()).Count;
        }

        public async Task<List<PosPaymentMethod>> GetItemsAsync()
        {
            await Init();
            return await Database.Table<PosPaymentMethod>().ToListAsync();
        }

        public async Task<PosPaymentMethod> GetItem(int id)
        {
            await Init();
            return await Database.Table<PosPaymentMethod>().Where(x=>x.Id == id).FirstOrDefaultAsync();
        }

        public async Task<int> InsertAsync(PosPaymentMethod item)
        {
            await Init();
            await Database.InsertAsync(item);
            return 0;
        }

        public async Task<int> InsertBatchAsync(PosPaymentMethod[] items)
        {
            await Init();
            await Database.InsertAllAsync(items, "OR REPLACE",true);            
            return 0;
        }

        public async Task<int> Truncate()
        {
            await Init();

            return await Database.DeleteAllAsync<PosPaymentMethod>();
        }

        async Task Init()
        {
            if (Database is not null)
                return;

            Database = new SQLiteAsyncConnection(Constants.DatabasePath, Constants.Flags);
            var result = await Database.CreateTableAsync<PosPaymentMethod>();
        }    
    }
}
