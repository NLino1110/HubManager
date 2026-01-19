using DMSA.Models.Odoo.Base;
using Newtonsoft.Json;
using SQLite;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMSA.Models.Odoo.DebitCollection
{
    [Table("tarjetas_tipo_pago")]
    public class TarjetasTipoPago : OdooEntity
    {
        [Key]
        [PrimaryKey]
        [JsonProperty("id")]
        [Column("id")]
        public int id { get; set; }
        [JsonProperty("active")]
        [Column("active")]
        public bool active { get; set; }
        [JsonProperty("name")]
        [Column("name")]
        public string name { get; set; }
        
        [JsonProperty("display_name")]
        [Column("display_name")]
        public string display_name { get; set; }
        [JsonProperty("create_date")]
        [Column("create_date")]
        public DateTime create_date { get; set; }
        [JsonProperty("write_date")]
        [Column("write_date")]
        public DateTime write_date { get; set; }
    }
}
