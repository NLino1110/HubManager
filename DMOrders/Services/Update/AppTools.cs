using DMSA.Sync.Core.Database.Sqlite;
using System;
using System.Collections.Generic;
using System.Text;

namespace DMOrders.Services.Update
{
    static public class AppTools
    {
        static public async Task<bool> ClearCacheData()
        {
            try
            {
                var resPartnerDb = new ResPartnerDb(App.Session.odooConnection.DbNameSqlite);
                await resPartnerDb.ClearFullCache();

                var productProductDb = new ProductProductDb(App.Session.odooConnection.DbNameSqlite);
                await productProductDb.ClearFullCache();

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error clearing cache: {ex.Message}");
                return false;
            }
        }
    }
}
