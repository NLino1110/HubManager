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
    [Table("GENUNIDADESMEDIDA")]
    public class GenUnidadesMedida
    {
        [Key]
        [Required]
        [Column("CODUNIDADMEDIDA")]
        [StringLength(15)]
        public string CodUnidadMedida { get; set; }

        [Required]
        [Column("DESCRIPCION")]
        [StringLength(50)]
        public string Descripcion { get; set; }

        [Column("CODUNIDADMINIMA")]
        [StringLength(15)]
        public string CodUnidadMinima { get; set; }

        [Required]
        [Column("VALORMINIMO", TypeName = "NUMBER(16,4)")]
        public decimal ValorMinimo { get; set; }

        [Required]
        [Column("ESUNIDADMINIMA")]
        [StringLength(1)]
        public string EsUnidadMinima { get; set; }

        [Required]
        [Column("CODESTADO")]
        public int CodEstado { get; set; }

        [Column("USUARIOCAMBIOESTADO")]
        [StringLength(15)]
        public string UsuarioCambioEstado { get; set; }

        [Column("FECHACAMBIOESTADO")]
        public DateTime? FechaCambioEstado { get; set; }

        [Required]
        [Column("CODUSUARIO")]
        [StringLength(15)]
        public string CodUsuario { get; set; }

        [Required]
        [Column("FECHA")]
        public DateTime Fecha { get; set; }
    }

}
