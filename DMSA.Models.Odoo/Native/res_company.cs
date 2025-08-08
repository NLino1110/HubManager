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
        public int credit_note_journal_id_ { get; set; }


        [JsonIgnore]
        public int partner_id_
        {
            get => GetId(partner_id);
            set => partner_id = SetId(partner_id, value);
        }
    }

    public class journal_base
    {
        public int id { get; set; }
        public string name { get; set; }
    }
}
