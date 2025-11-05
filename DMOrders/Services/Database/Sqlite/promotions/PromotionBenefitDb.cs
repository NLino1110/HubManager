using DMSA.Models.Odoo.DMOrders.promotions;
using DMSA.Models.Odoo.Native;
using Microsoft.Data.Sqlite;
using SQLite;
using SQLiteNetExtensions.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMOrders.Services.Database.Sqlite
{
    public class PromotionBenefitDb: SqliteDbBase<PromotionBenefit>
    {
        public PromotionBenefitDb(string _DatabaseFilename) : base(_DatabaseFilename)
        {

        }

        public async Task<List<PromotionBenefit>> GetItemsAsync()
        {
            await Init();
            return await Database.Table<PromotionBenefit>().ToListAsync();
        }

        public async Task<PromotionBenefit> GetItem(int id)
        {
            await Init();
            return await Database.Table<PromotionBenefit>().Where(x=>x.id == id).FirstOrDefaultAsync();
        }
  
    }
}
