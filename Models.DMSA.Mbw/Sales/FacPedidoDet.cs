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
using Models.DMSA.Shared;
using Models.DMSA.Shared.Structs;

namespace Models.DMSA.Mbw.Sales
{
    public class ApiResponseFacPedidoDet : ApiResponseGlobal
    {
        public FacPedidoDet[] data { get; set; }
    }

    [Table("FACPEDIDODET")]
    public class FacPedidoDet
    {
        [Column("CODAGENCIA")]
        public long CodAgencia { get; set; }

        [Column("TIPOPEDIDO")]
        [StringLength(3)]
        public string? TipoPedido { get; set; }

        [Column("NUMPEDIDO")]
        public long NumPedido { get; set; }

        [Column("NUMPEDIDODET")]
        public long NumPedidoDet { get; set; }

        [Column("CODARTICULO")]
        public long CodArticulo { get; set; }

        [Column("CODUNIDADMEDIDABASE")]
        [StringLength(15)]
        public string? CodUnidadMedidaBase { get; set; }

        [Column("CODUNIDADMEDIDA")]
        [StringLength(15)]
        public string? CodUnidadMedida { get; set; }

        [Column("CODBODEGAAGENCIA")]
        public long? CodBodegaAgencia { get; set; }

        [Column("CANTIDAD", TypeName = "decimal(16,4)")]
        public decimal Cantidad { get; set; }

        [Column("CANTIDADENTREGADA", TypeName = "decimal(16,4)")]
        public decimal CantidadEntregada { get; set; }

        [Column("CANTIDADFACTURADA", TypeName = "decimal(16,4)")]
        public decimal CantidadFacturada { get; set; }

        [Column("PRECIO", TypeName = "decimal(16,4)")]
        public decimal Precio { get; set; }

        [Column("SUBTOTAL", TypeName = "decimal(16,4)")]
        public decimal Subtotal { get; set; }

        [Column("DESCUENTO", TypeName = "decimal(16,4)")]
        public decimal Descuento { get; set; }

        [Column("PORCDESCUENTO", TypeName = "decimal(16,4)")]
        public decimal PorcDescuento { get; set; }

        [Column("DESCUENTOADIC", TypeName = "decimal(16,4)")]
        public decimal DescuentoAdicional { get; set; }

        [Column("PORCDESCUENTOADIC", TypeName = "decimal(16,4)")]
        public decimal PorcDescuentoAdicional { get; set; }

        [Column("IMPUESTO", TypeName = "decimal(16,4)")]
        public decimal Impuesto { get; set; }

        [Column("PORCIMPUESTO", TypeName = "decimal(16,4)")]
        public decimal PorcImpuesto { get; set; }

        [Column("TOTAL", TypeName = "decimal(16,4)")]
        public decimal Total { get; set; }

        [Column("ESPREMIOOPROMOCION")]
        [StringLength(1)]
        public string? EsPremioOPromocion { get; set; }

        [Column("CODUNIDADMEDIDAREACT")]
        [StringLength(15)]
        public string? CodUnidadMedidaReact { get; set; }

        [Column("VALORRECARGO", TypeName = "decimal(16,4)")]
        public decimal? ValorRecargo { get; set; }

        [Column("CODNIVEL")]
        public long? CodNivel { get; set; }

        [Column("CODEMPRESA")]
        public long? CodEmpresa { get; set; }

        [Column("PRECIOFINAL", TypeName = "decimal(16,4)")]
        public decimal? PrecioFinal { get; set; }

        [Column("TRANSPORTE", TypeName = "decimal(16,4)")]
        public decimal? Transporte { get; set; }

        [Column("ESDESCUENTOITEM")]
        [StringLength(1)]
        public string? EsDescuentoItem { get; set; }

        [Column("PRECIOORIGINAL", TypeName = "decimal(16,4)")]
        public decimal? PrecioOriginal { get; set; }

        [Column("ESCAMBIOPRECIO")]
        [StringLength(1)]
        public string? EsCambioPrecio { get; set; }

        [Column("TIPODESCUENTO")]
        [StringLength(1)]
        public string? TipoDescuento { get; set; }

        [Column("MINIMOAPLICADSCTO", TypeName = "decimal(16,4)")]
        public decimal? MinimoAplicadoDescuento { get; set; }

        [Column("BLOQUEADO")]
        [StringLength(1)]
        public string? Bloqueado { get; set; }

        [Column("CODEMPRESAPROMOCION")]
        public long? CodEmpresaPromocion { get; set; }

        [Column("CODPROMOCION")]
        public long? CodPromocion { get; set; }

        [Column("PORCCOMPENSOLI", TypeName = "decimal(16,4)")]
        public decimal? PorcCompensacionSOLI { get; set; }

        [Column("COMPENSOLI", TypeName = "decimal(16,4)")]
        public decimal? CompensacionSOLI { get; set; }

        [Column("CANTIDADRESPALDOWMS", TypeName = "decimal(16,4)")]
        public decimal? CantidadRespaldoWMS { get; set; }

        [Column("CANTIDADFINALAPROB", TypeName = "decimal(16,4)")]
        public decimal? CantidadFinalAprob { get; set; }

        [Column("ESRESPALDOPK")]
        [StringLength(1)]
        public string? EsRespaldoPK { get; set; }

        [Column("TIPOAPLICA")]
        [StringLength(1)]
        public string? TipoAplica { get; set; }

        [Column("CADENAAPLICA")]
        [StringLength(500)]
        public string? CadenaAplica { get; set; }

        [Column("CANTIDADSOLICITADA", TypeName = "decimal(16,4)")]
        public decimal? CantidadSolicitada { get; set; }

        [Column("STOCKAPROBACION", TypeName = "decimal(16,4)")]
        public decimal? StockAprobacion { get; set; }

        [Column("CADENALOTES")]
        [StringLength(2000)]
        public string? CadenaLotes { get; set; }

        [Column("PORCDESCUENTORSP", TypeName = "decimal(16,4)")]
        public decimal? PorcDescuentoRSP { get; set; }

        [Column("ESDESCUENTOLOTE")]
        [StringLength(1)]
        public string? EsDescuentoLote { get; set; }

        [Column("CODRANGOLOTE")]
        public long? CodRangoLote { get; set; }

        [Column("CODEMPRESARANGOLOTE")]
        public long? CodEmpresaRangoLote { get; set; }

        [Column("TIPODESCUENTOAPLICADO")]
        [StringLength(2)]
        public string? TipoDescuentoAplicado { get; set; }

        [Column("CODBONIFICADOARTICULO")]
        public long? CodBonificadoArticulo { get; set; }

        [Column("CODBONIFICADOLINEA")]
        public long? CodBonificadoLinea { get; set; }

        [Column("ESPRECIOESPECIAL")]
        [StringLength(1)]
        public string? EsPrecioEspecial { get; set; }

        [Column("ISCODIGOEAN128")]
        [StringLength(1)]
        public string? IsCodigoEAN128 { get; set; }
                
    }
}
