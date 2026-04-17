using ApiManagerOdoo.Base;
using CobranzasDMSA_Odoo.Models;
using DMSA.Models.Odoo.Accounting;
using DMSA.Models.Odoo.General.Responses;
using DMSA.Models.Odoo.Tools;
using DMSA.Models.Security;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;

namespace ApiManagerOdoo.Accounting
{
    [Obsolete]
    public class HubCreditNoteRequestDetail : HubBase
    {
        string[] fields_array = new[] {
                "id",
                "name",
                "create_date",
                "write_date"
                };

        public HubCreditNoteRequestDetail(AppSession _setAppSession) : base(_setAppSession)
        {
            EndPointApi = "/web/dataset/call_kw";
            _modelname = "account.move.line.send";
        }

        public async Task<ApiResponseOdooRpc?> GetHeaderCount()
        {
            object[] args = new object[] { };
            object[] _custom_args = new object[] {
            };
            return await GetCount(args, _custom_args);
        }
        public async Task<ApiResponseOdooRpcT<credit_note_request_detail[]>?> GetAccountMoveLineSend(int parent_move_id)
        {
            int limit = 100;
            int index = 0;
            int year = 0;
            int month = 0;
            int day = 0;

            var kwargs = new
            {
                limit,
                offset = index * limit,
                fields = fields_array
            };

            object[] args = new object[] { };
            object[] _custom_args = new object[] {
                new object[] {
                    "parent_move_id", "=", parent_move_id,
                    //"create_date", ">=", $"{year}-{month:00}-{day:00} 00:00:00",                
                    },
            };
            return await SearchRead<ApiResponseOdooRpcT<credit_note_request_detail[]>>(args, _custom_args, kwargs, true);
        }


        public async Task<ApiResponseOdooRpcT<int>?> SendAccountMoveLine(credit_note_request_detail SendObject)
        {
            var kwargs = new { };

            var settings = new JsonSerializerSettings
            {
                DateFormatString = "yyyy-MM-dd HH:mm:ss",
                ContractResolver = new IncludeJsonIgnoreResolver(new string[] { "was_odoo_synced" })
            };

            var serialized = JsonConvert.SerializeObject(SendObject, settings);

            var newJObject = JObject.Parse(serialized);

            object[] args = new object[] { newJObject };
            return await Create<ApiResponseOdooRpcT<int>>(args, kwargs);
        }
    }
}
