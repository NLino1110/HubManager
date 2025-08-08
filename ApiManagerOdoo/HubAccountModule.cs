using AppManagerOdoo.Tools;
using DMSA.Models.Odoo.General.Responses;
using DMSA.Models.Security;
//using Microsoft.AspNetCore.Components;
//using Microsoft.Extensions.Configuration;
//using Microsoft.Extensions.Logging;
//using Microsoft.Extensions.Options;
using RestSharp;

namespace ApiManager
{
    public class HubAccountModule
    {
        string EndPointServer = "";        
        string EndPointApi = "/api/account.module";

        readonly RestSharpMiddle _client;
        private AppSession _appSession { get; }

        public HubAccountModule(AppSession _setAppSession)
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

        public async Task<ApiResponseAccountModule?> GetAll(string ids)
        {
            ////App.Current.MainPage = new MainPage();
            string api_key = $"api_key={_appSession.CurrentUser.api_key}";
            //string EndPointParams = $"/search?api_key={api_key}&fields=['id','name']";
            string fields = "fields=['id','name','street','street2','city']";
            string domain = $"domain=[('id','in',[{ids}])]";
            string limit = "limit=300";
            string EndPointParams = $"/search?{api_key}&{fields}&{domain}&{limit}";

            var restRequest = new RestRequest(EndPointApi + EndPointParams);
            restRequest.RequestFormat = DataFormat.Json;
            var result = await _client.RestClient().ExecuteGetAsync(restRequest);
            Console.WriteLine(result);

            if (result != null && result.Content != null & result.Content != "")
            {
                var resultApi = Newtonsoft.Json.JsonConvert.DeserializeObject<ApiResponseAccountModule>(result.Content);
                return resultApi;
            }

            return null;
        }

        public async Task<ApiResponseAccountModule?> GetAll()
        {
            ////App.Current.MainPage = new MainPage();
            string api_key = $"api_key={_appSession.CurrentUser.api_key}";
            //string EndPointParams = $"/search?api_key={api_key}&fields=['id','name']";
            string fields = "fields=[]";
            string domain = $"domain=[]";
            string limit = "limit=300";
            string EndPointParams = $"/search?{api_key}&{fields}&{domain}&{limit}";

            var restRequest = new RestRequest(EndPointApi + EndPointParams);
            restRequest.RequestFormat = DataFormat.Json;
            var result = await _client.RestClient().ExecuteGetAsync(restRequest);
            Console.WriteLine(result);

            if (result != null && result.Content != null & result.Content != "")
            {
                var resultApi = Newtonsoft.Json.JsonConvert.DeserializeObject<ApiResponseAccountModule>(result.Content);
                return resultApi;
            }

            return null;
        }
    }
}
