using CobranzasDMSA_Odoo.Models;
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
    public class SaleOrderLineDb : SqliteDbBase<sale_order_line>
    {

        public SaleOrderLineDb(string _DatabaseFilename) : base(_DatabaseFilename)
        {

        }

        public async Task<List<sale_order_line>> GetItemsAsync(int order_id)
        {
            await Init();
            return await Database.Table<sale_order_line>()
                .Where(x => x._order_id == order_id)
                .ToListAsync();
        }

        public async Task<sale_order_line> GetItem(int id)
        {
            await Init();
            return await Database.Table<sale_order_line>().Where(x=>x.id == id).FirstOrDefaultAsync();
        }

        public async Task<List<sale_order_line>> GetItemsByParent(sale_order parent)
        {
            await Init();
            var resultItems = await Database.Table<sale_order_line>().Where(i => i._order_id == parent.id).ToListAsync();
            return resultItems;
        }

        public async Task<int> DeleteItemOfParent(sale_order parent)
        {
            await Init();
            int count = 0;
            //await Init();

            var resultItems = (await Database.Table<sale_order_line>().ToListAsync()).Where(i => i._order_id == parent.id);

            foreach (var item in resultItems)
            {
                count++;
                await Database.DeleteAsync(item);
            }

            return count;
        }  
    }
}
