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
    public class UomUomDb : SqliteDbBase<uom_uom>
    {
        public UomUomDb(string _DatabaseFilename) : base(_DatabaseFilename)
        {

        }

        public async Task<List<uom_uom>> GetItemsAsync()
        {
            await Init();
            return await Database.Table<uom_uom>().ToListAsync();
        }

        public async Task<uom_uom> GetItem(int id)
        {
            await Init();
            return await Database.Table<uom_uom>().Where(x=>x.id == id).FirstOrDefaultAsync();
        } 
    }
}
