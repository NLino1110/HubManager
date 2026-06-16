using ApiManagerOdoo.BaseClean;
using DMSA.Models.Odoo.Abstract.Server;
using DMSA.Models.Odoo.Abstract.Server.Dto;
using DMSA.Models.Security;
using RestSharp;
using System.Diagnostics;
using System.Net;

namespace ApiManagerOdoo.Specials
{
    public class HubPackageClient : HubBase
    {
        public HubPackageClient(AppSession session) : base(session)
        {
        }

        // =========================
        // PACKAGES
        // =========================

        public Task<List<Package>?> GetPackages()
            => Get<List<Package>>("/api/package");

        public Task<List<Package>?> GetPackages(string packageName)
            => Get<List<Package>>("/api/package");

        public Task<Package?> GetPackage(string packageName)
            => Get<Package>($"/api/package/{packageName}");

        public Task<PackageResponseDto> CreatePackage(CreatePackageDto dto)
            => Post<PackageResponseDto>("/api/package/create", dto);

        // =========================
        // FILES
        // =========================

        public Task<List<PackageFileResponseDto>?> GetFiles(string packageName)
            => Get<List<PackageFileResponseDto>>($"/api/package/{packageName}/files");

        //public Task<bool> CreateFile(string packageName, CreatePackageFileDto dto)
        //    => Post($"/api/packages/{packageName}/files", dto);

        public async Task<bool> UploadFileByPath(
            string packageName,
            string filePath,
            string fileName,
            string fileType,
            long totalSize)
        {
            using var content = new MultipartFormDataContent();

            var fileStream = File.OpenRead(filePath);
            var fileContent = new StreamContent(fileStream);
            fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/octet-stream");

            content.Add(fileContent, "file", fileName);

            // Otros campos
            content.Add(new StringContent(fileName), "file_name");
            content.Add(new StringContent(fileType), "file_type");
            content.Add(new StringContent(totalSize.ToString()), "total_file_size_expected");

            var request = new HttpRequestMessage(HttpMethod.Post, $"{_baseUrl}/api/package/{packageName}/file")
            {
                Content = content
            };

            request.Headers.Add("X-API-KEY", "t.0.0.r.1381");

            var response = await _httpClient.SendAsync(request);

            return response.IsSuccessStatusCode;


        }


        public async Task<bool> UploadFileBytes(
            string packageName,
            byte[] fileBytes,
            string fileName,
            string fileType,
            long totalSize)
        {
            using var content = new MultipartFormDataContent();

var fileContent = new ByteArrayContent(fileBytes);
            fileContent.Headers.ContentType =
                new System.Net.Http.Headers.MediaTypeHeaderValue("application/octet-stream");

            content.Add(fileContent, "file", fileName);

            // Otros campos
            content.Add(new StringContent(fileName), "file_name");
            content.Add(new StringContent(fileType), "file_type");
            content.Add(new StringContent(totalSize.ToString()), "total_file_size_expected");

            var request = new HttpRequestMessage(HttpMethod.Post,
                $"{_baseUrl}/api/package/{packageName}/file")
            {
                Content = content
            };

            request.Headers.Add("X-API-KEY", "t.0.0.r.1381");

            var response = await _httpClient.SendAsync(request);

            return response.IsSuccessStatusCode;


}


        public async Task<bool> UploadFile(
            string packageName,
            Stream fileStream,
            string fileName,
            string fileType,
            long totalSize)
        {
            using var content = new MultipartFormDataContent();


var fileContent = new StreamContent(fileStream);
            fileContent.Headers.ContentType =
                new System.Net.Http.Headers.MediaTypeHeaderValue("application/octet-stream");

            content.Add(fileContent, "file", fileName);

            content.Add(new StringContent(fileName), "file_name");
            content.Add(new StringContent(fileType), "file_type");
            content.Add(new StringContent(totalSize.ToString()), "total_file_size_expected");

            var request = new HttpRequestMessage(HttpMethod.Post,
                $"{_baseUrl}/api/package/{packageName}/file")
            {
                Content = content
            };

            request.Headers.Add("X-API-KEY", "t.0.0.r.1381");

            var response = await _httpClient.SendAsync(request);

            return response.IsSuccessStatusCode;

}




        public async Task<byte[]> DownloadDirect(string FullUrl)
        {
            try
            {
                string url = FullUrl;
                byte[] fileContent = await GetRawBytes(url);

                return fileContent;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                return Array.Empty<byte>();
            }
        }

        //private readonly CookieContainer _cookieContainer = new();

        //public async Task<byte[]> GetRawBytes(string url)
        //{
        //    //await RequireLogin();

        //    var request = new RestRequest(url, Method.Get);
        //    var options = new RestClientOptions(_baseUrl)
        //    {
        //        CookieContainer = _cookieContainer,
        //        RemoteCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true
        //    };

        //    var _client = new RestClient(options);
        //    var response = await _client.ExecuteAsync(request);

        //    if (!response.IsSuccessful)
        //        throw new Exception($"Error descargando archivo: {response.StatusCode}");

        //    if (response.RawBytes == null || response.RawBytes.Length == 0)
        //        throw new Exception("La respuesta no contiene datos binarios");

        //    return response.RawBytes;
        //}

        private readonly CookieContainer _cookieContainer = new();

        public async Task<byte[]> GetRawBytes(string url)
        {
            //var handler = new HttpClientHandler
            //{
            //    CookieContainer = _cookieContainer,
            //    ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
            //};

            //using var client = new HttpClient(handler)
            //{
            //    BaseAddress = new Uri(_baseUrl)
            //};

            var response = await _httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
                throw new Exception($"Error descargando archivo: {response.StatusCode}");

            var bytes = await response.Content.ReadAsByteArrayAsync();

            if (bytes == null || bytes.Length == 0)
                throw new Exception("La respuesta no contiene datos binarios");

            return bytes;
        }
    }
}