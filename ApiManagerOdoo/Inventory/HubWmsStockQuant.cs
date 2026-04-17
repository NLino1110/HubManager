using ApiManagerOdoo.Base;
using DMSA.Models.Odoo.General.Responses;
using DMSA.Models.Odoo.Native;
using DMSA.Models.Odoo.Native.Inventory;
using DMSA.Models.Security;

namespace ApiManager.Odoo.Inventory
{
    public class HubWmsStockQuant : HubBase
    {
        string[] fields_array = new[] {
            "id",
            "product_id",
            "product_tmpl_id",
            "location_id",
            "warehouse_id",
            "cantidad_disponible",
            "cantidad_reservada",
            "cantidad_total",
            "lot_id",
            "por_actualizar",
            "display_name",
            "create_date",
            "write_date",
        };

        public HubWmsStockQuant(AppSession _setAppSession) : base(_setAppSession)
        {
            EndPointApi = "/web/dataset/call_kw";
            _modelname = "wms.stock.quant";
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
        
        public async Task<ApiResponseOdooRpcT<wms_stock_quant[]>?> GetByWriteDate(int limit, int index, int year, int month, int day)
        {            
            var kwargs = new
            {
                limit = limit,
                offset = (index * limit),
                fields = fields_array
            };

            object[] args = new object[] { };
            object[] _custom_args = new object[] {                
                new object[] {"write_date", ">=", $"{year}-{month:00}-{day:00} 00:00:00" },
            };
            return await SearchRead<ApiResponseOdooRpcT<wms_stock_quant[]>>(args, _custom_args, kwargs);
        }
                
        public async Task<ApiResponseOdooRpcT<wms_stock_quant[]>?> GetByWriteDate(int[] whIds, int limit, int index, int year, int month, int day)
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
                new object[] { "write_date", ">", $"{year}-{month:00}-{day:00} 00:00:00" },
            };
            return await SearchRead<ApiResponseOdooRpcT<wms_stock_quant[]>>(args, _custom_args, kwargs, true);
        }
    }
}
