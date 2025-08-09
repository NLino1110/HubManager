using Newtonsoft.Json;
using SQLite;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMSA.Models.Odoo.DMCobranzas
{
    public class AccountPaymentInvoiceLineSend: AccountPaymentInvoiceLine
    {
        //[JsonIgnore]
        public int parent_payment_id { get; set; }

        //[JsonIgnore]
        [JsonProperty("invoice_date")]
        public DateTime invoice_date { get; set; }

        //[JsonIgnore]
        [JsonProperty("amount_residual")]
        public decimal amount_residual { get; set; }
    }
}
