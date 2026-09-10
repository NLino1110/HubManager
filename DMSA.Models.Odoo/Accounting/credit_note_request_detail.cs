using DMSA.Models.Odoo.Base;
using Newtonsoft.Json;
using SQLite;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace DMSA.Models.Odoo.Accounting
{
    [Table("credit_note_request_detail")]
    public class credit_note_request_detail: OdooEntity, INotifyPropertyChanged
    {
        [AutoIncrement]
        [PrimaryKey]
        [JsonIgnore]
        public int id { get; set; }        
        public int line_id { get; set; }
        [JsonIgnore]
        public int parent_id { get; set; }
        public int move_id { get; set; }
        public int product_id { get; set; }
        public int product_uom_id { get; set; }
        
        [Ignore]
        [JsonProperty("tax_ids")]
        public int[] tax_ids { get; set; }

        [JsonIgnore]
        public string tax_ids_json
        {
            get => tax_ids == null ? "[]" : JsonConvert.SerializeObject(tax_ids);
            set
            {
                tax_ids = string.IsNullOrWhiteSpace(value)
                    ? Array.Empty<int>()
                    : JsonConvert.DeserializeObject<int[]>(value);
            }
        }

        public int analitica_id { get; set; }
        [JsonIgnore]        
        public decimal original_quantity { get; set; }
        public decimal quantity_invoiced { get; set; }
        public decimal quantity_available { get; set; }
        public decimal price_unit { get; set; }
        public decimal price_return { get; set; }

        public decimal siv_price_unit { get; set; }
        public decimal siv_price_return { get; set; }

        decimal _quantity;

        public decimal quantity
        {
            get => _quantity;
            set
            {
                if (_quantity == value)
                    return;

                _quantity = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(quantity_by_uom));
            }
        }

        public decimal amount_tax { get; set; }
        public decimal price_subtotal { get; set; }

        [JsonIgnore]
        public int create_id { get; set; }
        [JsonIgnore]
        public string name { get; set; }
        [JsonIgnore]
        public int account_id { get; set; }
        [JsonIgnore]
        public int currency_id { get; set; }
        [JsonIgnore]
        public decimal price_total { get; set; }
        [JsonIgnore]
        public int sequence { get; set; }        

        [JsonIgnore]
        public decimal discount_percentage { get; set; }
        [JsonIgnore]
        public decimal discount_balance { get; set; }
        [JsonIgnore]
        public bool was_odoo_synced { get; set; }

        [JsonIgnore]
        [Ignore]
        public string? docnum_mask { get; set; }

        [Ignore]
        [JsonIgnore]
        public string? display_name { get; set; }
        [Ignore]
        [JsonIgnore]
        public DateTime? invoice_date { get; set; }

        [Ignore]
        [JsonIgnore]
        public string? invoice_header { get; set; }

        [Ignore]
        [JsonIgnore]
        public string? uom_display_name { get; set; }

        [Ignore]
        [JsonIgnore]
        public decimal uom_factor { get; set; } = 1m;

        [Ignore]
        [JsonIgnore]
        public decimal quantity_available_by_uom { get; set; }

        [Ignore]
        [JsonIgnore]
        public bool show_uom_conversion => uom_factor > 1m;

        [Ignore]
        [JsonIgnore]
        public decimal quantity_by_uom => quantity * uom_factor;

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        public void NotifyUomDisplayChanged()
        {
            OnPropertyChanged(nameof(uom_display_name));
            OnPropertyChanged(nameof(uom_factor));
            OnPropertyChanged(nameof(quantity_available_by_uom));
            OnPropertyChanged(nameof(show_uom_conversion));
            OnPropertyChanged(nameof(quantity_by_uom));
        }
    }
}
