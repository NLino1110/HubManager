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

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
