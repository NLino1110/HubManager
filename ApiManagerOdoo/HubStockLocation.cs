using ApiManagerOdoo.Base;
using DMSA.Models.Odoo.General.Responses;
using DMSA.Models.Odoo.Native;
using DMSA.Models.Security;

namespace ApiManager
{
    public class HubStockLocation : HubBase
    {
        string[] fields_array = new[] {
                "id",
                "name",
                "complete_name",
                "active",
                "usage",
                "comment",
                "posx",
                "posy",
                "posz",
                "parent_path",
                "scrap_location",
                //"return_location",
                "replenish_location",
                "cyclic_inventory_frequency",
                "barcode",
                "net_weight",
                "forecast_weight",
                "display_name",
                "create_date",
                "location_id",
                "child_ids",
                "child_internal_location_ids",
                "company_id",
                "removal_strategy_id",
                "putaway_rule_ids",
                "quant_ids",
                "last_inventory_date",
                "next_inventory_date",
                "warehouse_view_ids",
                "warehouse_id",
                "storage_category_id",
                "outgoing_move_line_ids",
                "incoming_move_line_ids",
                "create_uid",
                "write_uid",
                "write_date",
                "valuation_in_account_id",
                "valuation_out_account_id",
        };

        public HubStockLocation(AppSession _setAppSession) : base(_setAppSession)
        {
            EndPointApi = "/web/dataset/call_kw";
            _modelname = "stock.location";
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
                
        public async Task<ApiResponseOdooRpcT<stock_location[]>?> GetByCreateDate(int limit, int index, int year, int month, int day)
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
            return await SearchRead<ApiResponseOdooRpcT<stock_location[]>>(args, _custom_args, kwargs, true);
        }

        public async Task<ApiResponseOdooRpcT<stock_location[]>?> GetByWriteDate_dl(int year, int month, int day)
        {
            var kwargs = new
            {
                fields = new[] { "id", "write_date" }
            };

            object[] args = new object[] { };
            object[] _custom_args = new object[] {
                new object[] { "write_date", ">", $"{year}-{month:00}-{day:00} 23:59:59" },
            };
            return await SearchRead<ApiResponseOdooRpcT<stock_location[]>>(args, _custom_args, kwargs);
        }

        public async Task<ApiResponseOdooRpcT<stock_location[]>?> GetByCreateDate_dl(int year, int month, int day)
        {
            var kwargs = new
            {
                fields = new[] { "id", "create_date", "write_date" }
            };

            object[] args = new object[] { };
            object[] _custom_args = new object[] {
                new object[] { "create_date", ">", $"{year}-{month:00}-{day:00} 23:59:59" },
            };
            return await SearchRead<ApiResponseOdooRpcT<stock_location[]>>(args, _custom_args, kwargs);
        }
        
        public async Task<ApiResponseOdooRpcT<stock_location[]>?> GetByWriteDate(int year, int month, int day)
        {
            var kwargs = new
            {
                fields = fields_array
            };

            object[] args = new object[] { };
            object[] _custom_args = new object[] {                
                new object[] { "write_date", ">", $"{year}-{month:00}-{day:00} 23:59:59" },
            };
            return await SearchRead<ApiResponseOdooRpcT<stock_location[]>>(args, _custom_args, kwargs);
        }
    }
}
