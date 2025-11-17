using DMSA.Models.Odoo.Modules.Accounting;
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
    public class AccountTaxDb : SqliteDbBase<AccountTax>
    {
        public AccountTaxDb(string _DatabaseFilename) : base(_DatabaseFilename)
        {

        }

        public async Task<AccountTax> GetItem(int id)
        {
            await Init();
            return await Database.Table<AccountTax>().Where(x=>x.id == id).FirstOrDefaultAsync();
        }  
    }
}
