using CobranzasDMSA_Odoo.Models;
using DMSA.Models.Odoo.Base;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SQLite;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMSA.Models.Odoo.Native
{
    public class account_move : OdooEntity, INotifyPropertyChanged
    {
        [PrimaryKey]
        public int id { get; set; }
        [JsonProperty("name")]
        public string name { get; set; }        
        public string invoice_origin { get; set; }
        public string docnum_mask { get; set; }
        public DateTime invoice_date { get; set; }
        public DateTime invoice_date_due { get; set; }
        public string payment_state { get; set; }
        public string state { get; set; }
        public string move_type { get; set; }
        public string l10n_ec_authorization_number { get; set; }

        public decimal amount_residual { get; set; }
        public decimal amount_untaxed_signed { get; set; }
        public decimal amount_total_signed { get; set; }
        public decimal amount_total { get; set; }
        public decimal amount_tax { get; set; }

        //public int _partner_id { get; set; }
        [Ignore]
        public JToken reversed_entry_id { get; set; }
        [JsonIgnore]
        public int _reversed_entry_id
        {
            get => GetId(reversed_entry_id);
            set => reversed_entry_id = SetId(reversed_entry_id, value);
        }

        [JsonProperty("ref")]
        public string _ref { get; set; }


        [Ignore]
        public JToken refund_invoice_ids { get; set; }

        [JsonIgnore]
        [Column("_refund_invoice_ids")]
        public string _refund_invoice_ids
        {
            get => SetIdsJson(refund_invoice_ids);
            set { }
        }

        [Ignore]
        public JToken partner_id { get; set; }

        public int _partner_id
        {
            get => GetId(partner_id);
            set => partner_id = SetId(partner_id, value);
        }

        [Ignore]
        public JToken journal_id { get; set; }

        public int _journal_id
        {
            get => GetId(journal_id);
            set => journal_id = SetId(journal_id, value);
        }

        [Ignore]
        public JToken l10n_latam_document_type_id { get; set; }
        public int _l10n_latam_document_type_id
        {
            get => GetId(l10n_latam_document_type_id);
            set => l10n_latam_document_type_id = SetId(l10n_latam_document_type_id, value);
        }

        [Ignore]
        public JToken invoice_user_id { get; set; }
        public int _invoice_user_id
        {
            get => GetId(invoice_user_id);
            set => invoice_user_id = SetId(invoice_user_id, value);
        }

        [Ignore]
        public JToken company_id { get; set; }

        public int _company_id
        {
            get => GetId(company_id);
            set => company_id = SetId(company_id, value);
        }

        [Ignore]
        public JToken team_id { get; set; }

        public int _team_id
        {
            get => GetId(team_id);
            set => team_id = SetId(team_id, value);
        }

        [Ignore]
        public JToken invoice_line_ids { get; set; }
        public string _invoice_line_ids
        {
            get => SetIdsJson(invoice_line_ids);
            set { }
        }

        [Ignore]
        public JToken printer_id { get; set; }
        public int _printer_id
        {
            get => GetId(printer_id);
            set => printer_id = SetId(printer_id, value);
        }

        [JsonProperty("create_date")]
        [Column("create_date")]
        public DateTime? create_date { get; set; }


        [JsonProperty("write_date")]
        [Column("write_date")]
        public DateTime? write_date { get; set; }

        public event PropertyChangedEventHandler? PropertyChanged;
    }

    public class Partner_Id
    {
        public int id { get; set; }
        public string name { get; set; }
    }

    public class Journal_Id
    {
        public int id { get; set; }
        public string name { get; set; }
    }

    public class L10n_Latam_Document_Type_Id
    {
        public int id { get; set; }
        public string name { get; set; }
    }

    public class Invoice_User_Id
    {
        public int id { get; set; }
        public string name { get; set; }
    }

    //public class Company_Id
    //{
    //    public int id { get; set; }
    //    public string name { get; set; }
    //}

    public class Team_Id
    {
        public int id { get; set; }
        public string name { get; set; }
    }

    public class Invoice_Line_Ids
    {
        public int id { get; set; }
        public string name { get; set; }
    }

    public class Reverse_Entry_Ids
    {
        public int id { get; set; }
        public string name { get; set; }
    }

    public class Refund_Invoice_Ids
    {
        public int id { get; set; }
        public string name { get; set; }
    }

    public class Printer_Ids
    {
        public int id { get; set; }
        public string name { get; set; }
    }
}
