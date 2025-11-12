using ApiManager;
using DMOrders.Services.Database.Sqlite;
using DMSA.Models.General.Requests;
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
        public async Task<bool> OnlineSyncProductPricelist()
        {
            var stopwatch = Stopwatch.StartNew();

            ApiManager.HubProductPricelist hubmanager = new ApiManager.HubProductPricelist(App.Session);
            var resultCount = await hubmanager.GetCount();

            if (resultCount.result == 0)
            {
                return false;
            }

            int countTotal = resultCount.result / limit;

            var database = new ProductPricelistDb(DbNameSqlite);

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

        public async Task<bool> OnlineSyncProductPricelistItem()
        {
            var stopwatch = Stopwatch.StartNew();

            var hubmanager = new ApiManager.HubProductPricelistItem(App.Session);
            var databaseItems = new ProductPricelistItemDb(DbNameSqlite);

            var database = new ProductPricelistDb(DbNameSqlite);

            var activePriceLists = await database.GetItemsByStatus(true);

            foreach ( var activePriceList in activePriceLists)
            {
                var resultCount = await hubmanager.GetCount(activePriceList.id);

                if (resultCount.result == 0)
                {
                    continue;
                }

                int countTotal = resultCount.result / limit;

                for (int indice = 0; indice <= countTotal; indice++)
                {
                    Debug.WriteLine("Página:" + indice);

                    var responseAll = await hubmanager.GetByCreateDate(activePriceList.id, limit, indice, year, month, day);

                    if (responseAll.result != null && responseAll.result.Length > 0)
                    {
                        await databaseItems.InsertBatchAsync(responseAll.result);
                    }

                    if (indice >= maxIndexExceeded)
                    {
                        Debug.WriteLine("Página " + indice + ": Se terminará el proceso.");
                        break;
                    }
                }
            }            

            stopwatch.Stop();

            Debug.WriteLine(String.Format("Lapso transcurrido: {0} days, {1} hours, {2} minutes, {3} seconds",
                stopwatch.Elapsed.Days, stopwatch.Elapsed.Hours, stopwatch.Elapsed.Minutes, stopwatch.Elapsed.Seconds));

            return true;
        }


    }
}
