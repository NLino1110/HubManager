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

namespace CobranzasDMSA_Odoo.Settings.Sqlite
{
    public class AccountModuleDb
    {
        SQLiteAsyncConnection Database;
        
        public AccountModuleDb()
        {

        }

        public async Task<int> GetCount()
        {
            await Init();
            return await Database.Table<AccountModule>().CountAsync();
        }

        public async Task<int> Truncate()
        {
            await Init();            
            return await Database.DeleteAllAsync<AccountModule>();
        }
       

        //public async Task<List<AccountModule>> GetItemsAsync(int company_id, int partner_id, int limit)
        //{
        //    await Init();
        //    return await Database.Table<AccountModule>().Where(i => i._company_id == company_id &&
        //    i._partner_id == partner_id).Take(limit).ToListAsync();
        //}

        public async Task<List<AccountModule>> GetItemsAsync()
        {
            await Init();
            return await Database.Table<AccountModule>().ToListAsync();
            //return Database.Table<account_journal>().ToList();
        }

        public async Task<AccountModule> GetItem(int id)
        {
            await Init();
            return await Database.Table<AccountModule>().Where(i => i.id == id).FirstOrDefaultAsync();
        }

        public async Task<AccountModule> GetByNameItem(string name_doc)
        {
            await Init();
            return await Database.Table<AccountModule>().Where(i => i.name == name_doc).FirstOrDefaultAsync();
        }

        public async Task<int> InsertAsync(AccountModule item)
        {
            await Init();
            await Database.InsertAsync(item);
            return 0;
        }

        public async Task<int> InsertBatchAsync(AccountModule[] items)
        {
            await Init();
            await Database.InsertAllAsync(items, "OR REPLACE");            
            return 0;
        }

        async Task Init()
        {
            if (Database is not null)
                return;

            Database = new SQLiteAsyncConnection(Constants.DatabasePath, Constants.Flags);
            var result = await Database.CreateTableAsync<AccountModule>();
        }
    
    }
}
