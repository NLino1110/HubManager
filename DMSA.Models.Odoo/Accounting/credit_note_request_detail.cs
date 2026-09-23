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

        /// <summary>U.M. ventas en local. En Odoo: este valor si no hay cierre; si <see cref="client_permanently_closing"/> → se envía <see cref="_uom_id"/>.</summary>
        public int product_uom_id { get; set; }

        [Column("client_permanently_closing")]
        [JsonIgnore]
        public bool client_permanently_closing { get; set; }

        /// <summary>U.M. inventario del producto (solo persistencia local).</summary>
        [Column("_uom_id")]
        [JsonIgnore]
        public int _uom_id { get; set; }

        /// <summary>U.M. ventas de la línea de factura (<c>account_move_line.product_uom_id</c>).</summary>
        [Column("invoice_line_uom_id")]
        [JsonIgnore]
        public int invoice_line_uom_id { get; set; }

        [Column("quantity_available_invoice")]
        [JsonIgnore]
        public decimal quantity_available_invoice { get; set; }

        /// <summary>Disponible en U.M. base desde Odoo (<c>account_move_line.quantity_available_base</c>).</summary>
        [Column("quantity_available_base")]
        [JsonIgnore]
        public decimal quantity_available_base { get; set; }

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

        [JsonProperty("original_quantity")]
        public decimal original_quantity { get; set; }

        [JsonProperty("quantity_invoiced")]
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

        [JsonProperty("account_id")]
        public int account_id { get; set; }

        [JsonProperty("partner_id")]
        public int partner_id { get; set; }

        [JsonProperty("currency_id")]
        public int currency_id { get; set; }

        [JsonProperty("display_type")]
        public string? display_type { get; set; }

        [JsonProperty("use_type")]
        public string use_type { get; set; } = "preview";

        [JsonProperty("amount_currency")]
        public decimal amount_currency { get; set; }

        [JsonProperty("disc_amount")]
        public decimal disc_amount { get; set; }

        [JsonProperty("already_added")]
        public bool already_added { get; set; } = true;

        [JsonProperty("is_refund")]
        public bool is_refund { get; set; }

        /// <summary>Solo payload Odoo: mismo significado que cierre permanente del cliente al enviar la línea.</summary>
        [Ignore]
        [JsonProperty("calculo_unidad_base")]
        public bool calculo_unidad_base { get; set; }

        [JsonProperty("analitica_id")]
        public int analitica_id { get; set; }

        [JsonProperty("discount")]
        public decimal discount { get; set; }

        [JsonProperty("discount_percentage")]
        public decimal discount_percentage { get; set; }

        [JsonIgnore]
        public decimal price_total { get; set; }
        [JsonIgnore]
        public int sequence { get; set; }

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

        [Column("quantity_available_by_uom")]
        [JsonIgnore]
        public decimal quantity_available_by_uom { get; set; }

        /// <summary>Cierre de operaciones: devolución por U.M. de inventario del producto.</summary>
        [Ignore]
        [JsonIgnore]
        public bool use_inventory_return_uom { get; set; }

        [Ignore]
        [JsonIgnore]
        public int inventory_uom_id { get; set; }

        /// <summary>
        /// U.M. factura distinta a U.M. inventario: Cant.Disp = unidades base, Cant.xU.M = bultos inventario.
        /// </summary>
        [Ignore]
        [JsonIgnore]
        public bool inventory_return_display_swapped { get; set; }

        [Ignore]
        [JsonIgnore]
        public bool show_uom_conversion => uom_factor > 1m;

        [Ignore]
        [JsonIgnore]
        public decimal quantity_by_uom =>
            inventory_return_display_swapped
                ? (uom_factor > 0m ? quantity / uom_factor : quantity)
                : quantity * uom_factor;

        /// <summary>Precio unitario sin IVA para la fila: con cierre + conversión → <see cref="siv_price_return"/>; si no → <see cref="siv_price_unit"/>.</summary>
        [Ignore]
        [JsonIgnore]
        public decimal display_price_ex_vat
        {
            get
            {
                if (client_permanently_closing && siv_price_return > 0m)
                    return siv_price_return;

                if (siv_price_unit > 0m)
                    return siv_price_unit;

                return price_return;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        public void NotifyUomDisplayChanged()
        {
            OnPropertyChanged(nameof(uom_display_name));
            OnPropertyChanged(nameof(uom_factor));
            OnPropertyChanged(nameof(quantity_available));
            OnPropertyChanged(nameof(quantity_available_by_uom));
            OnPropertyChanged(nameof(show_uom_conversion));
            OnPropertyChanged(nameof(quantity_by_uom));
        }

        public void NotifyPricingDisplayChanged()
        {
            OnPropertyChanged(nameof(price_unit));
            OnPropertyChanged(nameof(price_return));
            OnPropertyChanged(nameof(price_subtotal));
            OnPropertyChanged(nameof(price_total));
            OnPropertyChanged(nameof(siv_price_unit));
            OnPropertyChanged(nameof(siv_price_return));
            OnPropertyChanged(nameof(display_price_ex_vat));
        }
    }
}
