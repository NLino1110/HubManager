using DMSA.Models.Odoo.Abstract;
using DMSA.Models.Odoo.Accounting;
using DMSA.Models.Odoo.Inventory;
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
        public OdooConnection odooConnection { get; set; }        
        public string SqliteCoreDbName { get; set; }

        public User CurrentUserFront { get; set; }
        public User CurrentUser { get; set; }
        public bool useOfflineMode { get; set; }
        public DateTime sync_date_since = new DateTime(2023, 1, 1);
        public CultureInfo ApplicationCultureInfo { get; set; }
        //public CultureInfo ApplicationCultureInfo = CultureInfo.GetCultureInfo("es-EC");
        public string AppVersion = "0.0.0";
        public string ApplicationName = "CobranzasDMSA"; //Este valor debe ser modificado para cada aplicacion
        public string AppCodeOdoo = "01";  //Este valor debe ser modificado para cada aplicacion
        public int AppMobileId = 1;

        //TODO: Revisar si se conserva, ya que la conexion contiene el codigo de empresa
        public res_company res_Company { get; set; }
        public res_center res_center { get; set; }
        public stock_warehouse stock_Warehouse { get; set; }

        [Obsolete("Debe ser eliminado")]
        public res_store res_Store { get; set; } //Sin referencias
        [Obsolete]
        public string UrlReportServer = "http://192.168.0.120:8080/jasperserver";
        //[Obsolete]
        //public string EndPointServer = "http://192.168.56.1:8069"; //ODOO SERVER        
        //[Obsolete]
        //public string EndPointServerProd = "https://www.dmujeres.com.ec:8069";
        //[Obsolete]
        //public string CacheFilesUrl = "/resources/tmp/android/sqlite/";
        //[Obsolete]
        //public string CacheFilesUrlExternal = "/resources/tmp/android/sqlite/";
        //[Obsolete]
        //public string StaticResources_Server = "https://192.168.204.66:2443";
        //[Obsolete]
        //public string StaticResources_Server_Prod = "https://200.110.68.142:2443";
        //[Obsolete]
        //public string DefaultDatabase = "macronegocios_dev2";
        //[Obsolete]
        //public int db_limit_default = 300;
        //[Obsolete]
        //public bool isProduction { get; set; } = false;
        //[Obsolete]
        //public bool isTestMode { get; set; } = true;

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
