using DMSA.Models.Odoo.Abstract;

namespace DMSA.Sync.Core.Database.Sqlite
{
    public class GlobalSettingsDb : SqliteDbBase<GlobalSettings>
    {
        public GlobalSettingsDb()
        {

        }

        public GlobalSettingsDb(string _DatabaseFilename) : base(_DatabaseFilename)
        {

        }

        public async Task<int> InitDefault()
        {
            await Init();
            
            GlobalSettings globalSettings = new GlobalSettings();
            globalSettings.Id = 1;
            globalSettings.PushServer = "https://manager.dmujeres.ec:5001/chatHub";
            globalSettings.PackageServer = "https://manager.dmujeres.ec:5001";

            var foundItem = await GetItemById(globalSettings.Id);

            if (foundItem == null)
            {
                await Database.InsertOrReplaceAsync(globalSettings);
            }            
                
            return 0;
        }

        public async Task<List<GlobalSettings>> GetItemsAsync()
        {
            await Init();
            return await Database.Table<GlobalSettings>().ToListAsync();            
        }

        public async Task<GlobalSettings> GetItemById(int id)
        {
            await Init();
            return await Database.Table<GlobalSettings>().Where(i => i.Id == id).FirstOrDefaultAsync();
        }

    }
}
