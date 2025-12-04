using DMSA.Models.Odoo.Abstract;
using SQLite;
using System.ComponentModel.DataAnnotations;

namespace DMOrders.Services.Database.Sqlite
{
    public class OdooConnectionDb
    {
        SQLiteAsyncConnection Database;
        
        public OdooConnectionDb()
        {

        }

        public async Task<int> InitDefault()
        {
            await Init();
            OdooConnection appSettings = new OdooConnection();
            
            foreach(var itemSetting in appSettings.LoadDefault())
            {
                //var foundItem = await GetItem(itemSetting.Name);
                //if (foundItem == null)
                //{
                //    await Database.InsertAsync(itemSetting);
                //}
                var foundItem = await GetItemById(itemSetting.Id);
                if(foundItem == null)
                    await Database.InsertOrReplaceAsync(itemSetting);
            }

            return 0;
        }

        public async Task<int> TruncateAsync()
        {
            await Init();
            return await Database.DeleteAllAsync<OdooConnection>();
        }

        public async Task<int> GetCount()
        {
            return (await Database.Table<OdooConnection>().ToListAsync()).Count;
        }

        public async Task<List<OdooConnection>> GetItemsAsync()
        {
            await Init();
            return await Database.Table<OdooConnection>().ToListAsync();            
        }

        public async Task<OdooConnection> GetItemById(int id)
        {
            await Init();
            return await Database.Table<OdooConnection>().Where(i => i.Id == id).FirstOrDefaultAsync();
        }

        public async Task<OdooConnection> GetItem(string name)
        {
            await Init();
            return await Database.Table<OdooConnection>().Where(i => i.Name == name).FirstOrDefaultAsync();
        }

        public async Task<int> InsertAsync(OdooConnection item)
        {
            await Init();
            int result = await Database.InsertOrReplaceAsync(item);
            return 0;
        }

        public async Task<int> UpdateAsync(OdooConnection item)
        {
            await Init();
            int result = await Database.UpdateAsync(item);
            return 0;
        }

        public async Task<int> InsertBatchAsync(OdooConnection[] items)
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
            var result = await Database.CreateTableAsync<OdooConnection>();
        }

        public string GetDbPath()
        {
            return Constants.DatabasePath;
        }
    }
}
