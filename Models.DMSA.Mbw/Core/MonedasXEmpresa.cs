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

    [Table("MONEDASXEMPRESA")]
    public class MonedasXEmpresa
    {
        [Required]
        [Column("CODEMPRESA")]
        public int CodEmpresa { get; set; }

        [Required]
        [Column("CODMONEDA")]
        [StringLength(5)]
        public string CodMoneda { get; set; }

        [Required]
        [Column("TIPOMONEDA")]
        [StringLength(1)]
        public string TipoMoneda { get; set; }

        [Required]
        [Column("FECHA")]
        public DateTime Fecha { get; set; }

        [Required]
        [Column("CODUSUARIO")]
        [StringLength(15)]
        public string CodUsuario { get; set; }

        [Column("FECHAMODIFICA")]
        public DateTime? FechaModifica { get; set; }

        [Column("USUARIOMODIFICA")]
        [StringLength(15)]
        public string UsuarioModifica { get; set; }

        [Column("CODESTADO")]
        public int? CodEstado { get; set; }
    }

}
