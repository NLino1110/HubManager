using DMSA.Models.Security;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace ApiManagerOdoo.BaseClean
{
    public abstract class HubBase
    {
        protected readonly HttpClient _httpClient;
        protected readonly string _baseUrl;

        protected HubBase(AppSession session)
        {
            _baseUrl = session.odooConnection.HostDump.TrimEnd('/');

            var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (_, _, _, _) => true
            };

            _httpClient = new HttpClient(handler)
            {
                Timeout = TimeSpan.FromSeconds(60)
            };

            _httpClient.DefaultRequestHeaders.Add("X-API-KEY", session.odooConnection.HostDumpApiKey);
        }

        // =========================
        // GET
        // =========================
        protected async Task<T?> Get<T>(string endpoint)
        {
            var response = await _httpClient.GetAsync($"{_baseUrl}{endpoint}");

            if (!response.IsSuccessStatusCode)
                return default;

            var json = await response.Content.ReadAsStringAsync();

            return JsonConvert.DeserializeObject<T>(json);
        }

        // =========================
        // POST
        // =========================
        protected async Task<T?> Post<T>(string endpoint, object body)
        {
            var json = JsonConvert.SerializeObject(body, new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore
            });
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync($"{_baseUrl}{endpoint}", content);

            //if (!response.IsSuccessStatusCode)
            //    return default;

            var responseJson = await response.Content.ReadAsStringAsync();

            return JsonConvert.DeserializeObject<T>(responseJson);
        }

        // =========================
        // POST (sin respuesta)
        // =========================
        protected async Task<bool> Post(string endpoint, object body)
        {
            var json = JsonConvert.SerializeObject(body, new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore
            });

            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync($"{_baseUrl}{endpoint}", content);

            return response.IsSuccessStatusCode;
        }

        // =========================
        // PUT
        // =========================
        protected async Task<bool> Put(string endpoint, object body)
        {
            var json = JsonConvert.SerializeObject(body);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PutAsync($"{_baseUrl}{endpoint}", content);

            return response.IsSuccessStatusCode;
        }

        // =========================
        // DELETE
        // =========================
        protected async Task<bool> Delete(string endpoint)
        {
            var response = await _httpClient.DeleteAsync($"{_baseUrl}{endpoint}");

            return response.IsSuccessStatusCode;
        }
    }
}
