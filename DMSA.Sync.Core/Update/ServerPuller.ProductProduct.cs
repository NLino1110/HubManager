using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Markup;
using DMSA.Sync.Core.Database.Sqlite;
using DMSA.Sync.Core.Update.Cloud;
using System.Diagnostics;

namespace DMSA.Sync.Core.Update
{
    public partial class ServerPuller
    {
        [Obsolete("Advertencia, no usar este metodo de forma arbitraria, es muy pesado, mejor separar las responsabilidades")]
        public async Task<bool> OnlineSyncProductProduct()
        {
            var database = new ProductProductDb(Constants.Session.odooConnection.DbNameSqlite);
            var stopwatch = Stopwatch.StartNew();

            DateTime dateEnd = DateTime.Now;

            ApiManager.HubProductProduct hubmanager = new ApiManager.HubProductProduct(appSession);
            DateTime? lastDate = await database.GetLastWriteDateAsync(sync_date_since_lower);
            var resultCount = await hubmanager.GetCount(lastDate);

            Debug.WriteLine(resultCount.result);

            if (resultCount.result == 0)
            {
                return false;
            }

            int countTotal = resultCount.result / 300;            

            for (int indice = 0; indice <= countTotal; indice++)
            {
                var responseAll = await hubmanager.GetByWriteDate(limit, indice, lastDate.Value);

                if (responseAll != null && responseAll.result != null && responseAll.result.Length > 0)
                {
                    await database.InsertBatchAsync(responseAll.result);
                }

                Console.WriteLine("Página:" + indice);

                //TODO: Se fuerza la salida para que no se quede ciclado en caso de que haya
                // problemas de conexion con el servidor
                // el objetivo es que el servidor no se sobrecargue

                if (indice >= maxIndexExceeded)
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

        public async Task<bool> OnlineSyncProductProductNoImage(Func<int, int, Task>? onProgress = null)
        {
            var database = new ProductProductDb(Constants.Session.odooConnection.DbNameSqlite);
            var stopwatch = Stopwatch.StartNew();

            DateTime dateEnd = DateTime.Now;

            ApiManager.HubProductProduct hubmanager = new ApiManager.HubProductProduct(appSession);
            DateTime? lastDate = await database.GetLastWriteDateAsync(sync_date_since_lower);
            var resultCount = await hubmanager.GetCount(lastDate);

            Debug.WriteLine(resultCount.result);

            if (resultCount.result == 0)
            {
                return false;
            }

            //int countTotal = resultCount.result / 300;
            int totalPages = (int)Math.Ceiling((double)resultCount.result / limit);

            for (int indice = 0; indice <= totalPages; indice++)
            {
                var responseAll = await hubmanager.GetByWriteDateNoImage(limit, indice, lastDate.Value);

                if (responseAll != null && responseAll.result != null && responseAll.result.Length > 0)
                {
                    await database.InsertBatchAsync(responseAll.result);
                }

                Debug.WriteLine("ProductPricelistItemNoImage Página:" + indice);

                if (onProgress != null)
                    await onProgress(indice, totalPages);

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

        public async Task<bool> OnlineSyncProductProductOnlyImages(bool fullUpdate)
        {
            var database = new ProductProductDb(Constants.Session.odooConnection.DbNameSqlite);            
            var stopwatch = Stopwatch.StartNew();

            DateTime dateEnd = DateTime.Now;

            ApiManager.HubProductProduct hubmanager = new ApiManager.HubProductProduct(appSession);
            DateTime? lastDate = await database.GetLastWriteDateAsync(sync_date_since_lower);

            if(fullUpdate)
            {
                lastDate = new DateTime(2024, 1, 1);
            }

            var resultCount = await hubmanager.GetCount(lastDate);

            Debug.WriteLine(resultCount.result);

            if (resultCount.result == 0)
            {
                return false;
            }

            int countTotal = resultCount.result / 300;

            for (int indice = 0; indice <= countTotal; indice++)
            {
                var responseAll = await hubmanager.GetByWriteOnlyImage(limit, indice, lastDate.Value);

                if (responseAll != null && responseAll.result != null && responseAll.result.Length > 0)
                {
                    await database.UpdateImagesBatchAsync(responseAll.result.ToList());                    
                }

                Console.WriteLine("ProductOnlyImages Página:" + indice + " de " + countTotal);

                if (indice >= maxIndexExceeded)
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

        public async Task<bool> OnlineSyncProductProductOnlyImagesV2(bool fullUpdate, Func<int, int, Task>? onProgress = null)
        {
            var databaseImages = new ProductProductPreviewDb(Constants.Session.odooConnection.DbNameSqliteStatic);
            if (fullUpdate)
            {
                await databaseImages.DeleteAllAsync(x=> x.id > 0);
                //databaseImages = new ProductProductPreviewDb(Constants.Session.odooConnection.DbNameSqlite);
            }

            var stopwatch = Stopwatch.StartNew();

            DateTime dateEnd = DateTime.Now;

            ApiManager.HubProductProduct hubmanager = new ApiManager.HubProductProduct(appSession);
            DateTime? lastDate = await databaseImages.GetLastWriteDateAsync(sync_date_since_lower);

            //if (fullUpdate)
            //{
                //lastDate = new DateTime(2024, 1, 1);
            //}

            var resultCount = await hubmanager.GetCountOnlyImage(lastDate);

            Debug.WriteLine(resultCount.result);

            if (resultCount.result == 0)
            {
                return false;
            }

            //int countTotal = resultCount.result / 300;
            int totalPages = (int)Math.Ceiling((double)resultCount.result / limit);

            for (int indice = 0; indice <= totalPages; indice++)
            {
                var responseAll = await hubmanager.GetByWriteOnlyImageV2(limit, indice, lastDate.Value);

                if (responseAll != null && responseAll.result != null && responseAll.result.Length > 0)
                {                    
                    await databaseImages.InsertBatchAsync(responseAll.result);
                }

                Debug.WriteLine("ProductOnlyImagesV2 Página:" + indice +  " de " + totalPages);

                if (onProgress != null)
                    await onProgress(indice, totalPages);

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

        public async Task<bool> ProductMarca(Func<int, int, Task>? onProgress = null)
        {
            var stopwatch = Stopwatch.StartNew();

            var database = new ProductMarcaDb(Constants.Session.odooConnection.DbNameSqlite);
            DateTime? lastDate = await database.GetLastWriteDateAsync(sync_date_since);

            var hubmanager = new ApiManager.HubProductMarca(appSession);
            var resultCount = await hubmanager.GetCount(lastDate.Value.Year, lastDate.Value.Month, lastDate.Value.Day);

            if (resultCount.result == 0)
            {
                return false;
            }

            int countTotal = resultCount.result / 300;
            
            for (int indice = 0; indice <= countTotal; indice++)
            {
                var responseAll = await hubmanager.GetItems(lastDate.Value, limit, indice);

                if (responseAll != null && responseAll.result != null && responseAll.result.Length > 0)
                {
                    await database.InsertBatchAsync(responseAll.result);
                }

                Console.WriteLine("ProductMarca Página:" + indice);

                if (onProgress != null)
                    await onProgress(indice, countTotal);

                if (indice >= maxIndexExceeded)
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
