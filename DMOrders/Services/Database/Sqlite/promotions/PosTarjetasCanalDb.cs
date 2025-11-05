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
    public class PosTarjetasCanalDb : SqliteDbBase<PosTarjetasCanal>
    {

        public PosTarjetasCanalDb(string _DatabaseFilename) : base(_DatabaseFilename)
        {

        }


        public async Task<List<PosTarjetasCanal>> GetItemsAsync()
        {
            await Init();
            return await Database.Table<PosTarjetasCanal>().ToListAsync();
        }

        public async Task<PosTarjetasCanal> GetItem(int id)
        {
            await Init();
            return await Database.Table<PosTarjetasCanal>().Where(x=>x.id == id).FirstOrDefaultAsync();
        }

    }
}
