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
    public class PromotionSelectionTypeDb : SqliteDbBase<PromotionSelectionType>
    {
        public PromotionSelectionTypeDb(string _DatabaseFilename) : base(_DatabaseFilename)
        {

        }

        public async Task<List<PromotionSelectionType>> GetItemsAsync()
        {
            await Init();
            return await Database.Table<PromotionSelectionType>().ToListAsync();
        }

        public async Task<PromotionSelectionType> GetItem(int id)
        {
            await Init();
            return await Database.Table<PromotionSelectionType>().Where(x=>x.Id== id).FirstOrDefaultAsync();
        }

    }
}
