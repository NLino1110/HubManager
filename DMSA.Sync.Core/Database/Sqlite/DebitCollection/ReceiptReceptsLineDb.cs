using DMSA.Models.Odoo.Accounting;
using DMSA.Models.Odoo.DebitCollection;
using System.Globalization;

namespace DMSA.Sync.Core.Database.Sqlite.DebitCollection
{
    public class ReceiptReceiptsLineDb : SqliteDbBase<ReceiptReceiptsLine>
    {
        public ReceiptReceiptsLineDb(string _DatabaseFilename) : base(_DatabaseFilename)
        {
            
        }
    }
}
