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
        public async Task OnlineSyncStockLocation()
        {
            ApiManager.HubStockLocation hubManagerInstance = new HubStockLocation(App.Session);
            ApiResponseOdooRpcT<stock_location[]> dataList = await hubManagerInstance.GetByCreateDate(limit, 0, year, month, day);

            if (dataList != null && dataList.result !=null && dataList.result.Length > 0)
            {
                var database = new StockLocationDb(App.Session.odooConnection.DbNameSqlite);
                await database.InsertBatchAsync(dataList.result);
            }
        }

    }
}
