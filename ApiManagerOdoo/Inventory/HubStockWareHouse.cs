using ApiManagerOdoo.Base;
using DMSA.Models.Odoo.General.Responses;
using DMSA.Models.Odoo.Inventory;
using DMSA.Models.Security;

namespace ApiManagerOdoo.Inventory
{
    public class HubStockWareHouse : HubBase
    {
        string[] fields_array = new[] {
                "id",
                "name",
                "active",
                "code",
                "reception_steps",
                "delivery_steps",
                "sequence",
                "display_name",
                "create_date",
                "write_date",
                "manufacture_to_resupply",
                "buy_to_resupply",                
                "company_id",
                "partner_id",
                "view_location_id",
                "lot_stock_id",
                "route_ids",
                "wh_input_stock_loc_id",
                "wh_qc_stock_loc_id",
                "wh_output_stock_loc_id",
                "wh_pack_stock_loc_id",
                "mto_pull_id",
                "pick_type_id",
                "pack_type_id",
                "out_type_id",
                "in_type_id",
                "int_type_id",
                //"return_type_id",
                "crossdock_route_id",
                "reception_route_id",
                "delivery_route_id",
                "resupply_wh_ids",
                "resupply_route_ids",
                "create_uid",
                "write_uid",
                "manufacture_pull_id",
                "manufacture_mto_pull_id",
                "pbm_mto_pull_id",
                "sam_rule_id",
                "manu_type_id",
                "pbm_type_id",
                "sam_type_id",
                "pbm_route_id",
                "pbm_loc_id",
                "sam_loc_id",
                "pos_type_id",
                "buy_pull_id",
                "center_id",
                "default"
        };

        public HubStockWareHouse(AppSession _setAppSession) : base(_setAppSession)
        {
            EndPointApi = "/web/dataset/call_kw";
            _modelname = "stock.warehouse";
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
                
        public async Task<ApiResponseOdooRpcT<stock_warehouse[]>?> GetByCreateDate(int limit, int index, int year, int month, int day)
        {            
            var kwargs = new
            {
                limit,
                offset = index * limit,
                fields = fields_array
            };

            object[] args = new object[] { };
            object[] _custom_args = new object[] {
                new object[] {"create_date", ">=", $"{year}-{month:00}-{day:00} 00:00:00" },
                //new object[] {"create_date", "<=", $"{year}-{month:00}-{day:00} 23:59:59" },
            };
            return await SearchRead<ApiResponseOdooRpcT<stock_warehouse[]>>(args, _custom_args, kwargs, true);
        }

        public async Task<ApiResponseOdooRpcT<stock_warehouse[]>?> GetWriteDateList(int year, int month, int day)
        {
            var kwargs = new
            {
                fields = new[] { "id", "write_date" }
            };

            object[] args = new object[] { };
            object[] _custom_args = new object[] {
                new object[] { "write_date", ">", $"{year}-{month:00}-{day:00} 23:59:59" },
            };
            return await SearchRead<ApiResponseOdooRpcT<stock_warehouse[]>>(args, _custom_args, kwargs);
        }

        public async Task<ApiResponseOdooRpcT<stock_warehouse[]>?> GetCreateDateList(int year, int month, int day)
        {
            var kwargs = new
            {
                fields = new[] { "id", "create_date", "write_date" }
            };

            object[] args = new object[] { };
            object[] _custom_args = new object[] {
                new object[] { "create_date", ">", $"{year}-{month:00}-{day:00} 23:59:59" },
            };
            return await SearchRead<ApiResponseOdooRpcT<stock_warehouse[]>>(args, _custom_args, kwargs);
        }
        
        public async Task<ApiResponseOdooRpcT<stock_warehouse[]>?> GetByWriteDate(int year, int month, int day)
        {
            var kwargs = new
            {
                fields = fields_array
            };

            object[] args = new object[] { };
            object[] _custom_args = new object[] {                
                new object[] { "write_date", ">", $"{year}-{month:00}-{day:00} 23:59:59" },
            };
            return await SearchRead<ApiResponseOdooRpcT<stock_warehouse[]>>(args, _custom_args, kwargs);
        }
    }
}
