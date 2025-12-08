using DMSA.Models.Odoo.Native;
using SQLite;

namespace DMSA.Sync.Core.Database.Sqlite.Payments
{
    public class ProductBrandDb : SqliteDbBase<product_brand>
    {
        public ProductBrandDb(string _DatabaseFilename) : base(_DatabaseFilename)
        {

        }
  
    }
}
