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
    public class LoyaltyFiltersDetailsDb : SqliteDbBase<LoyaltyFiltersDetail>
    {
        public LoyaltyFiltersDetailsDb(string _DatabaseFilename) : base(_DatabaseFilename)
        {

        }

        public async Task<List<LoyaltyFiltersDetail>> GetItemsAsync()
        {
            await Init();
            return await Database.Table<LoyaltyFiltersDetail>().ToListAsync();
        }

        public async Task<LoyaltyFiltersDetail> GetItem(int id)
        {
            await Init();
            return await Database.Table<LoyaltyFiltersDetail>().Where(x=>x.id == id).FirstOrDefaultAsync();
        } 
    }
}
