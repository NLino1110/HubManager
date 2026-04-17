
using ApiManagerOdoo.Base;
using DMSA.Models.Odoo.General.Requests;
using DMSA.Models.Odoo.General.Responses;
using DMSA.Models.Odoo.Import;
using DMSA.Models.Odoo.Native;
using DMSA.Models.Odoo.Tools;
using DMSA.Models.Security;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using RestSharp.Serializers;

namespace ApiManager
{
    public class HubAccountMoveTransientLegacy : HubBase
    {
        public HubAccountMoveTransientLegacy(AppSession _setAppSession) : base(_setAppSession)
        {
            EndPointApi = "/web/dataset/call_kw";
            _modelname = "am.transient.legacy";
        }

        //public async Task<ApiResponseOdooRpcT<int>?> Create(List<InvoiceHeader> SendObject, int company_id)
        //{
        //    var kwargs = new { };

        //    var settings = new JsonSerializerSettings
        //    {
        //        DateFormatString = "yyyy-MM-dd HH:mm:ss",
        //        //ContractResolver = new IncludeJsonIgnoreResolver(new string[] { "was_odoo_synced", "lines" })
        //    };

        //    var serialized = JsonConvert.SerializeObject(SendObject, settings);
        //    var newJObject = JObject.Parse(serialized);

        //    object[] args = new object[] { newJObject };
        //    return await Create<ApiResponseOdooRpcT<int>>(args, kwargs);
        //}

        public async Task<ApiResponseOdooRpcT<List<int>>?> Create(List<InvoiceHeader> SendObject, int company_id)
        {
            var kwargs = new { };

            var settings = new JsonSerializerSettings
            {
                DateFormatString = "yyyy-MM-dd HH:mm:ss",
                //ContractResolver = new IncludeJsonIgnoreResolver(new string[] { "was_odoo_synced", "lines" })
            };

            var validRecords = new List<JObject>();

            int index = 0;

            foreach (var record in SendObject)
            {
                try
                {
                    // Serializar registro individual
                    var serialized = JsonConvert.SerializeObject(record, settings);

                    // Deserializar para validar
                    var jObject = JObject.Parse(serialized);

                    // Si no hay errores se agrega
                    validRecords.Add(jObject);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"❌ Error en registro índice {index}");
                    System.Diagnostics.Debug.WriteLine($"Mensaje: {ex.Message}");

                    try
                    {
                        // Intentar mostrar el objeto que falló
                        var raw = JsonConvert.SerializeObject(record);
                        System.Diagnostics.Debug.WriteLine($"Objeto problemático: {raw}");
                    }
                    catch
                    {
                        System.Diagnostics.Debug.WriteLine("No se pudo serializar el objeto problemático.");
                    }
                }

                index++;
            }

            System.Diagnostics.Debug.WriteLine($"✔ Registros válidos: {validRecords.Count}");
            System.Diagnostics.Debug.WriteLine($"⚠ Registros inválidos: {SendObject.Count - validRecords.Count}");

            object[] args = new object[] { validRecords };

            return await Create<ApiResponseOdooRpcT<List<int>>>(args, kwargs);
        }

        //public async Task<ApiResponseOdooRpcT<int>?> CreateDetails(List<InvoiceDetails> SendObject, int company_id)
        //{
        //    //apiRequestOdooRpc_For_Dataset._params.kwargs.context.allowed_company_ids = new int[] { company_id };

        //    var kwargs = new { };

        //    var settings = new JsonSerializerSettings
        //    {
        //        DateFormatString = "yyyy-MM-dd HH:mm:ss",
        //        //ContractResolver = new IncludeJsonIgnoreResolver(new string[] { "was_odoo_synced", "lines" })
        //    };

        //    var serialized = JsonConvert.SerializeObject(SendObject, settings);

        //    var newJObject = JObject.Parse(serialized);

        //    object[] args = new object[] { newJObject };
        //    return await Create<ApiResponseOdooRpcT<int>>(args, kwargs, "am.transient.line.legacy");

        //}

        public async Task<ApiResponseOdooRpcT<List<int>>?> CreateDetails(List<InvoiceDetails> SendObject, int company_id)
        {
            var kwargs = new { };

            var settings = new JsonSerializerSettings
            {
                DateFormatString = "yyyy-MM-dd HH:mm:ss",
                //ContractResolver = new IncludeJsonIgnoreResolver(new string[] { "was_odoo_synced", "lines" })
            };

            var validRecords = new List<JObject>();

            int index = 0;

            foreach (var record in SendObject)
            {
                try
                {
                    // Serializar registro individual
                    var serialized = JsonConvert.SerializeObject(record, settings);

                    // Validar JSON
                    var jObject = JObject.Parse(serialized);

                    // Si todo está bien se agrega
                    validRecords.Add(jObject);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"❌ Error en InvoiceDetails índice {index}");
                    System.Diagnostics.Debug.WriteLine($"Mensaje: {ex.Message}");

                    try
                    {
                        var raw = JsonConvert.SerializeObject(record);
                        System.Diagnostics.Debug.WriteLine($"Objeto problemático: {raw}");
                    }
                    catch
                    {
                        System.Diagnostics.Debug.WriteLine("No se pudo serializar el objeto problemático.");
                    }
                }

                index++;
            }

            System.Diagnostics.Debug.WriteLine($"✔ Detalles válidos: {validRecords.Count}");
            System.Diagnostics.Debug.WriteLine($"⚠ Detalles inválidos: {SendObject.Count - validRecords.Count}");

            var finalArray = new JArray(validRecords);

            object[] args = new object[] { finalArray };

            return await Create<ApiResponseOdooRpcT<List<int>>>(args, kwargs, "am.transient.line.legacy");
        }

        //public async Task<ApiResponseOdooRpcT<int>?> CreatePayments(List<InvoicePayments> SendObject, int company_id)
        //{
        //    //apiRequestOdooRpc_For_Dataset._params.kwargs.context.allowed_company_ids = new int[] { company_id };

        //    var kwargs = new { };

        //    var settings = new JsonSerializerSettings
        //    {
        //        DateFormatString = "yyyy-MM-dd HH:mm:ss",
        //        //ContractResolver = new IncludeJsonIgnoreResolver(new string[] { "was_odoo_synced", "lines" })
        //    };

        //    var serialized = JsonConvert.SerializeObject(SendObject, settings);

        //    var newJObject = JObject.Parse(serialized);

        //    object[] args = new object[] { newJObject };
        //    return await Create<ApiResponseOdooRpcT<int>>(args, kwargs, "am.transient.payment.legacy");
        //}

        public async Task<ApiResponseOdooRpcT<List<int>>?> CreatePayments(List<InvoicePayments> SendObject, int company_id)
        {
            var kwargs = new { };

            var settings = new JsonSerializerSettings
            {
                DateFormatString = "yyyy-MM-dd HH:mm:ss",
                //ContractResolver = new IncludeJsonIgnoreResolver(new string[] { "was_odoo_synced", "lines" })
            };

            var validRecords = new List<JObject>();

            int index = 0;

            foreach (var record in SendObject)
            {
                try
                {
                    // Serializar el registro
                    var serialized = JsonConvert.SerializeObject(record, settings);

                    // Validar JSON
                    var jObject = JObject.Parse(serialized);

                    // Si todo está bien lo agregamos
                    validRecords.Add(jObject);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"❌ Error en InvoicePayments índice {index}");
                    System.Diagnostics.Debug.WriteLine($"Mensaje: {ex.Message}");

                    try
                    {
                        var raw = JsonConvert.SerializeObject(record);
                        System.Diagnostics.Debug.WriteLine($"Objeto problemático: {raw}");
                    }
                    catch
                    {
                        System.Diagnostics.Debug.WriteLine("No se pudo serializar el objeto problemático.");
                    }
                }

                index++;
            }

            System.Diagnostics.Debug.WriteLine($"✔ Payments válidos: {validRecords.Count}");
            System.Diagnostics.Debug.WriteLine($"⚠ Payments inválidos: {SendObject.Count - validRecords.Count}");

            var finalArray = new JArray(validRecords);

            object[] args = new object[] { finalArray };

            return await Create<ApiResponseOdooRpcT<List<int>>>(args, kwargs, "am.transient.payment.legacy");
        }
    }
}
