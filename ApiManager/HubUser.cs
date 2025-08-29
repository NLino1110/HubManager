using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text.Json;
using System.Threading.Tasks;
using DMSA.Models.General;
using DMSA.Models.General.Responses;
using Models.DMSA.Mbw.Clientes;
using Models.DMSA.Mbw.Security;
using Models.DMSA.Shared.General;
using Models.DMSA.Shared.Security;

using RestSharp;

namespace ApiManager
{
    public class HubUser
    {
        string EndPointServer = "";
        string EndPointApi = "/api/Aprobaciones";

        readonly RestClient _client;
        private AppSession _appSession { get; }

        //[Inject]
        //private IConfiguration configuration { get; set; }

        //private static readonly IConfiguration config = new ConfigurationBuilder().AddJsonFile("appsettings.json").AddEnvironmentVariables().Build();

        public HubUser(AppSession _setAppSession)//(IOptions<AppSettings> appSettings)
        {

            _appSession = _setAppSession;

            EndPointServer = "http://192.168.0.150:8080";
            EndPointApi = EndPointServer + EndPointApi;

            RestClientOptions restClientOptions = new RestClientOptions();
            restClientOptions.RemoteCertificateValidationCallback += (sender, certificate, chain, sslPolicyErrors) => true;
            restClientOptions.BaseUrl = new Uri($"{EndPointApi}");

            //Vtex_Tools.Code.Tools.ConfigurationHelper configurationHelper = new Vtex_Tools.Code.Tools.ConfigurationHelper();
            //_appSettings = appSettings.Value;
            
            _client = new RestClient(restClientOptions);
            _client.AddDefaultHeader(KnownHeaders.Accept, "application/json");

            ServicePointManager.ServerCertificateValidationCallback += (sender, certificate, chain, sslPolicyErrors) => true;
            //ServicePointManager.ServerCertificateValidationCallback = ValidateServerCertficate;
            
        }

        public Task<RestResponse> ExecuteGetAsync(string EndPoint)
        {
            var taskSource = new TaskCompletionSource();
            var restRequest = new RestRequest(EndPointApi);
            //restRequest.RequestFormat = DataFormat.Json;
            //restRequest.AddBody(bodyObject);

            //if (headers_priv.Count > 0)
            //{
            //    foreach (KeyValuePair<string, string> kvp in headers_priv)
            //    {
            //        //Console.WriteLine("Key = {0}, Value = {1}",
            //        //    kvp.Key, kvp.Value);
            //        restRequest.AddHeader(kvp.Key, kvp.Value);
            //    }
            //}

            return _client.ExecuteGetAsync(restRequest);
        }

        public Task<ApiResponse_v2?> GetAll()
        {
            //ApiResponse_v1 apiResponse_V1 = new ApiResponse_v1();
            return _client.GetAsync<ApiResponse_v2>(new RestRequest(EndPointApi));
        }

        public Task<ClienteAprobacion[]?> GetById(string id)
        {
            return _client.GetAsync<ClienteAprobacion[]>(new RestRequest(EndPointApi + "?id=" + id));
        }

        public Task<ClienteAprobacion?> Add(ClienteAprobacion requestObject)
        {
            var restRequest = new RestRequest(EndPointApi);
            restRequest.RequestFormat = DataFormat.Json;
            restRequest.AddJsonBody(requestObject);

            return _client.PostAsync<ClienteAprobacion>(restRequest);
        }
        
        public Task<ClienteAprobacion?> Update(ClienteAprobacion requestObject)
        {
            var restRequest = new RestRequest(EndPointApi);
            restRequest.RequestFormat = DataFormat.Json;
            restRequest.AddJsonBody(requestObject);

            return _client.PutAsync<ClienteAprobacion>(restRequest);
        }

        public async Task<User?> TryLogin(User requestObject, DateTime currentDate)
        {
            string Action = "VERIFICAR_USUARIO_WSJSON";
            string currentDateTime = currentDate.ToString("yyyy-MM-dd HH:mm:ss");
            string cadenaJson = "{\"codusuario\":\"" + requestObject.codusuario + "\",\"codclave\":\"" + requestObject.clave + "\",\"fechatablet\":\"" + currentDateTime + "\"}";

            List<RestSharp.Parameter> parameters = new List<RestSharp.Parameter>();

            parameters.Add(RestSharp.Parameter.CreateParameter("accion", Action, ParameterType.QueryString));
            parameters.Add(RestSharp.Parameter.CreateParameter("cadenaJson", cadenaJson, ParameterType.QueryString));

            //App.Current.MainPage = new MainPage();
            SoapClient client = new SoapClient(_appSession);
            var result = await client.asyncPostJson(parameters.ToArray());
            Console.WriteLine(result);

            if (result != "")
            {
                var resultUser = Newtonsoft.Json.JsonConvert.DeserializeObject<User>(result);
                return resultUser;
            }

            return null;
        }

        public async Task<ApiResponse_VALIDASINCRONIZACION?> ValidaSincronizacionAsync(string codusuario, DateTime currentDate)
        {
            //Solo se evalúa fechatablet en el body de cadenaJson
            string Action = "VALIDASINCRONIZACION";
            //string currentDateTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            string currentDateTime = currentDate.ToString("yyyy-MM-dd HH:mm:ss");
            string cadenaJson = "{\"fechatablet\":\"" + currentDateTime + "\"}";

            List<RestSharp.Parameter> parameters = new List<RestSharp.Parameter>();

            parameters.Add(RestSharp.Parameter.CreateParameter("codusuario", codusuario, ParameterType.QueryString));
            parameters.Add(RestSharp.Parameter.CreateParameter("accion", Action, ParameterType.QueryString));
            parameters.Add(RestSharp.Parameter.CreateParameter("cadenaJson", cadenaJson, ParameterType.QueryString));

            //App.Current.MainPage = new MainPage();
            SoapClient client = new SoapClient(_appSession);
            var result = await client.asyncPostJson(parameters.ToArray());
            Console.WriteLine(result);

            if (result != "")
            {
                var resultUser = Newtonsoft.Json.JsonConvert.DeserializeObject<ApiResponse_VALIDASINCRONIZACION>(result);
                return resultUser;
            }

            return null;
        }
    }
}
