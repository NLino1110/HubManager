using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text.Json;
using System.Threading.Tasks;
using CobranzasDMSA_Odoo.Models;
using DMSA.Models.Clientes;
using DMSA.Models.General;
using DMSA.Models.Security;
//using Microsoft.AspNetCore.Components;
//using Microsoft.Extensions.Configuration;
//using Microsoft.Extensions.Logging;
//using Microsoft.Extensions.Options;
using RestSharp;

namespace ApiManager
{
    public class HubStatic
    {
        string EndPointServer = "";
        string EndPointApi = "/api/Aprobaciones";

        readonly RestClient _client;
        private AppSession _appSession { get; }

        //[Inject]
        //private IConfiguration configuration { get; set; }

        //private static readonly IConfiguration config = new ConfigurationBuilder().AddJsonFile("appsettings.json").AddEnvironmentVariables().Build();

        public HubStatic(AppSession _setAppSession)//(IOptions<AppSettings> appSettings)
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

        public async Task<byte[]> GetBytesFromUrlAsync(string UrlResource)
        {
            RestClientOptions restClientOptions = new RestClientOptions();
            restClientOptions.RemoteCertificateValidationCallback += (sender, certificate, chain, sslPolicyErrors) => true;            
            restClientOptions.BaseUrl = new Uri($"{_appSession.odooConnection.HostDump}");

            RestClient restClient = new RestClient(restClientOptions);

            var responseFile = await restClient.ExecuteAsync(new RestRequest(UrlResource, Method.Get));            
            var fileBytes = responseFile.RawBytes;

            return fileBytes;
        }
    }
}
