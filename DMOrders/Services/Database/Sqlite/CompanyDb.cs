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
    public class CompanyDb : SqliteDbBase<res_company>
    {
        public CompanyDb(string _DatabaseFilename) : base(_DatabaseFilename)
        {

        }

        public async Task<List<res_company>> GetItemsAsync()
        {
            await Init();
            return await Database.Table<res_company>().ToListAsync();
            //return Database.Table<account_journal>().ToList();
        }

        public async Task<res_company> GetItem(int id)
        {
            await Init();
            return await Database.Table<res_company>().Where(i => i.id == id).FirstOrDefaultAsync();
        }
    }
}
