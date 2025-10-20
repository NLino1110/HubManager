using ApiManagerOdoo.Base;
using DMSA.Models.Odoo.General.Responses;
using DMSA.Models.Odoo.Native;
using DMSA.Models.Security;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ApiManager
{
    public class HubCuentas : HubBase
    {
        string[] fields_array = new[] {
            "id",
            "partner_id",
            "bank_id",
            "acc_number",
            "acc_holder_name",
            "type_account",
            "use_bank_type",
            "currency_id",
            "allow_out_payment"
        };

        public HubCuentas(AppSession _setAppSession) : base(_setAppSession)
        {
            EndPointApi = "/web/dataset/call_kw";
            _modelname = "res.partner.bank";
        }

        public async Task<ApiResponseOdooRpcT<res_partner_bank[]>?> GetAll(string accounts_journal_ids)
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
                new object[] {
                    "id","in",accounts_journal_ids
                },
            };
            return await SearchRead<ApiResponseOdooRpcT<res_partner_bank[]>>(args, _custom_args, kwargs);
        }

        public async Task<ApiResponseOdooRpcT<res_partner_bank[]>?> GetAll()
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
                
            };
            return await SearchRead<ApiResponseOdooRpcT<res_partner_bank[]>>(args, _custom_args, kwargs, true);
        }

        public async Task<ApiResponseOdooRpcT<res_partner_bank[]>?> Get(res_partner_bank_send new_partner_bank)
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
                new object[] { "acc_number","=",new_partner_bank.acc_number },
                new object[] { "bank_id.id", "=",new_partner_bank.bank_id },
                new object[] { "partner_id.id", "=",new_partner_bank.partner_id },
            };
            return await SearchRead<ApiResponseOdooRpcT<res_partner_bank[]>>(args, _custom_args, kwargs);
        }

        public async Task<ApiResponseOdooRpcT<int>?> CreateIfNotExists(res_partner_bank_send new_partner_bank)
        {
            var dataPrevious = await Get(new_partner_bank);
            //dataPrevious is { result.Length: > 0 }
            //if (dataPrevious is { success: true, data.Length: > 0 })

            if (dataPrevious.result.Length > 0 )
            {  

                ApiResponseOdooRpcT<int> resultTask = new ApiResponseOdooRpcT<int>()
                {
                    id = 0,
                    result = 0,
                    jsonrpc = "2.0",
                    error = new Error()
                    {
                        message = "Cuenta bancaria ya existente, no se creará nueva."
                    }
                };

                return resultTask;
            }

            var kwargs = new { };

            var settings = new JsonSerializerSettings
            {
                DateFormatString = "yyyy-MM-dd HH:mm:ss",
            };

            var serialized = JsonConvert.SerializeObject(new_partner_bank, settings);

            var newJObject = JObject.Parse(serialized);

            object[] args = new object[] { newJObject };

            return await Create<ApiResponseOdooRpcT<int>>(args, kwargs);
        }
    }
}
