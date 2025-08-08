using System;
using System.Net;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using RestSharp;


namespace ResourceBuilder.Services.Test
{
   
    public class OdooApiClient
    {
        private readonly string _baseUrl;
        private readonly string _db;
        private readonly string _username;
        private readonly string _password;
        private readonly RestClient _client;
        private readonly CookieContainer _cookieContainer = new();

        public OdooApiClient(string baseUrl, string db, string username, string password)
        {
            _baseUrl = baseUrl.TrimEnd('/');
            _db = db;
            _username = username;
            _password = password;

            var options = new RestClientOptions(_baseUrl)
            {
                CookieContainer = _cookieContainer
            };

            _client = new RestClient(options);
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

        public async Task<JToken> SearchReadProductAsync()
        {
            var request = new RestRequest("/web/dataset/call_kw", Method.Post);
            request.AddHeader("Content-Type", "application/json");

            var requestBody = new
            {
                jsonrpc = "2.0",
                method = "call",
                id = 1,
                @params = new
                {
                    model = "product.product",
                    method = "search_read",
                    args = new object[] { },
                    kwargs = new
                    {
                        domain = new object[]
                        {
                        new object[] { "create_date", ">=", "2024-09-13 00:00:00" },
                        new object[] { "create_date", "<=", "2024-09-13 23:59:59" }
                        },
                        fields = new[]
                        {
                        "id", "default_code", "code", "partner_ref", "active", "product_tmpl_id",
                        "barcode", "combination_indices", "is_product_variant", "standard_price",
                        "list_price", "price_extra", "lst_price", "volume", "weight", "display_name",
                        "name", "create_date", "write_date", "qty_available", "virtual_available",
                        "free_qty", "incoming_qty", "outgoing_qty", "detailed_type", "type",
                        "categ_id", "currency_id", "uom_id", "uom_name", "sale_ok", "purchase_ok",
                        "website_url", "product_brand_id", "macro_product_line_id",
                        "macro_product_group_brand_id", "marco_product_subcategory_id",
                        "macro_product_available", "base_unit_count", "base_unit_price", "base_unit_name"
                    },
                        limit = 10,
                        offset = 0
                    }
                }
            };

            request.AddJsonBody(requestBody);
            var response = await _client.ExecuteAsync(request);

            var content = JObject.Parse(response.Content);

            if (content["error"] != null)
            {
                var errorMsg = content["error"]?["data"]?["message"]?.ToString();
                Console.WriteLine("❌ Error desde Odoo: " + errorMsg);
                return null;
            }

            return content["result"];
        }

        public static async Task Main()
        {
            var client = new OdooApiClient(
                baseUrl: "https://tuservidor",
                db: "tu_basededatos",
                username: "usuario@correo.com",
                password: "tu_contraseña"
            );

            if (await client.LoginAsync())
            {
                var result = await client.SearchReadProductAsync();
                if (result != null)
                    Console.WriteLine("✅ Resultado:\n" + result.ToString());
            }
        }
    }

}
