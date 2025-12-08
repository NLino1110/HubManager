using DMSA.Models.Odoo.Native;
using SQLite;

namespace DMSA.Sync.Core.Database.Sqlite.Payments
{
    public class SriPrinterPointDb : SqliteDbBase<sri_printer_point>
    {
        public SriPrinterPointDb(string _DatabaseFilename) : base(_DatabaseFilename)
        {

        }  
    }
}
