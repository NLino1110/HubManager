using ApiManager;
using DMSA.Models.Odoo.General.Responses;
using DMSA.Models.Odoo.Native;
using DMSA.Sync.Core.Database.Sqlite;

namespace DMSA.Sync.Core.Update
{
    public partial class ServerPuller
    {
        public async Task OnlineSyncStockWarehouse(bool force)
        {
            var database = new StockWareHouseDb(Constants.Session.odooConnection.DbNameSqlite);
            if (await database.GetCount() > 0)
            {
                //Ya se ha sincronizado previamente
                return;
            }

            ApiManager.HubStockWareHouse hubManagerInstance = new HubStockWareHouse(Constants.Session);
            ApiResponseOdooRpcT<stock_warehouse[]> dataList = await hubManagerInstance.GetByCreateDate(limit, 0, year, month, day);

            if (dataList != null && dataList.result !=null && dataList.result.Length > 0)
            {                
                await database.InsertBatchAsync(dataList.result);
            }
        }
    }
}
