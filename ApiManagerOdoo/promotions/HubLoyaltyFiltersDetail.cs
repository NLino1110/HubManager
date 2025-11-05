using ApiManagerOdoo.Base;
using DMSA.Models.Odoo.DMOrders.promotions;
using DMSA.Models.Odoo.General.Responses;
using DMSA.Models.Security;

namespace ApiManagerOdoo.promotions
{
    public class HubLoyaltyFiltersDetail : HubBase
    {
        string[] fields_array = {
            "id",
            "parent_id",
            "filter_id",
            "discount",
            "create_uid",
            "write_uid",
            "filter_name",
            "exclude",
            "create_date",
            "write_date",
            "change_id",
            "type",
            "start_date",
            "end_date"
        };

        public HubLoyaltyFiltersDetail(AppSession _setAppSession) : base(_setAppSession)
        {
            EndPointApi = "/web/dataset/call_kw";
            _modelname = "loyalty.filters.detail";
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

        public async Task<ApiResponseOdooRpcT<LoyaltyFiltersDetail[]>?> GetItemsById(string ids)
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
            return await SearchRead<ApiResponseOdooRpcT<LoyaltyFiltersDetail[]>>(args, _custom_args, kwargs, true);
        }

        public async Task<ApiResponseOdooRpcT<LoyaltyFiltersDetail[]>?> GetItems(DateTime dateIni, int limit, int index)
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
            return await SearchRead<ApiResponseOdooRpcT<LoyaltyFiltersDetail[]>>(args, _custom_args, kwargs, true);
        }

        public async Task<ApiResponseOdooRpcT<LoyaltyFiltersDetail[]>?> GetActives(DateTime dateIni, int limit, int index)
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
            return await SearchRead<ApiResponseOdooRpcT<LoyaltyFiltersDetail[]>>(args, _custom_args, kwargs, true);
        }

        public async Task<ApiResponseOdooRpc?> GetCount(int parent_id)
        {
            object[] args = new object[] { };

            object[] _custom_args = new object[] {
                 new object[] { "loyalty_id", "=", parent_id }
            };
            return await GetCount(args, _custom_args);
        }

        public async Task<ApiResponseOdooRpcT<LoyaltyFiltersDetail[]>?> GetItemsByParentId(int id, int limit, int index)
        {
            var kwargs = new
            {
                limit,
                offset = index * limit,
                fields = fields_array
            };

            object[] args = new object[] { };
            object[] _custom_args = new object[] {
                new object[] { "promo_id", "=", id }
            };
            return await SearchRead<ApiResponseOdooRpcT<LoyaltyFiltersDetail[]>>(args, _custom_args, kwargs, true);
        }
    }
}
