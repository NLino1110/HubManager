using DMSA.Models.Odoo.Base;
using DMSA.Models.Odoo.Json.Converters;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SQLite;

namespace DMSA.Models.Odoo.Sales
{
    [Table("product_pricelist_item")]
    public class product_pricelist_item : OdooEntity
    {
        [PrimaryKey]
        [JsonProperty("id")]
        public int id { get; set; }

        [Ignore]        
        [JsonProperty("pricelist_id")]
        public JToken pricelist_id { get; set; }

        [JsonIgnore]
        public int _pricelist_id
        {
            get => GetId(pricelist_id);
            set => pricelist_id = SetId(pricelist_id, value);
        }

        [Column("date_start")]
        [JsonProperty("date_start")]
        [JsonConverter(typeof(OdooNullableDateTimeConverter))]
        public DateTime? date_start { get; set; }

        [Column("date_end")]
        [JsonProperty("date_end")]
        [JsonConverter(typeof(OdooNullableDateTimeConverter))]
        public DateTime? date_end { get; set; }


        [Column("min_quantity")]
        [JsonProperty("min_quantity")]
        public decimal min_quantity { get; set; }


        [Column("applied_on")]
        [JsonProperty("applied_on")]
        public string applied_on { get; set; }

        [Column("display_applied_on")]
        [JsonProperty("display_applied_on")]
        public string display_applied_on { get; set; }

        [Ignore]        
        public JToken product_tmpl_id { get; set; }

        [JsonIgnore]
        [Column("_product_tmpl_id")]
        public int _product_tmpl_id
        {
            get => GetId(product_tmpl_id);
            set => product_tmpl_id = SetId(product_tmpl_id, value);
        }

        [Ignore]
        public JToken uom_id { get; set; }

        [JsonIgnore]
        [Column("_uom_id")]
        public int _uom_id
        {
            get => GetId(uom_id);
            set => uom_id = SetId(uom_id, value);
        }

        [Column("product_uom")]
        [JsonProperty("product_uom")]
        public string product_uom { get; set; }

        [Column("base")]
        [JsonProperty("base")]
        public string base_type { get; set; }

        [Column("compute_price")]
        [JsonProperty("compute_price")]
        public string compute_price { get; set; }


        [Column("fixed_price")]
        [JsonProperty("fixed_price")]
        public decimal fixed_price { get; set; }

        [Column("percent_price")]
        [JsonProperty("percent_price")]
        public decimal percent_price { get; set; }

        [Column("price_discount")]
        [JsonProperty("price_discount")]
        public decimal price_discount { get; set; }

        [Column("price_round")]
        [JsonProperty("price_round")]
        public decimal price_round { get; set; }

        [Column("price_surcharge")]
        [JsonProperty("price_surcharge")]
        public decimal price_surcharge { get; set; }

        [Column("price_markup")]
        [JsonProperty("price_markup")]
        public decimal price_markup { get; set; }

        [Column("price_min_margin")]
        [JsonProperty("price_min_margin")]
        public decimal price_min_margin { get; set; }

        [Column("price_max_margin")]
        [JsonProperty("price_max_margin")]
        public decimal price_max_margin { get; set; }

        [Column("price")]
        [JsonProperty("price")]
        public string price { get; set; }


        [Column("cost_unit")]
        [JsonProperty("cost_unit")]
        public decimal cost_unit { get; set; }


        [Column("cost_price")]
        [JsonProperty("cost_price")]
        public decimal cost_price { get; set; }

        [Column("cost_price_tax")]
        [JsonProperty("cost_price_tax")]
        public decimal cost_price_tax { get; set; }

        [Column("margin")]
        [JsonProperty("margin")]
        public decimal margin { get; set; }

        [Column("display_name")]
        [JsonProperty("display_name")]
        public string display_name { get; set; }


        [Ignore]        
        public JToken currency_id { get; set; }
        [Ignore]        
        public JToken company_id { get; set; }
        [Ignore]        
        public JToken create_uid { get; set; }
        [Ignore]        
        public JToken write_uid { get; set; }
        
        [JsonIgnore]
        public int _currency_id
        {
            get => GetId(currency_id);
            set => currency_id = SetId(currency_id, value);
        }

        [JsonIgnore]
        public int _company_id
        {
            get => GetId(company_id);
            set => company_id = SetId(company_id, value);
        }

        [JsonIgnore]
        public int _create_uid
        {
            get => GetId(create_uid);
            set => create_uid = SetId(create_uid, value);
        }

        [JsonIgnore]
        public int _write_uid
        {
            get => GetId(write_uid);
            set => write_uid = SetId(write_uid, value);
        }


        [Column("create_date")]
        [JsonProperty("create_date")]
        [JsonConverter(typeof(OdooNullableDateTimeConverter))]
        public DateTime create_date { get; set; }

        [Column("write_date")]
        [JsonProperty("write_date")]
        [JsonConverter(typeof(OdooNullableDateTimeConverter))]
        public DateTime write_date { get; set; }

        
    }

}
