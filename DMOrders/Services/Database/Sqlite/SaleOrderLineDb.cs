using CobranzasDMSA_Odoo.Models;
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
    public class SaleOrderLineDb
    {
        SQLiteAsyncConnection Database;

        public SaleOrderLineDb()
        {

        }

        public async Task<int>  GetCount()
        {
            await Init();
            return (await Database.Table<sale_order_line>().ToListAsync()).Count;
        }

        [Obsolete]
        public async Task<List<sale_order_line>> GetItemsAsync()
        {
            await Init();
            return await Database.Table<sale_order_line>()
                .Take(100)
                .ToListAsync();
        }

        public async Task<List<sale_order_line>> GetItemsAsync(int order_id)
        {
            await Init();
            return await Database.Table<sale_order_line>()
                .Where(x => x._order_id == order_id)
                .ToListAsync();
        }

        public async Task<sale_order_line> GetItem(int id)
        {
            await Init();
            return await Database.Table<sale_order_line>().Where(x=>x.id == id).FirstOrDefaultAsync();
        }

        public async Task<int> InsertAsync(sale_order_line item)
        {
            await Init();
            return await Database.InsertAsync(item);            
        }

        public async Task<int> InsertBatchAsync(sale_order_line[] items)
        {
            await Init();
            return await Database.InsertAllAsync(items, "OR REPLACE", true);
        }

        public async Task<int> Truncate()
        {
            await Init();
            return await Database.DeleteAllAsync<sale_order_line>();
        }

        public async Task<int> DeleteItemOfParent(sale_order parent)
        {
            await Init();
            int count = 0;
            //await Init();

            var resultItems = (await Database.Table<sale_order_line>().ToListAsync()).Where(i => i._order_id == parent.id);

            foreach (var item in resultItems)
            {
                count++;
                await Database.DeleteAsync(item);
            }

            return count;
        }

        public async Task<int> UpdateAsync(sale_order_line item)
        {
            await Init();
            return await Database.UpdateAsync(item);
        }

        async Task Init()
        {
            if (Database is not null)
                return;

            Database = new SQLiteAsyncConnection(Constants.DatabasePath, Constants.Flags);
            var result = await Database.CreateTableAsync<sale_order_line>();
        }    
    }
}
