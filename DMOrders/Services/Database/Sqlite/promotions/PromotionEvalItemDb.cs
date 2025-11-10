using DMSA.Models.Odoo.DMOrders.promotions;
using DMSA.Models.Odoo.DMOrders.promotions.@abstract;
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
    public class PromotionEvalItemDb : SqliteDbBase<PromotionEvalItem>
    {

        public PromotionEvalItemDb(string _DatabaseFilename) : base(_DatabaseFilename)
        {

        }

        public async Task<List<PromotionEvalItem>> GetItemsAsync()
        {
            await Init();
            return await Database.Table<PromotionEvalItem>().ToListAsync();
        }

        public async Task<PromotionEvalItem> GetItem(PromotionBenefit id)
        {
            await Init();
            return await Database.Table<PromotionEvalItem>().Where(x=>x.Promotion == id).FirstOrDefaultAsync();
        }
    }
}
