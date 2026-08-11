using DMSA.Models.Odoo.Base;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SQLite;
using System.ComponentModel.DataAnnotations;

namespace DMSA.Models.Odoo.Native
{
    [Table("sale_order")]
    public class sale_order : OdooEntity
    {
        [Key]
        [PrimaryKey]
        [AutoIncrement]
        [NotNull]
        [JsonProperty("id")]
        public int id { get; set; }
                
        [Ignore]
        [JsonProperty("partner_id")]
        public JToken partner_id { get; set; }

        [JsonIgnore]
        public int _partner_id
        {
            get => GetId(partner_id);
            set => partner_id = SetId(partner_id, value);
        }

        [Ignore]
        [JsonProperty("company_id")]
        public JToken company_id { get; set; }

        [JsonIgnore]
        public int _company_id
        {
            get => GetId(company_id);
            set => company_id = SetId(company_id, value);
        }

        [JsonIgnore]
        public int _center_id
        {
            get => GetId(center_id);
            set => center_id = SetId(center_id, value);
        }

        [Ignore]
        [JsonProperty("center_id")]
        public JToken center_id { get; set; }

        [JsonProperty("date_order")]
        public DateTime date_order { get; set; }

        [Ignore]
        [JsonProperty("order_line")]
        public List<OrderLineWrapper> order_line { get; set; }

        [JsonIgnore]
        public int _warehouse_id
        {
            get => GetId(warehouse_id);
            set => warehouse_id = SetId(warehouse_id, value);
        }

        [Ignore]
        [JsonProperty("warehouse_id")]
        public JToken warehouse_id { get; set; }

        [Ignore]
        [JsonProperty("currency_id")]
        public JToken currency_id { get; set; }
        
        public int _currency_id
        {
            get => GetId(currency_id);
            set => currency_id = SetId(currency_id, value);
        }

        [JsonProperty("client_order_ref")]
        public string client_order_ref { get; set; }

        [JsonProperty("note")]
        public string note { get; set; }

        [JsonProperty("note2")]
        public string note2 { get; set; }

        [Ignore]
        [JsonProperty("partner_invoice_id")]
        public JToken partner_invoice_id { get; set; }

        [Ignore]
        [JsonProperty("partner_shipping_id")]
        public JToken partner_shipping_id { get; set; }

        [Ignore]
        [JsonProperty("pricelist_id")]
        public JToken pricelist_id { get; set; }
                
        [JsonProperty("_pricelist_id")]
        public int _pricelist_id { get; set; }

        [Ignore]
        [JsonProperty("payment_term_id")]
        public JToken payment_term_id { get; set; }

        [Ignore]
        [JsonProperty("team_id")]
        public JToken team_id { get; set; }

        [Ignore]
        [JsonProperty("user_id")]
        public JToken user_id { get; set; }

        [JsonProperty("amount_untaxed")]
        public decimal amount_untaxed { get; set; }

        [JsonProperty("amount_tax")]
        public decimal amount_tax { get; set; }

        [JsonProperty("amount_total")]
        public decimal amount_total { get; set; }

        [JsonProperty("is_intercompany")]
        public bool is_intercompany { get; set; }

        [JsonProperty("mobile_sync")]
        public bool mobile_sync { get; set; }

        [JsonProperty("external_create_uid")]
        public int external_create_uid { get; set; }
        [JsonProperty("external_guid")]
        public string external_guid { get; set; }

        [JsonProperty("create_date")]
        public DateTime create_date { get; set; }

        [JsonProperty("mobile_create_date")]
        public DateTime mobile_create_date { get; set; }        

        [JsonProperty("write_date")]
        public DateTime write_date { get; set; }

        [JsonIgnore]
        public bool is_synchronized { get; set; }
        [JsonIgnore]
        public DateTime date_synchronized { get; set; }
        [JsonIgnore]
        public bool is_imported { get; set; }
        [JsonIgnore]
        public DateTime date_imported { get; set; }
                
        public int sale_channel { get; set; }

        [JsonIgnore]
        public int erp_id { get; set; }
        
        [JsonProperty("name")]
        public string? name { get; set; }

        [JsonIgnore]
        public string erp_name { get; set; }

        [JsonProperty("id_referencia")]
        public string id_referencia { get; set; }
                
        [JsonIgnore]
        public string partner_display_name { get; set; } = "-";
                
        [JsonIgnore]
        public string partner_display_address { get; set; } = "-";
        
        [JsonIgnore]
        public string partner_display_status { get; set; } = "-";

        [JsonProperty("state")]
        public string? state { get; set; }

        /// <summary>
        /// Etapa del flujo web (Odoo free_order_state). Vacío si el pedido aún no está en ERP.
        /// </summary>
        [JsonProperty("free_order_state")]
        public string? free_order_state { get; set; }

        [JsonProperty("partner_sale_id")]
        public int partner_sale_id { get; set; }

        [JsonProperty("_partner_invoice_id")]
        public int _partner_invoice_id { get; set; }

        [JsonProperty("_partner_shipping_id")]
        public int _partner_shipping_id { get; set; }

        [Ignore]
        [JsonIgnore]
        public string? state_view =>
                (state, is_synchronized) switch
                {
                    ("draft", true) => "SINCRONIZADO",
                    ("draft", false) => "ACTIVO",
                    ("sent", _) => "SINCRONIZADO",
                    ("sale", _) => "FACTURADO",
                    ("done", _) => "TERMINADO",
                    ("cancel", _) => "CANCELADO",
                    _ => state
                };

        /// <summary>
        /// Texto de etapa para el listado. Null/vacío (pedido local no sync) → "-".
        /// </summary>
        [Ignore]
        [JsonIgnore]
        public string free_order_state_view =>
            string.IsNullOrWhiteSpace(free_order_state)
                ? "-"
                : free_order_state switch
                {
                    "INGRESADO" => "INGRESADO",
                    "REVCREDITO" => "REVISIÓN CREDITO",
                    "ESPERAAPROBACION" => "EN ESPERA APROBACIÓN",
                    "REVCOMPLETA" => "REVISIÓN COMPLETA",
                    "ESPERAWMS" => "EN PROCESO WMS",
                    "RESTRICCION" => "RESTRICCIÓN",
                    "FINALIZADO" => "FACTURADO",
                    "RECHAZADO" => "RECHAZADO",
                    "APROBADO" => "APROBADO",
                    "RESPALDO" => "RESPALDO PEDIDO",
                    "False" => "-",
                    "false" => "-",
                    _ => free_order_state
                };

        [Ignore]
        [JsonProperty("promotion_ids")]
        public int[] promotion_ids { get; set; }        

        [JsonIgnore]
        public string promotion_ids_json
        {
            get => promotion_ids == null ? "[]" : JsonConvert.SerializeObject(promotion_ids);
            set
            {
                promotion_ids = string.IsNullOrWhiteSpace(value)
                    ? Array.Empty<int>()
                    : JsonConvert.DeserializeObject<int[]>(value);
            }
        }

        [Ignore]
        [JsonProperty("external_payload")]
        public JObject external_payload { get; set; }
    }
}
