using DMSA.Models.Odoo.Base;
using DMSA.Models.Odoo.Json.Converters;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMSA.Models.Odoo.Sales
{
    [Table("product_pricelist")]
    public class product_pricelist : OdooEntity
    {
        [PrimaryKey]
        [JsonProperty("id")]
        public int id { get; set; }
        public string name { get; set; }
        public bool active { get; set; }
        public int sequence { get; set; }
        public string display_name { get; set; }
        public string clave_externa { get; set; }
        public string tipo_canal { get; set; }
        [JsonProperty("use_mobile_app")]
        public bool use_mobile_app { get; set; }

        [Ignore]        
        public JToken currency_id { get; set; }
        [Ignore]        
        public JToken company_id { get; set; }
        [Ignore]        
        public JToken create_uid { get; set; }
        [Ignore]        
        public JToken write_uid { get; set; }
        [Ignore]        
        public JToken tipo_canal_id { get; set; }

        [JsonIgnore]
        public int _currency_id
        {
            get => GetId(currency_id);
            set => currency_id = SetId(currency_id, value);
        }

        [JsonIgnore]
        public int _company_id
        {
            get => GetId(company_id);
            set => company_id = SetId(company_id, value);
        }

        [JsonIgnore]
        public int _create_uid
        {
            get => GetId(create_uid);
            set => create_uid = SetId(create_uid, value);
        }

        [JsonIgnore]
        public int _write_uid
        {
            get => GetId(write_uid);
            set => write_uid = SetId(write_uid, value);
        }

        [JsonIgnore]
        public int _tipo_canal_id
        {
            get => GetId(tipo_canal_id);
            set => tipo_canal_id = SetId(tipo_canal_id, value);
        }

        [Column("create_date")]
        [JsonProperty("create_date")]
        [JsonConverter(typeof(OdooNullableDateTimeConverter))]
        public DateTime create_date { get; set; }

        [Column("write_date")]
        [JsonProperty("write_date")]
        [JsonConverter(typeof(OdooNullableDateTimeConverter))]
        public DateTime write_date { get; set; }

        //public bool date_start { get; set; }
        //public bool date_end { get; set; }
    }

}
