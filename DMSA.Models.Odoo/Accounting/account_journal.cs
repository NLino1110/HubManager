using CobranzasDMSA_Odoo.Models;
using DMSA.Models.Odoo.Base;
using DMSA.Models.Odoo.Native;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SQLite;
using SQLiteNetExtensions.Attributes;

namespace DMSA.Models.Odoo.Accounting
{
    public class account_journal : OdooEntity
    {
        [PrimaryKey]
        public int id { get; set; }
        public string code { get; set; }
        public string type { get; set; }
        public bool active { get; set; }
        public string name { get; set; }

        public bool use_mobile_app { get; set; }

        [Obsolete("Ya no usado")]
        public bool credit_card { get; set; }
        [Obsolete("Ya no usado")]
        public bool credit_note { get; set; }

        [JsonProperty("aplica_cheque")]
        [Column("aplica_cheque")]
        public bool aplica_cheque { get; set; }
        [JsonProperty("aplica_tarjeta")]
        [Column("aplica_tarjeta")]
        public bool aplica_tarjeta { get; set; }

        [JsonProperty("create_date")]
        [Column("create_date")]
        public DateTime? create_date { get; set; }


        [JsonProperty("write_date")]
        [Column("write_date")]
        public DateTime? write_date { get; set; }

        [Ignore]
        [JsonIgnore]
        public mobile_app_tag[] mobile_app_tag_ids { get; set; }

        [Ignore]
        [JsonProperty("mobile_app_tag_ids")]
        public JToken virtual_mobile_app_tag_ids { get; set; }

        [Ignore]        
        public int[] _mobile_app_tag_ids
        {
            get => GetIds(virtual_mobile_app_tag_ids);
            set => virtual_mobile_app_tag_ids = SetIds(virtual_mobile_app_tag_ids, value);
        }

        [ForeignKey(typeof(BankAccount))]
        public int _bank_account_id
        {
            get => GetId(bank_account_id);
            set => bank_account_id = SetId(bank_account_id, value);
        }

        [ForeignKey(typeof(res_company))]
        public int _company_id
        {
            get => GetId(company_id);
            set => company_id = SetId(company_id, value);
        }

        //[ManyToOne(CascadeOperations = CascadeOperation.All)]
        [Ignore]
        public JToken bank_account_id { get; set; }

        [Ignore]
        //[ManyToOne(CascadeOperations = CascadeOperation.All)]
        public JToken company_id { get; set; }

        [Ignore]
        public JToken inbound_payment_method_line_ids { get; set; }

        [Ignore]
        public int[] _inbound_payment_method_line_ids
        {
            get => GetIds(inbound_payment_method_line_ids);
            set => inbound_payment_method_line_ids = SetIds(inbound_payment_method_line_ids, value);
        }

    }



    public class mobile_app_tag
    {
        public string code { get; set; }
        public string name { get; set; }
        public string description { get; set; }
    }

    public class BankAccount
    {
        [PrimaryKey]
        public int id { get; set; }

        //public string acc_number { get; set; }
        // Otras propiedades relevantes para BankAccount

        // Propiedad para la clave foránea a Bank
        //[ForeignKey(typeof(account_journal))]
        //public int BankId { get; set; }

        // Constructor vacío requerido para SQLite-Net-PCL
        public BankAccount()
        {

        }
    }

    public class Bank
    {
        public int id { get; set; }
        public string name { get; set; }
        public bool street { get; set; }
        public bool street2 { get; set; }
        public bool city { get; set; }
    }

    //public class Company
    //{
    //    [PrimaryKey]
    //    public int id { get; set; }

    //    //[ForeignKey(typeof(account_journal))]
    //    //public int CmpId { get; set; }

    //    public string name { get; set; }
    //}

    [Obsolete]
    public class account_journal_type
    {
        [PrimaryKey]
        public int id { get; set; }
        public string code { get; set; }
        public string name { get; set; }
    }

    [Obsolete]
    public class inbound_payment_method
    {
        [JsonIgnore]
        [PrimaryKey]
        [AutoIncrement]
        public int sequence { get; set; }

        [JsonIgnore]
        public int parent_id { get; set; }

        public int id { get; set; }
        public string code { get; set; }
        public string name { get; set; }
    }

}
