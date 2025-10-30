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

        //public bool ShouldSerializepartner_id() => false;
        //public bool ShouldSerialize_partner_id() => true;

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

        [Ignore]
        [JsonProperty("partner_invoice_id")]
        public JToken partner_invoice_id { get; set; }

        [Ignore]
        [JsonProperty("partner_shipping_id")]
        public JToken partner_shipping_id { get; set; }

        [Ignore]
        [JsonProperty("pricelist_id")]
        public JToken pricelist_id { get; set; }

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

        [JsonProperty("create_date")]
        public DateTime create_date { get; set; }
        
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

        [JsonProperty("id_referencia")]
        public string id_referencia { get; set; }

        [Ignore]
        [JsonIgnore]
        public string partner_display_name { get; set; } = "-";

        [Ignore]
        [JsonIgnore]
        public string partner_display_address { get; set; } = "-";
        [Ignore]
        [JsonIgnore]
        public string partner_display_status { get; set; } = "-";
    }
}
