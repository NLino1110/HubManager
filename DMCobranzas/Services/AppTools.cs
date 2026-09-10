using DMSA.Models.Security;
using DMSA.Sync.Core.Database.Sqlite;
using DMSA.Sync.Core.Services;

namespace DMCobranzas.Services
{
    static public class AppTools
    {
        static async public Task GlobalSettingInit(AppSession session)
        {
            try
            {
                var globalSettingsDb = new GlobalSettingsDb();
                await globalSettingsDb.InitDefault();
                session.globalSettings = (await globalSettingsDb.GetItemsAsync(x => x.Id > 0)).FirstOrDefault();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error GlobalSettingInit: {ex.Message}");
            }
        }

        static public void BuildPushRelay(AppSession session)
        {
            if (App.PushRelayGlobal != null)
                return;

            App.PushRelayGlobal = new PushRelay(session.globalSettings.PushServer, AppTools.GetDeviceId());
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

        /// <summary>
        /// Fecha en que se instaló o actualizó el APK en el dispositivo.
        /// </summary>
        static public DateTime? GetAppInstallOrUpdateDate()
        {
            try
            {
#if ANDROID
                var context = Android.App.Application.Context;
                var packageInfo = context.PackageManager.GetPackageInfo(context.PackageName, (Android.Content.PM.PackageInfoFlags)0);
                long millis = packageInfo.LastUpdateTime;
                if (millis <= 0)
                    millis = packageInfo.FirstInstallTime;

                if (millis > 0)
                    return DateTimeOffset.FromUnixTimeMilliseconds(millis).LocalDateTime;
#elif WINDOWS
                var path = Environment.ProcessPath;
                if (!string.IsNullOrEmpty(path) && File.Exists(path))
                    return File.GetLastWriteTime(path);
#endif
            }
            catch (Exception ex)
            {
                Console.WriteLine($"GetAppInstallOrUpdateDate: {ex.Message}");
            }

            return null;
        }
    }
}
