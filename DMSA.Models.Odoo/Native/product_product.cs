using DMSA.Models.Odoo.Base;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SQLite;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace DMSA.Models.Odoo.Native
{
    [Table("product_product")]
    public class product_product: OdooEntity, INotifyPropertyChanged
    {
        [PrimaryKey]
        [JsonProperty("id")]
        public int id { get; set; }

        [JsonProperty("default_code")]
        public string default_code { get; set; }

        [JsonProperty("code")]
        public string code { get; set; }

        [JsonProperty("partner_ref")]
        public string partner_ref { get; set; }

        [JsonProperty("active")]
        public bool active { get; set; }

        [Ignore]
        [JsonProperty("product_tmpl_id")]
        public JToken product_tmpl_id { get; set; }

        [JsonIgnore]
        public int _product_tmpl_id
        {
            get => GetId(product_tmpl_id);
            set => product_tmpl_id = SetId(product_tmpl_id, value);
        }

        [JsonProperty("barcode")]
        public string barcode { get; set; }

        [JsonProperty("volume")]
        public float volume { get; set; }

        [JsonProperty("weight")]
        public float weight { get; set; }

        [JsonProperty("image_256")]
        public string image_256 { get; set; }

        [JsonProperty("can_image_1024_be_zoomed")]
        public bool can_image_1024_be_zoomed { get; set; }

        [JsonProperty("display_name")]
        public string display_name { get; set; }

        [JsonProperty("create_date")]
        public DateTime create_date { get; set; }

        [JsonProperty("write_date")]
        public DateTime write_date { get; set; }

        [JsonProperty("qty_available")]
        public float qty_available { get; set; }

        [JsonProperty("virtual_available")]
        public float virtual_available { get; set; }

        [JsonProperty("free_qty")]
        public float free_qty { get; set; }

        [JsonProperty("list_price")]
        public float list_price { get; set; }

        [JsonProperty("base_unit_count")]
        public float base_unit_count { get; set; }

        //[JsonProperty("base_unit_id")]
        //public object base_unit_id { get; set; }

        [JsonProperty("name")]
        public string name { get; set; }

        //[JsonProperty("sequence")]
        //public int sequence { get; set; }
        
        [JsonProperty("type")]
        public string type { get; set; }

        [Ignore]
        [JsonProperty("categ_id")]
        public JToken categ_id { get; set; }

        [JsonIgnore]
        public int _categ_id
        {
            get => GetId(categ_id);
            set => categ_id = SetId(categ_id, value);
        }

        [Ignore]
        [JsonProperty("uom_id")]
        public JToken uom_id { get; set; }

        [JsonIgnore]
        public int _uom_id
        {
            get => GetId(uom_id);
            set => uom_id = SetId(uom_id, value);
        }

        [Ignore]
        [JsonProperty("general_marca_id")]
        public JToken general_marca_id { get; set; }

        [JsonIgnore]
        public int _general_marca_id
        {
            get => GetId(general_marca_id);
            set => general_marca_id = SetId(general_marca_id, value);
        }

        [JsonProperty("website_url")]
        public string website_url { get; set; }


        [Ignore]
        [JsonProperty("general_linea_id")]
        public JToken general_linea_id { get; set; }

        [JsonIgnore]
        public int _general_linea_id
        {
            get => GetId(general_linea_id);
            set => general_linea_id = SetId(general_linea_id, value);
        }

        [Ignore]
        [JsonProperty("general_categoria_id")]
        public JToken general_categoria_id { get; set; }

        [JsonIgnore]
        public int _general_categoria_id
        {
            get => GetId(general_categoria_id);
            set => general_categoria_id = SetId(general_categoria_id, value);
        }

        [Ignore]
        [JsonProperty("general_subcategoria_id")]
        public JToken general_subcategoria_id { get; set; }

        [JsonIgnore]
        public int _general_subcategoria_id
        {
            get => GetId(general_subcategoria_id);
            set => general_subcategoria_id = SetId(general_subcategoria_id, value);
        }

        [Ignore]
        [JsonProperty("general_grupor_tipo_id")]
        public JToken general_grupor_tipo_id { get; set; }

        [JsonIgnore]
        public int _general_grupor_tipo_id
        {
            get => GetId(general_grupor_tipo_id);
            set => general_grupor_tipo_id = SetId(general_grupor_tipo_id, value);
        }

        [JsonProperty("general_registro_sanitario")]
        public string general_registro_sanitario { get; set; }

        [JsonProperty("sale_ok")]
        public bool sale_ok { get; set; }



        [JsonIgnore]
        private bool _isSelected;
        [Ignore]
        [JsonIgnore]
        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                if (_isSelected != value)
                {
                    _isSelected = value;
                    OnPropertyChanged();
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
