using ApiManager.Odoo.Inventory;
using DMSA.Sync.Core.Database.Sqlite;
using System.Diagnostics;

namespace DMSA.Sync.Core.Update
{
    public partial class ServerPuller
    {
        public async Task<bool> OnlineSyncWmsStockQuant(Func<int, int, Task>? onProgress = null)
        {            
            var stopwatch = Stopwatch.StartNew();
            var database = new WmsStockQuantDb(Constants.Session.odooConnection.DbNameSqlite);
            DateTime? lastDate = await database.GetLastWriteDateAsync(sync_date_since);

            var hubmanager = new HubWmsStockQuant(Constants.Session);
            
            int res_center = Constants.Session.odooConnection.res_center_default;
            //Obtenermos los warehouses asociados al centro de operaciones
            var databaseWhs = new StockWareHouseDb(Constants.Session.odooConnection.DbNameSqlite);
            var whsList = await databaseWhs.GetDefaultByResCenter(res_center);
            int[] whsIds = whsList.Select(w => w.id).ToArray();

            var resultCount = await hubmanager.GetCount(whsIds, lastDate.Value);

            if (resultCount.result == 0)
            {
                return false;
            }

            int countTotal = resultCount.result / 300;            

            for (int indice = 0; indice <= countTotal; indice++)
            {
                Debug.WriteLine("Página:" + indice + " de " + countTotal);

                var responseAll = await hubmanager.GetByWriteDate(whsIds, limit, indice, year, month, day);

                if (responseAll.result != null && responseAll.result.Length > 0)
                {
                    await database.InsertBatchAsync(responseAll.result);
                }

                if (onProgress != null)
                    await onProgress(indice, countTotal);

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

        public async Task<bool> UpdateWmsStockQuant(Func<int, int, Task>? onProgress = null)
        {
            var stopwatch = Stopwatch.StartNew();
            var wms_database = new WmsStockQuantDb(Constants.Session.odooConnection.DbNameSqlite);
            //int res_center = Constants.Session.odooConnection.res_center_default;
            //var databaseWhs = new StockWareHouseDb(Constants.Session.odooConnection.DbNameSqlite);
            //var whsList = await databaseWhs.GetDefaultByResCenter(res_center);
            //int[] whsIds = whsList.Select(w => w.id).ToArray();

            int[] whsIds = (await wms_database.GetItemsAsync(w => w.id > 0))
                    .Select(w => w._warehouse_id)
                    .Distinct()
                    .ToArray();

            await wms_database.UpdateCantidadDisponibleAsync(whsIds);

            stopwatch.Stop();

            Debug.WriteLine(String.Format("Lapso transcurrido: {0} days, {1} hours, {2} minutes, {3} seconds",
                stopwatch.Elapsed.Days, stopwatch.Elapsed.Hours, stopwatch.Elapsed.Minutes, stopwatch.Elapsed.Seconds));

            return true;
        }
    }
}
