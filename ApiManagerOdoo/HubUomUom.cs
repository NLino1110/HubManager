using ApiManagerOdoo.Base;
using DMSA.Models.Odoo.General.Responses;
using DMSA.Models.Odoo.Inventory;
using DMSA.Models.Security;

namespace ApiManager
{
    public class HubUomUom : HubBase
    {
        string[] fields_array = {
            "id",
            "name",
            "active",
            "clave_externa",
            "factor",
            "factor_inv",
            "rounding",
            "uom_type",
            "ratio",
            "color",
            "display_name",
            "fiscal_country_codes",
            "create_date",
            "write_date"
        };

        string[] sync_fields_array = {
            "id",
            "category_id",
            "clave_externa",
            "display_name",
            "factor",
            "factor_inv",
            "fiscal_country_codes",
            "name",
            "rounding",
            "uom_type"
        };

        public HubUomUom(AppSession _setAppSession) : base(_setAppSession)
        {
            EndPointApi = "/web/dataset/call_kw";
            _modelname = "uom.uom";
        }

        public async Task<ApiResponseOdooRpc?> GetCount(int year, int month, int day)
        {            
            object[] args = new object[] { };

            object[] _custom_args = new object[] {
                new object[] { "write_date", ">=", $"{year}-{month:00}-{day:00} 00:00:00" },                
            };
            return await GetCount(args, _custom_args);
        }

        public async Task<ApiResponseOdooRpc?> GetCountAll()
        {
            object[] args = new object[] { };
            object[] _custom_args = new object[] { };
            return await GetCount(args, _custom_args);
        }

        public async Task<ApiResponseOdooRpcT<uom_uom[]>?> GetAll(int limit, int index)
        {
            var kwargs = new
            {
                limit,
                offset = index * limit,
                fields = sync_fields_array
            };

            object[] args = new object[] { };
            object[] _custom_args = new object[] { };
            return await SearchRead<ApiResponseOdooRpcT<uom_uom[]>>(args, _custom_args, kwargs, true);
        }

        public async Task<ApiResponseOdooRpcT<uom_uom[]>?> GetItemsById(string ids)
        {            
            
            int limit = 300;
            int index = 0 ;

            var kwargs = new
            {
                limit = limit,
                offset = (index * limit),
                fields = fields_array
            };

            object[] args = new object[] { };
            object[] _custom_args = new object[] {
                new object[] { "id", "in", $"[{ids}]" }
            };
            return await SearchRead<ApiResponseOdooRpcT<uom_uom[]>>(args, _custom_args, kwargs, true);
        }

        public async Task<ApiResponseOdooRpcT<uom_uom[]>?> GetItems(DateTime dateIni, int limit, int index)
        {
            var kwargs = new
            {
                limit = limit,
                offset = (index * limit),
                fields = fields_array
            };

            object[] args = new object[] { };
            object[] _custom_args = new object[] {
                new object[] { "write_date", ">=", dateIni.ToString("yyyy-MM-dd") }
            };
            return await SearchRead<ApiResponseOdooRpcT<uom_uom[]>>(args, _custom_args, kwargs, true);
        }
    }
}
