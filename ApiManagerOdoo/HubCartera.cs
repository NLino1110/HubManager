using ApiManagerOdoo.Base;
using AppManagerOdoo;
using AppManagerOdoo.Tools;
using CobranzasDMSA_Odoo.Models;

//using CobranzasDMSA_Odoo.Models;
using DMSA.Models.Clientes;
using DMSA.Models.General;
using DMSA.Models.General.Requests;
using DMSA.Models.General.Responses;
using DMSA.Models.Odoo.General.Requests;
using DMSA.Models.Odoo.General.Responses;
using DMSA.Models.Odoo.Native;
using DMSA.Models.Security;
using Newtonsoft.Json;
using RestSharp;

namespace ApiManager
{
    public class HubCartera : HubBase
    {
        string[] fields_array = new[] {
                "id",
                "name",
                "uom_id",
                "default_code",
                "categ_id",
                "type",
                "product_brand_id",
                "active",
                "macro_product_available",
                "sale_ok",
                "purchase_ok",
                "trade_ok"
        };        

        public HubCartera(AppSession _setAppSession) : base(_setAppSession)
        {
            EndPointApi = "/web/dataset/call_kw";
            EndPointApi = "/connect/get_credit_header";
            _modelname = "account.payment.header";
        }        

        public async Task<ApiResponseOdooRpcT<CobCarteraCab[]>?> GetHeaders(int year, int month, int day, int limit, int index)
        {   
            var kwargs = new
            {
                fields = fields_array 
            };

            object[] args = new object[] { };
            object[] _custom_args = new object[] {
                //new object[] { "write_date", ">", $"{year}-{month:00}-{day:00} 23:59:59" },
                new object[] { "active", "=", true },
            };
            return await SearchRead<ApiResponseOdooRpcT<CobCarteraCab[]>>(args, _custom_args, kwargs);
        }

        public async Task<ApiResponseOdooRpc?> GetHeaderCount()
        {
            object[] args = new object[] { };
            object[] _custom_args = new object[] { };
            return await GetCount(args, _custom_args);
        }

        //public async Task<ApiResponse_OBTENER_CARTERA_DET?> ObtenerDetalles(ApiRequest_v1 requestObject)
        //{
        //    string Action = "OBTENER_CARTERA_DET";
        //    string currentDateTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        //    //string cadenaJson = requestObject.cadenaJson;

        //    List<RestSharp.Parameter> parameters = new List<RestSharp.Parameter>();

        //    parameters.Add(RestSharp.Parameter.CreateParameter("accion", Action, ParameterType.QueryString));
        //    parameters.Add(RestSharp.Parameter.CreateParameter("codusuario", requestObject.uid, ParameterType.QueryString));
        //    parameters.Add(RestSharp.Parameter.CreateParameter("cadenaJson", requestObject.cadenaJson, ParameterType.QueryString));

        //    //App.Current.MainPage = new MainPage();
        //    SoapClient client = new SoapClient(_appSession);
        //    var result = await client.asyncPostJson(parameters.ToArray());
        //    Console.WriteLine(result);

        //    if (result != "")
        //    {
        //        var resultUser = Newtonsoft.Json.JsonConvert.DeserializeObject<ApiResponse_OBTENER_CARTERA_DET>(result);
        //        return resultUser;
        //    }

        //    return null;
        //}
    }
}
