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

    [Table("FACCMPRVENTA")]
    public class Faccmprventa
    {
        [Required]
        [Column("CODAGENCIA")]
        public decimal CodAgencia { get; set; }

        [Required]
        [Column("CODTIPOCMPR")]
        [StringLength(5)]
        public string CodTipoCmpr { get; set; }

        [Required]
        [Column("NUMCMPRVENTA")]
        public decimal NumCmprVenta { get; set; }

        [Required]
        [Column("CODCLIENTE")]
        public decimal CodCliente { get; set; }

        [Column("NOMBRESCLIENTE")]
        [StringLength(100)]
        public string NombresCliente { get; set; }

        [Column("APELLIDOSCLIENTE")]
        [StringLength(100)]
        public string ApellidosCliente { get; set; }

        [Column("TIPOIDENTIFICACION")]
        [StringLength(1)]
        public string TipoIdentificacion { get; set; }

        [Column("IDENTIFICACION")]
        [StringLength(15)]
        public string Identificacion { get; set; }

        [Column("DIRECCION")]
        [StringLength(500)]
        public string Direccion { get; set; }

        [Column("TELEFONO")]
        [StringLength(50)]
        public string Telefono { get; set; }

        [Column("NUMDOCUMENTO")]
        [StringLength(20)]
        public string NumDocumento { get; set; }

        [Column("NUMDOCUMENTOAPLICA")]
        [StringLength(20)]
        public string NumDocumentoAplica { get; set; }

        [Required]
        [Column("CODEMPRESAVEND")]
        public decimal CodEmpresaVend { get; set; }

        [Required]
        [Column("CODVENDEDOR")]
        public decimal CodVendedor { get; set; }

        [Required]
        [Column("CODMONEDA")]
        [StringLength(5)]
        public string CodMoneda { get; set; }

        [Required]
        [Column("TASACAMBIO")]
        public decimal TasaCambio { get; set; }

        [Column("CODAGENCIAPEDIDO")]
        public decimal? CodAgenciaPedido { get; set; }

        [Column("TIPOPEDIDO")]
        [StringLength(3)]
        public string TipoPedido { get; set; }

        [Column("NUMPEDIDO")]
        public decimal? NumPedido { get; set; }

        [Required]
        [Column("TIPOPAGO")]
        [StringLength(3)]
        public string TipoPago { get; set; }

        [Column("OBSERVACIONES")]
        [StringLength(500)]
        public string Observaciones { get; set; }

        [Required]
        [Column("SUBTOTAL")]
        public decimal Subtotal { get; set; }

        [Required]
        [Column("DESCUENTO")]
        public decimal Descuento { get; set; }

        [Required]
        [Column("PORCIMPUESTO")]
        public decimal PorcImpuesto { get; set; }

        [Required]
        [Column("IMPUESTO")]
        public decimal Impuesto { get; set; }

        [Required]
        [Column("TOTAL")]
        public decimal Total { get; set; }

        [Required]
        [Column("FECHAREGISTRO")]
        public DateTime FechaRegistro { get; set; }

        [Column("FECHAMODIFICACION")]
        public DateTime? FechaModificacion { get; set; }

        [Column("CODEMPRESACNT")]
        public decimal? CodEmpresaCnt { get; set; }

        [Column("CODTIPOCMPRCNT")]
        [StringLength(5)]
        public string CodTipoCmprCnt { get; set; }

        [Column("NUMCMPRCNT")]
        public decimal? NumCmprCnt { get; set; }

        [Column("CODAGENCIACAJA")]
        public decimal? CodAgenciaCaja { get; set; }

        [Column("NUMCAJA")]
        public decimal? NumCaja { get; set; }

        [Column("NUMSESIONCAJA")]
        public decimal? NumSesionCaja { get; set; }

        [Required]
        [Column("IMPRESO")]
        public decimal Impreso { get; set; }

        [Required]
        [Column("AUTOMATICO")]
        [StringLength(1)]
        public string Automatico { get; set; }

        [Required]
        [Column("CODESTADO")]
        public decimal CodEstado { get; set; }

        [Column("USUARIOCAMBIOESTADO")]
        [StringLength(15)]
        public string UsuarioCambioEstado { get; set; }

        [Column("FECHACAMBIOESTADO")]
        public DateTime? FechaCambioEstado { get; set; }

        [Required]
        [Column("CODUSUARIO")]
        [StringLength(15)]
        public string CodUsuario { get; set; }

        [Column("FECHA")]
        public DateTime? Fecha { get; set; }

        [Required]
        [Column("USUARIOMODIFICA")]
        [StringLength(15)]
        public string UsuarioModifica { get; set; }

        [Column("ESDSCTOPORVALOR")]
        [StringLength(1)]
        public string EsDsctoPorValor { get; set; }

        [Column("CODAGENCIAAPLICA")]
        public decimal? CodAgenciaAplica { get; set; }

        [Column("CODTIPOCMPRAPLICA")]
        [StringLength(5)]
        public string CodTipoCmprAplica { get; set; }

        [Column("NUMCMPRVENTAAPLICA")]
        public decimal? NumCmprVentaAplica { get; set; }

        [Column("CONCEPTO")]
        [StringLength(1)]
        public string Concepto { get; set; }

        [Column("NUMAUTORIZACION")]
        [StringLength(15)]
        public string NumAutorizacion { get; set; }

        [Column("TIPONC")]
        public decimal? TipoNc { get; set; }

        [Column("MOTIVOANULACION")]
        [StringLength(200)]
        public string MotivoAnulacion { get; set; }

        [Column("CODFORMATO")]
        public decimal? CodFormato { get; set; }

        [Column("VALORSEGURO")]
        public decimal? ValorSeguro { get; set; }

        [Column("CODMPRESAVEND2")]
        public decimal? CodMpresaVend2 { get; set; }

        [Column("CODVENDEDOR2")]
        public decimal? CodVendedor2 { get; set; }

        [Column("SALDO")]
        public decimal? Saldo { get; set; }

        [Column("CODCIUDAD")]
        public decimal? CodCiudad { get; set; }

        [Column("CODTIPOCLIENTE")]
        public decimal? CodTipoCliente { get; set; }

        [Column("CODDIRECCION")]
        public decimal? CodDireccion { get; set; }

        [Column("NCVARIASFACTURAS")]
        [StringLength(1)]
        public string NcVariasFacturas { get; set; }

        [Column("CODTARJETA")]
        public decimal? CodTarjeta { get; set; }

        [Column("TASACAMBIOMONEDABASE")]
        public decimal? TasaCambioMonedaBase { get; set; }

        [Column("OBSERVACIONES2")]
        [StringLength(500)]
        public string Observaciones2 { get; set; }

        [Column("CNTREGENERADO")]
        [StringLength(1)]
        public string CntRegenerado { get; set; }

        [Column("TIPOAPLICANC")]
        [StringLength(5)]
        public string TipoAplicaNc { get; set; }

        [Column("TRANSFERIDOWMS")]
        [StringLength(1)]
        public string TransferidoWms { get; set; }

        [Column("ESCANJEXOC")]
        [StringLength(1)]
        public string EsCanjeXOc { get; set; }

        [Column("BASEOC")]
        public decimal? BaseOc { get; set; }

        [Column("EMAIL")]
        [StringLength(200)]
        public string Email { get; set; }

        [Column("PORCCOMPENSOLI")]
        public decimal? PorcCompensoli { get; set; }

        [Column("COMPENSOLI")]
        public decimal? Compensoli { get; set; }

        [Column("ESCMPRELECTRONICO")]
        [StringLength(1)]
        public string EsCmpreElectronico { get; set; }

        [Column("CODFORMAPAGOSRI")]
        [StringLength(2)]
        public string CodFormaPagoSri { get; set; }

        [Column("TIEMPOPLAZOSRI")]
        public decimal? TiempoPlazoSri { get; set; }

        [Column("APLICANC")]
        [StringLength(1)]
        public string AplicaNc { get; set; }

        [Column("CLIFIDELIZA")]
        [StringLength(1)]
        public string CliFideliza { get; set; }

        [Column("ESSERVICIO")]
        [StringLength(1)]
        public string EsServicio { get; set; }

        [Column("BASEOCTXT")]
        [StringLength(250)]
        public string BaseOcTxt { get; set; }

        [Column("ORDENCOMPRA")]
        [StringLength(100)]
        public string OrdenCompra { get; set; }

        [Column("IDDEVOLUCION")]
        [StringLength(100)]
        public string IdDevolucion { get; set; }

        [Column("PREENTRADA")]
        [StringLength(100)]
        public string PreEntrada { get; set; }

        [Column("ESREIMPRESIONFACTURA")]
        [StringLength(1)]
        public string EsReimpresionFactura { get; set; }

        [Column("TIPOND")]
        public decimal? TipoNd { get; set; }

        [Column("CODEMPRESAETAPA")]
        public decimal? CodEmpresaEtapa { get; set; }

        [Column("CODETAPA")]
        public decimal? CodEtapa { get; set; }

        [Column("CODBODEGAAGENCIANC")]
        public decimal? CodBodegaAgenciaNc { get; set; }

        [Column("TIEMPOCREDITO")]
        public decimal? TiempoCredito { get; set; }

        [Column("DIASCREDITCLIENTE")]
        public decimal? DiasCreditoCliente { get; set; }

        [Column("TIPONCDEVALMACEN")]
        [StringLength(1)]
        public string TipoNcDevAlmacen { get; set; }

        [Column("FECHAMODIFICACIONNC")]
        public DateTime? FechaModificacionNc { get; set; }

        [Column("ESVALIDSRIMANUAL")]
        [StringLength(1)]
        public string EsValidSriManual { get; set; }

        [Column("TIEMPOCREDITOAPROB")]
        public decimal? TiempoCreditoAprob { get; set; }

        [Column("TIPOCREDITO")]
        [StringLength(2)]
        public string TipoCredito { get; set; }

        [Column("ESREEMBOLSO")]
        [StringLength(1)]
        public string EsReembolso { get; set; }

        [Column("SEXO")]
        [StringLength(1)]
        public string Sexo { get; set; }

        [Column("GANADMUJER")]
        [StringLength(1)]
        public string GanadMujer { get; set; }

        [Column("GANADPADRETAURUS")]
        [StringLength(1)]
        public string GanadPadreTaurus { get; set; }

        [Column("GANADPADREPLAY")]
        [StringLength(3)]
        public string GanadPadrePlay { get; set; }

        [Column("CODPROMOCION")]
        public decimal? CodPromocion { get; set; }

        [Column("SORTEOCUPON")]
        public decimal? SorteoCupon { get; set; }
    }

}
