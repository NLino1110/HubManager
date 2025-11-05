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
    public class PromotionProductDetailDb : SqliteDbBase<PromotionProductDetail>
    {


        public PromotionProductDetailDb(string _DatabaseFilename) : base(_DatabaseFilename)
        {

        }

        public async Task<List<PromotionProductDetail>> GetItemsAsync()
        {
            await Init();
            return await Database.Table<PromotionProductDetail>().ToListAsync();
        }

        public async Task<PromotionProductDetail> GetItem(int id)
        {
            await Init();
            return await Database.Table<PromotionProductDetail>().Where(x=>x.id == id).FirstOrDefaultAsync();
        }
    }
}
