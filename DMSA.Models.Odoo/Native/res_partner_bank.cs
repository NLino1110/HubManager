using DMSA.Models.Odoo.Base;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SQLite;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMSA.Models.Odoo.Native
{
    [Table("res_partner_bank")]
    public class res_partner_bank : OdooEntity, INotifyPropertyChanged
    {
        [PrimaryKey]
        public int id { get; set; }
        public string acc_number { get; set; }
        public string acc_holder_name { get; set; }

        public string type_account { get; set; }
        public string use_bank_type { get; set; }
        public bool allow_out_payment { get; set; }

        [Ignore]
        public JToken partner_id { get; set; }
        [JsonIgnore]
        public int _partner_id
        {
            get => GetId(partner_id);
            set => partner_id = SetId(partner_id, value);
        }

        [Ignore]
        public JToken bank_id { get; set; }

        [JsonIgnore]
        public int _bank_id
        {
            get => GetId(bank_id);
            set => bank_id = SetId(bank_id, value);
        }

        [Ignore]
        public JToken currency_id { get; set; }
        [JsonIgnore]
        public int _currency_id
        {
            get => GetId(currency_id);
            set => currency_id = SetId(currency_id, value);
        }

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

        [Column("create_date")]
        public DateTime? create_date { get; set; }

        [Column("write_date")]
        public DateTime? write_date { get; set; }

        public event PropertyChangedEventHandler? PropertyChanged;
    }

    //public class Partner_Id
    //{
    //    public int id { get; set; }
    //    public string name { get; set; }
    //}
    
    public class Currency_Id
    {
        [PrimaryKey]
        public int id { get; set; }
        public string name { get; set; }
    }
}
