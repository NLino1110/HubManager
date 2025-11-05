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
    public class PosPaymentMethodDb : SqliteDbBase<PosPaymentMethod>
    {        
        public PosPaymentMethodDb(string _DatabaseFilename) : base(_DatabaseFilename)
        {

        }

        public async Task<List<PosPaymentMethod>> GetItemsAsync()
        {
            await Init();
            return await Database.Table<PosPaymentMethod>().ToListAsync();
        }

        public async Task<PosPaymentMethod> GetItem(int id)
        {
            await Init();
            return await Database.Table<PosPaymentMethod>().Where(x=>x.Id == id).FirstOrDefaultAsync();
        } 
    }
}
