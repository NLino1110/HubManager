using ApiManagerOdoo.Sale;
using DMOrders.Services.Database.Sqlite;
using DMSA.Sync.Core.Database.Sqlite;
using System.Diagnostics;

namespace DMSA.Sync.Core.Update
{
    public partial class ServerPuller
    {
        public async Task<bool> SyncSaleOrders()
        {
            var stopwatch = Stopwatch.StartNew();

            var hubmanager = new HubSaleOrder(Constants.Session);
            
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
