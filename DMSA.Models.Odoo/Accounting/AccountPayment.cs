using Newtonsoft.Json;
using SQLite;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMSA.Models.Odoo.Accounting
{
    [Obsolete("Eliminar")]
    public class AccountPayment
    {
        [JsonIgnore]
        [Key]
        [PrimaryKey]
        [AutoIncrement]
        [NotNull]
        public int id { get; set; }

        [JsonIgnore]
        public int parent_id { get; set; }
        [JsonIgnore]
        public int sequence { get; set; }
        [JsonIgnore]
        public string journal_name { get; set; }

        public int company_id { get; set; }
        public int partner_id { get; set; }
        public int journal_id { get; set; }
        public int payment_method_line_id { get; set; } //metodo de pago especial
        
        //[JsonIgnore]
        public int partner_bank_id { get; set; }

        public int bank_account_id { get; set; }
        public string number_check_customer { get; set; }

        //public string check_number { get; set; }
        public DateTime date_release { get; set; }

        public string partner_type { get; set; }
        public string payment_type { get; set; }
        public DateTime date { get; set; }
        [JsonProperty("ref")]
        public string _ref { get; set; }
        public decimal amount { get; set; }
        
        [Ignore]
        //[JsonIgnore]
        public bool is_from_mobile { get; set; }

        //[Ignore]
        //[JsonIgnore]
        //public int model_id { get; set; }

        //[Ignore]
        //[JsonIgnore]
        //public int create_id { get; set; }

        [Ignore]
        [JsonIgnore]
        public AccountPaymentInvoiceLine[] lines { get; set; }

        //[Ignore]
        //[JsonIgnore]
        //public object[] lines_object { get; set; }

        //Para el envío
        [Ignore]
        //[JsonIgnore]
        [JsonProperty]
        public List<object> payment_invoice_line_ids { get; set; }

        [Ignore]
        public string? recipe_name { get; set; }

        public string? reference_tc { get; set; }
        [JsonIgnore]
        public string? auth_tc { get; set; }
        public string? lote_tc { get; set; }


        [JsonProperty("center_id")]
        public int center_id { get; set; }
        [JsonProperty("destination_account_id")]
        public int destination_account_id { get; set; }
        [JsonProperty("analytic_account_id")]
        public int analytic_account_id { get; set; }
         
        [JsonProperty("depositos_confirmar_id")]
        public int depositos_confirmar_id { get; set; }

        [JsonProperty("subclasificacion_gasto_id")]
        public int subclasificacion_gasto_id { get; set; }

        [JsonProperty("deducible")] //si/no
        public string deducible { get; set; }

        [JsonProperty("deducible")] //si/no
        public string bank_type_emision { get; set; }
    }

    public class JournalSummary
    {
        public string JournalName { get; set; }
        public int TotalRecords { get; set; }
        public decimal TotalAmount { get; set; }
    }
}
