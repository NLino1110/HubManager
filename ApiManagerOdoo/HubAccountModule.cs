using ApiManagerOdoo.Base;
using AppManagerOdoo.Tools;
using DMSA.Models.Odoo.DMCobranzas;
using DMSA.Models.Odoo.General.Responses;
using DMSA.Models.Security;
//using Microsoft.AspNetCore.Components;
//using Microsoft.Extensions.Configuration;
//using Microsoft.Extensions.Logging;
//using Microsoft.Extensions.Options;
using RestSharp;

namespace ApiManager
{
    public class HubAccountModule: HubBase
    {
        string[] fields_array = {
            "id",
            "name",
            "code",
            "create_date",
            "write_date"
        };

        public HubAccountModule(AppSession _setAppSession) : base(_setAppSession)
        {
            EndPointApi = "/web/dataset/call_kw";
            _modelname = "account.module";
        }

        //public HubAccountModule(AppSession _setAppSession)
        //{
        //    _appSession = _setAppSession;                        
        //    _client = new RestSharpMiddle(_setAppSession);
        //}

        public async Task<ApiResponseOdooRpcT<AccountModule[]>?> GetAll(string ids)
        {
            int limit = 300;
            int index = 0;

            var kwargs = new
            {
                limit = limit,
                offset = (index * limit),
                fields = fields_array
            };

            object[] args = new object[] { };
            object[] _custom_args = new object[] {
                new object[] { "id", "in", $"[{ids}]" }
            };
            return await SearchRead<ApiResponseOdooRpcT<AccountModule[]>>(args, _custom_args, kwargs, true);
        }

        public async Task<ApiResponseOdooRpcT<AccountModule[]>?> GetAll()
        {
            int limit = 300;
            int index = 0;

            var kwargs = new
            {
                limit = limit,
                offset = (index * limit),
                fields = fields_array
            };

            object[] args = new object[] { };
            object[] _custom_args = new object[] {
                
            };
            return await SearchRead<ApiResponseOdooRpcT<AccountModule[]>>(args, _custom_args, kwargs, true);
        }
    }
}
