using ApiManagerOdoo.Base;
using AppManagerOdoo.Tools;
using CobranzasDMSA.Models;
using DMSA.Models.Clientes;
using DMSA.Models.General;
using DMSA.Models.General.Responses;
using DMSA.Models.Odoo.DMOrders.promotions;
using DMSA.Models.Odoo.General.Responses;
using DMSA.Models.Odoo.Native;
using DMSA.Models.Security;
//using Microsoft.AspNetCore.Components;
//using Microsoft.Extensions.Configuration;
//using Microsoft.Extensions.Logging;
//using Microsoft.Extensions.Options;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Text.Json;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace ApiManagerOdoo.promotions
{
    public class HubPromoRules : HubBase
    {
        string[] fields_array = {
            "id",
            "promo_id",
            "value",
            "minimum_value",
            "maximum_value",
            "product_id",
            "product_uom_id",
            "qty",
            "discount",
            "count_products",
            "create_uid",
            "write_uid",
            "variable",
            "operator",
            "product_promotion",
            "code",
            "is_fixed",
            "create_date",
            "write_date",
            "raffle_template_id",
            "selection_type_id",
            "payment_method_id",
            "start_date",
            "end_date",
            "unlimited_time",
            "state",
            "change_id",
            "type",
            "discount_base"
        };

        public HubPromoRules(AppSession _setAppSession) : base(_setAppSession)
        {
            EndPointApi = "/web/dataset/call_kw";
            _modelname = "promo.rules";
        }

        public async Task<ApiResponseOdooRpc?> GetCount(int year, int month, int day)
        {            
            object[] args = new object[] { };

            object[] _custom_args = new object[] {
                //new object[] { "write_date", ">=", $"{year}-{month:00}-{day:00} 00:00:00" },
                 new object[] { "end_datetime", ">=", $"{year}-{month:00}-{day:00} 00:00:00" }
            };
            return await GetCount(args, _custom_args);
        }

        public async Task<ApiResponseOdooRpcT<PromoRules[]>?> GetItemsById(string ids)
        {            
            //string fields = "fields=['id','name','description']";
            
            int limit = 300;
            int index = 0 ;

            var kwargs = new
            {
                limit,
                offset = index * limit,
                fields = fields_array
            };

            object[] args = new object[] { };
            object[] _custom_args = new object[] {
                new object[] { "id", "in", $"[{ids}]" }
            };
            return await SearchRead<ApiResponseOdooRpcT<PromoRules[]>>(args, _custom_args, kwargs, true);
        }

        public async Task<ApiResponseOdooRpcT<PromoRules[]>?> GetItems(DateTime dateIni, int limit, int index)
        {
            var kwargs = new
            {
                limit,
                offset = index * limit,
                fields = fields_array
            };

            object[] args = new object[] { };
            object[] _custom_args = new object[] {
                new object[] { "write_date", ">=", dateIni.ToString("yyyy-MM-dd") }
            };
            return await SearchRead<ApiResponseOdooRpcT<PromoRules[]>>(args, _custom_args, kwargs, true);
        }

        public async Task<ApiResponseOdooRpcT<PromoRules[]>?> GetActives(DateTime dateIni, int limit, int index)
        {
            var kwargs = new
            {
                limit,
                offset = index * limit,
                fields = fields_array
            };

            object[] args = new object[] { };
            object[] _custom_args = new object[] {
                new object[] { 
                    "end_datetime", ">=", dateIni.ToString("yyyy-MM-dd") 
                },
                new object[] {                    
                    "active", "=", true
                }
            };
            return await SearchRead<ApiResponseOdooRpcT<PromoRules[]>>(args, _custom_args, kwargs, true);
        }

        public async Task<ApiResponseOdooRpc?> GetCount(int parent_id)
        {
            object[] args = new object[] { };

            object[] _custom_args = new object[] {
                 new object[] { "promo_id", "=", parent_id }
            };
            return await GetCount(args, _custom_args);
        }

        public async Task<ApiResponseOdooRpcT<PromoRules[]>?> GetItemsByParentId(int id, int limit, int index)
        {
            var kwargs = new
            {
                limit,
                offset = index * limit,
                fields = fields_array
            };

            object[] args = new object[] { };
            object[] _custom_args = new object[] {
                new object[] { "promo_id", "=", id }
            };
            return await SearchRead<ApiResponseOdooRpcT<PromoRules[]>>(args, _custom_args, kwargs, true);
        }
    }
}
