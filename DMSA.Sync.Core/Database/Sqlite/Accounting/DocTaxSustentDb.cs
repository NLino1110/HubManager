using DMSA.Models.Odoo.Accounting;

namespace DMSA.Sync.Core.Database.Sqlite.Accounting
{
    public class DocTaxSustentDb : SqliteDbBase<doc_tax_sustent>
    {
        public DocTaxSustentDb(string _DatabaseFilename) : base(_DatabaseFilename)
        {

        }
    }
}
