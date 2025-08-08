using DMSA.Models.Security;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace ApiManager
{
    [Obsolete]
    public class SoapClient
    {
        //Url de desarrollo por defecto
        //string targetUri = "http://192.168.204.108:8081/MyBusiness-MyBusinessEJB/WSPedidosAndroidv2";
        string targetUri = "http://192.168.204.108:8081/MyBusinessWeb/servlet/SWSIntegracionesApp?";

        //public SoapClient()
        //{
            
        //    //EndPointServer
        //}

        public SoapClient(AppSession appSession)
        {
            //Se obtiene URL desde la sesión iniciada
            targetUri = appSession.EndPointServer;            
        }

        public async Task<string> asyncPost(string SoapAction, string SoapBody)
        {            
            string postData =
            $@"<soapenv:Envelope xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/"" xmlns:web=""http://webservices.etech.com/"">
               <soapenv:Header/>
               <soapenv:Body>
                  <web:{SoapAction}>
                     {SoapBody}
                  </web:{SoapAction}>
               </soapenv:Body>
            </soapenv:Envelope>";
            var client = new RestClient(targetUri);
            var request = new RestRequest(targetUri, Method.Post);
            request.AddHeader("Content-Type", "text/xml");
            //request.RequestFormat = DataFormat.None;
            request.AddStringBody(postData, DataFormat.Xml);
            //request.AddParameter("application/xml",
            //        postData,
            //ParameterType.RequestBody);

            RestResponse response = await client.ExecutePostAsync(request);

            //var response = await client.ExecutePostAsync(request);
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                System.Xml.XmlDocument xmlDocument = new System.Xml.XmlDocument();
                xmlDocument.LoadXml("<?xml version=\"1.0\" encoding=\"UTF-8\"?>" + response.Content);
                string resultado = xmlDocument.GetElementsByTagName("resultado")[0].InnerText;
                return resultado; // response.Content;
            }
            else
            {
                return ""; // throw new Exception("Soap Request Failed");
            }
        }

        public async Task<string> asyncPostJson(Parameter[] parameters)
        {
            var client = new RestClient(targetUri);
            var request = new RestRequest(targetUri, Method.Post);

            //request.AddParameter("accion", parameters[0]);
            //request.AddParameter("cadenaJson", parameters[1]);

            request.Parameters.AddParameters(parameters);
            request.Timeout = new TimeSpan(30000);
            //request.AddParameter("accion", "VERIFICAR_USUARIO_WSJSON");
            //request.AddParameter("cadenaJson", "{\"codusuario\":\"djimenez\",\"codclave\":\"triaton03\",\"fechatablet\":\"2023-04-24 11:11\"}");


            //request.AddHeader("Content-Type", "text/xml");
            //request.RequestFormat = DataFormat.None;
            //request.AddStringBody(parameters, DataFormat.Json);
            //request.AddParameter("application/xml",
            //        postData,
            //ParameterType.RequestBody);

            RestResponse response = await client.ExecutePostAsync(request);

            //var response = await client.ExecutePostAsync(request);
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                string resultado = response.Content;
                return resultado; // response.Content;
            }
            else
            {
                return ""; // throw new Exception("Soap Request Failed");
            }
        }
    }
}
