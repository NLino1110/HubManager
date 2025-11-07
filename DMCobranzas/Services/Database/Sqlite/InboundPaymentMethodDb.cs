using DMCobranzas.Models;
using DMCobranzas.Settings;
using DMSA.Models.Odoo.Native;
using Microsoft.Data.Sqlite;
using SQLite;
using SQLiteNetExtensions.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMCobranzas.Services.Database.Sqlite
{
    public class InboundPaymentMethodDb
    {
        SQLiteAsyncConnection Database;

        public InboundPaymentMethodDb()
        {

        }

        public async Task<int> GetCount()
        {
            return (await Database.Table<inbound_payment_method>().ToListAsync()).Count;
        }

        public async Task<int> TruncateAsync()
        {
            await Init();           
            return await Database.DeleteAllAsync<inbound_payment_method>();
        }

        public async Task<List<inbound_payment_method>> GetItemsAsync()
        {
            await Init();
            return await Database.Table<inbound_payment_method>().ToListAsync();
        }

        public async Task<inbound_payment_method> GetItemAsync(int id)
        {
            await Init();
            return await Database.Table<inbound_payment_method>().Where(x=>x.id == id ).FirstOrDefaultAsync();
        }

        public async Task<List<inbound_payment_method>> GetItemsByParentAsync(int parent_id)
        {
            await Init();
            return await Database.Table<inbound_payment_method>().Where(x=>x.parent_id == parent_id).ToListAsync();
        }

        public async Task<int> InsertAsync(inbound_payment_method item)
        {
            await Init();
            await Database.InsertAsync(item);
            return 0;
        }

        public async Task<int> InsertBatchAsync(inbound_payment_method[] items)
        {
            await Init();
            //await Database.InsertAllAsync(items, "OR REPLACE");
            await Database.InsertAllAsync(items);
            return 0;
        }

        async Task Init()
        {            
            if (Database is not null)
                return;

            Database = new SQLiteAsyncConnection(Constants.DatabasePath);
            var result = await Database.CreateTableAsync<inbound_payment_method>();
        }
    
    }
}
