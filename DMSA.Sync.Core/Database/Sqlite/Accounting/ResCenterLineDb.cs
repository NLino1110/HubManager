using DMSA.Models.Odoo.Accounting;

namespace DMSA.Sync.Core.Database.Sqlite.Accounting
{
    public class ResCenterLineDb : SqliteDbBase<res_center_line>
    {
        public ResCenterLineDb(string _DatabaseFilename) : base(_DatabaseFilename)
        {

        }
    }
}
