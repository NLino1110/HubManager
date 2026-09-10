using ApiManagerOdoo.Inventory;
using DMSA.Sync.Core.Database.Sqlite;
using System.Diagnostics;

namespace DMSA.Sync.Core.Update
{
    public partial class ServerPuller
    {
        public async Task<bool> OnlineSyncStockQuant()
        {            
            var stopwatch = Stopwatch.StartNew();
            var database = new StockQuantDb(Constants.Session.odooConnection.DbNameSqlite);
            DateTime? lastDate = await database.GetLastWriteDateAsync(sync_date_since);

            HubStockQuant hubmanager = new HubStockQuant(Constants.Session);
            
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
                Debug.WriteLine("StockQuant Página:" + indice + " de " + countTotal);

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

        public async Task<bool> UomUom(bool force)
        {
            var stopwatch = Stopwatch.StartNew();

            ApiManager.HubUomUom hubmanager = new ApiManager.HubUomUom(Constants.Session);
            var resultCount = await hubmanager.GetCount(year, month, day);

            if (resultCount.result == 0)
            {
                return false;
            }

            int countTotal = resultCount.result / 300;

            var database = new UomUomDb(Constants.Session.odooConnection.DbNameSqlite);

            for (int indice = 0; indice <= countTotal; indice++)
            {
                Debug.WriteLine("Página:" + indice);

                var responseAll = await hubmanager.GetItems(sync_date_since.Value, limit, indice);

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

        public async Task<bool> OnlineSyncUomUom(Func<int, int, Task>? onProgress = null)
        {
            var stopwatch = Stopwatch.StartNew();

            var hubmanager = new ApiManager.HubUomUom(Constants.Session);
            var resultCount = await hubmanager.GetCountAll();

            Debug.WriteLine("OnlineSyncUomUom count: " + resultCount.result);

            if (resultCount.result == 0)
            {
                return false;
            }

            int totalPages = (int)Math.Ceiling((double)resultCount.result / limit);
            var database = new UomUomDb(Constants.Session.odooConnection.DbNameSqlite);
            await database.Truncate();

            for (int indice = 0; indice < totalPages; indice++)
            {
                var responseAll = await hubmanager.GetAll(limit, indice);

                if (responseAll?.result != null && responseAll.result.Length > 0)
                {
                    await database.InsertBatchAsync(responseAll.result);
                }

                Debug.WriteLine("UomUom Página:" + (indice + 1) + " de " + totalPages);

                if (onProgress != null)
                    await onProgress(indice + 1, totalPages);

                if (indice >= maxIndexExceeded)
                {
                    Debug.WriteLine("Página " + indice + ": Se terminará el proceso UomUom.");
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
