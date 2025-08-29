using DMSA.Models.General;
using Models.DMSA.Mbw.Clientes;
using Models.DMSA.Mbw.Security;
using Models.DMSA.Shared.General;
using Models.DMSA.Shared.Security;
//using Microsoft.AspNetCore.Components;
//using Microsoft.Extensions.Configuration;
//using Microsoft.Extensions.Logging;
//using Microsoft.Extensions.Options;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text.Json;
using System.Threading.Tasks;

namespace ApiManager
{
    public class HubClienteAprobacion
    {
        string EndPointServer = "https://192.168.0.127:5100";

        string EndPointApi = "/api/Aprobaciones";
        
        private AppSession _appSession { get; }
        readonly RestClient _client;

        //public AppSettings _appSettings { get; }
        //[Inject]
        //private IConfiguration configuration { get; set; }

        //private static readonly IConfiguration config = new ConfigurationBuilder().AddJsonFile("appsettings.json").AddEnvironmentVariables().Build();

        public HubClienteAprobacion(AppSession _setAppSession)//(IOptions<AppSettings> appSettings)
        {
            _appSession = _setAppSession;

            EndPointServer = _appSession.EndPointServerNewApi;
            EndPointApi = EndPointServer + EndPointApi;

            //targetUri = appSession.EndPointServer;

            RestClientOptions restClientOptions = new RestClientOptions();
            restClientOptions.RemoteCertificateValidationCallback += (sender, certificate, chain, sslPolicyErrors) => true;
            restClientOptions.BaseUrl = new Uri($"{EndPointApi}");
            //restClientOptions.MaxTimeout = 6000;
            restClientOptions.Timeout = TimeSpan.FromMilliseconds(30000);

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

        public Task<ApiResponseClienteAprobacion?> GetAll()
        {
            //ApiResponse_v1 apiResponse_V1 = new ApiResponse_v1();
            return _client.GetAsync<ApiResponseClienteAprobacion>(new RestRequest(EndPointApi));
        }

        [Obsolete]
        public Task<ApiResponseClienteAprobacion?> GetForAgree()
        {
            //ApiResponse_v1 apiResponse_V1 = new ApiResponse_v1();
            return _client.GetAsync<ApiResponseClienteAprobacion>(new RestRequest(EndPointApi+ "/GetByStatus/54"));
        }

        public Task<ApiResponseClienteAprobacion?> GetForAgree(string codusuario, int codstatus)
        {
            //ApiResponse_v1 apiResponse_V1 = new ApiResponse_v1();
            string fullEndPoint = EndPointApi + $"/GetFromFacPedido/{codusuario}/{codstatus}";
            return _client.GetAsync<ApiResponseClienteAprobacion>(new RestRequest(fullEndPoint));
        }

        public Task<ApiResponseClienteAprobacion?> GetForAgree(int codagencia, int codstatus, string textobusqueda)
        {
            //ApiResponse_v1 apiResponse_V1 = new ApiResponse_v1();
            string fullEndPoint = EndPointApi + $"/GetFromFacPedidoAgencia/{codagencia}/{codstatus}/{textobusqueda}";
            return _client.GetAsync<ApiResponseClienteAprobacion>(new RestRequest(fullEndPoint));
        }

        public Task<ClienteAprobacion[]?> GetById(string id)
        {
            return _client.GetAsync<ClienteAprobacion[]>(new RestRequest(EndPointApi + "?id=" + id));
        }

        public Task<ApiResponse_v1?> Add(ClienteAprobacion requestObject)
        {
            var restRequest = new RestRequest(EndPointApi);
            restRequest.RequestFormat = DataFormat.Json;
            restRequest.AddJsonBody(requestObject);

            return _client.PostAsync<ApiResponse_v1>(restRequest);
        }

        public Task<ApiResponse_v1?> AddWithFull(ClienteAprobacion requestObject)
        {
            var restRequest = new RestRequest(EndPointApi+ "/PostFull");
            restRequest.RequestFormat = DataFormat.Json;
            restRequest.AddJsonBody(requestObject);

            return _client.PostAsync<ApiResponse_v1>(restRequest);
        }

        public Task<ClienteAprobacion?> Update(ClienteAprobacion requestObject)
        {
            var restRequest = new RestRequest(EndPointApi);
            restRequest.RequestFormat = DataFormat.Json;
            restRequest.AddJsonBody(requestObject);

            return _client.PutAsync<ClienteAprobacion>(restRequest);
        }
    }
}
