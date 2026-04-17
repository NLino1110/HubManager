using ApiManagerOdoo;
using DMSA.Sync.Core.Database.Sqlite;
using System.Diagnostics;

namespace DMSA.Sync.Core.Update
{
    public partial class ServerPuller
    {
        public async Task<bool> GetCities(Func<int, int, Task>? onProgress = null)
        {
            var stopwatch = Stopwatch.StartNew();
            var database = new ResCityDb(Constants.Session.odooConnection.DbNameSqlite);
            DateTime? lastDate = await database.GetLastWriteDateAsync(sync_date_since);

            var hubmanager = new HubResCity(Constants.Session);

            int res_center = Constants.Session.odooConnection.res_center_default;
            
            var resultCount = await hubmanager.GetCount(lastDate.Value);

            if (resultCount.result == 0)
            {
                return false;
            }

            int totalPages = (int)Math.Ceiling((double)resultCount.result / limit);

            for (int indice = 0; indice <= totalPages; indice++)
            {
                Debug.WriteLine("GetCities Página:" + indice + " de " + totalPages);

                var responseAll = await hubmanager.GetItems(lastDate.Value, limit, indice);

                if (responseAll.result != null && responseAll.result.Length > 0)
                {
                    await database.InsertBatchAsync(responseAll.result);
                }

                if (onProgress != null)
                    await onProgress(indice + 1, totalPages);

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
