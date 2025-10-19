using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMSA.Models.Odoo.DMOrders.tareas
{
    [Table("motivo_actividad_diaria")]
    public class MotivoActividadDiaria
    {
        [PrimaryKey]
        public int id { get; set; }
        public string name { get; set; }
        public string? description { get; set; }
        public DateTime create_date { get; set; }
        public DateTime write_date { get; set; }
    }
}
