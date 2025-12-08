using DMSA.Models.Odoo.Native;
using SQLite;

namespace DMSA.Sync.Core.Database.Sqlite.Payments
{
    public class ProductCategoryDb : SqliteDbBase<product_category>
    {
        public ProductCategoryDb(string _DatabaseFilename) : base(_DatabaseFilename)
        {

        }  
    }
}
