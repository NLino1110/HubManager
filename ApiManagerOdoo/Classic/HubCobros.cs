using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text.Json;
using System.Threading.Tasks;
using CobranzasDMSA_Odoo.Models;
using DMSA.Models.Clientes;
using DMSA.Models.General;
using DMSA.Models.General.Requests;
using DMSA.Models.General.Responses;
using DMSA.Models.Security;
//using Microsoft.AspNetCore.Components;
//using Microsoft.Extensions.Configuration;
//using Microsoft.Extensions.Logging;
//using Microsoft.Extensions.Options;
using RestSharp;

namespace ApiManager
{
    [Obsolete]
    public class HubCobros
    {
        string EndPointServer = "";
        string EndPointApi = "/api/Aprobaciones";

        readonly RestClient _client;
        private AppSession _appSession { get; }

        //[Inject]
        //private IConfiguration configuration { get; set; }

        //private static readonly IConfiguration config = new ConfigurationBuilder().AddJsonFile("appsettings.json").AddEnvironmentVariables().Build();

        public HubCobros(AppSession _setAppSession)//(IOptions<AppSettings> appSettings)
        {
            _appSession = _setAppSession;

            EndPointServer = "https://192.168.204.108:5100";
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="requestObject"></param>
        /// <returns></returns>
        public async Task<ApiResponse_GUARDAR_COBRO_CXC> Send(ApiRequest_v1 requestObject)
        {            
            string Action = "GUARDAR_COBRO_CXC";
            string currentDateTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            //string cadenaJson = requestObject.cadenaJson;

            List<RestSharp.Parameter> parameters = new List<RestSharp.Parameter>();

            parameters.Add(RestSharp.Parameter.CreateParameter("accion", Action, ParameterType.QueryString));
            parameters.Add(RestSharp.Parameter.CreateParameter("codusuario", requestObject.uid, ParameterType.QueryString));
            parameters.Add(RestSharp.Parameter.CreateParameter("cadenaJson", requestObject.cadenaJson, ParameterType.QueryString));

            //App.Current.MainPage = new MainPage();
            SoapClient client = new SoapClient(_appSession);
            var result = await client.asyncPostJson(parameters.ToArray());
            Debug.WriteLine(result);

            if (result != "")
            {                
                var _resResponse = Newtonsoft.Json.JsonConvert.DeserializeObject<ApiResponse_GUARDAR_COBRO_CXC>(result);
                return _resResponse;
            }

            return null;
        }

        
    }
}
