using CobranzasDMSA_Odoo.Models;
using SQLite;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace DMSA.Models.Odoo.Native
{
    public class account_move
    {
        [PrimaryKey]
        public int id { get; set; }
        [JsonProperty("name")]
        public string name { get; set; }
        
        public string invoice_origin { get; set; }
        public DateTime invoice_date { get; set; }
        public DateTime invoice_date_due { get; set; }
        public string payment_state { get; set; }
        public string move_type { get; set; }
        public string l10n_ec_authorization_number { get; set; }

        public decimal amount_residual { get; set; }
        public decimal amount_untaxed_signed { get; set; }
        public decimal amount_total_signed { get; set; }
        public decimal amount_total { get; set; }
        public decimal amount_tax { get; set; }

        //public int _partner_id { get; set; }
        [Ignore]
        public Reverse_Entry_Ids[] reversed_entry_id { get; set; }
        [JsonProperty("ref")]
        public string _ref { get; set; }

        [JsonIgnore]
        public int _reversed_entry_id { get; set; }

        public string _refund_invoice_ids
        {
            get
            {
                return JsonConvert.SerializeObject(refund_invoice_ids);
            }

            set
            {
                value = JsonConvert.SerializeObject(refund_invoice_ids);
            }
        }

        [Ignore]
        public Refund_Invoice_Ids[] refund_invoice_ids { get; set; }

        public int _partner_id { get; set; }
        
        public int _journal_id { get; set; }
        
        public int _l10n_latam_document_type_id { get; set; }
        public int _invoice_user_id { get; set; }
        

        public int _printer_id { get; set; }
        

        public int _company_id { get; set; }        

        public int _team_id { get; set; }
        
        [Ignore]
        public Partner_Id[] partner_id { get; set; }
        [Ignore]
        public Journal_Id[] journal_id { get; set; }
        [Ignore]
        public L10n_Latam_Document_Type_Id[] l10n_latam_document_type_id { get; set; }
        [Ignore]
        public Invoice_User_Id[] invoice_user_id { get; set; }
        [Ignore]
        public res_company[] company_id { get; set; }
        [Ignore]
        public Team_Id[] team_id { get; set; }
        [Ignore]
        public Invoice_Line_Ids[] invoice_line_ids { get; set; }
        [Ignore]
        public Printer_Ids[] printer_id { get; set; }
        public DateTime write_date { get; set; }
        public DateTime create_date { get; set; }
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
