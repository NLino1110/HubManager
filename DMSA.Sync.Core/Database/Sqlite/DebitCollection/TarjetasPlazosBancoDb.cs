using DMSA.Models.Odoo.Accounting;
using DMSA.Models.Odoo.DebitCollection;
using System.Globalization;

namespace DMSA.Sync.Core.Database.Sqlite.DebitCollection
{
    public class TarjetasPlazosBancoDb : SqliteDbBase<TarjetasPlazosBanco>
    {
        public TarjetasPlazosBancoDb(string _DatabaseFilename) : base(_DatabaseFilename)
        {
            
        }
    }
}
