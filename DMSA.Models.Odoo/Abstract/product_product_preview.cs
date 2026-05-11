using DMSA.Models.Odoo.Base;
using DMSA.Models.Odoo.DMOrders.promotions.abstractCustom;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SQLite;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace DMSA.Models.Odoo.Native
{
    [Table("product_product_preview")]
    public class product_product_preview: OdooEntity, INotifyPropertyChanged
    {
        [PrimaryKey]
        [JsonProperty("id")]
        public int id { get; set; }

        [JsonProperty("code")]
        public string code { get; set; }

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

        [JsonProperty("image_256")]
        public string image_256 { get; set; }

        [JsonProperty("image_1920")]
        public string image_1920 { get; set; }

        [JsonProperty("image_url")]
        public string image_url { get; set; }

        [JsonProperty("create_date")]
        [Column("create_date")]
        public DateTime? create_date { get; set; }

        [JsonProperty("write_date")]
        [Column("write_date")]
        public DateTime? write_date { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
