using DMSA.Sync.Core.Database.Sqlite.Sales;
using System.Diagnostics;

namespace DMSA.Sync.Core.Update
{
    public partial class ServerPuller
    {
        public async Task<bool> OnlineSyncProductPricelist()
        {
            var stopwatch = Stopwatch.StartNew();

            ApiManager.HubProductPricelist hubmanager = new ApiManager.HubProductPricelist(Constants.Session);
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

        public async Task<bool> OnlineSyncProductPricelistItem()
        {
            var stopwatch = Stopwatch.StartNew();

            var database = new ProductPricelistDb(DbNameSqlite);
            DateTime? lastDate = await database.GetLastWriteDateAsync(sync_date_since_lower);

            var hubmanager = new ApiManager.HubProductPricelistItem(Constants.Session);
            var databaseItems = new ProductPricelistItemDb(DbNameSqlite);

            var activePriceLists = await database.GetItemsByStatus(true);

            foreach ( var activePriceList in activePriceLists)
            {
                var resultCount = await hubmanager.GetCount(activePriceList.id);

                if (resultCount.result == 0)
                {
                    continue;
                }

                int countTotal = resultCount.result / limit;

                for (int indice = 0; indice <= countTotal; indice++)
                {
                    Debug.WriteLine("Página:" + indice);

                    var responseAll = await hubmanager.GetByCreateDate(activePriceList.id, limit, indice, year, month, day);

                    if (responseAll.result != null && responseAll.result.Length > 0)
                    {
                        await databaseItems.InsertBatchAsync(responseAll.result);
                    }

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
