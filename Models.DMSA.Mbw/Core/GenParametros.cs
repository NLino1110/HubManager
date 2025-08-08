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

    [Table("GENPARAMETROS")]
    public class GenParametros
    {
        [Required]
        [Column("CODEMPRESA")]
        public int CodEmpresa { get; set; }

        [Required]
        [Column("CODPARAMETRO")]
        [StringLength(50)]
        public string? CodParametro { get; set; }

        [Required]
        [Column("DESCRIPCION")]
        [StringLength(100)]
        public string? Descripcion { get; set; }

        [Required]
        [Column("VALOR")]
        [StringLength(350)]
        public string? Valor { get; set; }

        [Column("CODSISTEMA")]
        [StringLength(3)]
        public string? CodSistema { get; set; }

        [Required]
        [Column("CODUSUARIO")]
        [StringLength(15)]
        public string? CodUsuario { get; set; }

        [Required]
        [Column("FECHA")]
        public DateTime Fecha { get; set; }

        [Column("MODIFICA")]
        [StringLength(1)]
        public string? Modifica { get; set; }
    }


}
