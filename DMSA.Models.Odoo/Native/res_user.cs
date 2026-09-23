using CobranzasDMSA_Odoo.Models;
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
    public class res_user: OdooEntity
    {
        [PrimaryKey]
        public int id { get; set; }
        public string login { get; set; }
        public string name { get; set; }
        public string complete_name { get; set; }

        //[JsonIgnore]
        //public string partner_name { get; set; }

        [Ignore]
        public JToken company_id { get; set; }
        [Ignore]
        public JToken partner_id { get; set; }
        [Ignore]
        public JToken sale_team_id { get; set; }

        /// <summary>Campo Odoo "Mobile App" (Administrador Apps Móviles = 218).</summary>
        [Ignore]
        public JToken mobile_app_id { get; set; }

        [Ignore]
        public JToken groups_id { get; set; }


        [JsonIgnore]
        public int _company_id
        {
            get => GetId(company_id);
            set => company_id = SetId(company_id, value);
        }


        [JsonIgnore]
        public int _partner_id
        {
            get => GetId(partner_id);
            set => partner_id = SetId(partner_id, value);
        }

        [JsonIgnore]
        public int _sale_team_id
        {
            get => GetId(sale_team_id);
            set => sale_team_id = SetId(sale_team_id, value);
        }

        [JsonIgnore]
        public int _mobile_app_id
        {
            get => GetId(mobile_app_id);
            set => mobile_app_id = SetId(mobile_app_id, value);
        }

        [JsonIgnore]
        public int[] GroupsIds => GetIds(groups_id);

        [Column("create_date")]
        public DateTime? create_date { get; set; }

        [Column("write_date")]
        public DateTime? write_date { get; set; }
    }

    public class Sale_Team_Id
    {
        public int id { get; set; }
        public string name { get; set; }
    }
}
