
using ApiManagerOdoo.Base;
using DMSA.Models.Odoo.General.Requests;
using DMSA.Models.Odoo.General.Responses;
using DMSA.Models.Odoo.Import;
using DMSA.Models.Odoo.Native;
using DMSA.Models.Odoo.Tools;
using DMSA.Models.Security;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;

namespace ApiManager
{
    public class HubAccountMoveTransientLegacy : HubBase
    {
        public HubAccountMoveTransientLegacy(AppSession _setAppSession) : base(_setAppSession)
        {
            EndPointApi = "/web/dataset/call_kw";
            _modelname = "am.transient.legacy";
        }

        public async Task<ApiResponseOdooRpcT<int>?> Create(List<InvoiceHeader> SendObject, int company_id)
        {
            var kwargs = new { };

            var settings = new JsonSerializerSettings
            {
                DateFormatString = "yyyy-MM-dd HH:mm:ss",
                //ContractResolver = new IncludeJsonIgnoreResolver(new string[] { "was_odoo_synced", "lines" })
            };

            var serialized = JsonConvert.SerializeObject(SendObject, settings);

            var newJObject = JObject.Parse(serialized);

            object[] args = new object[] { newJObject };
            return await Create<ApiResponseOdooRpcT<int>>(args, kwargs);
        }

        public async Task<ApiResponseOdooRpcT<int>?> CreateDetails(List<InvoiceDetails> SendObject, int company_id)
        {
            //apiRequestOdooRpc_For_Dataset._params.kwargs.context.allowed_company_ids = new int[] { company_id };

            var kwargs = new { };

            var settings = new JsonSerializerSettings
            {
                DateFormatString = "yyyy-MM-dd HH:mm:ss",
                //ContractResolver = new IncludeJsonIgnoreResolver(new string[] { "was_odoo_synced", "lines" })
            };

            var serialized = JsonConvert.SerializeObject(SendObject, settings);

            var newJObject = JObject.Parse(serialized);

            object[] args = new object[] { newJObject };
            return await Create<ApiResponseOdooRpcT<int>>(args, kwargs, "am.transient.line.legacy");

        }

        public async Task<ApiResponseOdooRpcT<int>?> CreatePayments(List<InvoicePayments> SendObject, int company_id)
        {
            //apiRequestOdooRpc_For_Dataset._params.kwargs.context.allowed_company_ids = new int[] { company_id };

            var kwargs = new { };

            var settings = new JsonSerializerSettings
            {
                DateFormatString = "yyyy-MM-dd HH:mm:ss",
                //ContractResolver = new IncludeJsonIgnoreResolver(new string[] { "was_odoo_synced", "lines" })
            };

            var serialized = JsonConvert.SerializeObject(SendObject, settings);

            var newJObject = JObject.Parse(serialized);

            object[] args = new object[] { newJObject };
            return await Create<ApiResponseOdooRpcT<int>>(args, kwargs, "am.transient.payment.legacy");
        }
    }
}
