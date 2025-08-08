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
//using Microsoft.AspNetCore.Components;
//using Microsoft.Extensions.Configuration;
//using Microsoft.Extensions.Logging;
//using Microsoft.Extensions.Options;
using RestSharp;

namespace ApiManager
{
    public class HubTiposNc
    {
        string EndPointServer = "";
        //string EndPointApi = "/api/pos.payment.method"; //Se espera que este sea el endpoint definitivo
        string EndPointApi = "/connect/get_tipos_nc"; //Se espera que este sea el endpoint definitivo

        readonly RestSharpMiddle _client;
        private AppSession _appSession { get; }

        public HubTiposNc(AppSession _setAppSession)
        {
            _appSession = _setAppSession;                        
            _client = new RestSharpMiddle(_setAppSession);
        }

        public void setApiKey(string apikey)
        {
            if(_appSession.CurrentUser==null)
            {
                _appSession.CurrentUser = new User();
            }

            _appSession.CurrentUser.api_key = apikey;
        }

        public async Task<ApiResponseTiposNc?> GetAll()
        {
            ////App.Current.MainPage = new MainPage();
            string api_key = $"api_key={_appSession.CurrentUser.api_key}";
            //string EndPointParams = $"/search?api_key={api_key}&fields=['id','name']";
            string EndPointParams = $"?{api_key}&codusuario=0&fecha_tablet=0";

            var restRequest = new RestRequest(EndPointApi + EndPointParams);
            restRequest.RequestFormat = DataFormat.Json;
            var result = await _client.RestClient().ExecuteGetAsync(restRequest); ;
            Console.WriteLine(result);

            if (result != null && result.Content != null & result.Content != "")
            {
                var resultApi = Newtonsoft.Json.JsonConvert.DeserializeObject<ApiResponseTiposNc>(result.Content);
                return resultApi;
            }

            return null;
        }        
    }
}
