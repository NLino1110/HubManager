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
    public class PromotionProductDb : SqliteDbBase<PromotionProduct>
    {        
        public PromotionProductDb(string _DatabaseFilename) : base(_DatabaseFilename)
        {

        }

        public async Task<List<PromotionProduct>> GetItemsAsync()
        {
            await Init();
            return await Database.Table<PromotionProduct>().ToListAsync();
        }

        public async Task<PromotionProduct> GetItem(int id)
        {
            await Init();
            return await Database.Table<PromotionProduct>().Where(x=>x.id == id).FirstOrDefaultAsync();
        }
    }
}
