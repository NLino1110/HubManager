using DMSA.Models.General;
using DMSA.Models.Security;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiManager
{
    [Obsolete]
    public class HubParametros
    {
        private AppSession _appSession { get; }

        public HubParametros(AppSession _setAppSession)//(IOptions<AppSettings> appSettings)
        {
            _appSession = _setAppSession;
        }

        public async Task<ApiResponse_PARAMETROS_CXC> GetAll(string Empresa, int uid, string fechaActualizaTablet)
        {
            ApiResponse_PARAMETROS_CXC resultData = new ApiResponse_PARAMETROS_CXC();

            string Action = "PARAMETROS_CXC";
            fechaActualizaTablet = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            string cadenaJson = "{\"EMPRESA\":\"" + Empresa + "\",\"fechatablet\":\"" + fechaActualizaTablet + "\"}";

            List<RestSharp.Parameter> parameters = new List<RestSharp.Parameter>();
                        
            parameters.Add(RestSharp.Parameter.CreateParameter("accion", Action, ParameterType.QueryString));
            parameters.Add(RestSharp.Parameter.CreateParameter("codusuario", uid, ParameterType.QueryString));
            parameters.Add(RestSharp.Parameter.CreateParameter("cadenaJson", cadenaJson, ParameterType.QueryString));

            //App.Current.MainPage = new MainPage();
            SoapClient client = new SoapClient(_appSession);
            var result = await client.asyncPostJson(parameters.ToArray());
            Console.WriteLine(result);

            if (result != "")
            {
                resultData = Newtonsoft.Json.JsonConvert.DeserializeObject<ApiResponse_PARAMETROS_CXC>(result);               
            }

            return resultData;
        }

        //public async Task<ApiResponse_ACTUALIZAFECHASINCRO_NC> actualizaFechaSincroNotaCredito(DateTime fechaActualizaTablet)
        //{
        //    ApiResponse_ACTUALIZAFECHASINCRO_NC resultData = new ApiResponse_ACTUALIZAFECHASINCRO_NC();

        //    string Action = "ACTUALIZAFECHASINCRO_NC";
        //    string string_fechaActualizaTablet = fechaActualizaTablet.ToString("yyyy-MM-dd HH:mm:ss");
        //    string cadenaJson = "{\"fechaActualizaTablet\":\"" + string_fechaActualizaTablet + "\"}";

        //    List<RestSharp.Parameter> parameters = new List<RestSharp.Parameter>();

        //    parameters.Add(RestSharp.Parameter.CreateParameter("accion", Action, ParameterType.QueryString));
        //    //parameters.Add(RestSharp.Parameter.CreateParameter("codusuario", codusuario, ParameterType.QueryString));
        //    parameters.Add(RestSharp.Parameter.CreateParameter("cadenaJson", cadenaJson, ParameterType.QueryString));

        //    //App.Current.MainPage = new MainPage();
        //    SoapClient client = new SoapClient(_appSession);
        //    var result = await client.asyncPostJson(parameters.ToArray());
        //    Console.WriteLine(result);

        //    if (result != "")
        //    {
        //        resultData = Newtonsoft.Json.JsonConvert.DeserializeObject<ApiResponse_ACTUALIZAFECHASINCRO_NC>(result);
        //    }

        //    return resultData;
        //}
    }
}
