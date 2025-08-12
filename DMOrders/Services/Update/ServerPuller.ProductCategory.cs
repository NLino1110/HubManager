using ApiManager;
using DMOrders.Services.Database.Sqlite;
using DMSA.Models.General.Requests;
using DMSA.Models.Odoo.General.Responses;
using DMSA.Models.Odoo.Native;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMOrders.Services.Update
{
    public partial class ServerPuller
    {        
        public async Task OnlineSyncProductCategory()
        {            
            DateTime dateIni = DateTime.Now.AddDays(-1000);

            ApiManager.HubProductCategory hubStore = new HubProductCategory(App.Session);
            ApiResponseOdooRpcT<product_category[]> dataList = await hubStore.GetByCreateDate(1000, 0, dateIni.Year, dateIni.Month, dateIni.Day);

            if (dataList != null && dataList.result !=null && dataList.result.Length > 0)
            {
                var database = new ProductCategoryDb();
                await database.InsertBatchAsync(dataList.result);
            }
        }

    }
}
