using DMOrders.Services.Database.Sqlite;
using System.Diagnostics;

namespace DMOrders.Services.Update
{
    public partial class ServerPuller
    {
        public async Task<bool> OnlineSyncResPartner()
        {
            //int limit = 20; // limit / 3;

            var stopwatch = Stopwatch.StartNew();

            var database = new ResPartnerDb(DbNameSqlite);

            DateTime? lastDate = await database.GetLastWriteDateAsync(sync_date_since);
            
            ApiManager.HubResPartner hubmanager = new ApiManager.HubResPartner(appSession);
            var resultCount = await hubmanager.GetCount(lastDate.Value.Year, lastDate.Value.Month, lastDate.Value.Day);

            Debug.WriteLine(resultCount.result);

            if (resultCount.result == 0)
            {
                return false;
            }

            int countTotal = resultCount.result / limit;
            
            for (int indice = 0; indice <= countTotal; indice++)
            {
                var responseAll = await hubmanager.GetByWriteDate(lastDate.Value, limit, indice);

                if (responseAll != null && responseAll.result != null && responseAll.result.Length > 0)
                {
                    await database.InsertBatchAsync(responseAll.result);
                    //await database.InsertBatchControlAsync(responseAll.result);
                }

                Console.WriteLine("Página:" + indice);

                if (indice >= 600)
                {
                    Console.WriteLine("Página " + indice + ": Se terminará el proceso.");
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
