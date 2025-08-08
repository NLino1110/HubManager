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
    public class CompanyDb
    {
        SQLiteAsyncConnection Database;

        public CompanyDb()
        {

        }

        public async Task<int> GetCount()
        {
            return (await Database.Table<res_company>().ToListAsync()).Count;
        }

        public async Task<List<res_company>> GetItemsAsync()
        {
            await Init();
            return await Database.Table<res_company>().ToListAsync();
            //return Database.Table<account_journal>().ToList();
        }

        public async Task<res_company> GetItem(int id)
        {
            await Init();
            return await Database.Table<res_company>().Where(i => i.id == id).FirstOrDefaultAsync();
        }

        public async Task<int> InsertAsync(res_company item)
        {
            await Init();

            await Database.InsertOrReplaceAsync(item);

            return 0;
        }

        public async Task<int> InsertBatchAsync(res_company[] items)
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
            var result = await Database.CreateTableAsync<res_company>();
        }
    }
}
