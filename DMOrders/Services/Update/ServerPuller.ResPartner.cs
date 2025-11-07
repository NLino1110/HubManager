using ApiManager;
using DMOrders.Controls;
using DMOrders.Services.Database.Sqlite;
using DMSA.Models.General.Requests;
using DMSA.Models.Odoo.General.Responses;
using DMSA.Models.Odoo.Native;
using DMSA.Models.Odoo.Origin;
using Newtonsoft.Json;
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
        public async Task<bool> OnlineSyncResPartner()
        {
            //int limit = 20; // limit / 3;

            var stopwatch = Stopwatch.StartNew();

            var database = new ResPartnerDb(App.Session.odooConnection.DbNameSqlite);

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
