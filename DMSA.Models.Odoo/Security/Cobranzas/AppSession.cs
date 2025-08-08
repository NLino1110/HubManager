using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMSA.Models.Security
{
    [Obsolete]
    public class __AppSession
    {
        public User CurrentUser { get; set; }        
        public string UrlReportServer = "http://192.168.0.120:8080/jasperserver";

        public bool isProduction {  get; set; } = false;
        public bool isTestMode { get; set; } = true;
        /// <summary>
        /// Api Original
        /// </summary>
        public string EndPointServer = "http://172.19.64.1:8069"; //ODOO SERVER
        //public string EndPointServer = "http://localhost:8069"; //ODOO SERVER
        public string EndPointServerProd = "https://www.dmujeres.com.ec:8069";

        public string CacheFilesUrl = "/resources/tmp/android/sqlite/";
        [Obsolete]
        public string CacheFilesUrlExternal = "/resources/tmp/android/sqlite/";
        
        public string StaticResources_Server = "https://192.168.204.66:2443";
        public string StaticResources_Server_Prod = "https://200.110.68.142:2443";

        public string DefaultDatabase = "macronegocios_dev2";

        public CultureInfo ApplicationCultureInfo { get; set; }
        //public CultureInfo ApplicationCultureInfo = CultureInfo.GetCultureInfo("es-EC");
        public string AppVersion = "0.0.0";
        public string ApplicationName = "CobranzasDMSA";

        public string AppCodeOdoo = "01";

        public __AppSession()
        {
            //ApplicationCultureInfo = CultureInfo.GetCultureInfo("en-US");
            //ApplicationCultureInfo.NumberFormat = new NumberFormatInfo() {
            //    NumberGroupSeparator = ""
            //};

            ApplicationCultureInfo = new CultureInfo("en-US");
            ApplicationCultureInfo.NumberFormat.NumberGroupSeparator = "";
        }
    }
}
