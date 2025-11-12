using DMSA.Models.Odoo.Base;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SQLite;
using SQLiteNetExtensions.Attributes;

namespace DMSA.Models.Odoo.Modules.Accounting
{
    [Table("account_tax")]
    public class AccountTax : OdooEntity
    {
        [PrimaryKey]
        [JsonProperty("id")]
        public int id { get; set; }
        [JsonProperty("name")]
        public string name { get; set; }
        [JsonProperty("type_tax_use")]
        public string type_tax_use { get; set; }
        [JsonProperty("tax_scope")]
        public bool tax_scope { get; set; }
        [JsonProperty("amount_type")]
        public string amount_type { get; set; }
        [JsonProperty("active")]
        public bool active { get; set; }


        [Ignore]
        [JsonProperty("company_id")]
        public JToken company_id { get; set; }
        [JsonIgnore]
        public int _company_id
        {
            get => GetId(company_id);
            set => company_id = SetId(company_id, value);
        }


        [JsonProperty("sequence")]        
        public int sequence { get; set; }
        [JsonProperty("amount")]
        public float amount { get; set; }
        [JsonProperty("description")]
        public bool description { get; set; }
        [JsonProperty("invoice_label")]
        public string invoice_label { get; set; }
        [JsonProperty("price_include")]
        public bool price_include { get; set; }
        [JsonProperty("company_price_include")]
        public string company_price_include { get; set; }
        [JsonProperty("price_include_override")]
        public bool price_include_override { get; set; }
        [JsonProperty("include_base_amount")]
        public bool include_base_amount { get; set; }
        [JsonProperty("is_base_affected")]
        public bool is_base_affected { get; set; }
        [JsonProperty("analytic")]
        public bool analytic { get; set; }
        [JsonProperty("hide_tax_exigibility")]
        public bool hide_tax_exigibility { get; set; }
        [JsonProperty("tax_exigibility")]
        public string tax_exigibility { get; set; }
        [JsonProperty("cash_basis_transition_account_id")]
        public bool cash_basis_transition_account_id { get; set; }
        [JsonProperty("is_used")]
        public bool is_used { get; set; }
        [JsonProperty("repartition_lines_str")]
        public string repartition_lines_str { get; set; }
        [JsonProperty("invoice_legal_notes")]
        public bool invoice_legal_notes { get; set; }
        [JsonProperty("has_negative_factor")]
        public bool has_negative_factor { get; set; }
        [JsonProperty("display_name")]
        public string display_name { get; set; }
        [JsonProperty("l10n_ec_code_base")]
        public bool l10n_ec_code_base { get; set; }
        [JsonProperty("l10n_ec_code_applied")]
        public bool l10n_ec_code_applied { get; set; }
        [JsonProperty("l10n_ec_code_ats")]
        public bool l10n_ec_code_ats { get; set; }
        [JsonProperty("code_base")]
        public string code_base { get; set; }
        [JsonProperty("write_date")]
        public DateTime write_date { get; set; }
        [JsonProperty("create_date")]
        public DateTime create_date { get; set; }

    }



}
