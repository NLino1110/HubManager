using DMSA.Models.Odoo.Native;

namespace DMSA.Models.Odoo.Security
{
    /// <summary>
    /// Rol "Mobile App" en res.users (Odoo).
    /// UI: selección [218] Administrador Apps Móviles vs sin asignar.
    /// </summary>
    public static class MobileAppUserRole
    {
        /// <summary>ID del rol/grupo Administrador Apps Móviles en Odoo (sel_groups_218).</summary>
        public const int AdministradorAppsMovilesId = 218;

        /// <summary>Valor local en user_access.is_mobile_app_admin: no es admin.</summary>
        public const int LocalFlagNo = 0;

        /// <summary>Valor local en user_access.is_mobile_app_admin: sí es admin. Nunca guardar 218.</summary>
        public const int LocalFlagYes = 1;

        /// <summary>
        /// true si web_read devuelve sel_groups_218 = 218 (Administrador Apps Móviles).
        /// No usar search_read/mobile_app_id: ese campo no viene poblado en RPC.
        /// </summary>
        public static bool IsAdministradorAppsMoviles(res_user_mobile_app_read? user)
        {
            if (user == null)
                return false;

            return IsSelGroup218Assigned(user.sel_groups_218);
        }

        /// <summary>Persiste 1 si es admin, 0 si no. No guardar el 218 de Odoo.</summary>
        public static int ToLocalFlag(bool isAdmin) => isAdmin ? LocalFlagYes : LocalFlagNo;

        /// <summary>0 = no admin, 1 = admin. 218 se acepta solo si una versión vieja lo guardó.</summary>
        public static bool FromLocalFlag(int stored) =>
            stored == LocalFlagYes || stored == AdministradorAppsMovilesId;

        /// <summary>Odoo sel_groups_218: 218 = asignado; false/0 = no. No es un 1/0 de Odoo.</summary>
        internal static bool IsSelGroup218Assigned(object? selGroups218)
        {
            if (selGroups218 == null)
                return false;

            return selGroups218 switch
            {
                bool assigned => assigned,
                int i => i == AdministradorAppsMovilesId,
                long l => l == AdministradorAppsMovilesId,
                string s when int.TryParse(s, out var parsed) => parsed == AdministradorAppsMovilesId,
                _ => false
            };
        }
    }
}
