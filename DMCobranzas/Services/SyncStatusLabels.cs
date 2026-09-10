using DMSA.Models.Odoo.Abstract;
using DMSA.Models.Odoo.Security;
using DMSA.Models.Security;
using DMSA.Sync.Core.Database.Sqlite;

namespace DMCobranzas.Services
{
    public static class SyncStatusLabels
    {
        public const string LastSyncPreferenceKeyPrefix = "last_log_fec_sincro";

        public static string BuildLastSyncPreferenceKey(string dbNameSqlite)
        {
            if (string.IsNullOrWhiteSpace(dbNameSqlite))
                return LastSyncPreferenceKeyPrefix;

            return $"{LastSyncPreferenceKeyPrefix}_{dbNameSqlite}";
        }

        public static void PersistLastSyncDate(DateTime syncDate, OdooConnection connection = null)
        {
            if (syncDate.Year <= 2000)
                return;

            var conn = connection ?? App.Session?.odooConnection;
            Preferences.Set(BuildLastSyncPreferenceKey(conn?.DbNameSqlite), syncDate.ToString("o"));
        }

        public static async Task<DateTime> ResolveLastSyncDateAsync(
            AppSession session = null,
            string usernameHint = null,
            OdooConnection connection = null)
        {
            var source = session ?? App.Session;
            var conn = connection ?? source?.odooConnection ?? App.Session?.odooConnection;
            var dbNameSqlite = conn?.DbNameSqlite;

            if (!string.IsNullOrWhiteSpace(dbNameSqlite))
            {
                var syncFromDb = await ResolveFromConnectionDatabaseAsync(
                    dbNameSqlite,
                    source,
                    usernameHint);

                if (syncFromDb.Year > 2000)
                    return syncFromDb;

                var raw = Preferences.Get(BuildLastSyncPreferenceKey(dbNameSqlite), string.Empty);
                if (!string.IsNullOrEmpty(raw)
                    && DateTime.TryParse(raw, out var prefDate)
                    && prefDate.Year > 2000)
                {
                    return prefDate;
                }
            }

            if (source?.CurrentUserFront != null
                && source.CurrentUserFront.log_fec_sincro.Year > 2000
                && source.odooConnection != null
                && conn != null
                && string.Equals(
                    source.odooConnection.DbNameSqlite,
                    conn.DbNameSqlite,
                    StringComparison.Ordinal))
            {
                return source.CurrentUserFront.log_fec_sincro;
            }

            return DateTime.MinValue;
        }

        private static async Task<DateTime> ResolveFromConnectionDatabaseAsync(
            string dbNameSqlite,
            AppSession source,
            string usernameHint)
        {
            var userDb = new UserAccessDb(dbNameSqlite);
            user_access found = null;

            if (source?.CurrentUserFront?.uid > 0)
                found = await userDb.GetItemAsync(source.CurrentUserFront.uid);

            if ((found == null || found.log_fec_sincro.Year <= 2000)
                && !string.IsNullOrWhiteSpace(usernameHint))
            {
                var users = await userDb.GetItemsAsync();
                found = users?.FirstOrDefault(u => u.username == usernameHint);
            }

            if (found == null || found.log_fec_sincro.Year <= 2000)
            {
                var users = await userDb.GetItemsAsync();
                found = users?
                    .Where(u => u.log_fec_sincro.Year > 2000)
                    .OrderByDescending(u => u.log_fec_sincro)
                    .FirstOrDefault();
            }

            return found != null && found.log_fec_sincro.Year > 2000
                ? found.log_fec_sincro
                : DateTime.MinValue;
        }

        public static string FormatLoginLastSyncText(DateTime syncDate) =>
            syncDate.Year > 2000
                ? "Ult. sincronizacion: " + syncDate.ToString("dd/MM/yyyy HH:mm")
                : "Ult. sincronizacion: -";

        public static string FormatHomeLastSyncText(DateTime syncDate) =>
            syncDate.Year > 2000
                ? "Ult. Actualización: " + syncDate.ToString("dd/MM/yyyy HH:mm:ss")
                : "Ult. Actualización: -";

        public static string FormatAppUpdateText()
        {
            var updateDate = AppTools.GetAppInstallOrUpdateDate();
            return updateDate.HasValue && updateDate.Value.Year > 2000
                ? "Actualizacion APK: " + updateDate.Value.ToString("dd/MM/yyyy HH:mm")
                : "Actualizacion APK: -";
        }
    }
}
