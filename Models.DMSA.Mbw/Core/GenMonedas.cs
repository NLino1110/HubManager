using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Models.DMSA.Mbw.Core
{    
    [Table("GENMONEDAS")]
    public class GenMonedas
    {
        [Key]
        [Required]
        [Column("CODMONEDA")]
        [StringLength(5)]
        public string CodMoneda { get; set; }

        [Required]
        [Column("DESCRIPCION")]
        [StringLength(40)]
        public string Descripcion { get; set; }

        [Required]
        [Column("SIMBOLO")]
        [StringLength(3)]
        public string Simbolo { get; set; }

        [Required]
        [Column("POSICION")]
        [StringLength(1)]
        public string Posicion { get; set; }

        [Required]
        [Column("MONEDABASE")]
        [StringLength(1)]
        public string MonedaBase { get; set; }

        [Column("CODUSUARIO")]
        [StringLength(15)]
        public string CodUsuario { get; set; }

        [Required]
        [Column("FECHA")]
        public DateTime Fecha { get; set; }

        [Column("CODUSUARIOMODIFICACION")]
        [StringLength(15)]
        public string CodUsuarioModificacion { get; set; }

        [Column("FECHAMODIFICACION")]
        public DateTime? FechaModificacion { get; set; }
    }


}
