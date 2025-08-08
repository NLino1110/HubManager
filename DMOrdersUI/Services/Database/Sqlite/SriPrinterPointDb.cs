using DMSA.Models.Odoo.Native;
using Microsoft.Data.Sqlite;
using SQLite;
using SQLiteNetExtensions.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMOrdersUI.Services.Database.Sqlite
{
    public class SriPrinterPointDb
    {
        SQLiteAsyncConnection Database;

        public SriPrinterPointDb()
        {

        }

        public async Task<int>  GetCount()
        {
            await Init();
            return (await Database.Table<sri_printer_point>().ToListAsync()).Count;
        }

        public async Task<List<sri_printer_point>> GetItemsAsync()
        {
            await Init();
            return await Database.Table<sri_printer_point>().ToListAsync();
        }

        public async Task<sri_printer_point> GetItem(int id)
        {
            await Init();
            return await Database.Table<sri_printer_point>().Where(x=>x.id == id).FirstOrDefaultAsync();
        }

        public async Task<int> InsertAsync(sri_printer_point item)
        {
            await Init();
            await Database.InsertAsync(item);
            return 0;
        }

        public async Task<int> InsertBatchAsync(sri_printer_point[] items)
        {
            await Init();
            await Database.InsertAllAsync(items, true);            
            return 0;
        }

        public async Task<int> Truncate()
        {
            await Init();

            return await Database.DeleteAllAsync<sri_printer_point>();
        }

        async Task Init()
        {
            if (Database is not null)
                return;

            Database = new SQLiteAsyncConnection(Constants.DatabasePath, Constants.Flags);
            var result = await Database.CreateTableAsync<sri_printer_point>();
        }    
    }
}
