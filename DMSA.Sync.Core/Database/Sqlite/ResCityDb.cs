using DMSA.Models.Odoo.Native;

namespace DMSA.Sync.Core.Database.Sqlite
{
    public class ResCityDb : SqliteDbBase<res_city>
    {
        public ResCityDb(string _DatabaseFilename) : base(_DatabaseFilename)
        {

        }        
    }
}
