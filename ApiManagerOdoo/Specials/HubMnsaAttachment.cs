using ApiManagerOdoo.Base;
using AppManagerOdoo.Tools;
using DMSA.Models.Odoo.DMCobranzas;
using DMSA.Models.Odoo.General.Responses;
using DMSA.Models.Odoo.Modules.Accounting;
using DMSA.Models.Odoo.Native;
using DMSA.Models.Odoo.Specials;
using DMSA.Models.Security;
using RestSharp;
using System.Net;
using System.Net.Http.Json;


namespace ApiManager
{
    public class HubMnsaAttachment : HubBase
    {
        string[] fields_array = new[] {
                "id",
                "file_content",
                "file_name",
                "file_type",
                "date_data_cutoff"                
            };

        public HubMnsaAttachment(AppSession _setAppSession) : base(_setAppSession)
        {
            EndPointApi = "/web/dataset/call_kw";
            _modelname = "mnsa.attachment";
        }

        public async Task<ApiResponseOdooRpc?> GetCount(int[] ids)
        {
            object[] args = new object[] { };
            object[] _custom_args = new object[] {
                new object[] { "id", "in", ids },
            };
            return await GetCount(args, _custom_args);
        }

        public async Task<ApiResponseOdooRpcT<mnsa_attachment[]>?> GetAll(int[] ids)
        {
            var kwargs = new
            {
                fields = fields_array
            };

            object[] args = new object[] { };
            object[] _custom_args = new object[] {
                new object[] { "id", "in", ids },
            };
            return await SearchRead<ApiResponseOdooRpcT<mnsa_attachment[]>>(args, _custom_args, kwargs);
        }

        public async Task<byte[]> DownloadFileAsync(int recordId, string downloadFileName)
        {
            //_appSession = _setAppSession;
            //_baseUrl = _appSession.odooConnection.Host;

            //if (_appSession.CurrentUser != null)
            //{
            //    _username = _appSession.CurrentUser.username;
            //    _password = _appSession.CurrentUser.GetPasswordDecrypt();
            //    _db = _appSession.CurrentUser.databasename;
            //}

            var handler = new HttpClientHandler
            {
                UseCookies = true,
                CookieContainer = new CookieContainer()
            };

            using var client = new HttpClient(handler)
            {
                BaseAddress = new Uri("http://localhost:8069")
            };

            // 1️⃣ Login
            var loginPayload = new
            {
                jsonrpc = "2.0",
                method = "call",
                @params = new
                {
                    db = "TU_DB",
                    login = "admin",
                    password = "admin"
                },
                id = 1
            };

            var loginResponse = await client.PostAsJsonAsync(
                "/web/session/authenticate",
                loginPayload
            );

            loginResponse.EnsureSuccessStatusCode();

            // 2️⃣ Descargar archivo
            using var response = await client.GetAsync(
                $"/web/content/mnsa.attachment/{recordId}/file_content?download=true",
                HttpCompletionOption.ResponseHeadersRead
            );

            response.EnsureSuccessStatusCode();

            // 3️⃣ Leer binario
            return await response.Content.ReadAsByteArrayAsync();
        }
    }
}
