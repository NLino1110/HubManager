using ApiManager;
using DMOrdersUI.Controls;
using DMOrdersUI.Services.Database.Sqlite;
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

namespace DMOrdersUI.Services.Update
{
    public partial class ServerPuller
    {
        public async Task<bool> OnlineSyncResPartner()
        {
            var stopwatch = Stopwatch.StartNew();

            DateTime dateIni = appSession.sync_date_since;
            DateTime dateEnd = DateTime.Now;

            ApiManager.HubPartner hubmanager = new ApiManager.HubPartner(appSession);
            var resultCount = await hubmanager.GetCount();

            Debug.WriteLine(resultCount.result);

            if (resultCount.result == 0)
            {
                return false;
            }

            int countTotal = resultCount.result / 300;

            var database = new ResPartnerDb();

            for (int indice = 0; indice <= countTotal; indice++)
            {
                var responseAll = await hubmanager.GetByCreateDateRange(limit, indice, dateIni, dateEnd);

                if (responseAll != null && responseAll.result != null && responseAll.result.Length > 0)
                {
                    await database.InsertBatchAsync(responseAll.result);
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
