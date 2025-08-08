using CobranzasDMSA_Odoo.Models;
using DMSA.Models.Odoo.Native;
using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMOrdersUI.Services.Database.Sqlite
{
    public class UserDb
    {
        SQLiteAsyncConnection Database;

        public UserDb()
        {

        }

        public async Task<int> GetCount()
        {
            await Init();
            return (await Database.Table<res_user>().ToListAsync()).Count;
        }

        public async Task<res_user> GetItemsAsync(int company_id, int user_id)
        {
            await Init();
            return await Database.Table<res_user>().Where(x => x._company_id == company_id &&
            x.id == user_id).FirstOrDefaultAsync();
        }

        public async Task<List<res_user>> GetItemsAsync()
        {
            await Init();
            return await Database.Table<res_user>().ToListAsync();
        }

        public async Task<res_user> GetItem(int id)
        {
            await Init();
            return await Database.Table<res_user>().Where(x => x.id == id).FirstOrDefaultAsync();
        }

        public async Task<int> InsertAsync(res_user item)
        {
            await Init();
            await Database.InsertAsync(item);
            return 0;
        }

        public async Task<int> InsertBatchAsync(res_user[] items)
        {
            await Init();
            await Database.InsertAllAsync(items, "OR REPLACE", true);
            return 0;
        }

        public async Task<int> Truncate()
        {
            await Init();

            return await Database.DeleteAllAsync<res_user>();
        }

        async Task Init()
        {
            if (Database is not null)
                return;

            Database = new SQLiteAsyncConnection(Constants.DatabasePath, Constants.Flags);
            var result = await Database.CreateTableAsync<res_user>();
        }
    }
}
