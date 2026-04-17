using DMSA.Models.Odoo.Base;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMSA.Models.Odoo.Native
{
    public class res_company : OdooEntity
    {
        [PrimaryKey]
        public int id { get; set; }
        public string name { get; set; }        

        public string email { get; set; }
        public string phone { get; set; }
        public string mobile { get; set; }
        public string social_twitter { get; set; }
        public string social_facebook { get; set; }
        public string social_github { get; set; }
        public string social_linkedin { get; set; }
        public string social_youtube { get; set; }
        [Ignore]
        public JToken partner_id { get; set; }
        [Ignore]
        public JToken check_journal_id { get; set; }
        public int check_journal_id_ { get; set; }
        [Ignore]
        public JToken credit_note_journal_id { get; set; }
        public int credit_note_journal_id_
        {
            get => GetId(credit_note_journal_id);
            set => credit_note_journal_id = SetId(credit_note_journal_id, value);
        }


        [Ignore]
        public JToken cuadratura_account_id { get; set; }
        public int cuadratura_account_id_
        {
            get => GetId(cuadratura_account_id);
            set => cuadratura_account_id = SetId(cuadratura_account_id, value);
        }


        [JsonIgnore]
        public int partner_id_
        {
            get => GetId(partner_id);
            set => partner_id = SetId(partner_id, value);
        }

        [Column("create_date")]
        public DateTime? create_date { get; set; }

        [Column("write_date")]
        public DateTime? write_date { get; set; }
    }

    public class journal_base
    {
        public int id { get; set; }
        public string name { get; set; }
    }
}
