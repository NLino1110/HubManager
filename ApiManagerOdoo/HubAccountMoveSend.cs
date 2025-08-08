using ApiManagerOdoo.Base;
using CobranzasDMSA_Odoo.Models;
using DMSA.Models.Odoo.General.Responses;
using DMSA.Models.Odoo.Native;
using DMSA.Models.Odoo.Tools;
using DMSA.Models.Security;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;

namespace ApiManager
{
    public class HubAccountMoveSend : HubBase
    {
        string[] fields_array = new[] {
                "id",
                "name",
                "create_date",
                "write_date"
                };

        public HubAccountMoveSend(AppSession _setAppSession) : base(_setAppSession)
        {
            EndPointApi = "/web/dataset/call_kw";
            _modelname = "account.move.send";
        }

        public async Task<ApiResponseOdooRpc?> GetHeaderCount()
        {
            object[] args = new object[] { };
            object[] _custom_args = new object[] {
            };
            return await GetCount(args, _custom_args);
        }

        public async Task<ApiResponseOdooRpcT<account_move_send[]>?> GetMovesByHeader(int parent_id)
        {
            int limit = 100;
            int index = 0;
            int year = 0;
            int month = 0;
            int day = 0;
                        
            var kwargs = new
            {
                limit = limit,
                offset = (index * limit),
                fields = fields_array
            };

            object[] args = new object[] { };
            object[] _custom_args = new object[] {
                new object[] {
                    "parent_id", "=", parent_id,
                    //"create_date", ">=", $"{year}-{month:00}-{day:00} 00:00:00",                
                    },
            };
            return await SearchRead<ApiResponseOdooRpcT<account_move_send[]>>(args, _custom_args, kwargs);
        }

        public async Task<ApiResponseOdooRpcT<int>?> SendAccountMove(account_move_send SendObject)
        {            
            var kwargs = new { };

            var settings = new JsonSerializerSettings
            {
                DateFormatString = "yyyy-MM-dd HH:mm:ss",
                ContractResolver = new IncludeJsonIgnoreResolver(new string[] { "was_odoo_synced", "lines" })
            };

            var serialized = JsonConvert.SerializeObject(SendObject, settings);

            var newJObject = JObject.Parse(serialized);

            object[] args = new object[] { newJObject };
            return await Create<ApiResponseOdooRpcT<int>>(args, kwargs);
        }
    }
}
