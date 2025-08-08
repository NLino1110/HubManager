using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Models.DMSA.Mbw.Inventario;

namespace Models.DMSA.Mbw.Sales
{ 

    [Table("FACRANGODESCUENTOLOTE")]
    public class FacRangoDescuentoLote
    {
        [Required]
        [Column("CODRANGOLOTE")]
        public decimal CodRangoLote { get; set; }

        [Required]
        [Column("CODEMPRESA")]
        public decimal CodEmpresa { get; set; }

        [Required]
        [Column("CODTIPOPROMOCION")]
        public decimal CodTipoPromocion { get; set; }

        [Required]
        [Column("DESCRIPCION")]
        [StringLength(300)]
        public string? Descripcion { get; set; }

        [Required]
        [Column("VALORINICIAL")]
        public decimal ValorInicial { get; set; }

        [Required]
        [Column("VALORFINAL")]
        public decimal ValorFinal { get; set; }

        [Required]
        [Column("PORCINICIAL")]
        public decimal PorcInicial { get; set; }

        [Required]
        [Column("PORCFINAL")]
        public decimal PorcFinal { get; set; }

        [Required]
        [Column("CODESTADO")]
        public decimal CodEstado { get; set; }

        [Required]
        [Column("CODUSUARIO")]
        [StringLength(15)]
        public string? CodUsuario { get; set; }

        [Required]
        [Column("FECHAREGISTRO")]
        public DateTime FechaRegistro { get; set; }

        [Column("USUARIOMODIFICA")]
        [StringLength(15)]
        public string? UsuarioModifica { get; set; }

        [Column("FECHAMODIFICA")]
        public DateTime? FechaModifica { get; set; }

        [Column("PORCINVERSION")]
        public decimal? PorcInversion { get; set; }
    }

}
