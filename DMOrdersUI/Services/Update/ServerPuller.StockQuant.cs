using ApiManager;
using DMOrdersUI.Services.Database.Sqlite;
using DMSA.Models.General.Requests;
using DMSA.Models.Odoo.General.Responses;
using DMSA.Models.Odoo.Native;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMOrdersUI.Services.Update
{
    public partial class ServerPuller
    {
        public async Task<bool> OnlineSyncStockQuant()
        {
            //ApiRequestOdoo_v1 apiRequest = new ApiRequestOdoo_v1();
            //apiRequest.uid = App.Session.CurrentUser.uid;
            //apiRequest.password = App.Session.CurrentUser.codclave;
            //apiRequest.databasename = "";

            //apiRequest.index = 0;
            //apiRequest.limit = App.Session.db_limit_default;
            //apiRequest.dateIni = DateTime.Now.AddDays(-1000);
            //DateTime dateTimeIni = DateTime.Now;            

            var stopwatch = Stopwatch.StartNew();

            ApiManager.HubStockQuant hubmanager = new ApiManager.HubStockQuant(App.Session);
            var resultCount = await hubmanager.GetCount();

            if (resultCount.result == 0)
            {
                return false;
            }

            int countTotal = resultCount.result / 300;

            var database = new StockQuantDb();

            for (int indice = 0; indice <= countTotal; indice++)
            {
                Debug.WriteLine("Página:" + indice);

                var responseAll = await hubmanager.GetByCreateDate(limit, indice, year, month, day);

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
