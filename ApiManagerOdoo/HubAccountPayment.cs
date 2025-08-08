using ApiManagerOdoo.Base;
using AppManagerOdoo.Tools;
using CobranzasDMSA_Odoo.Models;
using DMSA.Models.Odoo.General.Requests;
using DMSA.Models.Odoo.General.Responses;
using DMSA.Models.Odoo.Native;
using DMSA.Models.Security;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;

using System.Net;


namespace ApiManager
{
    public class HubAccountPayment : HubBase
    {
        string[] fields_array = {
                "id",
                "parent_id",
                "sequence",
                "journal_name",
                "amount",
                "create_date",
                "write_date",
                "is_from_mobile"
                };

        public HubAccountPayment(AppSession _setAppSession) : base(_setAppSession)
        {
            EndPointApi = "/web/dataset/call_kw";
            _modelname = "account.payment";
        }

        public async Task<ApiResponseOdooRpcT<AccountPayment[]>?> GetAll(string bank_ids)
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
                    "id", "in", bank_ids,                      
                    },
            };
            return await SearchRead<ApiResponseOdooRpcT<AccountPayment[]>>(args, _custom_args, kwargs);
        }

        public async Task<ApiResponseOdooRpcT<int>?> Send(AccountPayment SendObject)
        {
            //Nuevo campo
            SendObject.is_from_mobile = true;
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

        public async Task<ApiResponseOdooRpcT<int>?> call_button(
            AccountPayment SendObject, 
            ApiResponseOdooRpcT<int> apiResponseOdooRpc)
        {

            SendObject.is_from_mobile = true;
            var kwargs = new { };

            var settings = new JsonSerializerSettings
            {
                DateFormatString = "yyyy-MM-dd HH:mm:ss",
            };

            var serialized = JsonConvert.SerializeObject(SendObject, settings);

            var newJObject = JObject.Parse(serialized);

            object[] args = new object[] { apiResponseOdooRpc.result };

            return await CallMethod<ApiResponseOdooRpcT<int>>("/web/dataset/call_button", 
                Method.Post,
                args, 
                kwargs, "account.payment", "action_post");

            //////ApiRequestOdooRpc_for_dataset apiRequestOdooRpc_For_Dataset = new ApiRequestOdooRpc_for_dataset();

            //////apiRequestOdooRpc_For_Dataset.id = 98877;
            //////apiRequestOdooRpc_For_Dataset.jsonrpc = "2.0";
            //////apiRequestOdooRpc_For_Dataset.method = "call";
            //////apiRequestOdooRpc_For_Dataset._params = new Params();
            //////apiRequestOdooRpc_For_Dataset._params.args = new object[] { apiResponseOdooRpc.result };
            //////apiRequestOdooRpc_For_Dataset._params.model = "account.payment";
            //////apiRequestOdooRpc_For_Dataset._params.method = "action_post";
            //////apiRequestOdooRpc_For_Dataset._params.kwargs = new Kwargs();
            //////apiRequestOdooRpc_For_Dataset._params.kwargs.context = new DMSA.Models.Odoo.General.Requests.Context();
            //////apiRequestOdooRpc_For_Dataset._params.kwargs.context.lang = "es_EC";
            //////apiRequestOdooRpc_For_Dataset._params.kwargs.context.tz = "America/Guayaquil";

            ////////Parametro para definir el ambito de la operación
            //////apiRequestOdooRpc_For_Dataset._params.kwargs.context.uid = _appSession.CurrentUser.uid;
            //////apiRequestOdooRpc_For_Dataset._params.kwargs.context.allowed_company_ids = new int[] { SendObject.company_id };

            ////////Se requiere obtener las cookies para saber quien es el usuario que envía los pagos
            //////ApiManager.HubUser hubUser = new ApiManager.HubUser(_appSession);
            //////User user = new User();
            //////user.username = _appSession.CurrentUser.username;
            //////user.codclave = _appSession.CurrentUser.codclave;
            //////DateTime currentDate = DateTime.Now;
            //////ApiResponseLogin responseUser = await hubUser.TryLogin(user, currentDate);

            //////if (responseUser == null)
            //////{
            //////    return new ApiResponseOdooRpc_v2()
            //////    {
            //////        jsonrpc = "2.0",
            //////        id = 0,
            //////        result = false
            //////    };
            //////}

            //////////App.Current.MainPage = new MainPage();
            //////string api_key = $"api_key={_appSession.CurrentUser.api_key}";
            //////string EndPointParams = "/web/dataset/call_button";

            //////var restRequest = await _client.BuildAuthorizedRequest(EndPointParams);
            //////restRequest.RequestFormat = DataFormat.Json;

            //////string strBody = Newtonsoft.Json.JsonConvert.SerializeObject(apiRequestOdooRpc_For_Dataset);
            //////restRequest.AddBody(strBody);

            //////////Se requiere para saber que usuario es quien envía el pago
            ////////_client.setUseAuthorization();
            ////////foreach (Cookie cookie in responseUser.Cookies)
            ////////{
            ////////    restRequest.AddCookie(cookie.Name, cookie.Value, cookie.Path, cookie.Domain);
            ////////}

            //////var result = await _client.RestClient().ExecutePostAsync(restRequest);
            //////Console.WriteLine(result);

            //////if (result.StatusCode == System.Net.HttpStatusCode.OK)
            //////{
            //////    if (result != null && result.Content != null & result.Content != "")
            //////    {
            //////        var resultApi = Newtonsoft.Json.JsonConvert.DeserializeObject<ApiResponseOdooRpc_v2>(result.Content);
            //////        return resultApi;
            //////    }
            //////}
            //////else
            //////{
            //////    var error = new DMSA.Models.Odoo.General.Responses.Error();
            //////    error.code = 600;
            //////    error.data = new Data()
            //////    {
            //////        name = "",
            //////        debug = "",
            //////        message = "",
            //////        arguments = new[] { "" }
            //////    };

            //////    error.message = "---";

            //////    return new ApiResponseOdooRpc_v2()
            //////    {
            //////        jsonrpc = "2.0",
            //////        id = 0,
            //////        result = false,
            //////        error = error
            //////    };
            //////}

            //////return null;
        }
    }
}
