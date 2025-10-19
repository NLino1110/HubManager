using ApiManager;
using DMOrders.Services.Database.Sqlite;
using DMSA.Models.General.Requests;
using DMSA.Models.Odoo.DMOrders.promotions;
using DMSA.Models.Odoo.General.Responses;
using DMSA.Models.Odoo.Native;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMOrders.Services.Update
{
    public partial class ServerPuller
    {
        public async Task<bool> OnlineMotivoActividadDiaria(bool force)
        {            
            DateTime current_datetime = DateTime.Now.AddYears(-App.Session.odooConnection.DataToleranceDays);

            var stopwatch = Stopwatch.StartNew();

            ApiManager.HubMotivoActividadDiaria hubmanager = new ApiManager.HubMotivoActividadDiaria(App.Session);
            var resultCount = await hubmanager.GetCount(current_datetime.Year, current_datetime.Month, current_datetime.Day);

            if (resultCount.result == 0)
            {
                return false;
            }

            int countTotal = resultCount.result / App.Session.odooConnection.DbLimitDefault;

            var database = new MotivoActividadDiariaDb();

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
