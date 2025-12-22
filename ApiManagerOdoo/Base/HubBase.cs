using DMSA.Models.Odoo.General.Requests;
using DMSA.Models.Odoo.General.Responses;
using DMSA.Models.Security;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using System;
using System.Diagnostics;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace ApiManagerOdoo.Base
{
    public class HubBase
    {
        private bool _disposed = false;
        private string CallKw = "web/dataset/call_kw";
        public string EndPointServer { get; set; } //http://localhost:8069/
        public string EndPointApi { get; set; }        
        public AppSession _appSession { get; set; }
        public string _modelname { get; set; }
        private readonly RestClient _client;
        private readonly CookieContainer _cookieContainer = new();
        private readonly string _baseUrl;
        private readonly string _db;
        private readonly string _username;
        private readonly string _password;

        public HubBase(AppSession _setAppSession)
        {
            _appSession = _setAppSession;            
            _baseUrl = _appSession.odooConnection.Host;

            if (_appSession.CurrentUser != null)
            {
                _username = _appSession.CurrentUser.username;
                _password = _appSession.CurrentUser.GetPasswordDecrypt();
                _db = _appSession.CurrentUser.databasename;
            }

            var options = new RestClientOptions(_baseUrl)
            {
                CookieContainer = _cookieContainer
            };

            _client = new RestClient(options);
        }

        public async Task RequireLogin()
         {
            if(_cookieContainer.Count>0)
            {
                return;
            }

            if (string.IsNullOrEmpty(_username) || string.IsNullOrEmpty(_password) || string.IsNullOrEmpty(_db))
            {
                throw new InvalidOperationException("Username, password or database name is not set.");
            }

            if (!await LoginAsync())
            {
                throw new UnauthorizedAccessException("Login failed. Please check your credentials.");
            }
        }

        public async Task<bool> LoginAsync()
        {
            var loginRequest = new RestRequest("/web/session/authenticate", Method.Post);
            loginRequest.AddHeader("Content-Type", "application/json");

            var loginBody = new
            {
                jsonrpc = "2.0",
                method = "call",
                @params = new
                {
                    db = _db,
                    login = _username,
                    password = _password
                }
            };

            loginRequest.AddJsonBody(loginBody);
            var response = await _client.ExecuteAsync(loginRequest);

            if (!response.IsSuccessful)
            {
                Console.WriteLine("Login fallido: " + response.Content);
                return false;
            }

            var sessionId = _cookieContainer
                .GetCookies(new Uri(_baseUrl))
                .OfType<Cookie>()
                .FirstOrDefault(c => c.Name == "session_id")?.Value;

            if (string.IsNullOrEmpty(sessionId))
            {
                Console.WriteLine("❌ No se recibió session_id");
                return false;
            }

            Console.WriteLine("✅ Login exitoso. Session ID: " + sessionId);
            return true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {                    
                    Console.WriteLine("Liberando recursos administrados...");
                }
                _disposed = true;
            }
        }

        ~HubBase()
        {
            Dispose(false);
        }

        public static string ConvertToArgs(string domainString)
        {            
            string formattedString = domainString.Replace("'", "\"");
            
            var regex = new Regex(@"\[""(?<field>\w+)"",\s*""(?<operator>[^""]+)"",\s*(?<value>[^,\]]+)\]");
            var matches = regex.Matches(formattedString);

            StringBuilder argsBuilder = new StringBuilder();
            argsBuilder.AppendLine("[");

            foreach (Match match in matches)
            {
                string field = match.Groups["field"].Value;
                string operatorPart = match.Groups["operator"].Value;
                string value = match.Groups["value"].Value.Trim();
                string parsedValue = ConvertValue(value);
                argsBuilder.AppendLine($"    [\"{field}\", \"{operatorPart}\", {parsedValue}],");
            }
            
            if (argsBuilder.Length > 1)
            {
                argsBuilder.Length -= 3;
            }

            argsBuilder.AppendLine("\n]");
            return argsBuilder.ToString();
        }

        private static string ConvertValue(string value)
        {            
            if (value == "False" || value == "True")
            {
                return value.ToLower();
            }
            else if (value.StartsWith("\"") && value.EndsWith("\""))
            {
                return value; 
            }
            else if (Regex.IsMatch(value, @"^\d+$"))
            {
                return value;
            }
            else
            {
                return $"\"{value}\"";
            }
        }

        public async Task<ApiResponseOdooRpc?> GetCount(object[] _args_, object[] _custom_args)
        {
            string _method_name = "search_count";

            var restRequest = new RestRequest(CallKw, Method.Post);
            restRequest.RequestFormat = DataFormat.Json;
            await RequireLogin();

            object[] combinedArgs = new object[] {
                    //_args_,
                    _custom_args
                };

            var apiRequestRpc = new ApiRequestOdooRpc_v2();
            apiRequestRpc.method = "call";
            apiRequestRpc.id = 1;
            apiRequestRpc._params = new Params_v2();
            //apiRequestRpc._params.service = "object";
            apiRequestRpc._params.model = _modelname;
            apiRequestRpc._params.method = _method_name;
            //apiRequestRpc.context = new ContextRpc() { lang = "es_ES" };
            apiRequestRpc._params.args = combinedArgs;
            apiRequestRpc._params.kwargs = new object() {  };// new Kwargs_v2();
            //apiRequestRpc._params.kwargs = new Kwargs_v2();

            string jsonBody = JsonConvert.SerializeObject(apiRequestRpc);

            restRequest.AddJsonBody(jsonBody);

            var result = await _client.ExecutePostAsync(restRequest);

            if (result != null && result.Content != null & result.Content != "")
            {
                try
                {
                    var resultNative = JsonConvert.DeserializeObject<ApiResponseOdooRpc>(result.Content);
                    return resultNative;
                }
                catch (Exception e)
                {
                    Console.WriteLine("GetCount:" + e.Message);
                    Console.WriteLine(result.Content);

                    return new ApiResponseOdooRpc()
                    {
                        error = new Error()
                        {
                            code = 666,
                            message = e.Message,
                        },
                        jsonrpc = "2.0",
                        id = 1,
                        result = 0
                    };
                }
            }

            return new ApiResponseOdooRpc()
            {
                jsonrpc = "2.0",
                id = 1,
                result = 0
            };
        }

        public async Task<T?> SearchRead<T>(
            object[] _args_,
            object[] _custom_args,
            object kargs) where T : class, new()
        {
            return await SearchRead<T>(_args_, _custom_args, kargs, isRequiredLogin:false);
        }

        public async Task<T?> SearchRead<T>(
            object[] _args_,
            object[] _custom_args,
            object kargs,
            bool isRequiredLogin) where T : class, new()
        {
            return await SearchRead<T>(_modelname, _args_, _custom_args, kargs, isRequiredLogin);
        }

        public async Task<T?> SearchRead<T>(
            string modelName,
            object[] _args_, 
            object[] _custom_args, 
            object kargs,
            bool isRequiredLogin) where T : class, new()
        {
            string _method_name = "search_read";

            var restRequest = new RestRequest(CallKw, Method.Post);
            restRequest.RequestFormat = DataFormat.Json;
            if (isRequiredLogin)
            {
                await RequireLogin();
            }

            object[] combinedArgs = new object[] {
                    //_args_,
                    _custom_args
            };

            var apiRequestRpc = new ApiRequestOdooRpc_v2();
            apiRequestRpc.method = "call";
            apiRequestRpc.id = 1;
            apiRequestRpc._params = new Params_v2();            
            apiRequestRpc._params.model = modelName;
            apiRequestRpc._params.method = _method_name;
            apiRequestRpc._params.args = combinedArgs;
            apiRequestRpc._params.kwargs = kargs;
            apiRequestRpc._params.db = "local";
            apiRequestRpc._params.login = "local";
            apiRequestRpc._params.password = "local";

            string jsonBody = JsonConvert.SerializeObject(apiRequestRpc);

            restRequest.AddJsonBody(jsonBody);

            var result = await _client.ExecutePostAsync(restRequest);

            if (result != null && result.Content != null & result.Content != "")
            {
                if (IsOdooError(result.Content, out string errorMessage))
                {
                    Debug.WriteLine("Se detectó un error de Odoo:");
                    Debug.WriteLine(errorMessage);
                }
                else
                {
                    var resultNative = JsonConvert.DeserializeObject<T>(result.Content);
                    return resultNative;
                }
            }

            return new T();
        }

        public async Task<T?> Create<T>(
            object[] _args_,            
            object kargs) where T : class, new()
        {
            return await Create<T>(_args_, kargs, _modelname);
        }

        public async Task<T?> Create<T>(
            object[] _args_,
            object kargs,
            string model_name) where T : class, new()
        {
            string _method_name = "create";

            var restRequest = new RestRequest(CallKw, Method.Post);
            restRequest.RequestFormat = DataFormat.Json;
            await RequireLogin();

            var apiRequestRpc = new ApiRequestOdooRpc_v2();
            apiRequestRpc.method = "call";
            apiRequestRpc.id = 1;
            apiRequestRpc._params = new Params_v2();
            apiRequestRpc._params.model = model_name;
            apiRequestRpc._params.method = _method_name;
            apiRequestRpc._params.args = _args_;
            apiRequestRpc._params.kwargs = kargs;

            string jsonBody = JsonConvert.SerializeObject(apiRequestRpc);

            restRequest.AddJsonBody(jsonBody);

            var result = await _client.ExecutePostAsync(restRequest);

            if (result != null && result.Content != null & result.Content != "")
            {
                if (IsOdooError(result.Content, out string errorMessage))
                {
                    Debug.WriteLine("Se detectó un error de Odoo:");
                    Debug.WriteLine(errorMessage);

                    //TODO: Aquí validacion de HTML
                    // las respuestas a veces son de NGINX

                    var resultNativeError = JsonConvert.DeserializeObject<T>(result.Content);
                    return resultNativeError;
                }
                else
                {
                    var resultNative = JsonConvert.DeserializeObject<T>(result.Content);
                    return resultNative;
                }
            }

            return new T();
        }

        public async Task<T?> Write<T>(
            object[] _args_,
            object kargs,
            string model_name) where T : class, new()
        {
            string _method_name = "write";

            var restRequest = new RestRequest(CallKw, Method.Post);
            restRequest.RequestFormat = DataFormat.Json;
            await RequireLogin();

            var apiRequestRpc = new ApiRequestOdooRpc_v2();
            apiRequestRpc.method = "call";
            apiRequestRpc.id = 1;
            apiRequestRpc._params = new Params_v2();
            apiRequestRpc._params.model = model_name;
            apiRequestRpc._params.method = _method_name;
            apiRequestRpc._params.args = _args_;
            apiRequestRpc._params.kwargs = kargs;

            string jsonBody = JsonConvert.SerializeObject(apiRequestRpc);

            restRequest.AddJsonBody(jsonBody);

            var result = await _client.ExecutePostAsync(restRequest);

            if (result != null && result.Content != null & result.Content != "")
            {
                if (IsOdooError(result.Content, out string errorMessage))
                {
                    Debug.WriteLine("Se detectó un error de Odoo:");
                    Debug.WriteLine(errorMessage);

                    var resultNativeError = JsonConvert.DeserializeObject<T>(result.Content);
                    return resultNativeError;
                }
                else
                {
                    var resultNative = JsonConvert.DeserializeObject<T>(result.Content);
                    return resultNative;
                }
            }

            return new T();
        }

        public bool IsOdooError(string jsonResponse, out string errorMessage)
        {
            errorMessage = null;

            if (string.IsNullOrWhiteSpace(jsonResponse))
                return false;

            try
            {
                var json = JObject.Parse(jsonResponse);

                // Verifica si existe un nodo "error"
                var errorNode = json["error"];
                if (errorNode != null)
                {
                    var message = errorNode["message"]?.ToString();
                    var detailedMessage = errorNode["data"]?["message"]?.ToString();
                    var debug = errorNode["data"]?["debug"]?.ToString();

                    // Puedes adaptar la forma de concatenar el mensaje según tus necesidades
                    errorMessage = $"Odoo Error: {message}\nDetails: {detailedMessage}\nTraceback:\n{debug}";

                    return true;
                }
            }
            catch (Exception ex)
            {
                errorMessage = $"Error parsing JSON: {ex.Message}";
                return true;
            }

            return false;
        }

        public async Task<TResponse?> Call<TRequest, TResponse>(
            string url,
            Method method,
            TRequest paramObject,
            bool isRequiredLogin
            ) where TResponse : class, new()
        {
            var restRequest = new RestRequest(url, method)
            {
                RequestFormat = DataFormat.Json
            };

            if (isRequiredLogin) 
            {
                await RequireLogin();
            }

            string jsonBody = JsonConvert.SerializeObject(paramObject);

            restRequest.AddJsonBody(jsonBody);

            var result = await _client.ExecuteAsync(restRequest);

            if (!string.IsNullOrWhiteSpace(result?.Content))
            {
                if (IsOdooError(result.Content, out string errorMessage))
                {
                    Debug.WriteLine("Se detectó un error de Odoo:");
                    Debug.WriteLine(errorMessage);
                }
                else
                {
                    var resultNative = JsonConvert.DeserializeObject<TResponse>(result.Content);
                    return resultNative;
                }
            }

            return new TResponse();
        }

        public async Task<T?> CallMethod<T>(
            string url,
            Method method,
            object[] _args_,
            object kargs,
            string model_name,
            string method_name) where T : class, new()
        {
            var restRequest = new RestRequest(url, method);
            restRequest.RequestFormat = DataFormat.Json;
            await RequireLogin();

            var apiRequestRpc = new ApiRequestOdooRpc_v2();
            apiRequestRpc.method = "call";
            apiRequestRpc.id = 1;
            apiRequestRpc._params = new Params_v2();
            apiRequestRpc._params.model = model_name;
            apiRequestRpc._params.method = method_name;
            apiRequestRpc._params.args = _args_;
            apiRequestRpc._params.kwargs = kargs;

            string jsonBody = JsonConvert.SerializeObject(apiRequestRpc);

            restRequest.AddJsonBody(jsonBody);

            var result = await _client.ExecutePostAsync(restRequest);

            if (!string.IsNullOrWhiteSpace(result?.Content))
            {
                if (IsOdooError(result.Content, out string errorMessage))
                {
                    Debug.WriteLine("Se detectó un error de Odoo:");
                    Debug.WriteLine(errorMessage);

                    var resultNativeError = JsonConvert.DeserializeObject<T>(result.Content);
                    return resultNativeError;
                }
                else
                {
                    var resultNative = JsonConvert.DeserializeObject<T>(result.Content);
                    return resultNative;
                }
            }

            return new T();
        }

        public async Task<T?> Authenticate<T>(string db, string login, string password) where T : class, new()
        {
            string url = "/web/session/authenticate";
            Method method = Method.Post;
            object[] _args_ = new object[] {};
            object kargs = new { };
            string model_name = "";
            string method_name = "";

            var restRequest = new RestRequest(url, method);
            restRequest.RequestFormat = DataFormat.Json;
            
            var apiRequestRpc = new ApiRequestOdooRpc_v2();
            apiRequestRpc.method = "call";
            apiRequestRpc.id = 1;
            apiRequestRpc._params = new Params_v2();
            apiRequestRpc._params.model = model_name;
            apiRequestRpc._params.method = method_name;
            apiRequestRpc._params.args = _args_;
            apiRequestRpc._params.kwargs = kargs;

            apiRequestRpc._params.db = db;
            apiRequestRpc._params.login = login;
            apiRequestRpc._params.password = password;

            string jsonBody = JsonConvert.SerializeObject(apiRequestRpc);

            restRequest.AddJsonBody(jsonBody);

            var result = await _client.ExecutePostAsync(restRequest);

            if (!string.IsNullOrWhiteSpace(result?.Content))
            {
                if (IsOdooError(result.Content, out string errorMessage))
                {
                    Debug.WriteLine("Se detectó un error de Odoo:");
                    Debug.WriteLine(errorMessage);

                    var resultNativeError = JsonConvert.DeserializeObject<T>(result.Content);
                    return resultNativeError;
                }
                else
                {
                    var resultNative = JsonConvert.DeserializeObject<T>(result.Content);
                    return resultNative;
                }
            }

            return new T();
        }

        public async Task<byte[]> GetRawBytes(string url)
        {
            await RequireLogin();

            var request = new RestRequest(url, Method.Get);

            var response = await _client.ExecuteAsync(request);

            if (!response.IsSuccessful)
                throw new Exception($"Error descargando archivo: {response.StatusCode}");

            if (response.RawBytes == null || response.RawBytes.Length == 0)
                throw new Exception("La respuesta no contiene datos binarios");

            return response.RawBytes;
        }
    }
}
