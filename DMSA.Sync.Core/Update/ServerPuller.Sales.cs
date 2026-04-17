using ApiManager;
using DMSA.Sync.Core.Database.Sqlite.Sales;
using System.Diagnostics;

namespace DMSA.Sync.Core.Update
{
    public partial class ServerPuller
    {
        public async Task<bool> OnlineSyncProductPricelist(Func<int, int, Task>? onProgress = null)
        {
            var stopwatch = Stopwatch.StartNew();

            HubProductPricelist hubmanager = new HubProductPricelist(Constants.Session);
            var resultCount = await hubmanager.GetCount();

            if (resultCount.result == 0)
            {
                return false;
            }

            int countTotal = resultCount.result / limit;

            var database = new ProductPricelistDb(DbNameSqlite);

            for (int indice = 0; indice <= countTotal; indice++)
            {
                Debug.WriteLine("Página:" + indice);

                var responseAll = await hubmanager.GetByCreateDate(limit, indice, year, month, day);

                if (responseAll.result != null && responseAll.result.Length > 0)
                {
                    await database.InsertBatchAsync(responseAll.result);
                }

                if (onProgress != null)
                    await onProgress(indice + 1, countTotal);

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

        public async Task<bool> OnlineSyncProductPricelistItem(Func<int, int, Task>? onProgress = null)
        {
            var stopwatch = Stopwatch.StartNew();

            var hubmanager = new HubProductPricelistItem(Constants.Session);            
            var databaseItems = new ProductPricelistItemDb(DbNameSqlite);
            DateTime? lastDate = await databaseItems.GetLastWriteDateAsync(sync_date_since_lower);
            var database = new ProductPricelistDb(DbNameSqlite);

            var activePriceLists = await database.GetItemsAsync(x=>x.active == true && x.use_mobile_app == true);

            foreach ( var activePriceList in activePriceLists)
            {
                var resultCount = await hubmanager.GetCount(activePriceList.id,
                    lastDate.Value.Year,
                    lastDate.Value.Month,
                    lastDate.Value.Day);

                if (resultCount.result == 0)
                {
                    continue;
                }

                int countTotal = resultCount.result / limit;

                for (int indice = 0; indice <= countTotal; indice++)
                {
                    Debug.WriteLine("ProductPricelistItem Página:" + indice + " de " + countTotal);

                    var responseAll = await hubmanager.GetByWriteDate(activePriceList.id, limit, indice, year, month, day);

                    if (responseAll.result != null && responseAll.result.Length > 0)
                    {
                        await databaseItems.InsertBatchAsync(responseAll.result);
                    }

                    if (onProgress != null)
                        await onProgress(indice, countTotal);

                    if (indice >= maxIndexExceeded)
                    {
                        Debug.WriteLine("Página " + indice + ": Se terminará el proceso.");
                        break;
                    }
                }
            }

            stopwatch.Stop();

            Debug.WriteLine(String.Format("Lapso transcurrido: {0} days, {1} hours, {2} minutes, {3} seconds",
                stopwatch.Elapsed.Days, stopwatch.Elapsed.Hours, stopwatch.Elapsed.Minutes, stopwatch.Elapsed.Seconds));

            return true;
        }
    }
}
