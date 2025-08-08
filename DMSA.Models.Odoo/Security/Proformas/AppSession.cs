using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMSA.Models.Security.Proformas
{
    public class AppSession
    {
        public User CurrentUser { get; set; }
        public string urlServicioReportes = "https://www.dmujeres.com.ec:8443/MyBusinessWeb/servlet/ServicioTransporteReportes";

        public bool isProduction {  get; set; } = true;
        public bool isTestMode { get; set; } = false;
        /// <summary>
        /// Api Original 
        /// </summary>
        public string EndPointServer = "http://192.168.204.108:8081/MyBusiness-MyBusinessEJB/WSProformasPtoVta?";
        public string EndPointServerProd = "https://www.dmujeres.com.ec:8443/MyBusiness-MyBusinessEJB/WSProformasPtoVta?";

        /// <summary>
        /// Ruta al nuevo API
        /// </summary>
        public string EndPointServerNewApi = "https://192.168.204.108:5100";
        public string EndPointServerNewApiInternal = "https://192.168.0.127:5100"; //SE ASUME QUE ESTARÁN EN LA RED LOCAL
        public string EndPointServerNewApiExternal = "https://200.110.68.142:5100"; //PARA PUYO
        //public string EndPointServerNewApiExternal = "https://www.dmujeres.com.ec:5100";

        public string AppVersion = "1.0";
        public string AppName = "MNSA LOPDP";
    }
}
