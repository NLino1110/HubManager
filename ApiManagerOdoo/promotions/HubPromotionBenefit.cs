using ApiManagerOdoo.Base;
using DMSA.Models.Odoo.DMOrders.promotions;
using DMSA.Models.Odoo.General.Responses;
using DMSA.Models.Security;

namespace ApiManagerOdoo.promotions
{
    public class HubPromotionBenefit : HubBase
    {
        string[] fields_array = {
            "id",
            "name",
            "description",
            "active",
            "start_datetime",
            "end_datetime",
            "invoice_total",
            "state",
            "logs",
            "approval_date",
            "end_date",
            "summary",
            "code",
            "promotion_type_id",
            "target_segment_id",
            "selection_type_id",
            "approval_uid",
            "change_requested_uid",
            "company_id",
            "loyalty_company_id",
            "promotion_product_ids",
            "product_promotion_ids",
            "promotion_rules_ids",
            "customers_included_ids",
            "customers_excluded_ids",
            "create_date",
            "write_date"
        };

        public HubPromotionBenefit(AppSession _setAppSession) : base(_setAppSession)
        {
            EndPointApi = "/web/dataset/call_kw";
            _modelname = "promotion.benefit";
        }

        public async Task<ApiResponseOdooRpc?> GetCount(int year, int month, int day)
        {            
            object[] args = new object[] { };

            object[] _custom_args = new object[] {
                new object[] { "write_date", ">=", $"{year}-{month:00}-{day:00} 00:00:00" },
                 //new object[] { "end_datetime", ">=", $"{year}-{month:00}-{day:00} 00:00:00" },
                 new object[] { "target_segment_id", "=", 1 },
                 new object[] { "state", "=", "authorized" },
            };
            return await GetCount(args, _custom_args);
        }

        public async Task<ApiResponseOdooRpc?> GetCountPrecise(DateTime dateIni)
        {
            object[] args = new object[] { };

            object[] _custom_args = new object[] {
                new object[] { "write_date", ">=", dateIni.ToString("yyyy-MM-dd HH:mm:ss") },
                 //new object[] { "end_datetime", ">=", $"{year}-{month:00}-{day:00} 00:00:00" },
                 new object[] { "target_segment_id", "=", 1 },
                 new object[] { "state", "=", "authorized" },
            };
            return await GetCount(args, _custom_args);
        }

        public async Task<ApiResponseOdooRpcT<PromotionBenefit[]>?> GetItemsById(string ids)
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
            return await SearchRead<ApiResponseOdooRpcT<PromotionBenefit[]>>(args, _custom_args, kwargs, true);
        }

        public async Task<ApiResponseOdooRpcT<PromotionBenefit[]>?> GetItems(DateTime dateIni, int limit, int index)
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
            return await SearchRead<ApiResponseOdooRpcT<PromotionBenefit[]>>(args, _custom_args, kwargs, true);
        }

        public async Task<ApiResponseOdooRpcT<PromotionBenefit[]>?> GetActives(DateTime dateIni, int limit, int index)
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
                },
                new object[] { "target_segment_id", "=", 1 },
                //new object[] { "state", "=", "authorized" },
            };
            return await SearchRead<ApiResponseOdooRpcT<PromotionBenefit[]>>(args, _custom_args, kwargs, true);
        }


        public async Task<ApiResponseOdooRpcT<PromotionBenefit[]>?> GetActivesByWriteDate(DateTime write_date, DateTime expire_datetime, int limit, int index)
        {
            var kwargs = new
            {
                limit,
                offset = index * limit,
                fields = fields_array
            };

            object[] args = new object[] { };
            object[] _custom_args = new object[] {
                new object[] { "write_date", ">=", write_date.ToString("yyyy-MM-dd") },
                new object[] {
                    "end_datetime", ">=", expire_datetime.ToString("yyyy-MM-dd")
                },
                new object[] {
                    "active", "=", true
                },
                new object[] { "target_segment_id", "=", 1 },
                //new object[] { "state", "=", "authorized" },
            };
            return await SearchRead<ApiResponseOdooRpcT<PromotionBenefit[]>>(args, _custom_args, kwargs, true);
        }
    }
}
