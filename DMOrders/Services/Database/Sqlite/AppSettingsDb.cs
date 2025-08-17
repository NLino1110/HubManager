using DMSA.Models.Odoo;
using DMSA.Models.Odoo.Native;
using Microsoft.Data.Sqlite;
using SQLite;
using SQLiteNetExtensions.Extensions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMOrders.Services.Database.Sqlite
{
    public class AppSettingsDb
    {
        SQLiteAsyncConnection Database;

        public AppSettingsDb()
        {

        }

        public async Task<int> InitDefault()
        {
            await Init();
            AppSettings appSettings = new AppSettings();
            
            //if(appSettings.LoadDefault().Count() > await GetCount())
            //{
            //    await TruncateAsync();
            //    await Database.InsertAllAsync(appSettings.LoadDefault());
            //}

            foreach(var itemSetting in appSettings.LoadDefault())
            {
                var foundItem = await GetItem(itemSetting.name);
                if (foundItem == null)
                {
                    await Database.InsertAsync(itemSetting);
                }
            }

            return 0;
        }

        public async Task<int> TruncateAsync()
        {
            await Init();
            return await Database.DeleteAllAsync<AppSettings>();
        }

        public async Task<int> GetCount()
        {
            return (await Database.Table<AppSettings>().ToListAsync()).Count;
        }

        public async Task<List<AppSettings>> GetItemsAsync()
        {
            await Init();
            return await Database.Table<AppSettings>().ToListAsync();
            //return Database.Table<account_journal>().ToList();
        }

        public async Task<bool> getBoolean(string name)
        {
            await Init();
            var dbITem = await Database.Table<AppSettings>().Where(i => i.name == name).FirstOrDefaultAsync();
            bool returnValue = false;
            try
            {
                returnValue = bool.Parse(dbITem.value);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }

            return returnValue;
        }

        public async Task<DateTime> getDateTime(string name)
        {
            await Init();
            var dbItem = await Database.Table<AppSettings>().Where(i => i.name == name).FirstOrDefaultAsync();

            try
            {
                if (!string.IsNullOrWhiteSpace(dbItem?.value))
                {
                    return DateTime.Parse(dbItem.value);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }

            return DateTime.MinValue; // o cualquier valor predeterminado que prefieras
        }



        public async Task<int> getInteger(string name)
        {
            await Init();
            var dbITem = await Database.Table<AppSettings>().Where(i => i.name == name).FirstOrDefaultAsync();
            int returnValue = 0;
            try
            {
                returnValue = int.Parse(dbITem.value);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }

            return returnValue;
        }

        public async Task<string> getString(string name)
        {
            await Init();
            var dbITem = await Database.Table<AppSettings>().Where(i => i.name == name).FirstOrDefaultAsync();
            return dbITem.value;
        }

        public async Task<AppSettings> GetItem(string name)
        {
            await Init();
            return await Database.Table<AppSettings>().Where(i => i.name == name).FirstOrDefaultAsync();
        }

        public async Task<int> InsertAsync(AppSettings item)
        {
            await Init();
            int result = await Database.InsertOrReplaceAsync(item);
            return 0;
        }

        public async Task<int> InsertBatchAsync(AppSettings[] items)
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
            var result = await Database.CreateTableAsync<AppSettings>();
        }

        public string GetDbPath()
        {
            return Constants.DatabasePath;
        }
    }
}
