using ApiManagerOdoo.Accounting;
using DMSA.Sync.Core.Database.Sqlite;
using DMSA.Sync.Core.Database.Sqlite.Accounting;
using System.Diagnostics;

namespace DMSA.Sync.Core.Update
{
    public partial class ServerPuller
    {
        public async Task OnlineSyncResCenter(bool force)
        {
            var database = new ResCenterDb(Constants.Session.odooConnection.DbNameSqlite);
            if (await database.GetCount() > 0)
            {
                //Ya se ha sincronizado previamente
                return;
            }

            HubResCenter hubManagerInstance = new HubResCenter(Constants.Session);
            var dataList = await hubManagerInstance.GetItems(1000,0,2023,1,1);

            if (dataList != null && dataList.result !=null && dataList.result.Length > 0)
            {
                foreach (var item in dataList.result)
                {
                    item.company_id = 1;
                }                
                await database.InsertBatchAsync(dataList.result);
            }
        }

        public async Task<bool> GetFullResCenterLine(bool force, Func<int, int, Task>? onProgress = null)
        {
            var database = new ResCenterDb(DbNameSqlite);
            var itemsCenter = await database.GetItemsAsync(x=>x.id > 0);

            //obtenemos los ids
            if (itemsCenter == null || itemsCenter.Count == 0)
            {
                return false;
            }

            int[] center_ids = itemsCenter.Select(x => x.id).ToArray();

            await GetResCenterLine(true, center_ids, onProgress);
            await GetDocAuthorizationLine(true, center_ids, onProgress);

            //foreach (var item in itemsCenter)
            //{
            //    await GetResCenterLine(true, item.id, onProgress);
            //    await GetDocAuthorizationLine(true, item.id, onProgress);
            //}

            await GetDocTaxSustento(true, onProgress);

            return true;
        }


        public async Task<bool> GetResCenterLine(bool force, int[] center_ids, Func<int, int, Task>? onProgress = null)
        {
            var stopwatch = Stopwatch.StartNew();

            var database = new ResCenterLineDb(DbNameSqlite);
            DateTime? lastDate = await database.GetLastWriteDateAsync(sync_date_since);

            //DateTime? lastDate = await database.GetSafeLastWriteDateAsync(2);

            var hubmanager = new HubResCenterLine(Constants.Session);

            var resultCount = await hubmanager.GetCount(lastDate.Value, center_ids);

            if (resultCount.result == 0)
            {
                return false;
            }

            int totalPages = (int)Math.Ceiling((double)resultCount.result / limit);

            for (int indice = 0; indice <= totalPages; indice++)
            {
                Debug.WriteLine("GetResCenterLine Página:" + indice);

                var responseAll = await hubmanager.GetItems(lastDate.Value, limit, indice, center_ids);

                if (responseAll?.result != null && responseAll.result.Length > 0)
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


        [Obsolete("Muy lento")]
        public async Task<bool> GetResCenterLine(bool force, int center_id, Func<int, int, Task>? onProgress = null)
        {
            var stopwatch = Stopwatch.StartNew();

            var database = new ResCenterLineDb(DbNameSqlite);
            //DateTime? lastDate = await database.GetLastWriteDateAsync(sync_date_since);

            DateTime? lastDate = await database.GetSafeLastWriteDateAsync(2);

            var hubmanager = new HubResCenterLine(Constants.Session);
            
            var resultCount = await hubmanager.GetCount(lastDate.Value, center_id);

            if (resultCount.result == 0)
            {
                return false;
            }

            int totalPages = (int)Math.Ceiling((double)resultCount.result / limit);

            for (int indice = 0; indice <= totalPages; indice++)
            {
                Debug.WriteLine("GetResCenterLine Página:" + indice);

                var responseAll = await hubmanager.GetItems(lastDate.Value, limit, indice, center_id);

                if (responseAll?.result != null && responseAll.result.Length > 0)
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

        public async Task<bool> GetDocAuthorizationLine(bool force, int[] center_ids, Func<int, int, Task>? onProgress = null)
        {
            var stopwatch = Stopwatch.StartNew();

            var database = new DocAuthorizationLineDb(DbNameSqlite);
            DateTime? lastDate = await database.GetLastWriteDateAsync(sync_date_since);
            //DateTime? lastDate = await database.GetSafeLastWriteDateAsync(2);
            //DateTime? lastDate = new DateTime(2005, 8, 5);

            var hubmanager = new HubDocAuthorizationLine(Constants.Session);

            var resultCount = await hubmanager.GetCount(lastDate.Value, center_ids);

            if (resultCount.result == 0)
            {
                return false;
            }

            int totalPages = (int)Math.Ceiling((double)resultCount.result / limit);

            for (int indice = 0; indice <= totalPages; indice++)
            {
                Debug.WriteLine("GetDocAuthorizationLine Página:" + indice);

                var responseAll = await hubmanager.GetItems(lastDate.Value, limit, indice, center_ids);

                if (responseAll?.result != null && responseAll.result.Length > 0)
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


        [Obsolete("Muy Lento")]
        public async Task<bool> GetDocAuthorizationLine(bool force, int center_id, Func<int, int, Task>? onProgress = null)
        {
            var stopwatch = Stopwatch.StartNew();

            var database = new DocAuthorizationLineDb(DbNameSqlite);
            //DateTime? lastDate = await database.GetLastWriteDateAsync(sync_date_since);
            //DateTime? lastDate = await database.GetSafeLastWriteDateAsync(2);
            DateTime? lastDate = new DateTime(2005, 8, 5);

            var hubmanager = new HubDocAuthorizationLine(Constants.Session);

            var resultCount = await hubmanager.GetCount(lastDate.Value, center_id);

            if (resultCount.result == 0)
            {
                return false;
            }

            int totalPages = (int)Math.Ceiling((double)resultCount.result / limit);

            for (int indice = 0; indice <= totalPages; indice++)
            {
                Debug.WriteLine("GetDocAuthorizationLine Página:" + indice);

                var responseAll = await hubmanager.GetItems(lastDate.Value, limit, indice, center_id);

                if (responseAll?.result != null && responseAll.result.Length > 0)
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

        public async Task<bool> GetDocTaxSustento(bool force, Func<int, int, Task>? onProgress = null)
        {
            string tax_code = "01";
            var stopwatch = Stopwatch.StartNew();

            var database = new DocTaxSustentDb(DbNameSqlite);
            DateTime? lastDate = await database.GetLastWriteDateAsync(sync_date_since);

            var hubmanager = new HubDocTaxSustent(Constants.Session);

            var resultCount = await hubmanager.GetCount(lastDate.Value, tax_code);

            if (resultCount.result == 0)
            {
                return false;
            }

            int totalPages = (int)Math.Ceiling((double)resultCount.result / limit);

            for (int indice = 0; indice <= totalPages; indice++)
            {
                Debug.WriteLine("GetDocTaxSustento Página:" + indice);

                var responseAll = await hubmanager.GetItems(lastDate.Value, limit, indice, tax_code);

                if (responseAll?.result != null && responseAll.result.Length > 0)
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
