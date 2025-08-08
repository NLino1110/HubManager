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
    [Table("GENEMPRESAS")]
    public class GenEmpresa
    {
        [Key]
        [Column("CODEMPRESA")]
        public int CodEmpresa { get; set; }

        [Required]
        [Column("NOMBRE")]
        [MaxLength(50)]
        public string Nombre { get; set; }

        [Required]
        [Column("GERENTE")]
        [MaxLength(50)]
        public string Gerente { get; set; }

        [Required]
        [Column("RUCCONTADOR")]
        [MaxLength(20)]
        public string RucContador { get; set; }

        [Column("TIPOIDREPLEGAL")]
        [MaxLength(1)]
        public string? TipoIdRepLegal { get; set; }

        [Column("IDREPLEGAL")]
        [MaxLength(20)]
        public string? IdRepLegal { get; set; }

        [Column("LOGO")]
        [MaxLength(50)]
        public string? Logo { get; set; }

        [Column("DEFINICIONLOGO")]
        [MaxLength(50)]
        public string? DefinicionLogo { get; set; }

        [Required]
        [Column("CODMETODOCOSTEO")]
        public decimal CodMetodoCosteo { get; set; }

        [Required]
        [Column("CODUSUARIO")]
        [MaxLength(15)]
        public string CodUsuario { get; set; }

        [Required]
        [Column("FECHA")]
        public DateTime Fecha { get; set; }

        [Required]
        [Column("CODPAIS")]
        public int CodPais { get; set; }

        [Column("CODIGORDC")]
        [MaxLength(7)]
        public string? CodigoRdc { get; set; }

        [Column("HOSTCORREO")]
        [MaxLength(50)]
        public string? HostCorreo { get; set; }

        [Column("RUCEMPRESA")]
        [MaxLength(20)]
        public string? RucEmpresa { get; set; }

        [Column("TIPOIDEMPRESA")]
        [MaxLength(1)]
        public string? TipoIdEmpresa { get; set; }

        [Column("PERSONERIA")]
        [MaxLength(1)]
        public string? Personeria { get; set; }

        [Column("ESCONTRIBUYENTEESPECIAL")]
        [MaxLength(1)]
        public string? EsContribuyenteEspecial { get; set; }

        [Column("NUMRESOLUCION")]
        [MaxLength(15)]
        public string? NumResolucion { get; set; }

        [Required]
        [Column("LINEAGRUPOEMPRESARIAL")]
        public int LineaGrupoEmpresarial { get; set; }
    }
}
