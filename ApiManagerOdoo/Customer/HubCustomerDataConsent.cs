using ApiManagerOdoo.Base;
using ApiManagerOdoo.Sale;
using DMSA.Models.Odoo.Customers;
using DMSA.Models.Odoo.General.Responses;
using DMSA.Models.Odoo.Tools;
using DMSA.Models.Security;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ApiManager
{
    public class HubCustomerDataConsent : HubBase
    {
        string[] fields_array = new[] {                
            "id",
            "res_partner_id",
            "res_center_id",
            "consent_type",
            "policy_version",
            "ip_address",                
            "email",
            "user_id",
            "doc_type_identification_id",
            "vat_doc",
            "first_name",
            "last_name",
            "phone",
            "address",
            "application_origin",
            "device_app_version",
            "device_platform_mod",
            "device_platform_source",
            "device_manufacturer",
            "device_model",
            "response_state",
            "write_date",
            "create_date",
            "write_ui",
            "create_ui"
        };

        public HubCustomerDataConsent(AppSession _setAppSession) : base(_setAppSession)
        {            
            EndPointApi = "/web/dataset/call_kw";
            _modelname = "customer.data.consent";
        }

        public async Task<ApiResponseOdooRpc?> GetCount(int year, int month, int day)
        {
            object[] args = new object[] { };            
            object[] _custom_args = new object[] {
                new object[] {"write_date", ">=", $"{year}-{month:00}-{day:00} 00:00:00" },
            };
            return await GetCount(args, _custom_args);            
        }

        public async Task<ApiResponseOdooRpc?> GetCountBySeller(int year, int month, int day, int seller)
        {
            object[] args = new object[] { };
            object[] _custom_args = new object[] {
                new object[] {"write_date", ">=", $"{year}-{month:00}-{day:00} 00:00:00" },
                new object[] { "adic_comercial_id", "=", seller },               
            };
            return await GetCount(args, _custom_args);
        }

        public async Task<ApiResponseOdooRpcT<CustomerDataConsent[]>?> GetByWriteDate(DateTime dateIni, int limit, int index)
        {
            var kwargs = new
            {
                limit = limit,
                offset = (index * limit),
                fields = fields_array
            };

            object[] args = new object[] { };
            object[] _custom_args = new object[] {
                new object[] {"write_date", ">=", dateIni.ToString("yyyy-MM-dd 00:00:00") },
            };
            return await SearchRead<ApiResponseOdooRpcT<CustomerDataConsent[]>>(args, _custom_args, kwargs, true);
        }

        public async Task<ApiResponseOdooRpcT<CustomerDataConsent[]>?> GetByIds(int[] res_parent_ids, int limit, int index)
        {
            var kwargs = new
            {
                limit = limit,
                offset = (index * limit),
                fields = fields_array
            };

            object[] args = new object[] { };
            object[] _custom_args = new object[] {
                new object[] {"res_parent_id", "in", res_parent_ids },
            };
            return await SearchRead<ApiResponseOdooRpcT<CustomerDataConsent[]>>(args, _custom_args, kwargs, true);
        }

        public async Task<ApiResponseOdooRpcT<int>?> Add(CustomerDataConsent newResponse)
        {
            var kwargs = new { };
            var settings = new JsonSerializerSettings
            {
                DateFormatString = "yyyy-MM-dd HH:mm:ss",
                ContractResolver = new IncludeJsonIgnoreResolver()
            };

            var serialized = JsonConvert.SerializeObject(newResponse, settings);

            var newJObject = JObject.Parse(serialized);

            object[] args = new object[] { newJObject };
            var created_data = await Create<ApiResponseOdooRpcT<int>>(args, kwargs);

            return created_data;
        }

        public async Task<ApiResponseOdooRpcT<bool>?> UpdateState(CustomerDataConsent newResponse)
        {
            var kwargs = new { };
            var settings = new JsonSerializerSettings
            {
                DateFormatString = "yyyy-MM-dd HH:mm:ss",                
            };

            var serialized = JsonConvert.SerializeObject(newResponse, settings);

            var newJObject = JObject.Parse(serialized);

            JObjectExtensions.RemoveProperty(newJObject, "res_partner_id");
            JObjectExtensions.RemoveProperty(newJObject, "res_center_id");
            JObjectExtensions.RemoveProperty(newJObject, "policy_version");
            JObjectExtensions.RemoveProperty(newJObject, "ip_address");
            JObjectExtensions.RemoveProperty(newJObject, "email");
            JObjectExtensions.RemoveProperty(newJObject, "vat_doc");
            JObjectExtensions.RemoveProperty(newJObject, "first_name");
            JObjectExtensions.RemoveProperty(newJObject, "last_name");
            JObjectExtensions.RemoveProperty(newJObject, "phone");
            JObjectExtensions.RemoveProperty(newJObject, "address");

            JObjectExtensions.RemoveProperty(newJObject, "consent_type");
            JObjectExtensions.RemoveProperty(newJObject, "res_partner");
            JObjectExtensions.RemoveProperty(newJObject, "res_center");
            JObjectExtensions.RemoveProperty(newJObject, "doc_type_identification_id");

            JObjectExtensions.RemoveProperty(newJObject, "application_origin");
            JObjectExtensions.RemoveProperty(newJObject, "device_app_version");
            JObjectExtensions.RemoveProperty(newJObject, "device_platform_mod");
            JObjectExtensions.RemoveProperty(newJObject, "device_platform_source");
            JObjectExtensions.RemoveProperty(newJObject, "device_manufacturer");
            JObjectExtensions.RemoveProperty(newJObject, "device_model");

            JObjectExtensions.RemoveProperty(newJObject, "create_date");
            JObjectExtensions.RemoveProperty(newJObject, "write_date");
            JObjectExtensions.RemoveProperty(newJObject, "create_uid");
            JObjectExtensions.RemoveProperty(newJObject, "write_uid");

            JObjectExtensions.RemoveProperty(newJObject, "db");
            JObjectExtensions.RemoveProperty(newJObject, "login");
            JObjectExtensions.RemoveProperty(newJObject, "password");


            //object[] args = new object[] { newJObject };

            object[] args = new object[]
            {
                    new object[] { newResponse.Id },
                    newJObject
            };

            var created_data = await Write<ApiResponseOdooRpcT<bool>>(args, kwargs, _modelname);

            return created_data;
        }

        public async Task<ApiResponseOdooRpcT<bool>?> UpdateFull(CustomerDataConsent newResponse)
        {
            var kwargs = new { };
            var settings = new JsonSerializerSettings
            {
                DateFormatString = "yyyy-MM-dd HH:mm:ss",
                ContractResolver = new IncludeJsonIgnoreResolver()
            };

            var serialized = JsonConvert.SerializeObject(newResponse, settings);

            var newJObject = JObject.Parse(serialized);

            object[] args = new object[] { newJObject };
            var created_data = await Write<ApiResponseOdooRpcT<bool>>(args, kwargs, _modelname);

            return created_data;
        }

        public async Task<ApiResponseOdooRpcT<List<CustomerDataConsent>>?> GetForAgree(int[] res_centers, int numero, string filter)
        {
            int limit = 200;
            int index = 0;
            
            DateTime dateTime = DateTime.Now.AddDays(-60);

            string[] fields_array_get = { "id", 
                "res_partner_id", 
                "res_center_id", 
                "consent_type",
                "policy_version",
                "ip_address",
                "email",
                "doc_type_identification_id",
                "vat_doc",
                "first_name",
                "last_name",
                "phone",
                "address",
                "application_origin",
                "device_app_version",
                "device_platform_mod",
                "device_platform_source",
                "device_manufacturer",
                "device_model",
                "response_state",
                "display_name",

            };
            var kwargs = new
            {
                limit = limit,
                offset = (index * limit),
                fields = fields_array_get
            };

            object[] args = new object[] { };
            object[] _custom_args = new object[] {
                new object[] { "res_center_id", "in", res_centers },
                new object[] { "first_name", "ilike", filter },
                new object[] { "write_date", ">=", dateTime.ToString( "yyyy-MM-dd" ) },
                new object[] { "response_state","=","none" }
            };

            return await SearchRead<ApiResponseOdooRpcT<List<CustomerDataConsent>>>(args, _custom_args, kwargs, true);
        }

        public async Task<ApiResponseOdooRpcT<List<CustomerDataConsent>>?> GetForAgree_Old(int[] res_centers, int numero, string filter)
        {
            ApiResponseOdooRpcT<List<CustomerDataConsent>> retultData = 
                new ApiResponseOdooRpcT<List<CustomerDataConsent>>();

            DateTime date = DateTime.Now;

            date = new DateTime(date.Year, 3, 13);

            var hubSale = new HubSaleOrder(this._appSession);
            var hubResPartner = new HubResPartner(this._appSession);

            var result_orders = await hubSale.GetListByParameter(res_centers, filter, date, 100, 0);

            if (result_orders != null && result_orders.result != null 
                && result_orders.result.Count() > 0)
            {
                var sale_orders = result_orders.result;
                //obtener lista de clientes 
                var res_partner_list = sale_orders.Select(x=>x._partner_id).ToList();
                var consents = await GetByIds(res_partner_list.ToArray(), 100, 0);
                var res_partner_data = await hubResPartner.GetByIds(100, 0, res_partner_list.ToArray());

                if (consents.result == null || consents.result.Count() == 0)
                {
                    foreach (var sale_order in result_orders.result)
                    {
                        var new_consent = new CustomerDataConsent();
                        
                        retultData.result.Add(new_consent);
                    }

                    return retultData;
                }

                var data_consent = consents.result;

                foreach (var consent in data_consent)
                {
                    if (consent != null)
                    {
                        retultData.result.Add(consent);
                    }
                }
            }

            return retultData;
        }
    }
}
