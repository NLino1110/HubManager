using DMSA.Models.Odoo.Native;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMOrdersUI.Services.Database.Sqlite
{
    public class StockWareHouseDb : SqliteDbBase<stock_warehouse>
    {
        public async Task<stock_warehouse> GetItem(int id)
        {
            await Init();
            return await Database.Table<stock_warehouse>().Where(x => x.id == id).FirstOrDefaultAsync();
        }

        public async Task<List<stock_warehouse>> GetItemsAsync(int company_id)
        {
            await Init();
            return await Database.Table<stock_warehouse>()
                .Where(x => x._company_id == company_id)
                .ToListAsync();
        }
    }
}
