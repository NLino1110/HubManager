using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Models.DMSA.Mbw.Inventario;

namespace Models.DMSA.Mbw.Core
{    
    [Table("BITACORA_WEB")]
    public class BitacoraWeb
    {
        [Key]
        [Column("ID_BITACORA")]
        public decimal IdBitacora { get; set; } // Representa el campo ID_BITACORA

        [Required]
        [Column("CODEMPRESA")]
        public decimal CodEmpresa { get; set; } // Representa el campo CODEMPRESA

        [Required]
        [Column("PROCESO")]
        [StringLength(100)]
        public string Proceso { get; set; } // Representa el campo PROCESO

        [Column("PROCESO_DETALLE")]
        [StringLength(100)]
        public string ProcesoDetalle { get; set; } // Representa el campo PROCESO_DETALLE

        [Column("CADENA_ENVIO")]
        public string CadenaEnvio { get; set; } // Representa el campo CADENA_ENVIO

        [Column("FECHA_INICIO")]
        public DateTime? FechaInicio { get; set; } // Representa el campo FECHA_INICIO

        [Column("FECHA_FIN")]
        public DateTime? FechaFin { get; set; } // Representa el campo FECHA_FIN

        [Column("STATUS")]
        [StringLength(50)]
        public string Status { get; set; } // Representa el campo STATUS

        [Column("RESPONSE_MSG")]
        [StringLength(200)]
        public string ResponseMsg { get; set; } // Representa el campo RESPONSE_MSG

        [Column("ERROR_GENERADO")]
        [StringLength(2000)]
        public string ErrorGenerado { get; set; } // Representa el campo ERROR_GENERADO
    }


}
