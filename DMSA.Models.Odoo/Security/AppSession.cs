using DMSA.Models.Odoo.Native;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMSA.Models.Security
{
    public class AppSession
    {
        public User CurrentUser { get; set; }        
        public string UrlReportServer = "http://192.168.0.120:8080/jasperserver";

        public bool isProduction {  get; set; } = false;
        public bool isTestMode { get; set; } = true;
        
        public string EndPointServer = "http://192.168.56.1:8069"; //ODOO SERVER        
        public string EndPointServerProd = "https://www.dmujeres.com.ec:8069";

        public string CacheFilesUrl = "/resources/tmp/android/sqlite/";
        [Obsolete]
        public string CacheFilesUrlExternal = "/resources/tmp/android/sqlite/";
        
        public string StaticResources_Server = "https://192.168.204.66:2443";
        public string StaticResources_Server_Prod = "https://200.110.68.142:2443";

        public string DefaultDatabase = "macronegocios_dev2";

        public int db_limit_default = 300;
        public DateTime sync_date_since = new DateTime(2023, 1, 1);

        public CultureInfo ApplicationCultureInfo { get; set; }
        //public CultureInfo ApplicationCultureInfo = CultureInfo.GetCultureInfo("es-EC");
        public string AppVersion = "0.0.0";
        public string ApplicationName = "CobranzasDMSA";
        public string AppCodeOdoo = "01";

        public res_company res_Company { get; set; }
        [Obsolete("Debe ser eliminado")]
        public res_store res_Store { get; set; }
        public res_center res_center { get; set; }
        public stock_warehouse stock_Warehouse { get; set; }

        public AppSession()
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
