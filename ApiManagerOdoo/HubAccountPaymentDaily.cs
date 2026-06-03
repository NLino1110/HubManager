using ApiManagerOdoo.Base;
using DMSA.Models.Odoo.Accounting;
using DMSA.Models.Odoo.General.Responses;
using DMSA.Models.Security;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Security.Cryptography;

namespace ApiManager
{
    public class HubAccountPaymentDaily : HubBase
    {
        string[] fields_array = new[] {
                "closing_id",
                "datetime_closing",
                "company_id",
                "bank_id",
                "uid",
                "payment_reference",
                "closing_amount",
                "closing_details",
                //"was_odoo_synced"
                };

        public HubAccountPaymentDaily(AppSession _setAppSession) : base(_setAppSession)
        {
            EndPointApi = "/web/dataset/call_kw";
            _modelname = "account.payment.daily";
        }

        //public async Task<ApiResponseOdooRpcT<account_move[]>?> GetAccountMoves(DateTime dateIni, int limit, int index)
        //{
        //    var kwargs = new
        //    {
        //        limit,
        //        offset = index * limit,
        //        fields = fields_array,
        //        order = "write_date asc"
        //    };

        //    object[] args = new object[] { };
        //    object[] _custom_args = new object[] {
        //        //new object[] { "write_date", ">=", dateIni.ToString("yyyy-MM-dd") },
        //        new object[] { "write_date", ">=", $"{dateIni.Year}-{dateIni.Month:00}-{dateIni.Day:00} 00:00:00" },
        //        new object[] { "state", "=", "posted" },
        //        new object[] { "move_type", "=", "out_invoice" },
        //        new object[] { "invoice_date", "!=", false },
        //    };
        //    return await SearchRead<ApiResponseOdooRpcT<account_move[]>>(args, _custom_args, kwargs, true);
        //}

        public async Task<ApiResponseOdooRpc?> GetCountByUser(int uid, DateTime dateIni)
        {
            object[] args = new object[] { };
            object[] _custom_args = new object[] {                
                //"external_create_uid", "=", uid,
                new object[] { "uid", "=", uid },
                new object[] { "write_date", ">=", $"{dateIni.Year}-{dateIni.Month:00}-{dateIni.Day:00} 00:00:00" }
            };
            return await GetCount(args, _custom_args);
        }

        public async Task<ApiResponseOdooRpcT<AccountPaymentDaily[]>?> GetByUser(int uid, DateTime dateIni)
        {
            int limit = 100;
            int index = 0;

            var kwargs = new
            {
                limit = limit,
                offset = (index * limit),
                fields = fields_array
            };

            object[] args = new object[] { };
            object[] _custom_args = new object[] {                
                //"external_create_uid", "=", uid,
                new object[] { "uid", "=", uid },
                new object[] { "write_date", ">=", $"{dateIni.Year}-{dateIni.Month:00}-{dateIni.Day:00} 00:00:00" }                
            };
            return await SearchRead<ApiResponseOdooRpcT<AccountPaymentDaily[]>>(args, _custom_args, kwargs);
        }
        public async Task<ApiResponseOdooRpcT<int>?> Send(AccountPaymentDaily SendObject)
        {            
            var kwargs = new { };

            var settings = new JsonSerializerSettings
            {
                DateFormatString = "yyyy-MM-dd HH:mm:ss",
            };

            var serialized = JsonConvert.SerializeObject(SendObject, settings);

            var newJObject = JObject.Parse(serialized);

            object[] args = new object[] { newJObject };

            return await Create<ApiResponseOdooRpcT<int>>(args, kwargs);
        }
    }
}
