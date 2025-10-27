using ApiManagerOdoo.Base;
using DMSA.Models.Odoo.General.Responses;
using DMSA.Models.Odoo.Native;
using DMSA.Models.Odoo.Sales;
using DMSA.Models.Security;

namespace ApiManager
{
    public class HubProductPricelist : HubBase
    {
        string[] fields_array = new[] {
                "id",
                "name",
                "active",
                "sequence",
                "currency_id",
                "company_id",
                "display_name",
                "create_uid",
                "create_date",
                "write_uid",
                "write_date",
                "clave_externa",
                "tipo_canal",
                "tipo_canal_id",
                "date_start",
                "date_end"
                };

        public HubProductPricelist(AppSession _setAppSession) : base(_setAppSession)
        {
            EndPointApi = "/web/dataset/call_kw";
            _modelname = "product.pricelist";
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
                
        public async Task<ApiResponseOdooRpcT<product_pricelist[]>?> GetByCreateDate(int limit, int index, int year, int month, int day)
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
            return await SearchRead<ApiResponseOdooRpcT<product_pricelist[]>>(args, _custom_args, kwargs);
        }

        public async Task<ApiResponseOdooRpcT<product_pricelist[]>?> GetByCreateDateRange(int limit, int index, DateTime dateIni, DateTime dateEnd)
        {
            var kwargs = new
            {
                limit = limit,
                offset = (index * limit),
                fields = fields_array //new[] { "id", "name", "uom_id", "default_code", "categ_id", "type", "product_brand_id", "active", "macro_product_available", "sale_ok", "purchase_ok", "trade_ok" }
            };

            object[] args = new object[] { };
            object[] _custom_args = new object[] {
                new object[] {"create_date", ">=", dateIni.ToString("yyyy-MM-dd 00:00:00") },
                new object[] {"create_date", "<=", dateEnd.ToString("yyyy-MM-dd 23:59:59") },
            };
            return await SearchRead<ApiResponseOdooRpcT<product_pricelist[]>>(args, _custom_args, kwargs, true);
        }

        public async Task<ApiResponseOdooRpcT<product_pricelist[]>?> GetByWriteDate_dl(int year, int month, int day)
        {
            var kwargs = new
            {
                fields = new[] { "id", "write_date" }
            };

            object[] args = new object[] { };
            object[] _custom_args = new object[] {
                new object[] { "write_date", ">", $"{year}-{month:00}-{day:00} 23:59:59" },
            };
            return await SearchRead<ApiResponseOdooRpcT<product_pricelist[]>>(args, _custom_args, kwargs);
        }

        public async Task<ApiResponseOdooRpcT<product_pricelist[]>?> GetByCreateDate_dl(int year, int month, int day)
        {
            var kwargs = new
            {
                fields = new[] { "id", "create_date", "write_date" }
            };

            object[] args = new object[] { };
            object[] _custom_args = new object[] {
                new object[] { "create_date", ">", $"{year}-{month:00}-{day:00} 23:59:59" },
            };
            return await SearchRead<ApiResponseOdooRpcT<product_pricelist[]>>(args, _custom_args, kwargs);
        }
        
        public async Task<ApiResponseOdooRpcT<product_pricelist[]>?> GetByWriteDate(int year, int month, int day)
        {
            var kwargs = new
            {
                fields = fields_array 
            };

            object[] args = new object[] { };
            object[] _custom_args = new object[] {                
                new object[] { "write_date", ">", $"{year}-{month:00}-{day:00} 23:59:59" },
            };
            return await SearchRead<ApiResponseOdooRpcT<product_pricelist[]>>(args, _custom_args, kwargs);
        }
    }
}
