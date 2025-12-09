using SQLite;

namespace DMSA.Models.Odoo.DMOrders.tareas
{
    [Table("motivo_actividad_diaria")]
    public class MotivoActividadDiaria
    {
        [PrimaryKey]
        public int id { get; set; }
        public int codmotivo { get; set; }
        public string name { get; set; }
        public string? description { get; set; }
        public string? tipo { get; set; }
        public string? codsistema { get; set; }
        public DateTime create_date { get; set; }
        public DateTime write_date { get; set; }
    }
}
