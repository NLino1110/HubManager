using DMSA.Models.Odoo.Native;
using SQLite;

namespace DMSA.Sync.Core.Database.Sqlite.Payments
{
    [Obsolete("Parece que esta clase ya no se utiliza")]
    public class SriPrinterPointDb : SqliteDbBase<sri_printer_point>
    {
        public SriPrinterPointDb(string _DatabaseFilename) : base(_DatabaseFilename)
        {

        }  
    }
}
