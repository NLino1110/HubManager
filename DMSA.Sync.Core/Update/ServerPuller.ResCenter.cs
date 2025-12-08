using ApiManager;
using DMSA.Sync.Core.Database.Sqlite;

namespace DMSA.Sync.Core.Update
{
    public partial class ServerPuller
    {
        public async Task OnlineSyncResCenter(bool force)
        {
            var database = new ResCenterDb(Constants.Session.odooConnection.DbNameSqlite);
            if (await database.GetCount() > 0)
            {
                //Ya se ha sincronizado previamente
                return;
            }

            ApiManager.HubResCenter hubManagerInstance = new HubResCenter(Constants.Session);
            var dataList = await hubManagerInstance.GetItems(1000,0,2023,1,1);

            if (dataList != null && dataList.result !=null && dataList.result.Length > 0)
            {
                foreach (var item in dataList.result)
                {
                    item.company_id = 1;
                }                
                await database.InsertBatchAsync(dataList.result);
            }
        }

    }
}
