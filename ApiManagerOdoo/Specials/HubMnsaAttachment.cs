using ApiManagerOdoo.Base;
using DMSA.Models.Odoo.General.Responses;
using DMSA.Models.Odoo.Specials;
using DMSA.Models.Odoo.Tools;
using DMSA.Models.Security;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using System.Diagnostics;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace ApiManager
{
    public class HubMnsaAttachment : HubBase
    {
        string[] fields_array = new[] {
                "id",
                "server",
                "database_name",
                "file_name",
                "file_type",
                "date_data_cutoff",                
                "attachment_ids",
                "lines_url",
                "total_file_size",
                "create_date",
                "write_date"
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

        public async Task<ApiResponseOdooRpcT<mnsa_attachment[]>?> GetTop5(string dbNameSqlite)
        {
            string mobile_app_id_code = "00";

            mobile_app_id_code = _appSession.AppCodeOdoo;

            var kwargs = new
            {
                limit = 5,
                order = "date_data_cutoff desc",
                fields = fields_array
            };

            object[] args = new object[] { };
            object[] _custom_args = new object[] {
                new object[] { "file_type", "=", "application/zip" },
                new object[] { "mobile_app_id.code", "=", mobile_app_id_code },
                new object[] { "file_name", "=", dbNameSqlite}
            };

            return await SearchRead<ApiResponseOdooRpcT<mnsa_attachment[]>>(args, _custom_args, kwargs, true);
        }

        public async Task<ApiResponseOdooRpcT<mnsa_attachment[]>?> GetLastest(DateTime referenceDate)
        {
            string mobile_app_id_code = "00";

            mobile_app_id_code = _appSession.AppCodeOdoo;

            var kwargs = new
            {
                limit = 5,
                order = "date_data_cutoff desc",
                fields = fields_array
            };

            object[] args = new object[] { };
            object[] _custom_args = new object[] {
                new object[] { "file_type", "=", "application/zip" },
                new object[] { "mobile_app_id.code", "=", mobile_app_id_code },
                new object[] { "date_data_cutoff", "<=", referenceDate.ToString("yyyy-MM-dd 23:59:59") }
            };

            return await SearchRead<ApiResponseOdooRpcT<mnsa_attachment[]>>(args, _custom_args, kwargs, true);
        }

        public async Task<ApiResponseOdooRpcT<mnsa_attachment[]>?> GetLastestByFileName(DateTime referenceDate, string filename)
        {
            string mobile_app_id_code = "00";

            mobile_app_id_code = _appSession.AppCodeOdoo;

            var kwargs = new
            {
                limit = 5,
                order = "date_data_cutoff desc",
                fields = fields_array
            };

            object[] args = new object[] { };
            object[] _custom_args = new object[] {
                new object[] { "file_type", "=", "application/zip" },
                new object[] { "mobile_app_id.code", "=", mobile_app_id_code },
                new object[] { "file_name", "=", filename },
                //new object[] { "date_data_cutoff", "<=", referenceDate.ToString("yyyy-MM-dd 23:59:59") }
            };

            return await SearchRead<ApiResponseOdooRpcT<mnsa_attachment[]>>(args, _custom_args, kwargs, true);
        }

        public async Task<ApiResponseOdooRpcT<int>?> CreatePackage(mnsa_attachment SendObject)
        {
            var kwargs = new { };

            var settings = new JsonSerializerSettings
            {
                DateFormatString = "yyyy-MM-dd HH:mm:ss",
                //ContractResolver = new IncludeJsonIgnoreResolver(new string[] { "was_odoo_synced", "lines" })
            };

            var serialized = JsonConvert.SerializeObject(SendObject, settings);

            var newJObject = JObject.Parse(serialized);

            object[] args = new object[] { newJObject };
            return await Create<ApiResponseOdooRpcT<int>>(args, kwargs);
        }

        [Obsolete("Usar modo2")]
        public async Task<ApiResponseOdooRpcT<bool>?> Link(int parent_id, int attachment_id)
        { 

            var settings = new JsonSerializerSettings
            {
                DateFormatString = "yyyy-MM-dd HH:mm:ss",
                //ContractResolver = new IncludeJsonIgnoreResolver(new string[] { "was_odoo_synced", "lines" })
            };

            var kwargs = new { };

            object[] args = new object[]
            {
                    new object[] { parent_id },
                    new Dictionary<string, object>
                    {
                        {
                            "attachment_ids",
                            new object[]
                            {
                                new object[] { 4, attachment_id }
                            }
                        }
                    }
            };
            
            return await Write<ApiResponseOdooRpcT<bool>>(args, kwargs, _modelname);
        }

        public async Task<ApiResponseOdooRpcT<bool>?> LinkMode2(int parent_id, int attachment_id)
        {

            var settings = new JsonSerializerSettings
            {
                DateFormatString = "yyyy-MM-dd HH:mm:ss",
                //ContractResolver = new IncludeJsonIgnoreResolver(new string[] { "was_odoo_synced", "lines" })
            };

            var kwargs = new { };

            object[] args = new object[]
            {
                    new object[] { parent_id },
                    new Dictionary<string, object>
                    {
                        {
                            "lines_url",
                            new object[]
                            {
                                new object[] { 4, attachment_id }
                            }
                        }
                    }
            };

            //var serialized = JsonConvert.SerializeObject(SendObject, settings);
            //var newJObject = JObject.Parse(serialized);

            return await Write<ApiResponseOdooRpcT<bool>>(args, kwargs, _modelname);
        }

        [Obsolete("Usar modo2")]
        public async Task<ApiResponseOdooRpcT<int>?> SendAttachment(ir_attachment SendObject)
        {
            var kwargs = new { };

            var settings = new JsonSerializerSettings
            {
                DateFormatString = "yyyy-MM-dd HH:mm:ss",
                //ContractResolver = new IncludeJsonIgnoreResolver(new string[] { "was_odoo_synced", "lines" })
            };

            var serialized = JsonConvert.SerializeObject(SendObject, settings);

            var newJObject = JObject.Parse(serialized);

            JObjectExtensions.RemoveProperty(newJObject, "type");
            JObjectExtensions.RemoveProperty(newJObject, "display_name");
            JObjectExtensions.RemoveProperty(newJObject, "description");
            JObjectExtensions.RemoveProperty(newJObject, "file_size");
            JObjectExtensions.RemoveProperty(newJObject, "url");
            JObjectExtensions.RemoveProperty(newJObject, "local_url");
            JObjectExtensions.RemoveProperty(newJObject, "checksum");

            object[] args = new object[] { newJObject };
            return await Create<ApiResponseOdooRpcT<int>>(args, kwargs, "ir.attachment");
        }

        public async Task<ApiResponseOdooRpcT<int>?> SendAttachmentMode2(mnsa_attachment_line SendObject, string dbNameSqlite, string package_name)
        {
            

            var uploadResponse = await SendToExternalServer(SendObject.file_bytes, SendObject.file_name, package_name);

            if(uploadResponse == null)
                return null;

            SendObject.total_file_size_expected = SendObject.file_bytes.Length;
            SendObject.url = uploadResponse.url;
            SendObject.success_upload = true;

            var kwargs = new { };

            var settings = new JsonSerializerSettings
            {
                DateFormatString = "yyyy-MM-dd HH:mm:ss",
                //ContractResolver = new IncludeJsonIgnoreResolver(new string[] { "was_odoo_synced", "lines" })
            };

            var serialized = JsonConvert.SerializeObject(SendObject, settings);

            var newJObject = JObject.Parse(serialized);

            JObjectExtensions.RemoveProperty(newJObject, "name");
            JObjectExtensions.RemoveProperty(newJObject, "file_bytes");
            JObjectExtensions.RemoveProperty(newJObject, "package_id");

            object[] args = new object[] { newJObject };
            var responseCreate = await Create<ApiResponseOdooRpcT<int>>(args, kwargs, "mnsa.attachment.line");

            return responseCreate;
        }

        public class responseUpload
        {
            public string url { get; set; }
        }

        public async Task<responseUpload?> SendToExternalServer(byte[] fileBytes, string filename, string package_name)
        {
            var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
            };

            using var httpClient = new HttpClient(handler);

            httpClient.DefaultRequestHeaders.Add("X-API-KEY", _appSession.odooConnection.HostDumpApiKey);

            using var content = new MultipartFormDataContent();

            var fileContent = new ByteArrayContent(fileBytes);
            fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse("application/zip");

            content.Add(fileContent, "file", $"{filename}.zip");
            content.Add(new StringContent(filename), "fileName");
            content.Add(new StringContent(package_name), "packageName");

            var response = await httpClient.PostAsync($"{_appSession.odooConnection.HostDump}/api/upload/zip", content);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new Exception($"Error subiendo ZIP: {error}");
            }

            var json = await response.Content.ReadAsStringAsync();

            var responseData = JsonConvert.DeserializeObject<responseUpload>(json);

            return responseData;
        }


        [Obsolete("Usar modo2")]
        public async Task<byte[]> DownloadFileAsync(int recordId)
        {
            try
            {
                string url = $"/web/content/{recordId}?download=true";

                byte[] fileContent = await GetRawBytes(url);

                return fileContent;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                return Array.Empty<byte>();
            }
        }

        public async Task<byte[]> DownloadFileMode2Async(string FullUrl)
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

        [Obsolete("Ya no usado")]
        public async Task<byte[]> DownloadFileAsync1(int recordId, string downloadFileName)
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
