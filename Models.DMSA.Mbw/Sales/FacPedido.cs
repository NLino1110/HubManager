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
using Models.DMSA.Shared;
using Models.DMSA.Shared.Structs;

namespace Models.DMSA.Mbw.Sales
{
    public class ApiResponseFacPedido: ApiResponseGlobal
    {
        public FacPedido[] data { get; set; }
    }

    [Table("FACPEDIDO")]
    public class FacPedido
    {        
        [Column("CODAGENCIA")]
        public int CodAgencia { get; set; }

        [Required]
        [StringLength(3)]
        [Column("TIPOPEDIDO")]
        public string TipoPedido { get; set; }

        [Required]
        [Column("NUMPEDIDO")]
        public int NumPedido { get; set; }

        [Required]
        [Column("CODCLIENTE")]
        public int CodCliente { get; set; }

        [Required]
        [StringLength(100)]
        [Column("NOMBRESCLIENTE")]
        public string? NombresCliente { get; set; }

        [StringLength(100)]
        [Column("APELLIDOSCLIENTE")]
        public string? ApellidosCliente { get; set; }

        [Required]
        [StringLength(1)]
        [Column("TIPOIDENTIFICACION")]
        public string? TipoIdentificacion { get; set; }

        [Required]
        [StringLength(15)]
        [Column("IDENTIFICACION")]
        public string Identificacion { get; set; }

        [StringLength(500)]
        [Column("DIRECCIONCLIENTE")]
        public string? DireccionCliente { get; set; }

        [StringLength(50)]
        [Column("TELEFONOCLIENTE")]
        public string? TelefonoCliente { get; set; }

        [Required]
        [StringLength(5)]
        [Column("CMPRAFACTURAR")]
        public string? CmpraFacturar { get; set; }

        [StringLength(500)]
        [Column("OBSERVACIONES")]
        public string? Observaciones { get; set; }

        [Required]
        [Column("CODEMPRESA")]
        public int CodEmpresa { get; set; }

        [Required]
        [Column("CODVENDEDOR")]
        public int CodVendedor { get; set; }

        [Required]
        [StringLength(3)]
        [Column("TIPOPAGO")]
        public string TipoPago { get; set; }

        [Column("CODEMPLEADO")]
        public int? CodEmpleado { get; set; }

        [Column("NUMPEDIDOPENDIENTE")]
        public int? NumPedidoPendiente { get; set; }

        [Required]
        [StringLength(5)]
        [Column("CODMONEDA")]
        public string CodMoneda { get; set; }

        [Required]
        [Column("TASACAMBIO", TypeName = "decimal(16,4)")]
        public decimal TasaCambio { get; set; }

        [Required]
        [Column("SUBTOTAL", TypeName = "decimal(16,4)")]
        public decimal Subtotal { get; set; }

        [Required]
        [Column("DESCUENTO", TypeName = "decimal(16,4)")]
        public decimal Descuento { get; set; }

        [Required]
        [Column("DESCUENTOADIC", TypeName = "decimal(16,4)")]
        public decimal DescuentoAdicional { get; set; }

        [Required]
        [Column("PORCDESCUENTOADIC", TypeName = "decimal(16,4)")]
        public decimal PorcDescuentoAdicional { get; set; }

        [Required]
        [Column("PORCIMPUESTO", TypeName = "decimal(16,4)")]
        public decimal PorcImpuesto { get; set; }

        [Required]
        [Column("IMPUESTO", TypeName = "decimal(16,4)")]
        public decimal Impuesto { get; set; }

        [Required]
        [Column("TOTAL", TypeName = "decimal(16,4)")]
        public decimal Total { get; set; }

        [Column("PORCRECARGO", TypeName = "decimal(16,4)")]
        public decimal? PorcRecargo { get; set; }

        [Required]
        [Column("FECHAREGISTRO")]
        public DateTime FechaRegistro { get; set; }

        [Column("FECHAMODIFICACION")]
        public DateTime? FechaModificacion { get; set; }

        [Column("FECHAAPROBACION")]
        public DateTime? FechaAprobacion { get; set; }

        [Required]
        [Column("CODESTADO")]
        public int CodEstado { get; set; }

        [StringLength(15)]
        [Column("USUARIOCAMBIOESTADO")]
        public string? UsuarioCambioEstado { get; set; }

        [Column("FECHACAMBIOESTADO")]
        public DateTime? FechaCambioEstado { get; set; }
                
        [StringLength(15)]
        [Column("CODUSUARIO")]
        public string? CodUsuario { get; set; }

        [Column("CODAGENCIAREF")]
        public int? CodAgenciaRef { get; set; }

        [StringLength(3)]
        [Column("TIPOPEDIDOREF")]
        public string? TipoPedidoRef { get; set; }

        [Column("NUMPEDIDOREF")]
        public int? NumPedidoRef { get; set; }

        [StringLength(1)]
        [Column("RESPONSABLEENVIO")]
        public string? ResponsableEnvio { get; set; }

        [Column("CODPROVEEDOR")]
        public int? CodProveedor { get; set; }

        [StringLength(1)]
        [Column("ESDSCTOPORVALOR")]
        public string? EsDscPorValor { get; set; }

        [StringLength(1)]
        [Column("AUTORIZADO")]
        public string? Autorizado { get; set; }

        [StringLength(15)]
        [Column("USUARIOMODIFICA")]
        public string? UsuarioModifica { get; set; }

        [StringLength(200)]
        [Column("OBSERVACIONESRESTRICCION")]
        public string? ObservacionesRestriccion { get; set; }

        [StringLength(200)]
        [Column("OBSERVACIONESAPROBACION")]
        public string? ObservacionesAprobacion { get; set; }

        [Column("PORCTRANSPORTE", TypeName = "decimal(16,4)")]
        public decimal? PorcTransporte { get; set; }

        [Column("TRANSPORTE", TypeName = "decimal(16,4)")]
        public decimal? Transporte { get; set; }

        [StringLength(3)]
        [Column("TIPO")]
        public string? Tipo { get; set; }

        [StringLength(1)]
        [Column("PRIORIDAD")]
        public string? Prioridad { get; set; }

        [Column("CODBODEGAAGENCIA")]
        public int? CodBodegaAgencia { get; set; }

        [Column("NUMLINEAS")]
        public int? NumLineas { get; set; }

        [Column("CODFORMATO")]
        public int? CodFormato { get; set; }

        [Column("CODMPRESAVEND2")]
        public int? CodMpresaVend2 { get; set; }

        [Column("CODVENDEDOR2")]
        public int? CodVendedor2 { get; set; }

        [StringLength(20)]
        [Column("NUMPEDIDOINTERNO")]
        public string? NumPedidoInterno { get; set; }

        [Column("FECHATOMAPEDIDO")]
        public DateTime? FechaTomaPedido { get; set; }

        [Column("CODBODEGAAGENCIAORI")]
        public int? CodBodegaAgenciaOri { get; set; }

        [StringLength(10)]
        [Column("TIPOORIGEN")]
        public string? TipoOrigen { get; set; }

        [StringLength(15)]
        [Column("USUARIOPERMISODESCTO")]
        public string? UsuarioPermisoDescto { get; set; }

        [StringLength(15)]
        [Column("USUARIOPERMISOPRECIO")]
        public string? UsuarioPermisoPrecio { get; set; }

        [Column("CODTIPOCLIENTE")]
        public int? CodTipoCliente { get; set; }

        [Column("CODDIRECCION")]
        public int? CodDireccion { get; set; }

        [Column("CODCIUDAD")]
        public int? CodCiudad { get; set; }

        [StringLength(1)]
        [Column("PUBLICIDAD")]
        public string? Publicidad { get; set; }

        [Column("CODTARJETA")]
        public int? CodTarjeta { get; set; }

        [StringLength(200)]
        [Column("MOTIVOANULACION")]
        public string? MotivoAnulacion { get; set; }

        [StringLength(15)]
        [Column("USUARIOAPRUEBA")]
        public string? UsuarioAprueba { get; set; }

        [Column("FECHARESTRICCION")]
        public DateTime? FechaRestriccion { get; set; }

        [StringLength(15)]
        [Column("USUARIORESTRICCION")]
        public string? UsuarioRestriccion { get; set; }

        [Column("FECHAFACTURACION")]
        public DateTime? FechaFacturacion { get; set; }

        [StringLength(15)]
        [Column("USUARIOFACTURA")]
        public string? UsuarioFactura { get; set; }
               
       
        [Column("FECHAANULACION")]
        public DateTime? FechaAnulacion { get; set; }

        [StringLength(15)]
        [Column("USUARIOANULA")]
        public string? UsuarioAnulacion { get; set; }

        [StringLength(100)]
        [Column("OBSERVACIONES2")]
        public string? Observaciones2 { get; set; }

        [Column("PORCCOMPENSOLI", TypeName = "decimal(16,4)")]
        public decimal? PorcCompensoli { get; set; }

        [Column("COMPENSOLI", TypeName = "decimal(16,4)")]
        public decimal? Compensoli { get; set; }

        [Column("FECHAREGISTROPMS")]
        public DateTime? FechaRegistroPMS { get; set; }

        [Column("FECHAAPROBACIONPMS")]
        public DateTime? FechaAprobacionPMS { get; set; }

        [StringLength(1)]
        [Column("ESSERVICIO")]
        public string EsServicio { get; set; }

        [Column("CODEMPRESAETAPA")]
        public int? CodEmpresaEtapa { get; set; }

        [Column("CODETAPA")]
        public int? CodEtapa { get; set; }

        [StringLength(1)]
        [Column("REVERSAWMS")]
        public string? ReversaWMS { get; set; }

        [StringLength(100)]
        [Column("ORDENCOMPRA")]
        public string? OrdenCompra { get; set; }

        [StringLength(100)]
        [Column("IDDEVOLUCION")]
        public string? IdDevolucion { get; set; }

        [StringLength(100)]
        [Column("PREENTRADA")]
        public string? PreEntrada { get; set; }

        [StringLength(1)]
        [Column("ESREIMPRESIONFACTURA")]
        public string? EsReimpresionFactura { get; set; }

        [StringLength(20)]
        [Column("REFERENCIANOTACREDITO")]
        public string? ReferenciaNotaCredito { get; set; }

        [StringLength(200)]
        [Column("EMAIL")]
        public string? Email { get; set; }

        [Column("INTENTOSAP")]
        public int? IntentosAP { get; set; }

        [StringLength(1)]
        [Column("REPORTEAP")]
        public string? ReporteAP { get; set; }

        [StringLength(3)]
        [Column("CANALORIGENECOMM")]
        public string? CanalOrigenEcomm { get; set; }

        [Column("CODCOMPRAECOMM")]
        public int? CodCompraEcomm { get; set; }

        [Column("CODTARJETAFP")]
        public int? CodTarjetaFP { get; set; }

        [Column("MONTOMINIMOTJ", TypeName = "decimal(16,4)")]
        public decimal? MontoMinimoTJ { get; set; }

        [StringLength(1)]
        [Column("BLOQUEAPROMOTJ")]
        public string? BloqueAPromoTJ { get; set; }

        [StringLength(20)]
        [Column("NUMDOCUMENTOWEB")]
        public string? NumDocumentoWeb { get; set; }

        [Column("CODAGENCIASERV")]
        public int? CodAgenciaServ { get; set; }

        [StringLength(3)]
        [Column("TIPOPEDIDOSERV")]
        public string? TipoPedidoServ { get; set; }

        [Column("NUMPEDIDOSERV")]
        public int? NumPedidoServ { get; set; }

        [StringLength(100)]
        [Column("CODAUTHPAGOTJ")]
        public string? CodAuthPagoTJ { get; set; }

        [Column("CODBIN")]
        public int? CodBin { get; set; }

        [StringLength(200)]
        [Column("DESCTJC")]
        public string? DesctJC { get; set; }

        [StringLength(200)]
        [Column("DESCBCO")]
        public string? DescBCO { get; set; }

        [StringLength(1)]
        [Column("ESEMPLEADO")]
        public string? EsEmpleado { get; set; }

        [StringLength(1)]
        [Column("ESDESCUENTOROL")]
        public string? EsDescuentoRol { get; set; }

        [Column("MONTONCPROMO", TypeName = "decimal(16,4)")]
        public decimal? MontoNCPromo { get; set; }

        [StringLength(1)]
        [Column("APLICANCPROMO")]
        public string? AplicarNCPromo { get; set; }

        [StringLength(1)]
        [Column("ESAPROBLIBERA")]
        public string? EsAproblibera { get; set; }

        [StringLength(200)]
        [Column("COMENTAPROBLIBERA")]
        public string? ComentAproblibera { get; set; }

        [StringLength(15)]
        [Column("USUARIOAPROBLIBERA")]
        public string? UsuarioAproblibera { get; set; }

        [Column("FECHAAPROBLIBERA")]
        public DateTime? FechaAproblibera { get; set; }

        [Column("TMPTOTAL", TypeName = "decimal(16,4)")]
        public decimal? TmpTotal { get; set; }

        [StringLength(500)]
        [Column("DIRECCIONCLIENTEENVIO")]
        public string? DireccionClienteEnvio { get; set; }

        [Column("CODCIUDADENVIO")]
        public int? CodCiudadEnvio { get; set; }

        [StringLength(1)]
        [Column("VALIDAPROMOCIONREVERSA")]
        public string? ValidaPromocionReversa { get; set; }

        [Column("CODESTADOECOMM")]
        public int? CodEstadoEcomm { get; set; }

        [StringLength(1)]
        [Column("ESVALIDSRIMANUAL")]
        public string? EsValidSRIManual { get; set; }

        [StringLength(25)]
        [Column("PLATAFORMAORIGEN")]
        public string? PlataformaOrigen { get; set; }

        [StringLength(25)]
        [Column("PLATAFORMAMODIFICA")]
        public string? PlataformaModifica { get; set; }

        [StringLength(50)]
        [Column("MARCAEQUIPO")]
        public string? MarcaEquipo { get; set; }

        [StringLength(50)]
        [Column("MODELOEQUIPO")]
        public string? ModeloEquipo { get; set; }

        [StringLength(25)]
        [Column("VERSIONAPLICACION")]
        public string? VersionAplicacion { get; set; }

        [Column("TIEMPOCREDITO")]
        public int? TiempoCredito { get; set; }

        [StringLength(1)]
        [Column("ISPROFORMAWHATSAPP")]
        public string? IsProformaWhatsapp { get; set; }

        [StringLength(1)]
        [Column("ISPROFORMAWHATSAPPROL")]
        public string? IsProformaWhatsappRol { get; set; }

        [StringLength(1)]
        [Column("ESCONTRAENTREGA")]
        public string? EsContraEntrega { get; set; }

        [Column("CODPROMOCION")]
        public int? CodPromocion { get; set; }

        [NotMapped]
        public List<FacPedidoDet> detalle_productos { get; set; }
    }
}
