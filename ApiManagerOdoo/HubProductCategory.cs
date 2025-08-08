using ApiManagerOdoo.Base;
using DMSA.Models.Odoo.General.Responses;
using DMSA.Models.Odoo.Native;
using DMSA.Models.Security;

namespace ApiManager
{
    public class HubProductCategory : HubBase
    {
        string[] fields_array = new[] {
                "id",
                "parent_id",
                "name",
                "complete_name",
                "parent_path",
                "product_count",
                "removal_strategy_id",
                "packaging_reserve_method",
                "property_valuation",
                "property_cost_method",
                "create_date",
                "write_date"
                };

        public HubProductCategory(AppSession _setAppSession) : base(_setAppSession)
        {
            EndPointApi = "/web/dataset/call_kw";
            _modelname = "product.category";
        }

        public async Task<ApiResponseOdooRpc?> GetCount()
        {
            object[] args = new object[] { };
            object[] _custom_args = new object[] {
            };
            return await GetCount(args, _custom_args);
        }

        public async Task<ApiResponseOdooRpc?> GetCountByCreateDate(int year, int month, int day)
        {
            object[] args = new object[] { };
            object[] _custom_args = new object[] {
                new object[] {"create_date", ">=", $"{year}-{month:00}-{day:00} 00:00:00" },
                new object[] {"create_date", "<=", $"{year}-{month:00}-{day:00} 23:59:59" },
            };
            return await GetCount(args, _custom_args);
        }

        public async Task<ApiResponseOdooRpc?> GetCountByWriteDate(int year, int month, int day)
        {
            object[] args = new object[] { };            
            object[] _custom_args = new object[] {
                new object[] { "write_date", ">", $"{year}-{month:00}-{day:00} 23:59:59" },
            };
            return await GetCount(args, _custom_args);
        }
                
        public async Task<ApiResponseOdooRpcT<product_category[]>?> GetByCreateDate(int limit, int index, int year, int month, int day)
        {            
            var kwargs = new
            {
                limit = limit,
                offset = (index * limit),
                fields = fields_array
            };

            object[] args = new object[] { };
            object[] _custom_args = new object[] {
                new object[] {"create_date", ">=", $"{year}-{month:00}-{day:00} 00:00:00" },
                //new object[] {"create_date", "<=", $"{year}-{month:00}-{day:00} 23:59:59" },
            };
            return await SearchRead<ApiResponseOdooRpcT<product_category[]>>(args, _custom_args, kwargs);
        }

        public async Task<ApiResponseOdooRpcT<product_category[]>?> GetByWriteDate_dl(int year, int month, int day)
        {
            var kwargs = new
            {
                fields = new[] { "id", "write_date" }
            };

            object[] args = new object[] { };
            object[] _custom_args = new object[] {
                new object[] { "write_date", ">", $"{year}-{month:00}-{day:00} 23:59:59" },
            };
            return await SearchRead<ApiResponseOdooRpcT<product_category[]>>(args, _custom_args, kwargs);
        }

        public async Task<ApiResponseOdooRpcT<product_category[]>?> GetByCreateDate_dl(int year, int month, int day)
        {
            var kwargs = new
            {
                fields = new[] { "id", "create_date", "write_date" }
            };

            object[] args = new object[] { };
            object[] _custom_args = new object[] {
                new object[] { "create_date", ">", $"{year}-{month:00}-{day:00} 23:59:59" },
            };
            return await SearchRead<ApiResponseOdooRpcT<product_category[]>>(args, _custom_args, kwargs);
        }
        
        public async Task<ApiResponseOdooRpcT<product_category[]>?> GetByWriteDate(int year, int month, int day)
        {
            var kwargs = new
            {
                fields = fields_array
            };

            object[] args = new object[] { };
            object[] _custom_args = new object[] {                
                new object[] { "write_date", ">", $"{year}-{month:00}-{day:00} 23:59:59" },
            };
            return await SearchRead<ApiResponseOdooRpcT<product_category[]>>(args, _custom_args, kwargs);
        }
    }
}
