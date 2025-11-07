using DMSA.Models.Odoo.Base;
using DMSA.Models.Odoo.Json.Converters;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SQLite;
using System.ComponentModel.DataAnnotations;

namespace DMSA.Models.Odoo.DMOrders.promotions
{

    [Table("promotion_benefit")]
    public class PromotionBenefit: OdooEntity
    {
        [Key]
        [PrimaryKey]
        [AutoIncrement]
        [NotNull]
        [JsonProperty("id")]
        [Column("id")]
        public int id { get; set; }

        // ----------------------------------------------------------------------
        // Campos básicos
        // ----------------------------------------------------------------------
        [JsonProperty("name")]
        [Column("name")]
        public string name { get; set; }

        [JsonProperty("description")]
        [Column("description")]
        public string description { get; set; }

        [JsonProperty("active")]
        [Column("active")]
        public bool active { get; set; } = true;

        [JsonProperty("start_datetime")]
        [Column("start_datetime")]
        public DateTime? start_datetime { get; set; }

        [JsonProperty("end_datetime")]
        [Column("end_datetime")]
        public DateTime? end_datetime { get; set; }

        [JsonProperty("invoice_total")]
        [Column("invoice_total")]
        public int invoice_total { get; set; }

        [JsonProperty("state")]
        [Column("state")]
        public string state { get; set; } = "draft";

        [JsonProperty("logs")]
        [Column("logs")]
        public string logs { get; set; }

        [JsonProperty("approval_date")]
        [JsonConverter(typeof(OdooNullableDateTimeConverter))]
        public DateTime? approval_date { get; set; }

        [JsonProperty("end_date")]
        [JsonConverter(typeof(OdooNullableDateTimeConverter))]
        public DateTime? end_date { get; set; }

        [JsonProperty("summary")]
        public string summary { get; set; }

        [JsonProperty("code")]
        public string code { get; set; } = "Nuevo";

        // ----------------------------------------------------------------------
        // Many2one
        // ----------------------------------------------------------------------
        [Ignore]
        [JsonProperty("promotion_type_id")]
        public JToken promotion_type_id { get; set; }

        [JsonIgnore]
        public int _promotion_type_id
        {
            get => GetId(promotion_type_id);
            set => promotion_type_id = SetId(promotion_type_id, value);
        }

        [Ignore]
        [JsonProperty("target_segment_id")]
        public JToken target_segment_id { get; set; }

        [JsonIgnore]
        public int _target_segment_id
        {
            get => GetId(target_segment_id);
            set => target_segment_id = SetId(target_segment_id, value);
        }

        [Ignore]
        [JsonProperty("selection_type_id")]
        public JToken selection_type_id { get; set; }

        [JsonIgnore]
        public int _selection_type_id
        {
            get => GetId(selection_type_id);
            set => selection_type_id = SetId(selection_type_id, value);
        }

        [Ignore]
        [JsonProperty("approval_uid")]
        public JToken approval_uid { get; set; }

        [JsonIgnore]
        public int _approval_uid
        {
            get => GetId(approval_uid);
            set => approval_uid = SetId(approval_uid, value);
        }

        [Ignore]
        [JsonProperty("change_requested_uid")]
        public JToken change_requested_uid { get; set; }

        [JsonIgnore]
        public int _change_requested_uid
        {
            get => GetId(change_requested_uid);
            set => change_requested_uid = SetId(change_requested_uid, value);
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

        [Ignore]
        [JsonProperty("loyalty_company_id")]
        public JToken loyalty_company_id { get; set; }

        [Ignore]
        [JsonIgnore]
        public int _loyalty_company_id
        {
            get => GetId(loyalty_company_id);
            set => loyalty_company_id = SetId(loyalty_company_id, value);
        }

        // ----------------------------------------------------------------------
        // Many2many (arrays de IDs)
        // ----------------------------------------------------------------------
        [Ignore]
        [JsonProperty("customers_included_ids")]
        public JToken customers_included_ids { get; set; }

        [Ignore]
        [JsonIgnore]
        public int[] _customers_included_ids
        {
            get => GetIds(customers_included_ids);
            set => customers_included_ids = SetIds(customers_included_ids, value);
        }

        [JsonIgnore]
        public string customers_included_ids_json
        {
            get => SetIdsJson(customers_included_ids);
            //set => customers_included_ids = GetIdsFromJson(value);
            set { }
        }

        [Ignore]
        [JsonProperty("customers_excluded_ids")]
        public JToken customers_excluded_ids { get; set; }

        [Ignore]
        [JsonIgnore]
        public int[] _customers_excluded_ids
        {
            get => GetIds(customers_excluded_ids);
            set => customers_excluded_ids = SetIds(customers_excluded_ids, value);
        }
       
        [JsonIgnore]
        public string customers_excluded_ids_json
        {
            get => SetIdsJson(customers_excluded_ids);            
            set { }
        }

        [Ignore]
        //[JsonProperty("promotion_product_ids")]
        public JToken promotion_product_ids { get; set; }

        [Ignore]
        [JsonIgnore]
        public int[] _promotion_product_ids { get; set; }
        //{
        //    get => GetIds(promotion_product_ids);
        //    set => promotion_product_ids = SetIds(promotion_product_ids, value);
        //}
                
        [Ignore]
        [JsonProperty("product_promotion_ids")]
        public JArray product_promotion_ids { get; set; } = new JArray();
        //{
        //    get => string.IsNullOrEmpty(product_promotion_ids_json)
        //           ? null
        //           : JToken.Parse(product_promotion_ids_json);
        //    set => product_promotion_ids_json = value?.ToString(Formatting.None);
        //}

        [Ignore]
        [JsonIgnore]
        public List<PromotionProductDetail> _product_promotion_ids { get; set; }

        [Ignore]
        [JsonProperty("promotion_rules_ids")]
        public JToken promotion_rules_ids { get; set; }

        [Ignore]
        [JsonIgnore]
        public List<PromoRules> _promotion_rules_ids { get; set; }        

        [Ignore]
        [JsonProperty("changes_ids")]
        public JToken changes_ids { get; set; }

        [Ignore]
        [JsonIgnore]
        public int[] _changes_ids
        {
            get => GetIds(changes_ids);
            set => changes_ids = SetIds(changes_ids, value);
        }

        [Ignore]
        [JsonProperty("loyalty_filters_ids")]
        public JToken loyalty_filters_ids { get; set; }

        [JsonIgnore]
        public LoyaltyFilters[] _loyalty_filters_ids
        {
            //get => GetIds(loyalty_filters_ids);
            //set => loyalty_filters_ids = SetIds(loyalty_filters_ids, value);

            get => Array.Empty<LoyaltyFilters>();
        }

        [Ignore]
        [JsonProperty("centers_ids")]
        public JToken centers_ids { get; set; }

        [Ignore]
        [JsonIgnore]
        public List<PromoCenters> _centers_ids { get; set; }        

        [Ignore]
        [JsonProperty("log_ids")]
        public JToken log_ids { get; set; }

        [Ignore]
        [JsonIgnore]
        public int[] _log_ids
        {
            get => GetIds(log_ids);
            set => log_ids = SetIds(log_ids, value);
        }

        public DateTime create_date { get; set; }
        public DateTime write_date { get; set; }
    }

}
