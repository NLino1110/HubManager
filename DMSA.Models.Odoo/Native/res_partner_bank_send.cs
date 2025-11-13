using Newtonsoft.Json;
using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMSA.Models.Odoo.Native
{
    public class res_partner_bank_send
    {
        public bool active { get; set; }
        public string acc_number { get; set; }
        public string sanitized_acc_number { get; set; }
        public string acc_holder_name { get; set; }
        public string type_account { get; set; }
        public string use_bank_type { get; set; }
        public bool allow_out_payment { get; set; }
        public int partner_id { get; set; }
        public int bank_id { get; set; }
        public int sequence { get; set; }
        public int currency_id { get; set; }
        public object[] bank_account_id { get; set; }
        public object[] company_id { get; set; }

        [Column("create_date")]
        public DateTime? create_date { get; set; }

        [Column("write_date")]
        public DateTime? write_date { get; set; }
    }
}
