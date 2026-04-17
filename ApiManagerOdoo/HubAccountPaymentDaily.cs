using ApiManagerOdoo.Base;
using DMSA.Models.Odoo.Accounting;
using DMSA.Models.Odoo.General.Responses;
using DMSA.Models.Security;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

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
                "was_odoo_synced"
                };

        public HubAccountPaymentDaily(AppSession _setAppSession) : base(_setAppSession)
        {
            EndPointApi = "/web/dataset/call_kw";
            _modelname = "account.payment.daily";
        }

        public async Task<ApiResponseOdooRpc?> GetCountByUser()
        {
            object[] args = new object[] { };
            object[] _custom_args = new object[] {
            };
            return await GetCount(args, _custom_args);
        }

        public async Task<ApiResponseOdooRpcT<AccountPaymentDaily[]>?> GetByUser(int uid)
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
                    "uid", "=", uid,
                    //"create_date", ">=", $"{year}-{month:00}-{day:00} 00:00:00",                
                    },
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
