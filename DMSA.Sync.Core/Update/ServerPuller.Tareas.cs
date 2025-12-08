using DMSA.Sync.Core.Database.Sqlite.tareas;
using System.Diagnostics;

namespace DMSA.Sync.Core.Update
{
    public partial class ServerPuller
    {
        public async Task<bool> MotivoActividadDiaria(bool force)
        {            
            DateTime current_datetime = DateTime.Now.AddYears(-Constants.Session.odooConnection.DataToleranceDays);

            var stopwatch = Stopwatch.StartNew();

            ApiManager.HubMotivoActividadDiaria hubmanager = new ApiManager.HubMotivoActividadDiaria(Constants.Session);
            var resultCount = await hubmanager.GetCount(current_datetime.Year, current_datetime.Month, current_datetime.Day);

            if (resultCount.result == 0)
            {
                return false;
            }

            int countTotal = resultCount.result / Constants.Session.odooConnection.DbLimitDefault;

            var database = new MotivoActividadDiariaDb(Constants.Session.odooConnection.DbNameSqlite);

            for (int indice = 0; indice <= countTotal; indice++)
            {
                Debug.WriteLine("Página:" + indice);

                var responseAll = await hubmanager.GetItems(current_datetime, limit, indice);

                if (responseAll.result != null && responseAll.result.Length > 0)
                {
                    await database.InsertBatchAsync(responseAll.result);
                }

                if (indice >= 600)
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
