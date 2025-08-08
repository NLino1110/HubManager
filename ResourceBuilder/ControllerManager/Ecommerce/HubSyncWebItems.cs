using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text.Json;
using System.Threading.Tasks;
using AppManagerOdoo.Tools;
using CobranzasDMSA.Models;
using DMSA.Models.Clientes;
using DMSA.Models.General;
using DMSA.Models.General.Responses;
using DMSA.Models.Odoo.General.Responses;
using DMSA.Models.Security;
using Models.DMSA.Mbw.Abstract;
using Models.DMSA.Shared.Security;
using Newtonsoft.Json;

//using Microsoft.AspNetCore.Components;
//using Microsoft.Extensions.Configuration;
//using Microsoft.Extensions.Logging;
//using Microsoft.Extensions.Options;
using RestSharp;

namespace ResourceBuilder.ControllerManager.Ecommerce
{
    [Obsolete("Debe ser eliminado....")]
    public class HubSyncWebItems
    {
        string EndPointServer = "https://127.0.0.1:5200";
        string EndPointApi = "/api/SyncWebItems";

        readonly RestSharp.RestClient _client;
        private DMSA.Models.Security.AppSession _appSession { get; }
        private Profile _profile { get; }

        public HubSyncWebItems(DMSA.Models.Security.AppSession _setAppSession)
        {
            _appSession = _setAppSession;
            _client = new RestClient();
        }

        public HubSyncWebItems(Profile profile)
        {
            _profile = profile;
            _client = new RestClient();

            EndPointServer = _profile.ApiTradeHub;
        }
        
        public async Task<string> SendForUpdate(ParametersMode1 parametersMode)
        {
            var restRequest = new RestRequest(EndPointServer + EndPointApi);
            restRequest.RequestFormat = DataFormat.Json;
            restRequest.AddHeader("X-API-Key", "9+7e3A7t4qI1Rl8XQ2GjKjs8KhZ9Y8p1MfbQvKlkmP4=");
           
            restRequest.AddBody(JsonConvert.SerializeObject(parametersMode));

            //var result = await _client.ExecuteGetAsync(restRequest);
            var result = await _client.ExecutePostAsync(restRequest);
            //var result = await _client.ExecutePutAsync(restRequest);
            //Console.WriteLine(result);

            if (result?.Content != null && !string.IsNullOrEmpty(result.Content))
            {
                //var resultApi = JsonConvert.DeserializeObject<dynamic>(result.Content);
                //return resultApi;
                return result.Content;
            }

            return null;
        }

        public async Task<string> GetData(ParametersMode1 parametersMode)
        {
            var restRequest = new RestRequest(EndPointServer + EndPointApi);
            restRequest.RequestFormat = DataFormat.Json;
            restRequest.AddHeader("X-API-Key", "9+7e3A7t4qI1Rl8XQ2GjKjs8KhZ9Y8p1MfbQvKlkmP4=");

            restRequest.AddBody(JsonConvert.SerializeObject(parametersMode));

            var result = await _client.ExecuteGetAsync(restRequest);
            
            //Console.WriteLine(result);

            if (result?.Content != null && !string.IsNullOrEmpty(result.Content))
            {
                //var resultApi = JsonConvert.DeserializeObject<dynamic>(result.Content);
                //return resultApi;
                return result.Content;
            }

            return null;
        }
    }
}
