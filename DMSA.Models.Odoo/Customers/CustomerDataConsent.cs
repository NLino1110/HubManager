using DMSA.Models.Odoo.Base;
using DMSA.Models.Odoo.Json.Converters;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SQLite;

namespace DMSA.Models.Odoo.Customers
{
    public class CustomerDataConsent
    {
        [JsonProperty("id")]
        [Column("id")]
        public int Id { get; set; }

        [JsonProperty("res_partner_id")]
        [JsonConverter(typeof(OdooMany2OneConverter))]
        public OdooMany2One? ResPartner { get; set; }

        [JsonProperty("consent_type")]
        [Column("consent_type")]
        public string? ConsentType { get; set; }


        [JsonProperty("res_partner")]
        [Column("res_partner")]
        public JToken? res_partner { get; set; }

        [JsonProperty("res_center")]
        [Column("res_center")]
        public JToken? res_center { get; set; }

        [JsonProperty("policy_version")]
        [Column("policy_version")]
        public string? PolicyVersion { get; set; }

        [JsonProperty("ip_address")]
        [Column("ip_address")]
        public string? IpAddress { get; set; }
        
        [JsonProperty("email")]
        [Column("email")]
        public string? Email { get; set; }

        [JsonProperty("doc_type_identification_id")]
        [Column("doc_type_identification_id")]
        public string? doc_type_identification_id { get; set; }

        [JsonProperty("vat_doc")]
        [Column("vat_doc")]
        public string? vat_doc { get; set; }

        [JsonProperty("first_name")]
        [Column("first_name")]
        public string? FirstName { get; set; }
        
        [JsonProperty("last_name")]
        [Column("last_name")]
        public string? LastName { get; set; }

        [JsonProperty("phone")]
        [Column("phone")]
        public string? Phone { get; set; }

        [JsonProperty("address")]
        [Column("address")]
        public string? Address { get; set; }
        
        [JsonProperty("application_origin")]
        [Column("application_origin")]
        public string? ApplicationOrigin { get; set; }

        [JsonProperty("device_app_version")]
        [Column("device_app_version")]
        public string? device_app_version { get; set; }

        [JsonProperty("device_platform_mod")]
        [Column("device_platform_mod")]
        public string? device_platform_mod { get; set; }

        [JsonProperty("device_platform_source")]
        [Column("device_platform_source")]
        public string? device_platform_source { get; set; }

        [JsonProperty("device_manufacturer")]
        [Column("device_manufacturer")]
        public string? device_manufacturer { get; set; }

        [JsonProperty("device_model")]
        [Column("device_model")]
        public string? device_model { get; set; }

        [JsonProperty("response_state")]
        [Column("response_state")]
        public string? ResponseState { get; set; }

        [JsonProperty("create_date")]
        [Column("create_date")]
        public DateTime? CreateDate { get; set; }

        [JsonProperty("write_date")]
        [Column("write_date")]
        public DateTime? WriteDate { get; set; }

        [JsonProperty("create_uid")]
        [Column("create_uid")]
        public JToken? create_uid { get; set; }

        [JsonProperty("write_uid")]
        [Column("write_uid")]
        public JToken? write_uid { get; set; }
    }
}
