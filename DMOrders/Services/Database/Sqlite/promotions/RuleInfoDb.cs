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
    public class RuleInfoDb : SqliteDbBase<RuleInfo>
    {
        public RuleInfoDb(string _DatabaseFilename) : base(_DatabaseFilename)
        {

        }

        public async Task<List<RuleInfo>> GetItemsAsync()
        {
            await Init();
            return await Database.Table<RuleInfo>().ToListAsync();
        }

        public async Task<RuleInfo> GetItem(int id)
        {
            await Init();
            return await Database.Table<RuleInfo>().Where(x=>x.Id == id).FirstOrDefaultAsync();
        }  
    }
}
