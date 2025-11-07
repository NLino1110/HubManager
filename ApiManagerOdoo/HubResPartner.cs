
using ApiManagerOdoo.Base;
using AppManagerOdoo.Tools;
using CobranzasDMSA.Models;
using DMSA.Models.Clientes;
using DMSA.Models.General;
using DMSA.Models.General.Requests;
using DMSA.Models.General.Responses;
using DMSA.Models.Odoo.General.Responses;
using DMSA.Models.Odoo.Native;
using DMSA.Models.Security;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;

namespace ApiManager
{
    public class HubResPartner : HubBase
    {
        string[] fields_array = new[] {
                //"id_sequence",
                "id",
                "company_id",
                "vat",
                "vat_doc",
                "doc_type_identification_id",
                "name",
                "display_name",
                "email",
                "user_id",
                //"user_login",
                //"total_due",
                //"total_overdue",
                "debit",
                "credit",
                //"total_invoiced",
                "street",
                "street2",
                //"total_to_beat",
                //"positive_balance",
                //"client_type_id",
                //"client_type_name",
                "write_date",
                "create_date",
                //"birthdate",
                "parent_id",
                "zip",
                "phone",
                "mobile",
                "city_id",
                "city",
                "state_id",
                "country_id",
                //"contact_address_complete",
                "active",
                "product_pricelist_id",
                "adic_comercial_id",
                //"adic_comercial_secundarios_ids",
                "adic_lunes",
                "adic_martes",
                "adic_miercoles",
                "adic_jueves",
                "adic_viernes",
                "adic_sabado",
                "adic_domingo",
                "is_salesman",
                "sale_available",
                "calificacion_crediticia_id"
    };

        public HubResPartner(AppSession _setAppSession) : base(_setAppSession)
        {
            EndPointApi = "/connect/get_res_partner";
            EndPointApi = "/web/dataset/call_kw";
            _modelname = "res.partner";
        }

        public async Task<ApiResponseOdooRpc?> GetCount(int year, int month, int day)
        {
            object[] args = new object[] { };            
            object[] _custom_args = new object[] {
                new object[] {"write_date", ">=", $"{year}-{month:00}-{day:00} 00:00:00" },
            };
            return await GetCount(args, _custom_args);            
        }

        [Obsolete("Probablemente debe ser eliminado")]
        public async Task<ApiResponseOdooRpcT<res_partner[]>?> GetSpecial()
        {
            string EndPointApiLine = "/connect/get_res_partner";

            var kwargs = new { };

            var settings = new JsonSerializerSettings
            {
                DateFormatString = "yyyy-MM-dd HH:mm:ss",
                //ContractResolver = new IncludeJsonIgnoreResolver(new string[] { "was_odoo_synced", "lines" })
            };

            Object value = null;

            return await Call<Object, ApiResponseOdooRpcT<res_partner[]>>(EndPointApiLine,
                Method.Post, value, true);
        }

        public async Task<ApiResponseOdooRpcT<res_partner[]>?> GetByCreateDateRange(int limit, int index, DateTime dateIni, DateTime dateEnd)
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
            return await SearchRead<ApiResponseOdooRpcT<res_partner[]>>(args, _custom_args, kwargs, true);
        }

        public async Task<ApiResponseOdooRpcT<res_partner[]>?> GetByWriteDate(DateTime dateIni, int limit, int index)
        {
            var kwargs = new
            {
                limit = limit,
                offset = (index * limit),
                fields = fields_array
            };

            object[] args = new object[] { };
            object[] _custom_args = new object[] {
                new object[] {"write_date", ">=", dateIni.ToString("yyyy-MM-dd 00:00:00") },
            };
            return await SearchRead<ApiResponseOdooRpcT<res_partner[]>>(args, _custom_args, kwargs, true);
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

        public async Task<ApiResponseOdooRpcT<res_partner[]>?> GetByCreateDate(int year, int month, int day)
        {
            var kwargs = new
            {
                //fields = new[] { "id", "write_date" }
            };

            object[] args = new object[] { };
            object[] _custom_args = new object[] {
                new object[] { "create_date", ">=", $"{year}-{month:00}-{day:00} 00:00:00" },
                new object[] { "create_date", "<=", $"{year}-{month:00}-{day:00} 23:59:59" },
                new object[] { "active", "=", true },
            };
            return await SearchRead<ApiResponseOdooRpcT<res_partner[]>>(args, _custom_args, kwargs);
        }

        public async Task<ApiResponseOdooRpcT<res_partner[]>?> GetByWriteDate_dl(int year, int month, int day)
        {   
            var kwargs = new
            {
                fields = new[] { "id", "write_date" }
            };

            object[] args = new object[] { };
            object[] _custom_args = new object[] {
                new object[] { "write_date", ">", $"{year}-{month:00}-{day:00} 23:59:59" },
                new object[] { "active", "=", true },
            };
            return await SearchRead<ApiResponseOdooRpcT<res_partner[]>>(args, _custom_args, kwargs);
        }

        //public async Task<ApiResponsePartner?> GetByCreateDate_dl(ApiRequestOdoo_v1 apiRequestOdoo_V1, int year, int month, int day)
        public async Task<ApiResponseOdooRpcT<res_partner[]>?> GetByCreateDate_dl(int year, int month, int day)
        {
            var kwargs = new
            {
                fields = new[] { "id", "create_date", "write_date" }
            };

            object[] args = new object[] { };
            object[] _custom_args = new object[] {
                new object[] { "create_date", ">", $"{year}-{month:00}-{day:00} 23:59:59" },
                new object[] { "active", "=", true },
            };
            return await SearchRead<ApiResponseOdooRpcT<res_partner[]>>(args, _custom_args, kwargs);
        }

        public async Task<ApiResponseOdooRpcT<res_partner[]>?> GetByWriteDate(int year, int month, int day)
        {   
            var kwargs = new
            {
                fields = new[] { "id", "create_date", "write_date" }
            };

            object[] args = new object[] { };
            object[] _custom_args = new object[] {
                new object[] { "write_date", ">", $"{year}-{month:00}-{day:00} 23:59:59" },
                new object[] { "active", "=", true },
            };
            return await SearchRead<ApiResponseOdooRpcT<res_partner[]>>(args, _custom_args, kwargs);
        }
    }
}
