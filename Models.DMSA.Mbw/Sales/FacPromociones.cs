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

namespace Models.DMSA.Mbw.Sales
{  

    [Table("FACPROMOCIONES")]
    public class FacPromociones
    {
        [Required]
        [Column("CODEMPRESA")]
        public decimal CodEmpresa { get; set; }

        [Required]
        [Column("CODPROMOCION")]
        public decimal CodPromocion { get; set; }

        [Column("CODTIPOPROMOCION")]
        public decimal? CodTipoPromocion { get; set; }

        [Required]
        [Column("DESCRIPCION")]
        [StringLength(400)]
        public string Descripcion { get; set; }

        [Required]
        [Column("FECHADESDE")]
        public DateTime FechaDesde { get; set; }

        [Required]
        [Column("FECHAHASTA")]
        public DateTime FechaHasta { get; set; }

        [Column("AUTOMATICA")]
        [StringLength(1)]
        public string Automatica { get; set; }

        [Required]
        [Column("CODUSUARIO")]
        [StringLength(15)]
        public string CodUsuario { get; set; }

        [Required]
        [Column("FECHAREGISTRO")]
        public DateTime FechaRegistro { get; set; }

        [Column("USUARIOMODIFICA")]
        [StringLength(15)]
        public string UsuarioModifica { get; set; }

        [Column("FECHAMODIFICA")]
        public DateTime? FechaModifica { get; set; }

        [Required]
        [Column("CODESTADO")]
        public decimal CodEstado { get; set; }

        [Column("ESSOLOCNTROL")]
        [StringLength(1)]
        public string EsSoloControl { get; set; }

        [Column("CAMPOSCONTROL")]
        [StringLength(100)]
        public string CamposControl { get; set; }

        [Column("NOMBRE")]
        [StringLength(150)]
        public string Nombre { get; set; }

        [Column("TIPOPEDIDO")]
        [StringLength(3)]
        public string TipoPedido { get; set; }

        [Column("USUARIOAPROB")]
        [StringLength(15)]
        public string UsuarioAprob { get; set; }

        [Column("FECHAAPROB")]
        public DateTime? FechaAprob { get; set; }

        [Column("OBSERVACION")]
        [StringLength(200)]
        public string Observacion { get; set; }

        [Column("CLIENTESESPECIFICOS")]
        [StringLength(500)]
        public string ClientesEspecificos { get; set; }

        [Column("TIPODESCUENTO")]
        [StringLength(2)]
        public string TipoDescuento { get; set; }

        [Column("CLIENTESEXCLUIDOS")]
        [StringLength(650)]
        public string ClientesExcluidos { get; set; }

        [Column("PROMOCIONESSIMILARES", TypeName = "LONG")]
        public string PromocionesSimilares { get; set; }

        [Column("APLICADOXFACTURA")]
        public decimal? AplicadoXFactura { get; set; }
    }


    public class FacPromociones_ENUM
    {
        // Promociones
        public const long PROMO_BONIFICACIONES = 1L;
        public const long PROMO_SORTEOS = 2L;
        public const long PROMO_OBSEQUIOS = 3L;
        public const long PROMO_NXN = 4L;
        public const long PROMO_REBATES = 5L;
        public const long PROMO_DESCUENTOS = 6L;
        public const long PROMO_COMBOS = 7L;
        public const long PROMO_BONIFICACION_PARCIAL = 8L;

        // Tipos de Agrupación
        public const string AGR_CANAL_VENTA = "CV";
        public const string AGR_CLIENTE_APLICA = "CL";

        // Valores Totales
        public const string VALOR_TOTAL_PRODUCTO = "TPRO";
        public const string VALOR_TOTAL_MARCA = "TMAR";
        public const string VALOR_TOTAL_LINEA = "TLIN";
        public const string VALOR_TOTAL_SUBLINEA = "TSUB1";
        public const string VALOR_TOTAL_SUBLINEA2 = "TSUB2";
        public const string VALOR_TOTAL_GRUPO = "TGRU";
        public const string VALOR_TOTAL_USOAPLICAMAT = "TUSO";
        public const string VALOR_TOTAL_PEDIDO = "TPED";

        // Cantidades Totales
        public const string CANT_TOTAL_PRODUCTO = "CPRO";
        public const string CANT_TOTAL_MARCA = "CMAR";
        public const string CANT_TOTAL_LINEA = "CLIN";
        public const string CANT_TOTAL_SUBLINEA = "CSUB1";
        public const string CANT_TOTAL_SUBLINEA2 = "CSUB2";
        public const string CANT_TOTAL_GRUPO = "CGRU";
        public const string CANT_TOTAL_USOAPLICAMAT = "CUSO";

        // Descuentos
        public const string DESCUENTO_AUT_ARTICULO = "DA";
        public const string DESCUENTO_AUT_LINEA = "DL";
    }

}
