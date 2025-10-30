using DMSA.Models.Odoo.Base;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SQLite;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace DMSA.Models.Odoo.Native
{
    public class OrderLineWrapper : List<object>
    {
        public OrderLineWrapper(sale_order_line line)
        {
            Add(0);
            Add(0);
            Add(line);
        }
    }

    [Table("sale_order_line")]
    public class sale_order_line: OdooEntity, INotifyPropertyChanged
    {
        [Key]
        [PrimaryKey]
        [AutoIncrement]
        [NotNull]
        [JsonProperty("id")]
        public int id { get; set; }

        [JsonIgnore]
        public int ordinal { get; set; }

        [Ignore]
        [JsonProperty("order_id")]
        public JToken order_id { get; set; }

        [JsonIgnore]
        public int _order_id
        {
            get => GetId(order_id);
            set => order_id = SetId(order_id, value);
        }

        [JsonProperty("product_id")]
        public int product_id { get; set; }

        [Ignore]
        [JsonIgnore]
        public decimal _product_uom_qty { get; set; }

        [JsonProperty("product_uom_qty")]
        public decimal product_uom_qty
        {
            get => _product_uom_qty;
            set
            {
                if (_product_uom_qty != value)
                {
                    _product_uom_qty = value;
                    OnPropertyChanged(nameof(product_uom_qty));
                }
            }
        }

        [Ignore]
        [JsonIgnore]
        private decimal _product_uom_qty_real { get; set; }

        [JsonProperty("product_uom_qty_real")]
        public decimal product_uom_qty_real
        {
            get => _product_uom_qty_real;
            set { 
                if (_product_uom_qty_real != value) 
                { 
                    _product_uom_qty_real = value; 
                    OnPropertyChanged(nameof(product_uom_qty_real)); 
                } 
            }
        }

        [JsonProperty("price_unit")]
        public decimal price_unit { get; set; }

        [Ignore]
        [JsonIgnore]
        public decimal _qty_to_deliver { get; set; }

        [JsonProperty("qty_to_deliver")]
        public decimal qty_to_deliver
        {
            get => _qty_to_deliver;
            set
            {
                if (_qty_to_deliver == value) return;
                _qty_to_deliver = value;
                OnPropertyChanged(nameof(qty_to_deliver));
                
            }
        }


        [Ignore]
        [JsonProperty("product_uom_category_id")]
        public JToken product_uom_category_id { get; set; }

        [JsonIgnore]
        public int _product_uom_category_id
        {
            get => GetId(product_uom_category_id);
            set => product_uom_category_id = SetId(product_uom_category_id, value);
        }

        [JsonProperty("discount")]
        public decimal discount { get; set; }

        [JsonProperty("amount_discount")]
        public decimal amount_discount { get; set; }

        [JsonProperty("price_tax")]
        public decimal price_tax { get; set; }

        [JsonProperty("price_subtotal")]
        public decimal price_subtotal { get; set; }        

        [JsonProperty("price_total")]
        public decimal price_total { get; set; }

        [Ignore]
        [JsonIgnore]
        public string product_code { get; set; }
        [Ignore]
        [JsonIgnore]
        public string product_display { get; set; }

        [Ignore]
        [JsonIgnore]
        public string uom_category_display { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
        void OnPropertyChanged(string n) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(n));
    }
}
