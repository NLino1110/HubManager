using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SQLite;

namespace DMSA.Models.Odoo.Native
{
    public class product_template_dep
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
        

        [JsonIgnore]
        public int _categ_id
        {
            get => categ_id.Length > 0 ? categ_id[0].id : 0;
            set => value = categ_id.Length > 0 ? categ_id[0].id : 0;
        }

        [JsonIgnore]
        public int _product_brand_id
        {
            get => product_brand_id.Length > 0 ? product_brand_id[0].id : 0;
            set => value = product_brand_id.Length > 0 ? product_brand_id[0].id : 0;

            //get
            //{
            //    if (product_brand_id.Length > 0)
            //    {
            //        return product_brand_id[0].id;
            //    }
            //    return 0;
            //}

            //set
            //{
            //    value = 0;

            //    if (product_brand_id.Length > 0)
            //    {
            //        value = product_brand_id[0].id;
            //    }
            //}
        }

        [Ignore]
        public Uom_Id[] uom_id { get; set; }
        [Ignore]
        public string default_code { get; set; }
        [Ignore]
        public Categ_Id[] categ_id { get; set; }
        [Ignore]
        public Product_Brand_Id[] product_brand_id { get; set; }
    }

    public class Uom_Id
    {
        public int id { get; set; }
        public string name { get; set; }
    }

    public class Categ_Id
    {
        public int id { get; set; }
        public string name { get; set; }
    }

    public class Product_Brand_Id
    {
        public int id { get; set; }
        public string name { get; set; }
    }

}
