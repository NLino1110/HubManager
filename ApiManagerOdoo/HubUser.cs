
using ApiManagerOdoo.Base;
using AppManagerOdoo.Tools;
using DMSA.Models.Clientes;
using DMSA.Models.General;
using DMSA.Models.General.Requests;
using DMSA.Models.General.Responses;
using DMSA.Models.Odoo.General.Responses;
using DMSA.Models.Odoo.Native;
using DMSA.Models.Security;
using Newtonsoft.Json;

using Newtonsoft.Json.Linq;

//using Microsoft.AspNetCore.Components;
//using Microsoft.Extensions.Configuration;
//using Microsoft.Extensions.Logging;
//using Microsoft.Extensions.Options;
using RestSharp;

namespace ApiManager
{
    public class HubUser : HubBase
    {        
        string[] fields_array = new[] {
                "login",
                "name",                
                "complete_name",
                "lang", 
                "login_date", 
                "company_id", 
                "partner_id", 
                "company_ids", 
                "sale_team_id"
        };
        
        public HubUser(AppSession _setAppSession) : base(_setAppSession)
        {
            EndPointApi = "/web/dataset/call_kw";
            _modelname = "res.users";
        }

        public void setApiKey(string apikey)
        {
            if (_appSession.CurrentUser == null)
            {
                _appSession.CurrentUser = new User();
            }

            _appSession.CurrentUser.api_key = apikey;
        }
        
        //public Task<ApiResponse_v2?> GetAll()
        //{
        //    //ApiResponse_v1 apiResponse_V1 = new ApiResponse_v1();
        //    //return _client.RestClient().GetAsync<ApiResponse_v2>(new RestRequest(EndPointApi));
        //    return null;
        //}

        public async Task<ApiResponseOdooRpcT<res_user[]>?> GetById(int id)
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
                    "id", "=", id,
                    },
            };
            return await SearchRead<ApiResponseOdooRpcT<res_user[]>>(args, _custom_args, kwargs);
        }

        public async Task<ApiResponseOdooRpcT<res_user[]>?> GetItems()
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
            return await SearchRead<ApiResponseOdooRpcT<res_user[]>>(args, _custom_args, kwargs, true);
        }

        //public Task<ClienteAprobacion?> Add(ClienteAprobacion requestObject)
        //{
        //    var restRequest = new RestRequest(EndPointApi);
        //    restRequest.RequestFormat = DataFormat.Json;
        //    restRequest.AddJsonBody(requestObject);

        //    return _client.RestClient().PostAsync<ClienteAprobacion>(restRequest);
        //}
        
        //public Task<ClienteAprobacion?> Update(ClienteAprobacion requestObject)
        //{
        //    var restRequest = new RestRequest(EndPointApi);
        //    restRequest.RequestFormat = DataFormat.Json;
        //    restRequest.AddJsonBody(requestObject);

        //    return _client.RestClient().PutAsync<ClienteAprobacion>(restRequest);
        //}
        
        [Obsolete("Debe ser eliminado.")]
        public async Task<ApiResponseOdooRpc?> TryLoginRpc(User requestObject, DateTime currentDate)
        {
            string currentDateTime = currentDate.ToString("yyyy-MM-dd HH:mm:ss");
            
            //List<RestSharp.Parameter> parameters = new List<RestSharp.Parameter>();
            //parameters.Add(RestSharp.Parameter.CreateParameter("accion", Action, ParameterType.QueryString));
            //parameters.Add(RestSharp.Parameter.CreateParameter("cadenaJson", bodyJson, ParameterType.QueryString));

            //_client.BuilClient();
            //App.Current.MainPage = new MainPage();
            //SoapClient client = new SoapClient(_appSession);
            var restRequest = new RestRequest("/jsonrpc");
            restRequest.RequestFormat = DataFormat.Json;
            restRequest.AddJsonBody(@"{
                  ""jsonrpc"": ""2.0"",
                  ""method"": ""call"",
                  ""params"": {
                    ""service"": ""common"",
                    ""method"": ""login"",
                    ""args"": [""NOMBRE_DE_BASE_DE_DATOS"", ""USUARIO"", ""PASSWORD""]
                  },
                  ""id"": 1
                }");

            //var result = await _client.RestClient().ExecutePostAsync(restRequest); ;
            //Console.WriteLine(result);

            //if (result != null && result.Content != null & result.Content != "")
            //{
            //    var resultUser = Newtonsoft.Json.JsonConvert.DeserializeObject<ApiResponseOdooRpc>(result.Content);
            //    return resultUser;
            //}

            return null;
        }

        public async Task<ResponseAuthenticate?> TryLoginRpcWeb(User user, DateTime currentDate)
        {
            ////string currentDateTime = currentDate.ToString("yyyy-MM-dd HH:mm:ss");

            ////string dbName = user.databasename;
            ////string login = user.username;
            ////string password = user.codclave;

            ////var restRequest = new RestRequest("/web/session/authenticate");
            ////restRequest.RequestFormat = DataFormat.Json;
            ////restRequest.AddJsonBody($@"
            ////    {{
            ////      ""jsonrpc"": ""2.0"",
            ////      ""method"": ""call"",
            ////      ""params"" : {{
            ////            ""db"":  ""{dbName}"",
            ////            ""login"": ""{login}"",
            ////            ""password"": ""{password}""
            ////        }}
            ////    }}");

            ////var result = await _client.RestClient().ExecutePostAsync(restRequest); ;
            //////Console.WriteLine(result);

            ////if (result != null && result.Content != null & result.Content != "")
            ////{
            ////    try
            ////    {
            ////        var resultUser = Newtonsoft.Json.JsonConvert.DeserializeObject<ResponseAuthenticate>(result.Content);
            ////        if (resultUser != null && result.Cookies != null)
            ////        {
            ////            resultUser.Cookies = result.Cookies;
            ////        }
            ////        return resultUser;
            ////    }
            ////    catch(Exception err)
            ////    {
            ////        Console.WriteLine("Error HubUser TryLoginRpcWeb:" + err.Message);
            ////    }
            ////}

            ////return null;


            return await Authenticate<ResponseAuthenticate>(user.databasename, user.username, user.codclave);
        }

        public async Task<ApiResponse_VALIDASINCRONIZACION?> ValidaSincronizacionAsync(User user, DateTime currentDate)
        {
            string EndPointApiLine = $"/connect/validasincronizacion?domain=[('id', '=', {user.uid})]";
                        
            string currentDateTime = currentDate.ToString("yyyy-MM-dd HH:mm:ss");
            string cadenaJson = "{\"fechatablet\":\"" + currentDateTime + "\"}";
            
            var kwargs = new { };

            var settings = new JsonSerializerSettings
            {
                DateFormatString = "yyyy-MM-dd HH:mm:ss",
                //ContractResolver = new IncludeJsonIgnoreResolver(new string[] { "was_odoo_synced", "lines" })
            };

            //var serialized = JsonConvert.SerializeObject(SendObject, settings);

            //var newJObject = JObject.Parse(serialized);

            //object[] args = new object[] { newJObject };
            Object value = null;

            return await Call<Object, ApiResponse_VALIDASINCRONIZACION>(EndPointApiLine,
                Method.Get, value, false);
        }

        public async Task<ApiResponse_ACTUALIZAFECHASINCRO_NC?> actualizaFechaSincroNotaCredito(User user, DateTime currentDate)
        {
            string currentDateTime = currentDate.ToString("yyyy-MM-dd HH:mm:ss");
            string fecha_tablet = $"fecha_tablet={currentDateTime}";
            string codusuario = $"codusuario={user.uid}";

            string EndPointApiLine = $"/connect/actualizasincronizacion_nc?&{codusuario}&{fecha_tablet}";
            
            string cadenaJson = "{\"fechatablet\":\"" + currentDateTime + "\"}";

            var kwargs = new { };

            var settings = new JsonSerializerSettings
            {
                DateFormatString = "yyyy-MM-dd HH:mm:ss",
                //ContractResolver = new IncludeJsonIgnoreResolver(new string[] { "was_odoo_synced", "lines" })
            };

            //var serialized = JsonConvert.SerializeObject(SendObject, settings);

            //var newJObject = JObject.Parse(serialized);

            //object[] args = new object[] { newJObject };
            Object value = null;

            return await Call<Object, ApiResponse_ACTUALIZAFECHASINCRO_NC>(EndPointApiLine,
                Method.Get, value, false);
        }


    }
}
