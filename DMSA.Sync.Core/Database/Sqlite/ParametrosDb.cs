using DMSA.Models.Odoo.Inventory;

namespace DMSA.Sync.Core.Database.Sqlite
{
    public class AppParameterDb : SqliteDbBase<stock_quant>
    {
        public AppParameterDb(string _DatabaseFilename) : base(_DatabaseFilename)
        {

        }
    }
}
