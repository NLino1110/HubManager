using CobranzasDMSA_Odoo.Models;
using DMSA.Models.Odoo.Native;
using Microsoft.Data.Sqlite;
using ResourceBuilder.Data.Structs;
using SQLite;
using SQLiteNetExtensions.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ResourceBuilder.Data.Sqlite
{
    public class AppDeployDb
    {
        SQLiteAsyncConnection Database;

        public AppDeployDb()
        {

        }

        public async Task<int> GetCount()
        {
            return (await Database.Table<AppDeploy>().ToListAsync()).Count;
        }

        public async Task<List<AppDeploy>> GetItemsAsync()
        {
            await Init();
            return await Database.Table<AppDeploy>().ToListAsync();            
        }

        public async Task<AppDeploy> GetItem(int id)
        {
            await Init();
            return await Database.Table<AppDeploy>().Where(i => i.Id == id).FirstOrDefaultAsync();
        }

        public async Task<int> InsertAsync(AppDeploy item)
        {
            await Init();

            await Database.InsertOrReplaceAsync(item);

            return 0;
        }

        public async Task<int> InsertBatchAsync(AppDeploy[] items)
        {
            await Init();
            await Database.InsertAllAsync(items, "OR REPLACE");
            return 0;
        }

        async Task Init()
        {
            if (Database is not null)
                return;

            string DirectoryPath = Path.GetDirectoryName(Constants.DatabasePath);
            if(!Path.Exists(DirectoryPath))
                Directory.CreateDirectory(DirectoryPath);

            Database = new SQLiteAsyncConnection(Constants.DatabasePath);
            var result = await Database.CreateTableAsync<AppDeploy>();
        }
    }
}
