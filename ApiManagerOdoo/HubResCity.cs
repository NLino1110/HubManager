using ApiManagerOdoo.Base;
using DMSA.Models.Odoo.DebitCollection;
using DMSA.Models.Odoo.DMOrders.promotions;
using DMSA.Models.Odoo.General.Responses;
using DMSA.Models.Odoo.Native;
using DMSA.Models.Security;
using System;

namespace ApiManagerOdoo
{
    public class HubResCity : HubBase
    {
        string[] fields_array = {
            "id",
            "active",
            "name",
            "display_name",
            "code",
            "zip",            
            "display_name",
            "create_date",
            "write_date"
        };

        public HubResCity(AppSession _setAppSession) : base(_setAppSession)
        {
            EndPointApi = "/web/dataset/call_kw";
            _modelname = "res.city";
        }

        public async Task<ApiResponseOdooRpc?> GetCount(DateTime? dateTime)
        {            
            object[] args = new object[] { };

            object[] _custom_args = new object[] {
                new object[] { "write_date", ">=", dateTime?.ToString("yyyy-MM-dd") },
                new object[] { "zip", "like", "EC" },
            };
            return await GetCount(args, _custom_args);
        }

        public async Task<ApiResponseOdooRpcT<res_city[]>?> GetItems(DateTime dateIni, int limit, int index)
        {
            var kwargs = new
            {
                limit,
                offset = index * limit,
                fields = fields_array
            };

            object[] args = new object[] { };
            object[] _custom_args = new object[] {
                new object[] { "write_date", ">=", dateIni.ToString("yyyy-MM-dd") },
                new object[] { "zip", "like", "EC" },
            };
            return await SearchRead<ApiResponseOdooRpcT<res_city[]>>(args, _custom_args, kwargs, true);
        }

        public async Task<ApiResponseOdooRpcT<res_city[]>?> GetActives(DateTime dateIni, int limit, int index)
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
                    "end_datetime", ">=", dateIni.ToString("yyyy-MM-dd"),
                },
                new object[] {                    
                    "active", "=", true
                },
                new object[] { "zip", "like", "EC" },
            };
            return await SearchRead<ApiResponseOdooRpcT<res_city[]>>(args, _custom_args, kwargs, true);
        }
    }
}
