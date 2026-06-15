using DMSA.Models.Odoo.Abstract.Server;
using DMSA.Models.Odoo.Abstract.Server.Dto;
using DMSA.Models.Security;
using Newtonsoft.Json;
using System.Text;

namespace ApiManagerOdoo.Specials
{
    public class HubPackageClient
    {        
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl;
        
        public HubPackageClient(HttpClient httpClient, AppSession _setAppSession)
        {
            _httpClient = httpClient;
            _baseUrl = _setAppSession.odooConnection.HostDump.TrimEnd('/');

            _httpClient.DefaultRequestHeaders.Add("X-API-KEY", _setAppSession.odooConnection.HostDumpApiKey);

        }

        // =========================
        // PACKAGES
        // =========================

        public async Task<List<Package>?> GetPackages()
        {
            var response = await _httpClient.GetAsync($"{_baseUrl}/api/packages");

            if (!response.IsSuccessStatusCode)
                return null;

            var json = await response.Content.ReadAsStringAsync();

            return JsonConvert.DeserializeObject<List<Package>>(json);
        }

        public async Task<Package?> GetPackage(string packageName)
        {
            var response = await _httpClient.GetAsync($"{_baseUrl}/api/packages/{packageName}");

            if (!response.IsSuccessStatusCode)
                return null;

            var json = await response.Content.ReadAsStringAsync();

            return JsonConvert.DeserializeObject<Package>(json);
        }

        public async Task<bool> CreatePackage(CreatePackageDto dto)
        {
            var json = JsonConvert.SerializeObject(dto);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync($"{_baseUrl}/api/packages", content);

            return response.IsSuccessStatusCode;
        }

        // =========================
        // FILES
        // =========================

        public async Task<List<PackageFileResponseDto>?> GetFiles(string packageName)
        {
            var response = await _httpClient.GetAsync($"{_baseUrl}/api/packages/{packageName}/files");

            if (!response.IsSuccessStatusCode)
                return null;

            var json = await response.Content.ReadAsStringAsync();

            return JsonConvert.DeserializeObject<List<PackageFileResponseDto>>(json);
        }

        public async Task<bool> CreateFile(string packageName, CreatePackageFileDto dto)
        {
            var json = JsonConvert.SerializeObject(dto);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync($"{_baseUrl}/api/packages/{packageName}/files", content);

            return response.IsSuccessStatusCode;
        }
    }
}
