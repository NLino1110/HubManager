using DMSA.Models.Odoo.Base;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SQLite;

namespace DMSA.Models.Odoo.Accounting
{
    public class account_move_line : OdooEntity
    {
        [PrimaryKey]
        public int id { get; set; }        
        public int sequence { get; set; }
        public string name { get; set; }        
        public decimal quantity { get; set; }
        public decimal quantity_available { get; set; }

        [Column("quantity_available_base")]
        public decimal quantity_available_base { get; set; }

        public decimal price_unit { get; set; }
        public decimal price_subtotal { get; set; }
        public decimal discount { get; set; }
        public decimal discount_balance { get; set; }        
        public decimal price_total { get; set; }
        public decimal discount_percentage { get; set; }
        public string display_type { get; set; }        

        [Ignore]
        public JToken account_id { get; set; }
        public int _account_id
        {
            get => GetId(account_id);
            set => account_id = SetId(account_id, value);
        }

        [Ignore]
        public JToken move_id { get; set; }
        public int _move_id
        {
            get => GetId(move_id);
            set => move_id = SetId(move_id, value);
        }

        [Ignore]
        public JToken product_id { get; set; }
        public int _product_id
        {
            get => GetId(product_id);
            set => product_id = SetId(product_id, value);
        }

        [Ignore]
        public JToken product_uom_id { get; set; }
        public int _product_uom_id
        {
            get => GetId(product_uom_id);
            set => product_uom_id = SetId(product_uom_id, value);
        }

        [Ignore]
        public JToken analitica_id { get; set; }

        public int _analitica_id
        {
            get => GetId(analitica_id);
            set => analitica_id = SetId(analitica_id, value);
        }

        [Ignore]
        public JToken analytic_line_ids { get; set; }

        [Ignore]
        [JsonIgnore]
        public int[] _analytic_line_ids
        {
            get => GetIds(analytic_line_ids);
            set => analytic_line_ids = SetIds(analytic_line_ids, value);
        }

        [Column("analytic_line_ids_json")]
        public string analytic_line_ids_json
        {
            get => analytic_line_ids?.ToString(Formatting.None);
            set => analytic_line_ids = string.IsNullOrEmpty(value)
                ? null
                : JToken.Parse(value);
        }

        [Ignore]
        [JsonProperty("tax_ids")]
        public JToken tax_ids { get; set; } // promotion.product (bonus_id)

        [Ignore]
        [JsonIgnore]
        public int[] _tax_ids
        {
            get => GetIds(tax_ids);
            set => tax_ids = SetIds(tax_ids, value);
        }

        [Column("tax_ids_json")]
        public string tax_ids_json
        {
            get => tax_ids?.ToString(Formatting.None);
            set => tax_ids = string.IsNullOrEmpty(value)
                ? null
                : JToken.Parse(value);
        }

        [JsonProperty("create_date")]
        [Column("create_date")]
        public DateTime? create_date { get; set; }
                
        [JsonProperty("write_date")]
        [Column("write_date")]
        public DateTime? write_date { get; set; }
    }

    public class account_move_line_view : account_move_line
    {
        public string product_name { get; set; }
        public string product_code { get; set; }
        public string docnum_mask { get; set; }
        public string? display_name
        {
            get
            {
                string ShortName(string? name, int maxLength = 15)
                {
                    if (string.IsNullOrEmpty(name) || id < 1)
                        return name;

                    return name.Length > maxLength
                        ? name.Substring(0, maxLength - 3) + "..."
                        : name;
                }

                var productNameShort = ShortName(product_name);

                if (string.IsNullOrEmpty(product_code) && string.IsNullOrEmpty(docnum_mask))
                {
                    return $"{productNameShort}";
                }

                return $"{productNameShort} ({product_code}) · Disp: {quantity_available} · {docnum_mask}";
            }
        }
    }
}
