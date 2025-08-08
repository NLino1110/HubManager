using Newtonsoft.Json;
using SQLite;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace CobranzasDMSA_Odoo.Models
{
    public class AccountPaymentSend: AccountPayment
    {
        //[JsonIgnore]
        public int parent_id { get; set; }
        //[JsonIgnore]
        public int sequence { get; set; }
        //[JsonIgnore]
        public string journal_name { get; set; }
        //[Ignore]
        //[JsonIgnore]
        public AccountPaymentInvoiceLineSend[] lines { get; set; }
        
        [JsonIgnore]
        public List<object> payment_invoice_line_ids { get; set; }

        [OnSerializing]
        internal void OnSerializingMethod(StreamingContext context)
        {
            // Establecer el valor que se usará durante la serialización
            //parent_id = context.;
        }
    }
}
