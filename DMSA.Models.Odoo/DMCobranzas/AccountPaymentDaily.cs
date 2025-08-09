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
    public class AccountPaymentDaily
    {
        [Key]
        [PrimaryKey]
        [NotNull]
        public string closing_id { get; set; }
        public DateTime datetime_closing { get; set; }
        public int company_id { get; set; }
        public int bank_id { get; set; }
        public int uid { get; set; }
        public string payment_reference { get; set; }
        public decimal closing_amount { get; set; }
        public string closing_details { get; set; }

        [JsonIgnore]
        public bool was_odoo_synced { get; set; }
    }
}
