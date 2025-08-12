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
        public async Task OnlineSyncMacroProductSubCategory()
        {
            ApiManager.HubMacroProductSubCategory hubManagerInstance = new HubMacroProductSubCategory(App.Session);
            ApiResponseOdooRpcT<macro_product_sub_category[]> dataList = await hubManagerInstance.GetByCreateDate(1000, 0, 2023, 1, 1);

            if (dataList != null && dataList.result !=null && dataList.result.Length > 0)
            {
                var database = new MacroProductSubCategoryDb();
                await database.InsertBatchAsync(dataList.result);
            }
        }

    }
}
