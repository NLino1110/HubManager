using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Reflection;
using System.Security.AccessControl;
using System.Security.Cryptography.X509Certificates;
using System.Text.Json;
using System.Threading.Tasks;
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


namespace ApiManager
{
    public class HubStore : HubBase
    {
        private readonly Type _type;
        public HubStore(AppSession _setAppSession) : base(_setAppSession)
        {
            EndPointApi = "/web/dataset/call_kw";
            _modelname = "res.store";

            _type = typeof(res_store);
        }

        public async Task<ApiResponseOdooRpc?> GetCount()
        {
            object[] args = new object[] { };            
            object[] _custom_args = new object[] {};
            return await GetCount(args, _custom_args);
        }
        
        public async Task<ApiResponseOdooRpc?> GetCountByCreateDate(int year, int month, int day)
        {
            object[] args = new object[] { };
            //string _custom_args = $"[('create_date','>=','{year}-{month:00}-{day:00} 00:00:00'),('create_date','<=','{year}-{month:00}-{day:00} 23:59:59')]";
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

        public async Task<ApiResponseOdooRpcT<res_store[]>> GetByCreateDate(int limit, int index, int year, int month, int day)
        {
            
            var kwargs = new
            {
                limit = limit,
                offset = (index * limit),
                fields = new[] { "id", "name", "identifier_mybussines", "create_date", "write_date", "latitude", "longitude" }
            };

            object[] args = new object[] { };
            object[] _custom_args = new object[] {
                new object[] {"create_date", ">=", $"{year}-{month:00}-{day:00} 00:00:00" },
                //new object[] {"create_date", "<=", $"{year}-{month:00}-{day:00} 23:59:59" },
            };

            return await SearchRead<ApiResponseOdooRpcT<res_store[]>>( args, _custom_args, kwargs);

            //var method = typeof(HubStore).GetMethod(nameof(SearchRead), BindingFlags.NonPublic | BindingFlags.Instance);
            //var generic = method.MakeGenericMethod(_type);
            //return await (Task<object?>)generic.Invoke(this, new object[] { apiRequestOdoo_V1, year, month, day });
        }

        public async Task<object?> GetByWriteDate_dl(int year, int month, int day)
        {            
            var kwargs = new
            {
                fields = new[] { "id", "write_date" }
            };

            object[] args = new object[] { };
            object[] _custom_args = new object[] {
                new object[] { "write_date", ">", $"{year}-{month:00}-{day:00} 23:59:59" },
            };

            //return await SearchRead<ApiResponseOdooRpcT<product_template[]>>(apiRequestOdoo_V1, args, _custom_args, kwargs);

            var method = typeof(HubStore).GetMethod(nameof(SearchRead), BindingFlags.NonPublic | BindingFlags.Instance);
            var generic = method.MakeGenericMethod(_type);
            return await (Task<object?>)generic.Invoke(this, new object[] {year, month, day });
        }

        public async Task<ApiResponseOdooRpcT<product_template[]>?> GetByCreateDate_dl(int year, int month, int day)
        {            
            var kwargs = new
            {
                fields = new[] { "id", "create_date", "write_date" }
            };

            object[] args = new object[] { };
            object[] _custom_args = new object[] {
                new object[] { "create_date", ">", $"{year}-{month:00}-{day:00} 23:59:59" },
            };
            return await SearchRead<ApiResponseOdooRpcT<product_template[]>>(args, _custom_args, kwargs);
        }

        public async Task<ApiResponseOdooRpcT<product_template[]>?> GetByWriteDate(int year, int month, int day)
        {            
            var kwargs = new
            {
                fields = new[] { "id", "name", "uom_id", "default_code", "categ_id", "type", "product_brand_id", "active", "macro_product_available", "sale_ok", "purchase_ok", "trade_ok", "write_date" }
            };

            object[] args = new object[] { };
            object[] _custom_args = new object[] {                
                new object[] { "write_date", ">", $"{year}-{month:00}-{day:00} 23:59:59" },
            };
            return await SearchRead<ApiResponseOdooRpcT<product_template[]>>(args, _custom_args, kwargs);
        }
    }
}
