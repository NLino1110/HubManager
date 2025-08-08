using CobranzasDMSA_Odoo.Models;
using Microsoft.Data.Sqlite;
using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CobranzasDMSA_Odoo.Settings.Sqlite
{
    public class AppParameterDb

    {
        SQLiteAsyncConnection Database;

        public AppParameterDb()
        {

        }

        public async Task<int> GetCount()
        {
            return await Database.Table<AppParameter>().CountAsync();
        }

        public async Task<List<AppParameter>> GetItemsAsync()
        {
            await Init();
            return await Database.Table<AppParameter>().ToListAsync();
        }

        public async Task<AppParameter> GetItemAsync(string id)
        {
            await Init();
            return await Database.Table<AppParameter>().Where(i => i.name == id).FirstOrDefaultAsync();
        }

        public async Task<int> InsertAsync(AppParameter item)
        {   
            await Init();
            //if (item.CODARTICULO != "")
            //    return await Database.UpdateAsync(item);
            //else
            return await Database.InsertAsync(item);
        }

        public async Task<int> InsertBatchAsync(AppParameter[] items)
        {
            await Init();
            //if (item.CODARTICULO != "")
            //    return await Database.UpdateAsync(item);
            //else
            return await Database.InsertAllAsync(items);
        }

        public async Task<int> UpdateAsync(AppParameter item)
        {
            await Init();            
            return await Database.UpdateAsync(item);            
        }

        public async Task<int> DeleteItemAsync(AppParameter item)
        {
            //await Init();
            return await Database.DeleteAsync(item);
        }

        public async Task<int> TruncateAsync()
        {
            await Init();
            return await Database.DeleteAllAsync<AppParameter>();            

            //SQLiteCommand cmd = new SQLiteCommand(Database.GetConnection());
            //cmd.CommandText = @"DELETE FROM FACNOTACREDITODET";
            //cmd.ExecuteNonQuery();
        }

        async Task Init()
        {
            if (Database is not null)
                return;

            Database = new SQLiteAsyncConnection(Constants.DatabasePath, Constants.Flags);
            var result = await Database.CreateTableAsync<AppParameter>();
        }
    
    }
}
