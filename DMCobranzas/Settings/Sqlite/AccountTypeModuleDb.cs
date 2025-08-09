using DMCobranzas.Models;
using DMSA.Models.Odoo.DMCobranzas;
using DMSA.Models.Odoo.Native;
using Microsoft.Data.Sqlite;
using SQLite;
using SQLiteNetExtensions.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMCobranzas.Settings.Sqlite
{
    public class AccountTypeModuleDb
    {
        SQLiteAsyncConnection Database;
        
        public AccountTypeModuleDb()
        {

        }

        public async Task<int> GetCount()
        {
            await Init();
            return await Database.Table<AccountTypeModule>().CountAsync();
        }

        public async Task<int> Truncate()
        {
            await Init();            
            return await Database.DeleteAllAsync<AccountTypeModule>();
        }

        //public async Task<List<AccountModule>> GetItemsAsync(int company_id, int partner_id, int limit)
        //{
        //    await Init();
        //    return await Database.Table<AccountModule>().Where(i => i._company_id == company_id &&
        //    i._partner_id == partner_id).Take(limit).ToListAsync();
        //}

        public async Task<List<AccountTypeModule>> GetItemsAsync()
        {
            await Init();
            return await Database.Table<AccountTypeModule>().ToListAsync();
            //return Database.Table<account_journal>().ToList();
        }

        public async Task<AccountTypeModule> GetItem(int id)
        {
            await Init();
            return await Database.Table<AccountTypeModule>().Where(i => i.id == id).FirstOrDefaultAsync();
        }

        public async Task<AccountTypeModule> GetByNameItem(string name_doc)
        {
            await Init();
            return await Database.Table<AccountTypeModule>().Where(i => i.name == name_doc).FirstOrDefaultAsync();
        }

        public async Task<int> InsertAsync(AccountTypeModule item)
        {
            await Init();
            await Database.InsertAsync(item);
            return 0;
        }

        public async Task<int> InsertBatchAsync(AccountTypeModule[] items)
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
            var result = await Database.CreateTableAsync<AccountTypeModule>();
        }
    
    }
}
