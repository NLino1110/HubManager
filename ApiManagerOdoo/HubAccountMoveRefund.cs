using ApiManagerOdoo.Base;
using DMSA.Models.Clientes;
using DMSA.Models.General;
using DMSA.Models.General.Requests;
using DMSA.Models.General.Responses;
using DMSA.Models.Odoo.Accounting;
using DMSA.Models.Odoo.General.Requests;
using DMSA.Models.Odoo.General.Responses;
using DMSA.Models.Odoo.Native;
using DMSA.Models.Odoo.Tools;
using DMSA.Models.Security;
using Newtonsoft.Json;

using Newtonsoft.Json.Linq;

using RestSharp;

namespace ApiManager
{
    public class HubAccountMoveRefund : HubBase
    {
        string[] fields_array = new[] {
                "id",
                "name",
                "street",
                "street2",
                "city"
                };

        public HubAccountMoveRefund(AppSession _setAppSession) : base(_setAppSession)
        {
            EndPointApi = "/web/dataset/call_kw";
            _modelname = "account.move";            
        }
        
        public async Task<ApiResponseOdooRpcT<AccountPayment[]>?> GetAll(string bank_ids)
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
                new object[] {
                    "id", "in", $"[{bank_ids}]",
                    //"create_date", ">=", $"{year}-{month:00}-{day:00} 00:00:00",                
                    },
            };
            return await SearchRead<ApiResponseOdooRpcT<AccountPayment[]>>(args, _custom_args, kwargs);
        }

        public async Task<ApiResponseOdooRpcT<int>?> SendHeader_Mode_Dataset(account_move_send SendObject) //, account_move_line_send LinesSendObject)
        {
            //ODOO-18:
            //TODO: Talvez falten definir los argumentos como el modo viejo
            //ApiRequestOdooRpc_for_dataset apiRequestOdooRpc_For_Dataset = new ApiRequestOdooRpc_for_dataset();

            //apiRequestOdooRpc_For_Dataset.id = 88877;
            //apiRequestOdooRpc_For_Dataset.jsonrpc = "2.0";
            //apiRequestOdooRpc_For_Dataset.method = "call";
            //apiRequestOdooRpc_For_Dataset._params = new Params();
            //apiRequestOdooRpc_For_Dataset._params.args = new object[] { SendObject };
            //apiRequestOdooRpc_For_Dataset._params.model = "account.move";
            //apiRequestOdooRpc_For_Dataset._params.method = "create";
            //apiRequestOdooRpc_For_Dataset._params.kwargs = new Kwargs();
            //apiRequestOdooRpc_For_Dataset._params.kwargs.context = new DMSA.Models.Odoo.General.Requests.Context();
            //apiRequestOdooRpc_For_Dataset._params.kwargs.context.lang = "es_EC";
            //apiRequestOdooRpc_For_Dataset._params.kwargs.context.tz = "America/Guayaquil";

            ////Parametro para definir el ambito de la operación
            //apiRequestOdooRpc_For_Dataset._params.kwargs.context.uid = _appSession.CurrentUser.uid;
            //apiRequestOdooRpc_For_Dataset._params.kwargs.context.allowed_company_ids = new int[] { SendObject.company_id };
     
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

        public async Task<ApiResponseOdooRpcT<int>?> SendHeader(account_move_send SendObject)
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

            return await Create<ApiResponseOdooRpcT<int>>(args, kwargs, "account.move.send");
        }

        public async Task<ApiResponseOdooRpcT<int>?> SendLine(account_move_line_send SendObject)
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

            return await Create<ApiResponseOdooRpcT<int>>(args, kwargs, "account.move.line");
        }


        public async Task<ApiResponseOdooRpcT<check_cn_response[]>?> check_credit_note_overdraft(ApiRequestCheckCn SendObject)
        {
            string EndPointApiLine = "/connect/check_credit_note_overdraft";                 
         
            var kwargs = new { };

            var settings = new JsonSerializerSettings
            {
                DateFormatString = "yyyy-MM-dd HH:mm:ss",
                //ContractResolver = new IncludeJsonIgnoreResolver(new string[] { "was_odoo_synced", "lines" })
            };

            var serialized = JsonConvert.SerializeObject(SendObject, settings);

            var newJObject = JObject.Parse(serialized);

            object[] args = new object[] { newJObject };

            return await Call<ApiRequestCheckCn, ApiResponseOdooRpcT<check_cn_response[]>>(EndPointApiLine, 
                Method.Post, SendObject, true);
        }
    }
}
