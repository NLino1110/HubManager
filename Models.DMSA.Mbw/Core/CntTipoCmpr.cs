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

    [Table("CNTTIPOCMPR")]
    public class CntTipoCmpr
    {
        [Key]
        [Required]
        [Column("CODTIPOCMPR")]
        [StringLength(5)]
        public string CodTipoCmpr { get; set; }

        [Required]
        [Column("DESCRIPCION")]
        [StringLength(60)]
        public string Descripcion { get; set; }

        [Required]
        [Column("TIPOMVTO")]
        [StringLength(1)]
        public string TipoMvto { get; set; }

        [Required]
        [Column("USACNT")]
        [StringLength(1)]
        public string UsaCnt { get; set; }

        [Required]
        [Column("USABAN")]
        [StringLength(1)]
        public string UsaBan { get; set; }

        [Required]
        [Column("USACAJ")]
        [StringLength(1)]
        public string UsaCaj { get; set; }

        [Required]
        [Column("USACOM")]
        [StringLength(1)]
        public string UsaCom { get; set; }

        [Required]
        [Column("USAFAC")]
        [StringLength(1)]
        public string UsaFac { get; set; }

        [Required]
        [Column("USAINV")]
        [StringLength(1)]
        public string UsaInv { get; set; }

        [Required]
        [Column("USACXC")]
        [StringLength(1)]
        public string UsaCxc { get; set; }

        [Required]
        [Column("USACXP")]
        [StringLength(1)]
        public string UsaCxp { get; set; }

        [Required]
        [Column("AUTOMATICO")]
        [StringLength(1)]
        public string Automatico { get; set; }

        [Column("CODTIPOCMPRSRI")]
        [StringLength(5)]
        public string CodTipoCmprSri { get; set; }

        [Column("ESCMPRVTAMANUAL")]
        [StringLength(1)]
        public string EsCmprVtaManual { get; set; }

        [Required]
        [Column("ESAUTORIZADO")]
        [StringLength(1)]
        public string EsAutorizado { get; set; }

        [Required]
        [Column("CODESTADO")]
        public int CodEstado { get; set; }

        [Column("FECHACAMBIOESTADO")]
        public DateTime? FechaCambioEstado { get; set; }

        [Column("USUARIOCAMBIOESTADO")]
        [StringLength(15)]
        public string UsuarioCambioEstado { get; set; }

        [Required]
        [Column("CODUSUARIO")]
        [StringLength(15)]
        public string CodUsuario { get; set; }

        [Required]
        [Column("FECHA")]
        public DateTime Fecha { get; set; }
    }


}
