using DMSA.Sync.Core.Database.Sqlite;
using System.Diagnostics;

namespace DMSA.Sync.Core.Update
{
    public partial class ServerPuller
    {

        public async Task<bool> OnlineSyncCategoria()
        {
            var stopwatch = Stopwatch.StartNew();

            ApiManager.HubProductCategoria hubmanager = new ApiManager.HubProductCategoria(Constants.Session);
            var resultCount = await hubmanager.GetCount();

            if (resultCount.result == 0)
            {
                return false;
            }

            int countTotal = resultCount.result / limit;

            var database = new ProductCategoriaDb(DbNameSqlite);

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

        public async Task<bool> OnlineSyncSubcategoria()
        {
            var stopwatch = Stopwatch.StartNew();

            ApiManager.HubProductSubcategoria hubmanager = new ApiManager.HubProductSubcategoria(Constants.Session);
            var resultCount = await hubmanager.GetCount();

            if (resultCount.result == 0)
            {
                return false;
            }

            int countTotal = resultCount.result / limit;

            var database = new ProductSubcategoriaDb(DbNameSqlite);

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

        public async Task<bool> OnlineSyncProductLinea()
        {
            var stopwatch = Stopwatch.StartNew();

            ApiManager.HubProductLinea hubmanager = new ApiManager.HubProductLinea(Constants.Session);
            var resultCount = await hubmanager.GetCount();

            if (resultCount.result == 0)
            {
                return false;
            }

            int countTotal = resultCount.result / limit;

            var database = new ProductLineaDb(DbNameSqlite);

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

        public async Task<bool> OnlineSyncProductGrupoTipo()
        {
            var stopwatch = Stopwatch.StartNew();

            ApiManager.HubProductGrupoTipo hubmanager = new ApiManager.HubProductGrupoTipo(Constants.Session);
            var resultCount = await hubmanager.GetCount();

            if (resultCount.result == 0)
            {
                return false;
            }

            int countTotal = resultCount.result / limit;

            var database = new ProductGrupoTipoDb(DbNameSqlite);

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

    }
}
