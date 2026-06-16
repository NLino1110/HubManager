using DMSA.Models.Odoo.Abstract;
using DMSA.Models.Odoo.Accounting;
using DMSA.Models.Odoo.Inventory;
using DMSA.Models.Odoo.Native;
using System.Globalization;

namespace DMSA.Models.Security
{
    public class AppSession
    {
        public GlobalSettings globalSettings { get; set; }
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

        public res_company res_Company { get; set; }
        public res_center res_center { get; set; }
        public stock_warehouse stock_Warehouse { get; set; }

        [Obsolete("Debe ser eliminado")]
        public res_store res_Store { get; set; } //Sin referencias
        [Obsolete]
        public string UrlReportServer = "http://192.168.0.120:8080/jasperserver";        

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
