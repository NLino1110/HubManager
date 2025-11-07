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
    public class PromoRulesDb : SqliteDbBase<PromoRules>
    {

        public PromoRulesDb(string _DatabaseFilename) : base(_DatabaseFilename)
        {

        }

        public async Task<List<PromoRules>> GetItemsAsync()
        {
            await Init();
            return await Database.Table<PromoRules>().ToListAsync();
        }

        public async Task<PromoRules> GetItem(int id)
        {
            await Init();
            return await Database.Table<PromoRules>().Where(x=>x.id == id).FirstOrDefaultAsync();
        }

        public async Task<List<PromoRules>> GetItemsByParent(int parentId)
        {
            await Init();
            return await Database.Table<PromoRules>().Where(x=>x._promo_id == parentId).ToListAsync();
        }
    }
}
