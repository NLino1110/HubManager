using DMSA.Models.Odoo.Base;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SQLite;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMSA.Models.Odoo.DMOrders.promotions
{    
    [Table("product_pricelist_eliminar")]
    public class ProductPricelist_eliminar : OdooEntity
    {
        [Key]
        [PrimaryKey]
        [AutoIncrement]
        [NotNull]
        [JsonProperty("id")]
        public int id { get; set; }

        // ------------------ Básicos ------------------
        [JsonProperty("name")]
        public string name { get; set; }

        [JsonProperty("active")]
        public bool active { get; set; } = true;

        [JsonProperty("sequence")]
        public int sequence { get; set; } = 16;

        // ------------------ Many2one ------------------
        [Ignore]
        [JsonProperty("currency_id")]
        public JToken currency_id { get; set; }   // res.currency

        [JsonIgnore]
        public int _currency_id
        {
            get => GetId(currency_id);
            set => currency_id = SetId(currency_id, value);
        }

        [Ignore]
        [JsonProperty("company_id")]
        public JToken company_id { get; set; }    // res.company

        [JsonIgnore]
        public int _company_id
        {
            get => GetId(company_id);
            set => company_id = SetId(company_id, value);
        }

        // ------------------ Many2many ------------------
        [Ignore]
        [JsonProperty("country_group_ids")]
        public JToken country_group_ids { get; set; }  // res.country.group

        [JsonIgnore]
        public int[] _country_group_ids
        {
            get => GetIds(country_group_ids);
            set => country_group_ids = SetIds(country_group_ids, value);
        }

        // ------------------ One2many ------------------
        [Ignore]
        [JsonProperty("item_ids")]
        public JToken item_ids { get; set; }   // product.pricelist.item

        [JsonIgnore]
        public int[] _item_ids
        {
            get => GetIds(item_ids);
            set => item_ids = SetIds(item_ids, value);
        }
    }

}
