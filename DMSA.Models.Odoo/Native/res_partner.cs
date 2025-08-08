using DMSA.Models.Odoo.Base;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DMSA.Models.Odoo.Native
{
    [Table("res_partner")]
    public class res_partner : OdooEntity
    {
        [PrimaryKey]
        [AutoIncrement]
        [JsonIgnore]
        public int id_sequence { get; set; }
        public int id { get; set; }
        [Ignore]
        public JToken company_id { get; set; }
        [JsonIgnore]
        public int _company_id
        {
            get => GetId(company_id);
            set => company_id = SetId(company_id, value);
        }
        public string vat { get; set; }
        public string name { get; set; }
        public string display_name { get; set; }
        public string email { get; set; }
        [Ignore]
        public JToken user_id { get; set; }
        [JsonIgnore]
        public int _user_id
        {
            get => GetId(user_id);
            set => user_id = SetId(user_id, value);
        }
        
        public string user_login { get; set; }
        public decimal total_due { get; set; }
        public decimal total_overdue { get; set; }
        public decimal debit { get; set; }
        public decimal credit { get; set; }
        public decimal total_invoiced { get; set; }

        public string street { get; set; }
        public string street2 { get; set; }

        //[JsonIgnore]
        //[Ignore]
        public decimal total_to_beat { get; set; }

        //[JsonIgnore]
        //[Ignore]
        public decimal positive_balance { get; set; }

        public int client_type_id { get; set; }
        public string client_type_name { get; set; }
        public DateTime write_date { get; set; }
        public DateTime create_date { get; set; }
        public DateTime? birthdate { get; set; }
        
        [Ignore]
        public JToken parent_id { get; set; }
        //public int commercial_parent_id { get; set; }
        [JsonIgnore]
        public int _parent_id
        {
            get => GetId(parent_id);
            set => parent_id = SetId(parent_id, value);
        }
        public string zip { get; set; }
        public string phone { get; set; }
        public string mobile { get; set; }

        [Ignore]
        public JToken city_id { get; set; }
        [JsonIgnore]
        public int _city_id
        {
            get => GetId(city_id);
            set => city_id = SetId(city_id, value);
        }
        
        public string city { get; set; }
        [Ignore]
        public JToken state_id { get; set; }
        [JsonIgnore]
        public int _state_id
        {
            get => GetId(state_id);
            set => state_id = SetId(state_id, value);
        }
        
        [Ignore]
        public JToken country_id { get; set; }
        [JsonIgnore]
        public int _country_id
        {
            get => GetId(country_id);
            set => country_id = SetId(country_id, value);
        }
        
        public string contact_address_complete { get; set; }
        public bool active { get; set; }

        [Ignore]
        [JsonIgnore]
        public string client_type_display
        {
            get => string.Concat(client_type_id, "-", client_type_name);
        }

        [Ignore]
        [JsonIgnore]
        public string title
        {
            get => string.Concat(id, " - ", name);
        }

    }
}
