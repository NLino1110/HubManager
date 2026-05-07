using DMSA.Sync.Core.Database.Sqlite;
using DMSA.Sync.Core.Services;
using System;
using System.Collections.Generic;
using System.Text;

namespace DMCobranzas.Services
{
    static public class AppTools
    {
        static public void BuildPushRelay()
        {
            if (App.PushRelayGlobal != null)
                return;

            App.PushRelayGlobal = new PushRelay("https://manager.dmujeres.ec:5001/chatHub", AppTools.GetDeviceId());
            App.PushRelayGlobal.Name = "---";
            App.PushRelayGlobal.Message = "ConnectCommand";
        }

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

        static public string GetDeviceId()
        {
            string deviceID = string.Empty;

#if ANDROID
            deviceID = Android.Provider.Settings.Secure.GetString(
                Platform.CurrentActivity.ContentResolver,
                Android.Provider.Settings.Secure.AndroidId);

#elif IOS
    deviceID = UIKit.UIDevice.CurrentDevice.IdentifierForVendor.ToString();

#elif WINDOWS
    try
    {
        using var key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(
            @"SOFTWARE\Microsoft\Cryptography");

        deviceID = key?.GetValue("MachineGuid")?.ToString() ?? "";
    }
    catch
    {
        deviceID = Environment.MachineName;
    }
#endif

            return deviceID;
        }
    }
}
