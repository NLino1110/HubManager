using ApiManagerOdoo.Base;
using DMSA.Models.Odoo.DMOrders.promotions;
using DMSA.Models.Odoo.General.Responses;
using DMSA.Models.Security;

namespace ApiManagerOdoo.promotions
{
    public class HubSelectionType : HubBase
    {
        string[] fields_array = {
            "id",
            "name",
            "active",
            "display_name",
            "create_date",
            "write_date"
        };

        public HubSelectionType(AppSession _setAppSession) : base(_setAppSession)
        {
            EndPointApi = "/web/dataset/call_kw";
            _modelname = "selection.type";
        }

        public async Task<ApiResponseOdooRpc?> GetCount(int year, int month, int day)
        {            
            object[] args = new object[] { };

            object[] _custom_args = new object[] {
                //new object[] { "write_date", ">=", $"{year}-{month:00}-{day:00} 00:00:00" },
                 new object[] { "end_datetime", ">=", $"{year}-{month:00}-{day:00} 00:00:00" }
            };
            return await GetCount(args, _custom_args);
        }

        public async Task<ApiResponseOdooRpcT<PromotionSelectionType[]>?> GetItemsById(string ids)
        {            
            //string fields = "fields=['id','name','description']";
            
            int limit = 300;
            int index = 0 ;

            var kwargs = new
            {
                limit,
                offset = index * limit,
                fields = fields_array
            };

            object[] args = new object[] { };
            object[] _custom_args = new object[] {
                new object[] { "id", "in", $"[{ids}]" }
            };
            return await SearchRead<ApiResponseOdooRpcT<PromotionSelectionType[]>>(args, _custom_args, kwargs, true);
        }

        public async Task<ApiResponseOdooRpcT<PromotionSelectionType[]>?> GetItems(DateTime dateIni, int limit, int index)
        {
            var kwargs = new
            {
                limit,
                offset = index * limit,
                fields = fields_array
            };

            object[] args = new object[] { };
            object[] _custom_args = new object[] {
                new object[] { "write_date", ">=", dateIni.ToString("yyyy-MM-dd") }
            };
            return await SearchRead<ApiResponseOdooRpcT<PromotionSelectionType[]>>(args, _custom_args, kwargs, true);
        }

        public async Task<ApiResponseOdooRpcT<PromotionSelectionType[]>?> GetActives(DateTime dateIni, int limit, int index)
        {
            var kwargs = new
            {
                limit,
                offset = index * limit,
                fields = fields_array
            };

            object[] args = new object[] { };
            object[] _custom_args = new object[] {
                new object[] { 
                    "end_datetime", ">=", dateIni.ToString("yyyy-MM-dd") 
                },
                new object[] {                    
                    "active", "=", true
                }
            };
            return await SearchRead<ApiResponseOdooRpcT<PromotionSelectionType[]>>(args, _custom_args, kwargs, true);
        }
    }
}
