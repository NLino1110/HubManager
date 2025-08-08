using DMSA.Models.Odoo.Native;
using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CobranzasDMSA_Odoo.Models
{
    [Obsolete]
    public class __res_company
    {
        [PrimaryKey]
        public int id { get; set; }
        public string name { get; set; }

        [Ignore]
        public res_partner[] partner_id { get; set; }
        public int partner_id_ { get; set; }
        public string email { get; set; }
        public string phone { get; set; }
        public string mobile { get; set; }
        public string social_twitter { get; set; }
        public string social_facebook { get; set; }
        public string social_github { get; set; }
        public string social_linkedin { get; set; }
        public string social_youtube { get; set; }
        [Ignore]
        public journal_base[] check_journal_id { get; set; }
        public int check_journal_id_ { get; set; }
        [Ignore]
        public journal_base[] credit_note_journal_id { get; set; }
        public int credit_note_journal_id_ { get; set; }
    }

    public class journal_base
    {
        public int id { get; set; }
        public string name { get; set; }
    }
}
