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
    public class PromoCentersDb : SqliteDbBase<PromoCenters>
    {   

        public PromoCentersDb(string _DatabaseFilename) : base(_DatabaseFilename)
        {

        }

        public async Task<List<PromoCenters>> GetItemsAsync()
        {
            await Init();
            return await Database.Table<PromoCenters>().ToListAsync();
        }

        public async Task<PromoCenters> GetItem(int id)
        {
            await Init();
            return await Database.Table<PromoCenters>().Where(x=>x.id == id).FirstOrDefaultAsync();
        }

        public async Task<List<PromoCenters>> GetItemsByParent(int parentId)
        {
            await Init();
            return await Database.Table<PromoCenters>().Where(x => x._promo_id == parentId).ToListAsync();
        }
    }
}
