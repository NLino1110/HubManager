using ApiManager;
using DMSA.Models.Odoo.General.Responses;
using DMSA.Models.Odoo.Native;
using DMSA.Sync.Core.Database.Sqlite;
using System.Diagnostics;

namespace DMSA.Sync.Core.Update
{
    public partial class ServerPuller
    {
        public async Task<bool> OnlineSyncStockLocation()
        {
            //var database = new StockLocationDb(Constants.Session.odooConnection.DbNameSqlite);
            //DateTime? lastDate = await database.GetLastWriteDateAsync(sync_date_since);
            //ApiManager.HubStockLocation hubManagerInstance = new HubStockLocation(Constants.Session);
            //int res_center = Constants.Session.odooConnection.res_center_default;


            //ApiResponseOdooRpcT<stock_location[]> dataList = await hubManagerInstance.GetByCreateDate(limit, 0, year, month, day);

            //var databaseWhs = new StockWareHouseDb(Constants.Session.odooConnection.DbNameSqlite);
            //var whsList = await databaseWhs.GetByResCenter(res_center);
            //int[] whsIds = whsList.Select(w => w.id).ToArray();

            //if (dataList != null && dataList.result !=null && dataList.result.Length > 0)
            //{
                
            //    await database.InsertBatchAsync(dataList.result);
            //}





            var stopwatch = Stopwatch.StartNew();
            var database = new StockLocationDb(Constants.Session.odooConnection.DbNameSqlite);
            DateTime? lastDate = await database.GetLastWriteDateAsync(sync_date_since);

            ApiManager.HubStockLocation hubmanager = new ApiManager.HubStockLocation(Constants.Session);

            int res_center = Constants.Session.odooConnection.res_center_default;
            //Obtenermos los warehouses asociados al centro de operaciones
            var databaseWhs = new StockWareHouseDb(Constants.Session.odooConnection.DbNameSqlite);
            var whsList = await databaseWhs.GetByResCenter(res_center);
            int[] whsIds = whsList.Select(w => w.id).ToArray();

            var resultCount = await hubmanager.GetCount(whsIds, lastDate.Value);

            if (resultCount.result == 0)
            {
                return false;
            }

            int countTotal = resultCount.result / 300;

            for (int indice = 0; indice <= countTotal; indice++)
            {
                Debug.WriteLine("Página:" + indice);

                var responseAll = await hubmanager.GetByWriteDate(whsIds, limit, indice, year, month, day);

                if (responseAll.result != null && responseAll.result.Length > 0)
                {
                    await database.InsertBatchAsync(responseAll.result);
                }

                if (indice >= maxIndexExceeded)
                {
                    Debug.WriteLine("Página " + indice + ": Se terminará el proceso.");
                    break;
                }
            }

            stopwatch.Stop();

            Debug.WriteLine(String.Format("Lapso transcurrido: {0} days, {1} hours, {2} minutes, {3} seconds",
                stopwatch.Elapsed.Days, stopwatch.Elapsed.Hours, stopwatch.Elapsed.Minutes, stopwatch.Elapsed.Seconds));

            return true;


        }

    }
}
