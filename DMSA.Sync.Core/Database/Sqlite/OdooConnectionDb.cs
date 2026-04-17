using DMSA.Models.Odoo.Abstract;
using DMSA.Models.Odoo.Native;
using SQLite;
using System.Diagnostics;

namespace DMSA.Sync.Core.Database.Sqlite
{
    public class OdooConnectionDb : SqliteDbBase<OdooConnection>
    {
        public OdooConnectionDb()
        {

        }

        public OdooConnectionDb(string _DatabaseFilename) : base(_DatabaseFilename)
        {

        }

        public async Task<int> InitDefault()
        {
            await Init();
            OdooConnection appSettings = new OdooConnection();
            
            foreach(var itemSetting in appSettings.LoadDefault())
            {
                try
                {
                    var foundItem = await GetItemById(itemSetting.Id);
                    if (foundItem == null)
                        await Database.InsertOrReplaceAsync(itemSetting);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"OdooConnectionDb.InitDefault Exception: {ex.Message}");
                }
            }

            return 0;
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

        //public async Task<string> CompressDatabaseAsync(string dbPath)
        //{
        //    //await Init();
        //    string zipPath = dbPath + ".zip";
        //    if (File.Exists(zipPath))
        //    {
        //        File.Delete(zipPath);
        //    }
        //    System.IO.Compression.ZipFile.CreateFromDirectory(Path.GetDirectoryName(dbPath), zipPath, System.IO.Compression.CompressionLevel.Fastest, false);
        //    return zipPath;
        //}
    }
}
