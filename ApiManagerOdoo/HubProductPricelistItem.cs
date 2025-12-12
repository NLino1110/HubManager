using ApiManagerOdoo.Base;
using DMSA.Models.Odoo.General.Responses;
using DMSA.Models.Odoo.Native;
using DMSA.Models.Odoo.Sales;
using DMSA.Models.Security;

namespace ApiManager
{
    public class HubProductPricelistItem : HubBase
    {
        string[] fields_array = new[] {
               "id",
                "pricelist_id",
                "company_id",
                "currency_id",
                "date_start",
                "date_end",
                "min_quantity",
                "applied_on",
                "display_applied_on",
                "product_tmpl_id",
                "product_id",
                "product_uom",
                "product_variant_count",
                "base",
                "base_pricelist_id",
                "compute_price",
                "fixed_price",
                "percent_price",
                "price_discount",
                "price_round",
                "price_surcharge",
                "price_markup",
                "price_min_margin",
                "price_max_margin",
                "name",
                "price",
                "display_name",
                "create_uid",
                "create_date",
                "write_uid",
                "write_date",
                "active",
                "cost_unit",
                "cost_price",
                "cost_price_tax",
                "margin"
                };

        public HubProductPricelistItem(AppSession _setAppSession) : base(_setAppSession)
        {
            EndPointApi = "/web/dataset/call_kw";
            _modelname = "product.pricelist.item";
        }

        public async Task<ApiResponseOdooRpc?> GetCount()
        {
            object[] args = new object[] { };
            object[] _custom_args = new object[] {
            };
            return await GetCount(args, _custom_args);
        }

        public async Task<ApiResponseOdooRpc?> GetCount(int pricelist_id)
        {
            object[] args = new object[] { };
            object[] _custom_args = new object[] {
                new object[] { "pricelist_id", "=", pricelist_id },
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

        public async Task<ApiResponseOdooRpc?> GetCount(int pricelist_id, int year, int month, int day)
        {
            object[] args = new object[] { };            
            object[] _custom_args = new object[] {                
                new object[] { "pricelist_id", "=", pricelist_id },
                new object[] { "write_date", ">", $"{year}-{month:00}-{day:00} 23:59:59" },
            };
            return await GetCount(args, _custom_args);
        }
                
        public async Task<ApiResponseOdooRpcT<product_pricelist_item[]>?> GetByCreateDate(int limit, int index, int year, int month, int day)
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
            return await SearchRead<ApiResponseOdooRpcT<product_pricelist_item[]>>(args, _custom_args, kwargs);
        }


        public async Task<ApiResponseOdooRpcT<product_pricelist_item[]>?> GetByCreateDate(int pricelist_id, int limit, int index, int year, int month, int day)
        {
            var kwargs = new
            {
                limit = limit,
                offset = (index * limit),
                fields = fields_array
            };

            object[] args = new object[] { };
            object[] _custom_args = new object[] {
                new object[] { "pricelist_id", "=", pricelist_id },
            };
            return await SearchRead<ApiResponseOdooRpcT<product_pricelist_item[]>>(args, _custom_args, kwargs);
        }

        public async Task<ApiResponseOdooRpcT<product_pricelist_item[]>?> GetByCreateDateRange(int limit, int index, DateTime dateIni, DateTime dateEnd)
        {
            var kwargs = new
            {
                limit = limit,
                offset = (index * limit),
                fields = fields_array 
            };

            object[] args = new object[] { };
            object[] _custom_args = new object[] {
                new object[] {"create_date", ">=", dateIni.ToString("yyyy-MM-dd 00:00:00") },
                new object[] {"create_date", "<=", dateEnd.ToString("yyyy-MM-dd 23:59:59") },
            };
            return await SearchRead<ApiResponseOdooRpcT<product_pricelist_item[]>>(args, _custom_args, kwargs, true);
        }

        public async Task<ApiResponseOdooRpcT<product_pricelist_item[]>?> GetByWriteDate_dl(int year, int month, int day)
        {
            var kwargs = new
            {
                fields = new[] { "id", "write_date" }
            };

            object[] args = new object[] { };
            object[] _custom_args = new object[] {
                new object[] { "write_date", ">", $"{year}-{month:00}-{day:00} 23:59:59" },
            };
            return await SearchRead<ApiResponseOdooRpcT<product_pricelist_item[]>>(args, _custom_args, kwargs);
        }

        public async Task<ApiResponseOdooRpcT<product_pricelist_item[]>?> GetByCreateDate_dl(int year, int month, int day)
        {
            var kwargs = new
            {
                fields = new[] { "id", "create_date", "write_date" }
            };

            object[] args = new object[] { };
            object[] _custom_args = new object[] {
                new object[] { "create_date", ">", $"{year}-{month:00}-{day:00} 23:59:59" },
            };
            return await SearchRead<ApiResponseOdooRpcT<product_pricelist_item[]>>(args, _custom_args, kwargs);
        }
        
        public async Task<ApiResponseOdooRpcT<product_pricelist_item[]>?> GetByWriteDate(int pricelist_id, int limit, int index, int year, int month, int day)
        {
            var kwargs = new
            {
                limit = limit,
                offset = (index * limit),
                fields = fields_array
            };
            

            object[] args = new object[] { };
            object[] _custom_args = new object[] {
                new object[] { "pricelist_id", "=", pricelist_id },
                new object[] { "write_date", ">", $"{year}-{month:00}-{day:00} 23:59:59" },
            };
            return await SearchRead<ApiResponseOdooRpcT<product_pricelist_item[]>>(args, _custom_args, kwargs);
        }
    }
}
