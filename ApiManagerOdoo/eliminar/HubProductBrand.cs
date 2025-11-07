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
    [Obsolete("Debe ser eliminado (verificar)")]
    public class HubProductBrand
    {
        string EndPointServer = "";
        //string EndPointApi = "/api/pos.payment.method"; //Se espera que este sea el endpoint definitivo
        string EndPointApi = "/api/product.brand"; //Se espera que este sea el endpoint definitivo

        readonly RestSharpMiddle _client;
        private AppSession _appSession { get; }

        public HubProductBrand(AppSession _setAppSession)
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

        public async Task<ApiResponseProductBrand?> GetItemsById(string ids)
        {
            string api_key = $"api_key={_appSession.CurrentUser.api_key}";
            //string EndPointParams = $"/search?api_key={api_key}&fields=['id','name']";
            string fields = "fields=['id','name','description']";
            string domain = $"domain=[('id','in',[{ids}])]";
            string limit = "limit=300";
            string EndPointParams = $"/search?{api_key}&{fields}&{domain}&{limit}";

            var restRequest = new RestRequest(EndPointApi + EndPointParams);
            restRequest.RequestFormat = DataFormat.Json;
            var result = await _client.RestClient().ExecuteGetAsync(restRequest);
            Console.WriteLine(result);

            if (result != null && result.Content != null & result.Content != "")
            {
                var resultApi = Newtonsoft.Json.JsonConvert.DeserializeObject<ApiResponseProductBrand>(result.Content);
                return resultApi;
            }

            return null;
        }

        public async Task<ApiResponseProductBrand?> GetItems()
        {
            string api_key = $"api_key={_appSession.CurrentUser.api_key}";
            string fields = "fields=['id','name','description']";
            string domain = $"domain=[]";
            string limit = "limit=600";
            string EndPointParams = $"/search?{api_key}&{fields}&{domain}&{limit}";

            var restRequest = new RestRequest(EndPointApi + EndPointParams);
            restRequest.RequestFormat = DataFormat.Json;
            var result = await _client.RestClient().ExecuteGetAsync(restRequest);
            Console.WriteLine(result);

            if (result != null && result.Content != null & result.Content != "")
            {
                var resultApi = Newtonsoft.Json.JsonConvert.DeserializeObject<ApiResponseProductBrand>(result.Content);
                return resultApi;
            }

            return null;
        }
    }
}
