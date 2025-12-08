using ApiManager;
using DMSA.Models.Odoo.General.Responses;
using DMSA.Models.Odoo.Native;
using DMSA.Sync.Core.Database.Sqlite;

namespace DMSA.Sync.Core.Update
{
    public partial class ServerPuller
    {
        public async Task OnlineSyncStockLocation()
        {
            ApiManager.HubStockLocation hubManagerInstance = new HubStockLocation(Constants.Session);
            ApiResponseOdooRpcT<stock_location[]> dataList = await hubManagerInstance.GetByCreateDate(limit, 0, year, month, day);

            if (dataList != null && dataList.result !=null && dataList.result.Length > 0)
            {
                var database = new StockLocationDb(Constants.Session.odooConnection.DbNameSqlite);
                await database.InsertBatchAsync(dataList.result);
            }
        }

    }
}
