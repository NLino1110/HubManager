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
    public class HubReportes
    {
        private AppSession _appSession { get; }

        public HubReportes(AppSession _setAppSession)
        {
            _appSession = _setAppSession;
        }

        public async Task<ApiResponse_REPORTE_COMISIONES> Comisiones(int Empresa, int uid)
        {
            ApiResponse_REPORTE_COMISIONES resultData = new ApiResponse_REPORTE_COMISIONES();

            string Action = "REPORTE_COMISIONES";
            string currentDateTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            string cadenaJson = "{\"EMPRESA\":\"" + Empresa + "\",\"fechatablet\":\"" + currentDateTime + "\"}";

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
                resultData = Newtonsoft.Json.JsonConvert.DeserializeObject<ApiResponse_REPORTE_COMISIONES>(result);               
            }

            return resultData;
        }
    }
}
