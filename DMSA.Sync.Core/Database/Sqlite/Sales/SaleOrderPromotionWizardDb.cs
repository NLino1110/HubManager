using DMSA.Models.Odoo.Native;
using DMSA.Models.Odoo.Promotions.Wizard;
using SQLite;

namespace DMSA.Sync.Core.Database.Sqlite
{
    public class SaleOrderPromotionWizardDb : SqliteDbBase<SaleOrderPromotionWizard>
    {       
        public SaleOrderPromotionWizardDb(string _DatabaseFilename) : base(_DatabaseFilename)
        {

        } 
    }
}
