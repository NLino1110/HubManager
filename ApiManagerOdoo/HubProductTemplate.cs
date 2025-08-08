
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

using RestSharp;
using System;

namespace ApiManager
{
    public class HubProductTemplate : HubBase
    {
        string[] fields_array = new[] {
                "id", 
                "name", 
                "uom_id", 
                "default_code", 
                "categ_id", 
                "type", 
                "product_brand_id", 
                "active", 
                "macro_product_available", 
                "sale_ok", 
                "purchase_ok", 
                "trade_ok"
        };

        public HubProductTemplate(AppSession _setAppSession) : base(_setAppSession)
        {
            EndPointApi = "/web/dataset/call_kw";
            _modelname = "product.template";
        }

        public async Task<ApiResponseOdooRpc?> GetCount()
        {
            object[] args = new object[] { };            
            object[] _custom_args = new object[] {};
            return await GetCount(args, _custom_args);
        }

        public async Task<ApiResponseOdooRpcT<product_template[]>?> GetItemsById(string ids)
        {       
            int limit = 0;
            int index = 0 ;

            var kwargs = new
            {
                limit = limit,
                offset = (index * limit),
                fields = fields_array
            };

            object[] args = new object[] { };
            object[] _custom_args = new object[] {
                new object[] {"id", "in", ids }
                //new object[] {"create_date", "<=", $"{year}-{month:00}-{day:00} 23:59:59" },
            };
            return await SearchRead<ApiResponseOdooRpcT<product_template[]>>(args, _custom_args, kwargs);
        }

        public async Task<ApiResponseOdooRpcT<product_template[]>?> GetItems(int limit, int index)
        {
            var kwargs = new
            {
                limit = limit,
                offset = (index * limit),
                fields = fields_array
            };

            object[] args = new object[] { };
            object[] _custom_args = new object[] {
                new object[] {"active", "=", true }
                //new object[] {"create_date", "<=", $"{year}-{month:00}-{day:00} 23:59:59" },
            };
            return await SearchRead<ApiResponseOdooRpcT<product_template[]>>(args, _custom_args, kwargs);
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

        public async Task<ApiResponseOdooRpcT<product_template[]>?> GetByCreateDate(int year, int month, int day, int limit, int index)
        {
            int offset = (index * limit);

            //string _EndPointApi = "/api/account.move.line";
            var kwargs = new
            {
                limit = limit,
                offset = offset,
                fields = fields_array //new[] { "id", "name", "uom_id", "default_code", "categ_id", "type", "product_brand_id", "active", "macro_product_available", "sale_ok", "purchase_ok", "trade_ok" }
            };

            object[] args = new object[] { };
            object[] _custom_args = new object[] {
                new object[] {"create_date", ">=", $"{year}-{month:00}-{day:00} 00:00:00" },
                new object[] {"create_date", "<=", $"{year}-{month:00}-{day:00} 23:59:59" },
            };
            return await SearchRead<ApiResponseOdooRpcT<product_template[]>>(args, _custom_args, kwargs);
        }

        public async Task<ApiResponseOdooRpcT<product_template[]>?> GetByWriteDate_dl(int year, int month, int day)
        {
            //string _EndPointApi = "/api/account.move.line";
            var kwargs = new
            {
                fields = new[] { "id", "write_date" }
            };

            object[] args = new object[] { };
            object[] _custom_args = new object[] {
                new object[] { "write_date", ">", $"{year}-{month:00}-{day:00} 23:59:59" },
            };
            return await SearchRead<ApiResponseOdooRpcT<product_template[]>>(args, _custom_args, kwargs);
        }        

        public async Task<ApiResponseOdooRpcT<product_template[]>?> GetByCreateDate_dl(int year, int month, int day)
        {
            //string _EndPointApi = "/api/account.move.line";
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
            //string _EndPointApi = "/api/account.move.line";
            var kwargs = new
            {
                fields = fields_array // new[] { "id", "name", "uom_id", "default_code", "categ_id", "type", "product_brand_id", "active", "macro_product_available", "sale_ok", "purchase_ok", "trade_ok", "write_date" }
            };

            object[] args = new object[] { };
            object[] _custom_args = new object[] {                
                new object[] { "write_date", ">", $"{year}-{month:00}-{day:00} 23:59:59" },
            };
            return await SearchRead<ApiResponseOdooRpcT<product_template[]>>(args, _custom_args, kwargs);
        }

    }
}
