using Models.DMSA.Mbw.Security;
using RestSharp;
using System.Diagnostics;

namespace DMDataSafe
{   
    public class SoapClient
    { 
        string targetUriWebService = "http://192.168.204.108:8081/MyBusiness-MyBusinessEJB/WSProformasPtoVta?";
        
        public SoapClient(AppSession appSession)
        {
            targetUriWebService = appSession.EndPointServer;
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
            var client = new RestClient(targetUriWebService);
            var request = new RestRequest(targetUriWebService, Method.Post);
            request.AddHeader("Content-Type", "text/xml"); 
            request.AddStringBody(postData, DataFormat.Xml);            

            RestResponse response = await client.ExecutePostAsync(request);

            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                System.Xml.XmlDocument xmlDocument = new System.Xml.XmlDocument();
                xmlDocument.LoadXml("<?xml version=\"1.0\" encoding=\"UTF-8\"?>" + response.Content);
                string resultado = xmlDocument.GetElementsByTagName("resultado")[0].InnerText;
                return resultado;
            }
            else
            {
                Debug.WriteLine(response.ErrorMessage);
                return "";
            }
        }
    }
}
