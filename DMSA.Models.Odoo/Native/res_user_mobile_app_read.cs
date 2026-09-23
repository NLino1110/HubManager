using Newtonsoft.Json;

namespace DMSA.Models.Odoo.Native
{
    /// <summary>
    /// Respuesta parcial de Odoo web_read para rol Mobile App en res.users (login Cobranzas).
    /// </summary>
    public class res_user_mobile_app_read
    {
        public int id { get; set; }

        /// <summary>
        /// Campo UI Odoo "Mobile App". Odoo guarda el id del grupo (218), no 1/0.
        /// En SQLite se traduce a is_mobile_app_admin: 0 = no, 1 = sí.
        /// </summary>
        [JsonProperty("sel_groups_218")]
        public object? sel_groups_218 { get; set; }
    }
}
