using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Models.DMSA.Mbw.Core;
using System.Runtime.Serialization;

namespace Models.DMSA.Mbw.Inventario
{

    [Table("BODEGASXAGENCIA")]
    public class BodegasXAgencia
    {
        [Key]
        [Column("CODBODEGAAGENCIA")]
        public int CodBodegaAgencia { get; set; }

        [Column("CODAGENCIA")]
        public int CodAgencia { get; set; }

        [Column("CODBODEGA")]
        public int CodBodega { get; set; }

        [Column("OBSERVACIONES")]
        [StringLength(50)]
        public string? Observaciones { get; set; }

        [Column("CODESTADO")]
        public int CodEstado { get; set; }

        [Column("USUARIOCAMBIOESTADO")]
        [StringLength(15)]
        public string? UsuarioCambioEstado { get; set; }

        [Column("FECHACAMBIOESTADO")]
        public DateTime? FechaCambioEstado { get; set; }

        [Column("ESDEVOLUCION")]
        [StringLength(1)]
        public string? EsDevolucion { get; set; }

        [Column("PROCESAWMS")]
        [StringLength(1)]
        public string? ProcesaWMS { get; set; }

        [Column("CORREOCONTACTO")]
        [StringLength(200)]
        public string? CorreoContacto { get; set; }

        [Column("PRINCIPAL")]
        [StringLength(1)]
        public string? Principal { get; set; }

        [Column("WEB")]
        [StringLength(1)]
        public string? Web { get; set; } = "N";

        [Column("CONTROLMANUAL")]
        [StringLength(1)]
        public string? ControlManual { get; set; } = "N";

        [Column("CONTROLDISPONIBLEWMS")]
        [StringLength(1)]
        public string? ControlDisponibleWMS { get; set; } = "N";

        [Column("ENVIOECOMMERCE")]
        [StringLength(1)]
        public string? EnvioEcommerce { get; set; } = "N";
    }


}
