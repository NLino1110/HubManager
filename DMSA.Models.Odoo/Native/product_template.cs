using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SQLite;

namespace DMSA.Models.Odoo.Native
{
    public class product_template
    {
        [PrimaryKey]
        public int id { get; set; }
        public string name { get; set; }
        public string type { get; set; }
        public bool active { get; set; }

        public bool macro_product_available { get; set; }
        public bool sale_ok { get; set; }
        public bool purchase_ok { get; set; }
        public bool trade_ok { get; set; }

        public DateTime write_date { get; set; }
        public DateTime create_date  { get; set; }

        //[JsonIgnore]
        //public int _categ_id { get; set; }
        //[JsonIgnore]
        //public int _product_brand_id { get; set; }

        [JsonIgnore]
        public int _categ_id
        {
            get
            {                
                if (categ_id is JArray array && array.Count > 0)
                {
                    return array[0].Type == JTokenType.Integer ? (int)array[0] : 0;
                }
                
                else if (categ_id is JValue value && value.Type == JTokenType.Boolean)
                {
                    return 0;
                }
                
                return 0;
            }
            set
            {
                if (categ_id is JArray array && array.Count > 0)
                {
                    array[0] = value;
                }
                else if (categ_id is JValue)
                {
                    categ_id = new JArray { value };
                }
                else
                {
                    categ_id = new JArray { value };
                }
            }
        }

        [JsonIgnore]
        public int _product_brand_id
        {
            get
            {                
                if (product_brand_id is JArray array && array.Count > 0)
                {
                    return array[0].Type == JTokenType.Integer ? (int)array[0] : 0;
                }
                
                else if (product_brand_id is JValue value && value.Type == JTokenType.Boolean)
                {
                    return 0;
                }
                
                return 0;
            }
            set
            {
                if (product_brand_id is JArray array && array.Count > 0)
                {
                    array[0] = value; 
                }
                else if (product_brand_id is JValue)
                {
                    product_brand_id = new JArray { value };
                }
                else
                {
                    product_brand_id = new JArray { value };
                }
            }
        }


        //[Ignore]
        public string default_code { get; set; }

        [Ignore]
        public JToken uom_id { get; set; }
        [Ignore]
        public JToken categ_id { get; set; }
        [Ignore]
        public JToken product_brand_id { get; set; }

        public int company_id { get; set; }

        public int general_tipo_sri_id { get; set; }
        public int general_marca_id { get; set; }
        public int general_linea_id { get; set; }
        public int general_categoria_id { get; set; }
        public int general_subcategoria_id { get; set; }
        public int general_grupor_tipo_id { get; set; }
        public int general_tipo_marca_id { get; set; }
    }
}
