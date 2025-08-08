using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using CobranzasDMSA_Odoo.Models;
using SQLite;
using SQLiteNetExtensions.Attributes;

namespace DMSA.Models.Odoo.Native
{
    public class account_journal
    {
        [PrimaryKey]
        public int id { get; set; }
        public string code { get; set; }
        public string type { get; set; }
        public bool active { get; set; }
        public string name { get; set; }

        public bool use_mobile_app { get; set; }
        public bool credit_card { get; set; }
        public bool credit_note { get; set; }
        public DateTime write_date { get; set; }
        public DateTime create_date { get; set; }

        [Ignore]
        public mobile_app_tag[] mobile_app_tag_ids { get; set; }

        [ForeignKey(typeof(BankAccount))]
        public int BankAccountId { get; set; }

        [ForeignKey(typeof(res_company))]
        public int CompanyId { get; set; }

        //[ManyToOne(CascadeOperations = CascadeOperation.All)]
        [Ignore]
        public List<BankAccount> bank_account_id { get; set; }

        [Ignore]
        //[ManyToOne(CascadeOperations = CascadeOperation.All)]
        public List<res_company> company_id { get; set; }

        [Ignore]
        public List<inbound_payment_method> inbound_payment_method_line_ids { get; set; }

        //[JsonIgnore] // Ignoramos esta propiedad al serializar para evitar la redundancia
        //public int BankIdData => BankId != null && BankId.Count > 0 ? BankId[0].Id : 0;
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

    public class account_journal_type
    {
        [PrimaryKey]
        public int id { get; set; }
        public string code { get; set; }
        public string name { get; set; }
    }

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
