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

namespace ApiManager
{
    public class HubPromotionProductDetail : HubBase
    {
        string[] fields_array = {
            "id",
            "parent_id",
            "product_id",
            "promo_id",
            "bonus_id",
            "from_file",
            "product_uom_id",
            "qty",
            "discount",            
            "is_fixed",
            "product_relation_id",
            "display_name",
            "create_uid",
            "create_date",
            "write_uid",
            "write_date",
        };

        public HubPromotionProductDetail(AppSession _setAppSession) : base(_setAppSession)
        {
            EndPointApi = "/web/dataset/call_kw";
            _modelname = "promotion.product.detail";
        }

        public async Task<ApiResponseOdooRpc?> GetCount(int parent_id)
        {            
            object[] args = new object[] { };

            object[] _custom_args = new object[] {                
                 new object[] { "promo_id", "=", parent_id }
            };
            return await GetCount(args, _custom_args);
        }

        public async Task<ApiResponseOdooRpcT<PromotionProductDetail[]>?> GetItemsByParentId(int id, int limit, int index)
        {
            var kwargs = new
            {
                limit = limit,
                offset = (index * limit),
                fields = fields_array
            };

            object[] args = new object[] { };
            object[] _custom_args = new object[] {
                new object[] { "promo_id", "=", id }
            };
            return await SearchRead<ApiResponseOdooRpcT<PromotionProductDetail[]>>(args, _custom_args, kwargs, true);
        }

        public async Task<ApiResponseOdooRpcT<PromotionProductDetail[]>?> GetItemsByParentIds(string ids, int limit, int index)
        {
            var kwargs = new
            {
                limit = limit,
                offset = (index * limit),
                fields = fields_array
            };

            object[] args = new object[] { };
            object[] _custom_args = new object[] {
                new object[] { "promo_id", "in", $"[{ids}]" }
            };
            return await SearchRead<ApiResponseOdooRpcT<PromotionProductDetail[]>>(args, _custom_args, kwargs, true);
        }

    }
}
