using DMSA.Models.Odoo.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

namespace DMSA.Models.Odoo.DebitCollection
{
    [Table("multiple_cobros_invoice_line_ai")]
    public class MultipleCobrosInvoiceLineAi : OdooEntity
    {
        [JsonProperty("company_id")]
        [Column("company_id")]
        public int company_id { get; set; }
        [JsonProperty("invoice_id")]
        [Column("invoice_id")]
        public int invoice_id { get; set; }
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
        [JsonProperty("amount_asigned")]
        [Column("amount_asigned")]
        public decimal amount_asigned { get; set; }
        [JsonProperty("multiple_cobros_invoice_line_id")]
        [Column("multiple_cobros_invoice_line_id")]
        public int multiple_cobros_invoice_line_id { get; set; }
        [JsonProperty("payment_id")]
        [Column("payment_id")]
        public int payment_id { get; set; }
        [JsonProperty("type_invoice")]
        [Column("type_invoice")]
        public string type_invoice { get; set; } //out_invoice

        public DateTime invoice_date { get; set; }
        public DateTime invoice_date_due { get; set; }


        [JsonProperty("seller")]
        [Column("seller")]
        public string seller { get; set; }
    }
}
