using DMSA.Models.Odoo.Base;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SQLite;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace DMSA.Models.Odoo.Native
{
    public class OrderLineReadWrapper : List<object>
    {
        public OrderLineReadWrapper(sale_order_line_read line)
        {
            Add(0);
            Add(0);
            Add(line);
        }
    }

    [Table("sale_order_line_read")]
    public class sale_order_line_read: OdooEntity, INotifyPropertyChanged
    {        
        [JsonProperty("id")]
        public int id { get; set; }

        [JsonProperty("sequence")]
        public int sequence { get; set; }

        [JsonProperty("order_id")]
        public JToken order_id { get; set; }

        [JsonIgnore]
        public int _order_id
        {
            get => GetId(order_id);
            set => order_id = SetId(order_id, value);
        }

        [JsonProperty("product_id")]
        public JToken product_id { get; set; }
        [JsonIgnore]
        public int _product_id
        {
            get => GetId(product_id);
            set => product_id = SetId(product_id, value);
        }

        [JsonProperty("product_template_id")]
        public JToken product_template_id { get; set; }
        [JsonIgnore]
        public int _product_template_id
        {
            get => GetId(product_template_id);
            set => product_template_id = SetId(product_template_id, value);
        }

        [JsonProperty("product_uom_qty")]
        public decimal product_uom_qty { get; set; }

        [JsonProperty("product_uom_qty_real")]
        public decimal product_uom_qty_real { get; set; }

        [JsonProperty("price_unit")]
        public decimal price_unit { get; set; }

        [JsonProperty("qty_to_deliver")]
        public decimal qty_to_deliver { get; set; }

        [JsonProperty("discount")]
        public decimal discount { get; set; }

        [JsonProperty("amount_discount")]
        public decimal amount_discount { get; set; }

        [JsonProperty("price_subtotal")]
        public decimal price_subtotal { get; set; }

        [JsonProperty("promotion_ids")]
        public int[] promotion_ids { get; set; }
       
        [JsonProperty("rule_ids")]
        public int[] rule_ids { get; set; }        

        public event PropertyChangedEventHandler PropertyChanged;
        void OnPropertyChanged(string n) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(n));
    }
}
