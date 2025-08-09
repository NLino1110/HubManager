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
    public class AccountPaymentInvoiceLine
    {
        //account.payment.invoice.line

        [JsonIgnore]
        [Key]
        [PrimaryKey]
        [AutoIncrement]
        [NotNull]
        public int id { get; set; }

        [JsonIgnore]
        public int parent_payment_id { get; set; }

        //numero de detalle de movimiento de la factura display_type = 'payment_term'
        public int invoice_line_id { get; set; }
        public string invoice_line_id_name { get; set; }
        [JsonIgnore]
        public DateTime invoice_date { get; set; }
        public string payment_state { get; set; }
        public decimal reconcile_amount { get; set; }
        [JsonIgnore]
        public decimal amount_residual { get; set; }
    }

    //Clase informativa
    public class AccountPaymentInvoiceLineAuxiliar : AccountPaymentInvoiceLine
    {
        public string invoice_name { get; set; }
        public DateTime invoice_date { get; set; }
        public DateTime invoice_date_due { get; set; }
        public string invoice_origin { get; set; }
        public decimal invoice_amount_total { get; set; }
        public decimal invoice_amount_residual { get; set; }

        //
        public string seller { get; set; }
    }
}
