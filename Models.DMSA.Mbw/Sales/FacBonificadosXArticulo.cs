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
using Models.DMSA.Mbw.Inventario;

namespace Models.DMSA.Mbw.Sales
{  

    [Table("FACBONIFICADOSXARTICULO")]
    public class FacBonificadosXArticulo
    {
        [Key]
        [Required]
        [Column("CODBONIFICADOARTICULO")]
        public decimal CodBonificadoArticulo { get; set; }
                
        [Column("CODEMPRESA")]
        public int? CodEmpresa { get; set; }

        //[ForeignKey("CodArticulo, CodEmpresa")]
        [NotMapped]
        public ArticulosXEmpresa ArticulosXEmpresa { get; set; }

        [Column("CODAGENCIA")]
        public int CodAgencia { get; set; }
        
        [ForeignKey("CodAgencia")]
        public virtual GenAgencias GenAgencias { get; set; }

        [Column("CODTIPOCLIENTE")]
        public decimal? CodTipoCliente { get; set; }

        [Column("CODEMPRESANIVEL")]
        public decimal? CodEmpresaNivel { get; set; }

        [Column("CODNIVEL")]
        public int? CodNivel { get; set; }

        [ForeignKey(nameof(CodNivel))]
        public virtual FacNivelesPrecios FacNivelesPreciosFk { get; set; }

        [Column("CODARTICULO")]
        public int CodArticulo { get; set; }

        [ForeignKey("CodArticulo")]
        public virtual GenArticulos Articulo { get; set; }
                
        [Column("CODUNIDADMEDIDA")]
        [StringLength(15)]
        public string? CodUnidadMedida { get; set; }
                
        [ForeignKey(nameof(CodUnidadMedida))]
        public virtual GenUnidadesMedida GenUnidadesMedida { get; set; }


        [Column("FECHAINICIO")]
        public DateTime FechaInicio { get; set; }
                
        [Column("FECHAFIN")]
        public DateTime FechaFin { get; set; }
                
        [Column("MINIMOAPLICADSCTO")]
        public decimal MinimoAplicaDscto { get; set; }

        [Column("PORCDESCUENTO")]
        public decimal? PorcDescuento { get; set; }

        [Column("PRECIO")]
        public decimal? Precio { get; set; }

        [Column("VALORDESCUENTO")]
        public decimal? ValorDescuento { get; set; }

        [Required]
        [Column("CODUSUARIO")]
        [StringLength(15)]
        public string? CodUsuario { get; set; }

        [Required]
        [Column("FECHAINGRESO")]
        public DateTime FechaIngreso { get; set; }

        [Column("USUARIOMODIFICACION")]
        [StringLength(15)]
        public string? UsuarioModificacion { get; set; }

        [Column("FECHAMODIFICACION")]
        public DateTime? FechaModificacion { get; set; }

        
        //[Required]
        [Column("CODESTADO")]
        public int CodEstado { get; set; }
        [ForeignKey("CodEstado")]
        public virtual GenEstados GenEstados { get; set; }

        [Column("CODUSUARIOAPROBACION")]
        [StringLength(15)]
        public string? CodUsuarioAprobacion { get; set; }

        [Column("FECHAAPROBACION")]
        public DateTime? FechaAprobacion { get; set; }

        [Column("ESDESCUENTOMASTER")]
        [StringLength(1)]
        public string? EsDescuentoMaster { get; set; }
    }

}
