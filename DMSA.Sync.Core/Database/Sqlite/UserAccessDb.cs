using DMSA.Models.Odoo.DMApps;
using DMSA.Models.Odoo.Native;
using Microsoft.Data.Sqlite;
using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMOrders.Services.Database.Sqlite
{
    public class UserAccessDb : SqliteDbBase<user_access>
    {
        public UserAccessDb(string _DatabaseFilename) : base(_DatabaseFilename)
        {

        }

        public async Task<List<user_access>> GetItemsAsync()
        {
            await Init();
            return await Database.Table<user_access>().ToListAsync();
        }

        public async Task<user_access> GetItemAsync(int id)
        {
            await Init();
            return await Database.Table<user_access>().Where(i => i.uid == id).FirstOrDefaultAsync();
        }
    
    }
}
