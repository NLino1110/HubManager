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
        public async Task OnlineSyncResCenter(bool force)
        {
            var database = new ResCenterDb(DbNameSqlite);
            if (await database.GetCount() > 0)
            {
                //Ya se ha sincronizado previamente
                return;
            }

            ApiManager.HubResCenter hubManagerInstance = new HubResCenter(appSession);
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

    }
}
