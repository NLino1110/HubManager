using DMSA.Models.Odoo.Base;
using DMSA.Models.Odoo.Native;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SQLite;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMSA.Models.Odoo.DebitCollection
{
    [Table("tarjetas_plazos_banco")]
    public class TarjetasPlazosBanco : OdooEntity
    {
        [Key]
        [PrimaryKey]
        [JsonProperty("id")]
        [Column("id")]
        public int id { get; set; }
        
        [JsonProperty("name")]
        [Column("name")]
        public string name { get; set; }
        
        [JsonProperty("display_name")]
        [Column("display_name")]
        public string display_name { get; set; }

        //[Ignore]
        //[JsonProperty("account_journal_id")]
        //[Column("account_journal_id")]
        //public JToken account_journal_id { get; set; }

        //[JsonIgnore]
        //[JsonProperty("_account_journal_id")]
        //[Column("_account_journal_id")]
        //public int _account_journal_id
        //{
        //    get => GetId(account_journal_id);
        //    set => account_journal_id = SetId(account_journal_id, value);
        //}

        [Ignore]
        [JsonProperty("bank_id")]
        [Column("bank_id")]
        public JToken bank_id { get; set; }

        [JsonIgnore]
        [JsonProperty("_bank_id")]
        [Column("_bank_id")]
        public int _bank_id
        {
            get => GetId(bank_id);
            set => bank_id = SetId(bank_id, value);
        }

        [Ignore]
        [JsonProperty("pos_tipo_pago")]
        [Column("pos_tipo_pago")]
        public JToken pos_tipo_pago { get; set; }

        [JsonIgnore]
        [JsonProperty("_pos_tipo_pago")]
        [Column("_pos_tipo_pago")]
        public int _pos_tipo_pago
        {
            get => GetId(pos_tipo_pago);
            set => pos_tipo_pago = SetId(pos_tipo_pago, value);
        }

        [Ignore]
        [JsonProperty("tipo_pay")]
        [Column("tipo_pay")]
        public JToken tipo_pay { get; set; }
        [JsonIgnore]
        [JsonProperty("_tipo_pay")]
        [Column("_tipo_pay")]
        public int _tipo_pay
        {
            get => GetId(tipo_pay);
            set => tipo_pay = SetId(tipo_pay, value);
        }

        [Ignore]
        [JsonProperty("tipo_canal_ids")]
        [Column("tipo_canal_ids")]
        public JToken tipo_canal_ids { get; set; }

        [Column("tipo_canal_ids_json")]
        public string tipo_canal_ids_json
        {
            get => tipo_canal_ids?.ToString(Formatting.None);
            set => tipo_canal_ids = string.IsNullOrEmpty(value)
                ? null
                : JToken.Parse(value);
        }

        [Ignore]
        [JsonProperty("cuotas_mensuales")]
        public JToken cuotas_mensuales { get; set; }

        [JsonIgnore]
        [JsonProperty("_cuotas_mensuales")]
        [Column("_cuotas_mensuales")]
        public int _cuotas_mensuales
        {
            get => GetId(cuotas_mensuales);
            set => cuotas_mensuales = SetId(cuotas_mensuales, value);
        }

        [JsonProperty("porc_comision")]
        [Column("porc_comision")]
        public double porc_comision { get; set; }

        [JsonProperty("active")]
        [Column("active")]
        public bool active { get; set; }

        [JsonProperty("create_date")]
        [Column("create_date")]
        public DateTime create_date { get; set; }
        [JsonProperty("write_date")]
        [Column("write_date")]
        public DateTime write_date { get; set; }
    }
}
