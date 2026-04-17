using DMSA.Models.Odoo.Base;
using DMSA.Models.Odoo.Native;
using Newtonsoft.Json;
using SQLite;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;

namespace DMSA.Models.Odoo.DebitCollection
{
    public class MultipleCobrosInvoiceLineAiWrapper : List<object>
    {
        public MultipleCobrosInvoiceLineAiWrapper(MultipleCobrosInvoiceLineAi line)
        {
            Add(0);
            Add(0);
            Add(line);
        }
    }

    [Table("multiple_cobros_invoice_line_ai")]
    public class MultipleCobrosInvoiceLineAi : OdooEntity, INotifyPropertyChanged
    {
        [PrimaryKey]
        [AutoIncrement]
        [JsonProperty("id")]
        [Column("id")]
        public int Id { get; set; }

        [JsonProperty("company_id")]
        [Column("company_id")]
        public int company_id { get; set; }
        [JsonProperty("invoice_id")]
        [Column("invoice_id")]
        public int invoice_id { get; set; }

        [JsonIgnore]
        [JsonProperty("invoice_name")]
        [Column("invoice_name")]
        public string invoice_name { get; set; }

        [JsonProperty("invoice_line_id")]
        [Column("invoice_line_id")]
        public int invoice_line_id { get; set; }
        [JsonProperty("partner_id")]
        [Column("partner_id")]
        public int partner_id { get; set; }
        [JsonProperty("amount_total")]
        [Column("amount_total")]
        public decimal amount_total { get; set; }
        [JsonProperty("amount_residual")]
        [Column("amount_residual")]
        public decimal amount_residual { get; set; }


        private decimal _amount_asigned;

        [JsonProperty("amount_asigned")]
        [Column("amount_asigned")]
        public decimal amount_asigned
        {
            get => _amount_asigned;
            set
            {
                if (_amount_asigned != value)
                {
                    _amount_asigned = value;
                    OnPropertyChanged();
                }
            }
        }

        [JsonProperty("multiple_cobros_invoice_line_id")]
        [Column("multiple_cobros_invoice_line_id")]
        public int multiple_cobros_invoice_line_id { get; set; }
        [JsonProperty("payment_id")]
        [Column("payment_id")]
        public int payment_id { get; set; }
        [JsonProperty("type_invoice")]
        [Column("type_invoice")]
        public string type_invoice { get; set; } //out_invoice

        [JsonIgnore]
        public DateTime invoice_date { get; set; }
        [JsonIgnore]
        public DateTime invoice_date_due { get; set; }

        [JsonIgnore]
        [JsonProperty("seller")]
        [Column("seller")]
        public string seller { get; set; }
                
        [JsonIgnore]
        [Column("docnum_mask")]
        public string docnum_mask { get; set; }

        [JsonIgnore]
        [Ignore]
        public bool EventsOn { get; set; } = true;

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
