using Newtonsoft.Json;
using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMSA.Models.Odoo.Native
{   
    public class res_partner_bank
    {
        [PrimaryKey]
        public int id { get; set; }
        [JsonIgnore]
        public int PartnerId { get; set; }
        [JsonIgnore]
        public int BankId { get; set; }
        public string acc_number { get; set; }
        public string acc_holder_name { get; set; }

        public string type_account { get; set; }
        public string use_bank_type { get; set; }
        public bool allow_out_payment { get; set; }

        [JsonIgnore]
        public int CurrencyId { get; set; }

        [Ignore]
        public Partner_Id[] partner_id { get; set; }
        [Ignore]
        public Bank_Id[] bank_id { get; set; }
        [Ignore]
        public Currency_Id[] currency_id { get; set; }

        [Ignore]
        [JsonIgnore]
        public string display {
            get => string.Concat(acc_number, " - ", acc_holder_name);
        }

        [Ignore]
        [JsonIgnore]
        public string bank_name { get; set; }

        [Ignore]
        [JsonIgnore]
        public string displayFull
        {
            get => string.Concat(acc_number, "-", acc_holder_name, "-", bank_name);
        }
    }

    //public class Partner_Id
    //{
    //    public int id { get; set; }
    //    public string name { get; set; }
    //}

    public class Bank_Id
    {
        [PrimaryKey]
        public int id { get; set; }
        public string name { get; set; }
    }

    public class Currency_Id
    {
        [PrimaryKey]
        public int id { get; set; }
        public string name { get; set; }
    }
}
