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
    public class BankDb
    {
        SQLiteAsyncConnection Database;

        public BankDb()
        {

        }

        public async Task<int>  GetCount()
        {
            await Init();
            return (await Database.Table<Bank_Id>().ToListAsync()).Count;
        }

        public async Task<List<Bank_Id>> GetItemsAsync()
        {
            await Init();
            return await Database.Table<Bank_Id>().ToListAsync();
        }

        public async Task<Bank_Id> GetItem(int id)
        {
            await Init();
            return await Database.Table<Bank_Id>().Where(x=>x.id == id).FirstOrDefaultAsync();
        }

        public async Task<int> InsertAsync(Bank_Id item)
        {
            await Init();
            await Database.InsertAsync(item);
            return 0;
        }

        public async Task<int> InsertBatchAsync(Bank_Id[] items)
        {
            await Init();
            await Database.InsertAllAsync(items, true);            
            return 0;
        }

        public async Task<int> Truncate()
        {
            await Init();

            return await Database.DeleteAllAsync<Bank_Id>();
        }

        async Task Init()
        {
            if (Database is not null)
                return;

            Database = new SQLiteAsyncConnection(Constants.DatabasePath, Constants.Flags);
            var result = await Database.CreateTableAsync<Bank_Id>();
            //Database.CreateTable<Bank_Id>();
        }    
    }
}
