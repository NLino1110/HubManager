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
        public async Task<bool> SyncSaleOrders()
        {
            var stopwatch = Stopwatch.StartNew();

            var hubmanager = new ApiManager.HubSaleOrder(App.Session);
            
            var database = new SaleOrderDb(DbNameSqlite);

            int[] ids = await database.GetIdsForSync();

            if (ids.Length == 0) return false;

            var responseAll = await hubmanager.GetByIds(ids);

            if (responseAll.result != null && responseAll.result.Length > 0)
            {
                foreach(var orderItem in responseAll.result)
                {
                    var orderForUpdate = await database.GetItemAsync(x=>x.erp_id == orderItem.id);
                    if (orderForUpdate.state != orderItem.state)
                    {
                        orderForUpdate.state = orderItem.state;
                        await database.UpdateAsync(orderForUpdate);
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
