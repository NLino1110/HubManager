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
    public class ProductTemplateDb : SqliteDbBase<product_template>
    {
        public ProductTemplateDb(string _DatabaseFilename) : base(_DatabaseFilename)
        {

        }

        public async Task<List<product_template>> GetItemsAsync()
        {
            await Init();
            return await Database.Table<product_template>().Take(50).ToListAsync();
        }

        public async Task<product_template> GetItem(int id)
        {
            await Init();
            return await Database.Table<product_template>().Where(x=>x.id == id).FirstOrDefaultAsync();
        }   
    }
}
