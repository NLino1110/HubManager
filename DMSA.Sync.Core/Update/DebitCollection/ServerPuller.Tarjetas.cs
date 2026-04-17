using ApiManager;
using ApiManager.Odoo.Inventory;
using ApiManagerOdoo.promotions;
using DMSA.Sync.Core.Database.Sqlite;
using DMSA.Sync.Core.Database.Sqlite.DebitCollection;
using System.Diagnostics;

namespace DMSA.Sync.Core.Update
{
    public partial class ServerPuller
    {
        public async Task<bool> GetTarjetas(Func<int, int, Task>? onProgress = null)
        {            
            var stopwatch = Stopwatch.StartNew();
            var database = new TarjetasCreditoDb(Constants.Session.odooConnection.DbNameSqlite);
            DateTime? lastDate = await database.GetLastWriteDateAsync(sync_date_since);

            var hubmanager = new HubTarjetasCredito(Constants.Session);            
            int res_center = Constants.Session.odooConnection.res_center_default;
            int user_id = Constants.Session.CurrentUserFront.uid;

            var resultCount = await hubmanager.GetCount(lastDate.Value);

            if (resultCount.result == 0)
            {
                return false;
            }

            int totalPages = (int)Math.Ceiling((double)resultCount.result / limit);

            for (int indice = 0; indice <= totalPages; indice++)
            {
                Debug.WriteLine("GetTarjetas Página:" + indice + " de " + totalPages);

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

        public async Task<bool> GetTarjetasTipoPago(Func<int, int, Task>? onProgress = null)
        {
            var stopwatch = Stopwatch.StartNew();
            var database = new TarjetasTipoPagoDb(Constants.Session.odooConnection.DbNameSqlite);
            DateTime? lastDate = await database.GetLastWriteDateAsync(sync_date_since);

            var hubmanager = new HubTarjetasTipoPago(Constants.Session);
            int res_center = Constants.Session.odooConnection.res_center_default;
            int user_id = Constants.Session.CurrentUserFront.uid;

            var resultCount = await hubmanager.GetCount(lastDate.Value);

            if (resultCount.result == 0)
            {
                return false;
            }

            int totalPages = (int)Math.Ceiling((double)resultCount.result / limit);

            for (int indice = 0; indice <= totalPages; indice++)
            {
                Debug.WriteLine("GetTarjetasTipoPago Página:" + indice + " de " + totalPages);

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

        public async Task<bool> GetTarjetasPlazosBanco(Func<int, int, Task>? onProgress = null)
        {
            var stopwatch = Stopwatch.StartNew();
            var database = new TarjetasPlazosBancoDb(Constants.Session.odooConnection.DbNameSqlite);
            DateTime? lastDate = await database.GetLastWriteDateAsync(sync_date_since);

            var hubmanager = new HubTarjetasPlazosBanco(Constants.Session);
            int res_center = Constants.Session.odooConnection.res_center_default;
            int user_id = Constants.Session.CurrentUserFront.uid;

            var resultCount = await hubmanager.GetCount(lastDate.Value);

            if (resultCount.result == 0)
            {
                return false;
            }

            int totalPages = (int)Math.Ceiling((double)resultCount.result / limit);

            for (int indice = 0; indice <= totalPages; indice++)
            {
                Debug.WriteLine("GetTarjetasPlazosBanco Página:" + indice + " de " + totalPages);

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
