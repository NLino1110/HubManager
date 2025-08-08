using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace VtexStrucs.Tools
{
    public class RSHelper
    {
        Dictionary<string, string> headers_priv =
            new Dictionary<string, string>();

        string EndPointApi = "";

        readonly RestClient _client;

        public RSHelper(Dictionary<string, string> headers, string EndPointApi)
        {
            headers_priv = headers;
            this.EndPointApi = EndPointApi;

            //ServicePointManager.ServerCertificateValidationCallback += (sender, certificate, chain, sslPolicyErrors) => true;
            RestClientOptions restClientOptions = new RestClientOptions();
            restClientOptions.RemoteCertificateValidationCallback += (sender, certificate, chain, sslPolicyErrors) => true;
            restClientOptions.BaseUrl = new Uri ($"{EndPointApi}");
            _client = new RestClient(restClientOptions);
            //_client.AddDefaultHeader(KnownHeaders.Accept, "application/json");            
            _client.AddDefaultHeader(KnownHeaders.Accept, "application/vnd.vtex.ds.v10+json");
        }              

        public Task<T?> GetAsync<T>(string EndPoint)
        {  
            var restRequest = new RestRequest(EndPointApi);
            restRequest.RequestFormat = DataFormat.Json;            
            //restRequest.AddJsonBody(requestObject);

            if (headers_priv.Count > 0)
            {
                foreach (KeyValuePair<string, string> kvp in headers_priv)
                {
                    //Console.WriteLine("Key = {0}, Value = {1}",
                    //    kvp.Key, kvp.Value);
                    restRequest.AddHeader(kvp.Key, kvp.Value);
                }
            }
            
            return _client.GetAsync<T>(restRequest);
        }

        public Task<T?> PutAsync<T>(string EndPoint, object bodyObject )
        {
            var taskSource = new TaskCompletionSource();
            var restRequest = new RestRequest(EndPointApi);
            //restRequest.RequestFormat = DataFormat.Json;
            restRequest.AddBody(bodyObject);
            
            if (headers_priv.Count > 0)
            {
                foreach (KeyValuePair<string, string> kvp in headers_priv)
                {
                    //Console.WriteLine("Key = {0}, Value = {1}",
                    //    kvp.Key, kvp.Value);
                    restRequest.AddHeader(kvp.Key, kvp.Value);
                }
            }

            return _client.PutAsync<T>(restRequest);
        }

        public Task<T?> PostAsync<T>(string EndPoint, object bodyObject)
        {
            var taskSource = new TaskCompletionSource();
            var restRequest = new RestRequest(EndPointApi);
            //restRequest.RequestFormat = DataFormat.Json;
            restRequest.AddBody(bodyObject);

            if (headers_priv.Count > 0)
            {
                foreach (KeyValuePair<string, string> kvp in headers_priv)
                {
                    //Console.WriteLine("Key = {0}, Value = {1}",
                    //    kvp.Key, kvp.Value);
                    restRequest.AddHeader(kvp.Key, kvp.Value);
                }
            }

            return _client.PostAsync<T>(restRequest);
        }

        public Task<RestResponse> DeleteAsync(string EndPoint)
        {
            var taskSource = new TaskCompletionSource();
            var restRequest = new RestRequest(EndPointApi);
            //restRequest.RequestFormat = DataFormat.Json;
            //restRequest.AddBody(bodyObject);

            if (headers_priv.Count > 0)
            {
                foreach (KeyValuePair<string, string> kvp in headers_priv)
                {
                    //Console.WriteLine("Key = {0}, Value = {1}",
                    //    kvp.Key, kvp.Value);
                    restRequest.AddHeader(kvp.Key, kvp.Value);
                }
            }
            return _client.DeleteAsync(restRequest);
        }

        public Task<T?> DeleteAsync<T>(string EndPoint, object bodyObject)
        {
            var taskSource = new TaskCompletionSource();
            var restRequest = new RestRequest(EndPointApi);
            //restRequest.RequestFormat = DataFormat.Json;
            restRequest.AddBody(bodyObject);

            if (headers_priv.Count > 0)
            {
                foreach (KeyValuePair<string, string> kvp in headers_priv)
                {
                    //Console.WriteLine("Key = {0}, Value = {1}",
                    //    kvp.Key, kvp.Value);
                    restRequest.AddHeader(kvp.Key, kvp.Value);
                }
            }

            return _client.DeleteAsync<T>(restRequest);
        }

        public Task<RestResponse> ExecuteGetAsync(string EndPoint)
        {
            var taskSource = new TaskCompletionSource();
            var restRequest = new RestRequest(EndPointApi);
            //restRequest.RequestFormat = DataFormat.Json;
            //restRequest.AddBody(bodyObject);

            if (headers_priv.Count > 0)
            {
                foreach (KeyValuePair<string, string> kvp in headers_priv)
                {
                    //Console.WriteLine("Key = {0}, Value = {1}",
                    //    kvp.Key, kvp.Value);
                    restRequest.AddHeader(kvp.Key, kvp.Value);
                }
            }

            return _client.ExecuteGetAsync(restRequest);
        }
        public Task<RestResponse> ExecutePostAsync(string EndPoint, object bodyObject)
        {
            var taskSource = new TaskCompletionSource();
            var restRequest = new RestRequest(EndPointApi);
            //restRequest.RequestFormat = DataFormat.Json;
            restRequest.AddBody(bodyObject);

            if (headers_priv.Count > 0)
            {
                foreach (KeyValuePair<string, string> kvp in headers_priv)
                {
                    //Console.WriteLine("Key = {0}, Value = {1}",
                    //    kvp.Key, kvp.Value);
                    restRequest.AddHeader(kvp.Key, kvp.Value);
                }
            }

            return _client.ExecutePostAsync(restRequest);
        }

        public Task<RestResponse> ExecutePutAsync(string EndPoint, object bodyObject)
        {
            var taskSource = new TaskCompletionSource();
            var restRequest = new RestRequest(EndPointApi);
            //restRequest.RequestFormat = DataFormat.Json;
            restRequest.AddBody(bodyObject);

            if (headers_priv.Count > 0)
            {
                foreach (KeyValuePair<string, string> kvp in headers_priv)
                {
                    //Console.WriteLine("Key = {0}, Value = {1}",
                    //    kvp.Key, kvp.Value);
                    restRequest.AddHeader(kvp.Key, kvp.Value);
                }
            }

            return _client.ExecutePutAsync(restRequest);
        }

    }
}
