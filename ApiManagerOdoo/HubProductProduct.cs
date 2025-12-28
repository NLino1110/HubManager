using ApiManagerOdoo.Base;
using DMSA.Models.Odoo.General.Responses;
using DMSA.Models.Odoo.Native;
using DMSA.Models.Security;


namespace ApiManager
{
    public class HubProductProduct : HubBase
    {
        string[] fields_array = new[] { 
                "id",
                "default_code",
                "code",
                "partner_ref",
                "active",
                "product_tmpl_id",
                "barcode",
                "combination_indices",
                "is_product_variant",
                "standard_price",
                "list_price",
                "price_extra",
                "lst_price",
                "volume",
                "weight",
                "display_name",
                "name",
                "create_date",
                "write_date",
                "qty_available",
                "virtual_available",
                "free_qty",
                "incoming_qty",
                "outgoing_qty",
                //"detailed_type", //NO ENCONTRADO
                "type",
                "categ_id",
                "currency_id",
                "uom_id",
                "uom_name",
                "sale_ok",
                "purchase_ok",                
                "image_256",
                "image_1920",
                "cod_marca",
                "cod_linea",
                "cod_sublinea",
                "cod_sublineado",
                "es_perecible",
                "es_combo",
                "categ_id",
                "list_price",
                "product_variant_ids",
                "general_marca_id",
                "general_linea_id",
                "general_categoria_id",
                "general_subcategoria_id",
                "general_grupor_tipo_id",
                "general_registro_sanitario",
                "taxes_id",
                "supplier_taxes_id",
                "otras_venta_pedido"
                };

        public HubProductProduct(AppSession _setAppSession) : base(_setAppSession)
        {
            EndPointApi = "/web/dataset/call_kw";
            _modelname = "product.product";
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
                
        public async Task<ApiResponseOdooRpcT<product_product[]>?> GetByCreateDate(int limit, int index, int year, int month, int day)
        {            
            var kwargs = new
            {
                limit = limit,
                offset = (index * limit),
                fields = fields_array //new[] { "id", "name", "uom_id", "default_code", "categ_id", "type", "product_brand_id", "active", "macro_product_available", "sale_ok", "purchase_ok", "trade_ok" }
            };

            object[] args = new object[] { };
            object[] _custom_args = new object[] {
                new object[] {"create_date", ">=", $"{year}-{month:00}-{day:00} 00:00:00" },
                new object[] {"create_date", "<=", $"{year}-{month:00}-{day:00} 23:59:59" },
            };
            return await SearchRead<ApiResponseOdooRpcT<product_product[]>>(args, _custom_args, kwargs);
        }

        public async Task<ApiResponseOdooRpcT<product_product[]>?> GetByCreateDateRange(int limit, int index, DateTime dateIni, DateTime dateEnd)
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
            return await SearchRead<ApiResponseOdooRpcT<product_product[]>>(args, _custom_args, kwargs, true);
        }

        public async Task<ApiResponseOdooRpcT<product_product[]>?> GetByWriteDate(int limit, int index, DateTime dateIni)
        {
            var kwargs = new
            {
                limit = limit,
                offset = (index * limit),
                fields = fields_array
            };

            object[] args = new object[] { };
            object[] _custom_args = new object[] {
                new object[] {"write_date", ">=", dateIni.ToString("yyyy-MM-dd 00:00:00") }                
            };
            return await SearchRead<ApiResponseOdooRpcT<product_product[]>>(args, _custom_args, kwargs, true);
        }

        public async Task<ApiResponseOdooRpcT<product_product[]>?> GetByWriteDate_dl(int year, int month, int day)
        {
            var kwargs = new
            {
                fields = new[] { "id", "write_date" }
            };

            object[] args = new object[] { };
            object[] _custom_args = new object[] {
                new object[] { "write_date", ">", $"{year}-{month:00}-{day:00} 23:59:59" },
            };
            return await SearchRead<ApiResponseOdooRpcT<product_product[]>>(args, _custom_args, kwargs);
        }

        public async Task<ApiResponseOdooRpcT<product_product[]>?> GetByCreateDate_dl(int year, int month, int day)
        {
            var kwargs = new
            {
                fields = new[] { "id", "create_date", "write_date" }
            };

            object[] args = new object[] { };
            object[] _custom_args = new object[] {
                new object[] { "create_date", ">", $"{year}-{month:00}-{day:00} 23:59:59" },
            };
            return await SearchRead<ApiResponseOdooRpcT<product_product[]>>(args, _custom_args, kwargs);
        }
        
        public async Task<ApiResponseOdooRpcT<product_product[]>?> GetByWriteDate(int year, int month, int day)
        {
            var kwargs = new
            {
                fields = fields_array //new[] { "id", "name", "uom_id", "default_code", "categ_id", "type", "product_brand_id", "active", "macro_product_available", "sale_ok", "purchase_ok", "trade_ok", "write_date" }
            };

            object[] args = new object[] { };
            object[] _custom_args = new object[] {                
                new object[] { "write_date", ">", $"{year}-{month:00}-{day:00} 23:59:59" },
            };
            return await SearchRead<ApiResponseOdooRpcT<product_product[]>>(args, _custom_args, kwargs);
        }
    }
}
