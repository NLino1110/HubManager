using ApiManager;
using DMOrdersUI.Services.Database.Sqlite;
using DMSA.Models.General.Requests;
using DMSA.Models.Odoo.General.Responses;
using DMSA.Models.Odoo.Native;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMOrdersUI.Services.Update
{
    public partial class ServerPuller
    {        
        public async Task OnlineSyncMacroProductSubCategory()
        {
            ApiRequestOdoo_v1 apiRequest = new ApiRequestOdoo_v1();
            apiRequest.uid = App.Session.CurrentUser.uid;
            apiRequest.password = App.Session.CurrentUser.codclave;
            apiRequest.databasename = "";

            apiRequest.index = 0;
            apiRequest.limit = 1000;
            apiRequest.dateIni = DateTime.Now.AddDays(-1000);

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
