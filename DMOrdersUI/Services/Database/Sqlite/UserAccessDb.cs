using DMSA.Models.Odoo.DMApps;
using Microsoft.Data.Sqlite;
using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMOrdersUI.Services.Database.Sqlite
{
    public class UserAccessDb

    {
        SQLiteAsyncConnection Database;

        public UserAccessDb()
        {

        }

        public async Task<int> GetCount()
        {
            return await Database.Table<user_access>().CountAsync();
        }

        public async Task<List<user_access>> GetItemsAsync()
        {
            await Init();
            return await Database.Table<user_access>().ToListAsync();
        }

        public async Task<user_access> GetItemAsync(int id)
        {
            await Init();
            return await Database.Table<user_access>().Where(i => i.uid == id).FirstOrDefaultAsync();
        }

        public async Task<int> InsertAsync(user_access item)
        {   
            await Init();
            //if (item.CODARTICULO != "")
            //    return await Database.UpdateAsync(item);
            //else
            return await Database.InsertAsync(item);
        }

        public async Task<int> InsertBatchAsync(user_access[] items)
        {
            await Init();
            //if (item.CODARTICULO != "")
            //    return await Database.UpdateAsync(item);
            //else
            return await Database.InsertAllAsync(items);
        }

        public async Task<int> UpdateAsync(user_access item)
        {
            await Init();
            return await Database.UpdateAsync(item);
        }

        public async Task<int> DeleteItemAsync(user_access item)
        {
            //await Init();
            return await Database.DeleteAsync(item);
        }

        public async Task<int> TruncateAsync()
        {
            await Init();
            return await Database.DeleteAllAsync<user_access>();            

            //SQLiteCommand cmd = new SQLiteCommand(Database.GetConnection());
            //cmd.CommandText = @"DELETE FROM FACNOTACREDITODET";
            //cmd.ExecuteNonQuery();
        }

        public async Task Drop()
        {
            if (Database is not null)
                return;

            Database = new SQLiteAsyncConnection(Constants.DatabasePath, Constants.Flags);
            //No se procede a la creación automática porque contiene varios campos PK
            var result = await Database.DropTableAsync<user_access>();
        }

        async Task Init()
        {
            if (Database is not null)
                return;

            Database = new SQLiteAsyncConnection(Constants.DatabasePath);
            var result = await Database.CreateTableAsync<user_access>();
        }
    
    }
}
