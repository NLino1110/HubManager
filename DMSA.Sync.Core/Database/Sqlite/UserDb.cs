using CobranzasDMSA_Odoo.Models;
using DMSA.Models.Odoo.DMApps;
using DMSA.Models.Odoo.Native;
using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMOrders.Services.Database.Sqlite
{
    public class UserDb : SqliteDbBase<res_user>
    {
        public UserDb(string _DatabaseFilename) : base(_DatabaseFilename)
        {

        }

        public async Task<res_user> GetItemsAsync(int company_id, int user_id)
        {
            await Init();
            return await Database.Table<res_user>().Where(x => x._company_id == company_id &&
            x.id == user_id).FirstOrDefaultAsync();
        }

        public async Task<List<res_user>> GetItemsAsync()
        {
            await Init();
            return await Database.Table<res_user>().ToListAsync();
        }

        public async Task<res_user> GetItem(int id)
        {
            await Init();
            return await Database.Table<res_user>().Where(x => x.id == id).FirstOrDefaultAsync();
        }
    }
}
