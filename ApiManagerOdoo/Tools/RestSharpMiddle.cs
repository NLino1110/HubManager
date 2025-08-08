using ApiManager;
using DMSA.Models.Odoo.General.Responses;
using DMSA.Models.Security;
using Newtonsoft.Json.Linq;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace AppManagerOdoo.Tools
{
    public class RestSharpMiddle
    {
        private RestClient _client { get; set; }
        private AppSession _appSession { get; set; }
        private ResponseAuthenticate responseUser {  get; set; }
        private readonly CookieContainer _cookieContainer = new();

        public RestSharpMiddle(AppSession _setAppSession, bool defaulHeader = true)
        {
            _appSession = _setAppSession;
            BuildClient(defaulHeader);
        }

        public void setSession(AppSession _setAppSession)
        {
            _appSession = _setAppSession;
        }

        public void SetAuthentication(ResponseAuthenticate _responseUser)
        {
            responseUser = _responseUser;
        }

        private async Task SetUserStatus()
        {
            User user = new User();
            user.username = _appSession.CurrentUser.username;
            user.codclave = _appSession.CurrentUser.codclave;
            user.databasename = _appSession.CurrentUser.databasename;

            DateTime currentDate = DateTime.Now;
            if (responseUser == null || responseUser.error != null)
            {
                ApiManager.HubUser hubUser = new ApiManager.HubUser(_appSession);
                responseUser = await hubUser.TryLoginRpcWeb(user, currentDate);

                //Reintento de login
                if(responseUser == null)
                {
                    Console.WriteLine("Error en TryLoginRpcWeb...reintentando...N1");
                    responseUser = await hubUser.TryLoginRpcWeb(user, currentDate);

                    if (responseUser == null)
                    {
                        Console.WriteLine("Error en TryLoginRpcWeb...reintentando...N2");
                    }
                }
            }
        }

        public async Task<RestRequest> BuildAuthorizedRequest(string EndPointParams)
        {
            await SetUserStatus();

            var restRequest = new RestRequest(EndPointParams);
            restRequest.RequestFormat = DataFormat.Json;
            
            if (responseUser != null)
            {
                foreach (Cookie cookie in responseUser.Cookies)
                {
                    restRequest.AddCookie(cookie.Name, cookie.Value, cookie.Path, cookie.Domain);
                }
            }

            return restRequest;
        }

        /// <summary>
        /// Autenticacíón deprecated
        /// No se debe usar ya porque depende del módulo rest_api y este módulo
        /// no cubre todas las necesidades
        /// a su vez hay que usar RPC
        /// </summary>
        public void setUseAuthorization_deprecated()
        {
            if (_appSession != null && _appSession.CurrentUser != null)
            {                
                bool existeAut = _client.DefaultParameters.Any(p => p.Name == "Authorization");
                if (!existeAut)
                {
                    _client.AddDefaultHeader(KnownHeaders.Authorization, _appSession.CurrentUser.access_token);
                }
            }
        }

        public RestClient BuildClient(bool defaulHeader = true)
        {
            RestClientOptions restClientOptions = new RestClientOptions();
            //restClientOptions.MaxTimeout = 30000;
            restClientOptions.Timeout = TimeSpan.FromMilliseconds(30000);
            restClientOptions.RemoteCertificateValidationCallback += (sender, certificate, chain, sslPolicyErrors) => true;
            restClientOptions.BaseUrl = new Uri($"{_appSession.EndPointServer}");
            restClientOptions.UserAgent = "XYZ";
            restClientOptions.CookieContainer = _cookieContainer;

            _client  = new RestClient(restClientOptions);
            
            if (defaulHeader)
            {
                _client.AddDefaultHeader(KnownHeaders.Accept, "application/json");
            }

            return _client;
        }

        public RestClient RestClient()
        {            
            return _client;
        }

        public void AddHeader(string name, string value)
        {
            _client.AddDefaultHeader(name, value);
        }
    }
}
