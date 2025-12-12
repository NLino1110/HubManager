using ApiManagerOdoo.Base;
using DMSA.Models.Odoo.General.Responses;
using DMSA.Models.Odoo.Native;
using DMSA.Models.Security;

namespace ApiManager
{
    public class HubStockQuant : HubBase
    {
        string[] fields_array = new[] {
                "id",
                //"priority",
                "quantity",
                "reserved_quantity",
                "available_quantity",
                "in_date",
                "tracking",
                "on_hand",
                "inventory_quantity",
                "inventory_quantity_auto_apply",
                "inventory_diff_quantity",
                "inventory_date",
                "last_count_date",
                "inventory_quantity_set",
                "is_outdated",
                "display_name",
                "create_date",
                "write_date",
                "use_expiration_date",
                "value",
                "cost_method",
                //"dummy_id",
                "product_id",
                "product_tmpl_id",
                "product_uom_id",
                "company_id",
                "location_id",
                "warehouse_id",
                "storage_category_id",
                "lot_id",
                "sn_duplicated",
                "package_id",
                "owner_id",
                "product_categ_id",
                "user_id",
                "removal_date",
                "accounting_date",
                "currency_id",
                "create_uid",
                "write_uid",
        };

        public HubStockQuant(AppSession _setAppSession) : base(_setAppSession)
        {
            EndPointApi = "/web/dataset/call_kw";
            _modelname = "stock.quant";
        }

        public async Task<ApiResponseOdooRpc?> GetCount()
        {
            object[] args = new object[] { };
            object[] _custom_args = new object[] {
            };
            return await GetCount(args, _custom_args);
        }

        public async Task<ApiResponseOdooRpc?> GetCount(DateTime? dateTime)
        {
            object[] args = new object[] { };
            object[] _custom_args = new object[] {
                new object[] { "write_date", ">=", dateTime?.ToString("yyyy-MM-dd") }
            };
            return await GetCount(args, _custom_args);
        }

        public async Task<ApiResponseOdooRpc?> GetCount(int[] whIds, DateTime? dateTime)
        {
            object[] args = new object[] { };
            object[] _custom_args = new object[] {
                new object[] { "warehouse_id", "in", whIds },
                new object[] { "write_date", ">=", dateTime?.ToString("yyyy-MM-dd") }
            };
            return await GetCount(args, _custom_args);
        }

        public async Task<ApiResponseOdooRpc?> GetCountByCreateDate(int year, int month, int day)
        {
            object[] args = new object[] { };
            object[] _custom_args = new object[] {
                new object[] { "location_id.usage", "=", "internal" },
                new object[] {"create_date", ">=", $"{year}-{month:00}-{day:00} 00:00:00" },
                new object[] {"create_date", "<=", $"{year}-{month:00}-{day:00} 23:59:59" },
            };
            return await GetCount(args, _custom_args);
        }

        public async Task<ApiResponseOdooRpc?> GetCountByWriteDate(int year, int month, int day)
        {
            object[] args = new object[] { };            
            object[] _custom_args = new object[] {
                new object[] { "location_id.usage", "=", "internal" },
                new object[] { "write_date", ">", $"{year}-{month:00}-{day:00} 23:59:59" },
            };
            return await GetCount(args, _custom_args);
        }
                
        public async Task<ApiResponseOdooRpcT<stock_quant[]>?> GetByCreateDate(int limit, int index, int year, int month, int day)
        {            
            var kwargs = new
            {
                limit = limit,
                offset = (index * limit),
                fields = fields_array
            };

            object[] args = new object[] { };
            object[] _custom_args = new object[] {
                new object[] { "location_id.usage", "=", "internal" },
                new object[] {"create_date", ">=", $"{year}-{month:00}-{day:00} 00:00:00" },
                //new object[] {"create_date", "<=", $"{year}-{month:00}-{day:00} 23:59:59" },
            };
            return await SearchRead<ApiResponseOdooRpcT<stock_quant[]>>(args, _custom_args, kwargs);
        }

        public async Task<ApiResponseOdooRpcT<stock_quant[]>?> GetByWriteDate_dl(int year, int month, int day)
        {
            var kwargs = new
            {
                fields = new[] { "id", "write_date" }
            };

            object[] args = new object[] { };
            object[] _custom_args = new object[] {
                new object[] { "location_id.usage", "=", "internal" },
                new object[] { "write_date", ">", $"{year}-{month:00}-{day:00} 23:59:59" },
            };
            return await SearchRead<ApiResponseOdooRpcT<stock_quant[]>>(args, _custom_args, kwargs);
        }

        public async Task<ApiResponseOdooRpcT<stock_quant[]>?> GetByCreateDate_dl(int year, int month, int day)
        {
            var kwargs = new
            {
                fields = new[] { "id", "create_date", "write_date" }
            };

            object[] args = new object[] { };
            object[] _custom_args = new object[] {
                new object[] { "location_id.usage", "=", "internal" },
                new object[] { "create_date", ">", $"{year}-{month:00}-{day:00} 23:59:59" },
            };
            return await SearchRead<ApiResponseOdooRpcT<stock_quant[]>>(args, _custom_args, kwargs);
        }
        
        public async Task<ApiResponseOdooRpcT<stock_quant[]>?> GetByWriteDate(int[] whIds, int limit, int index, int year, int month, int day)
        {
            var kwargs = new
            {
                limit = limit,
                offset = (index * limit),
                fields = fields_array
            };

            object[] args = new object[] { };
            object[] _custom_args = new object[] {
                new object[] { "warehouse_id", "in", whIds },
                new object[] { "location_id.usage", "=", "internal" },
                new object[] { "write_date", ">", $"{year}-{month:00}-{day:00} 23:59:59" },
            };
            return await SearchRead<ApiResponseOdooRpcT<stock_quant[]>>(args, _custom_args, kwargs);
        }
    }
}
