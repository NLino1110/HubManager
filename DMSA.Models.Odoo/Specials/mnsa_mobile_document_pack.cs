using DMSA.Models.Odoo.Base;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DMSA.Models.Odoo.Specials
{
    /// <summary>
    /// Paquete ZIP diario de cabeceras/detalle (mnsa.mobile.document.pack).
    /// </summary>
    public class mnsa_mobile_document_pack : OdooEntity
    {
        [JsonProperty("id")]
        public int id { get; set; }

        [JsonProperty("name")]
        public string name { get; set; }

        [JsonProperty("date_from")]
        public DateTime date_from { get; set; }

        [JsonProperty("date_to")]
        public DateTime date_to { get; set; }

        [JsonProperty("state")]
        public string state { get; set; }

        [JsonProperty("header_count")]
        public int header_count { get; set; }

        [JsonProperty("line_count")]
        public int line_count { get; set; }

        [JsonProperty("attachment_id")]
        public JToken attachment_id { get; set; }

        [JsonIgnore]
        public int _attachment_id
        {
            get => GetId(attachment_id);
            set => attachment_id = SetId(attachment_id, value);
        }
    }
}
