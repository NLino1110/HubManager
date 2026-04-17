using DMSA.Models.Odoo.Accounting;

namespace DMSA.Sync.Core.Database.Sqlite.Accounting
{
    public class DocAuthorizationLineDb : SqliteDbBase<doc_authorization_line>
    {
        public DocAuthorizationLineDb(string _DatabaseFilename) : base(_DatabaseFilename)
        {

        }
    }
}
