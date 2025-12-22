using DMSA.Models.Odoo;
using SQLite;
using System.Diagnostics;

namespace DMSA.Sync.Core.Database.Sqlite
{    
    public class AppSettingsDb : SqliteDbBase<AppSettings>
    {
        public AppSettingsDb()
        {

        }

        public AppSettingsDb(string _DatabaseFilename) : base(_DatabaseFilename)
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

        //public async Task<bool> getBoolean(string name)
        //{
        //    await Init();
        //    var dbITem = await Database.Table<AppSettings>().Where(i => i.name == name).FirstOrDefaultAsync();
        //    bool returnValue = false;
        //    try
        //    {
        //        returnValue = bool.Parse(dbITem.value);
        //    }
        //    catch (Exception ex)
        //    {
        //        Debug.WriteLine(ex);
        //    }

        //    return returnValue;
        //}

        public async Task<bool> GetBooleanAsync(string name, bool defaultValue = false)
        {
            await Init();

            var dbItem = await Database.Table<AppSettings>()
                .Where(i => i.name == name)
                .FirstOrDefaultAsync();

            if (dbItem == null || string.IsNullOrWhiteSpace(dbItem.value))
                return defaultValue;

            return bool.TryParse(dbItem.value, out var result)
                ? result
                : defaultValue;
        }

        public async Task SetBooleanAsync(string name, bool value)
        {
            await Init();

            var dbItem = await Database.Table<AppSettings>()
                .Where(i => i.name == name)
                .FirstOrDefaultAsync();

            string strValue = value.ToString().ToLower();

            if (dbItem == null)
            {
                dbItem = new AppSettings
                {
                    name = name,
                    value = strValue
                };

                await Database.InsertAsync(dbItem);
            }
            else
            {
                dbItem.value = strValue;
                await Database.UpdateAsync(dbItem);
            }
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

        public string GetDbPath()
        {
            return Constants.DatabasePath;
        }
    }
}
