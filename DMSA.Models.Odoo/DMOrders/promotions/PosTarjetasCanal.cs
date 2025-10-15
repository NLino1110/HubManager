using DMSA.Models.Odoo.Base;
using Newtonsoft.Json;
using SQLite;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMSA.Models.Odoo.DMOrders.promotions
{
    [Table("pos_tarjetas_canal")]
    public class PosTarjetasCanal : OdooEntity
    {
        [Key]
        [PrimaryKey]
        [AutoIncrement]
        [NotNull]
        [JsonProperty("id")]
        public int id { get; set; }

        [JsonProperty("active")]
        public bool active { get; set; } = true;

        [JsonProperty("name")]
        public string name { get; set; }

        // Nombre técnico
        [JsonProperty("name_tech")]
        public string name_tech { get; set; }
    }

}
