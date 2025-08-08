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

namespace Models.DMSA.Mbw.Inventario
{
    [Table("ARTICULOSXEMPRESA")]
    public class ArticulosXEmpresa
    {
        [Key]
        [Column("CODARTICULOEMPRESA")]
        public int CodArticuloEmpresa { get; set; }

        
        [Column("CODEMPRESA", Order = 1)]
        public int CodEmpresa { get; set; }

        [ForeignKey("CodEmpresa")]
        public virtual GenEmpresa Empresa { get; set; }

        [Column("CODARTICULO",Order = 0)]        
        public int CodArticulo { get; set; }

        [ForeignKey("CodArticulo")]
        public virtual GenArticulos Articulo { get; set; }

        [Column("STOCKMINIMO", TypeName = "decimal(16,4)")]
        public decimal StockMinimo { get; set; }

        [Column("SEINVENTARIA", TypeName = "char(1)")]
        public string SeInventaria { get; set; }

        [Column("VENTAPEDIDOS", TypeName = "char(1)")]
        public string VentaPedidos { get; set; }

        [Column("VENTAALMACENES", TypeName = "char(1)")]
        public string VentaAlmacenes { get; set; }

        [Column("COMPRABLE", TypeName = "char(1)")]
        public string Comprable { get; set; }

        [Column("VENDIBLE", TypeName = "char(1)")]
        public string Vendible { get; set; }

        [Column("CODMOTIVOARTICULO")]
        public int? CodMotivoArticulo { get; set; }

        [Column("PRECIOCOMPRA", TypeName = "decimal(14,6)")]
        public decimal PrecioCompra { get; set; }

        [Column("PORCDSCTOCOMPRA", TypeName = "decimal(5,2)")]
        public decimal PorcDsctoCompra { get; set; }

        [Column("PORCDSCTOVENTA", TypeName = "decimal(5,2)")]
        public decimal? PorcDsctoVenta { get; set; }

        [Column("COSTOEMPRESA", TypeName = "decimal(16,4)")]
        public decimal CostoEmpresa { get; set; }

        [Column("STOCKEMPRESA", TypeName = "decimal(16,4)")]
        public decimal? StockEmpresa { get; set; }

        [ForeignKey("EmpresaCta")]
        [Column("CODEMPRESACTA")]
        public int? CodEmpresaCta { get; set; }

        [Column("NUMCUENTA", TypeName = "varchar(15)")]
        public string? NumCuenta { get; set; }

        [ForeignKey("Estado")]
        [Column("CODESTADO")]
        public int CodEstado { get; set; }

        [Column("USUARIOCAMBIOESTADO", TypeName = "varchar(15)")]
        public string? UsuarioCambioEstado { get; set; }

        [Column("FECHACAMBIOESTADO")]
        public DateTime? FechaCambioEstado { get; set; }

        [Column("NCPORVALOR", TypeName = "char(1)")]
        public string? NcPorValor { get; set; }

        [Column("INCLUYEIVAVENTAS", TypeName = "varchar(1)")]
        public string? IncluyeIvaVentas { get; set; }

        [ForeignKey("EmpresaAgrupacion")]
        [Column("CODEMPRESAAGRUPACION")]
        public int? CodEmpresaAgrupacion { get; set; }

        [ForeignKey("Agrupacion")]
        [Column("CODAGRUPACION")]
        public int? CodAgrupacion { get; set; }

        [Column("IMPORTADO", TypeName = "varchar(1)")]
        public string? Importado { get; set; }

        [Column("CALCULAMINMAX", TypeName = "varchar(1)")]
        public string? CalculaMinMax { get; set; }

        [Column("PORCDSCTOMAXIMO", TypeName = "decimal(5,2)")]
        public decimal? PorcDsctoMaximo { get; set; }

        [Column("LIQUIDAR", TypeName = "varchar(1)")]
        public string? Liquidar { get; set; }

        //[ForeignKey("Marca")]
        [Column("CODEMPRESAMARCA")]
        public int? CodEmpresaMarca { get; set; }

        [Column("CODMARCA")]
        public int? CodMarca { get; set; }

        [ForeignKey("CodEmpresaMarca, CodMarca")]
        public virtual GenMarca Marca { get; set; }

        [ForeignKey("Linea")]
        [Column("CODEMPRESALINEA")]
        public int? CodEmpresaLinea { get; set; }

        [Column("CODLINEA")]
        public int? CodLinea { get; set; }

        [ForeignKey("Sublinea")]
        [Column("CODEMPRESASUBLINEA")]
        public int? CodEmpresaSublinea { get; set; }

        [Column("CODSUBLINEA")]
        public int? CodSublinea { get; set; }

        [ForeignKey("SublineaDos")]
        [Column("CODEMPRESASUBLINEADOS")]
        public int? CodEmpresaSublineaDos { get; set; }

        [Column("CODSUBLINEADOS")]
        public int? CodSublineaDos { get; set; }

        [ForeignKey("GrupoTipo")]
        [Column("CODEMPRESAGRUPOTIPO")]
        public int? CodEmpresaGrupoTipo { get; set; }

        [Column("CODGRUPOTIPO")]
        public int? CodGrupoTipo { get; set; }

        [ForeignKey("UsoAplicacionMat")]
        [Column("CODEMPRESAUSOAPLICAMAT")]
        public int? CodEmpresaUsoAplicacionMat { get; set; }

        [Column("CODUSOAPLICAMAT")]
        public int? CodUsoAplicacionMat { get; set; }

        [Column("PMS_TIPOPRODUCTO", TypeName = "varchar(5)")]
        public string? PmsTipoProducto { get; set; }

        [Column("PMS_RESURTIDO", TypeName = "char(1)")]
        public string? PmsResurtido { get; set; }

        [Column("PMS_REABASTO", TypeName = "char(1)")]
        public string? PmsReabasto { get; set; }

        [Column("PMS_PARAMETROS", TypeName = "varchar(5)")]
        public string? PmsParametros { get; set; }

        [Column("PMS_ESREEMPLAZO", TypeName = "char(1)")]
        public string? PmsEsReemplazo { get; set; }

        [Column("PMS_FECHAALTA")]
        public DateTime? PmsFechaAlta { get; set; }

        [Column("PMS_FECHAVIGENCIA")]
        public DateTime? PmsFechaVigencia { get; set; }

        [Column("PMS_VIGENCIAMESES")]
        public int? PmsVigenciaMeses { get; set; }

        [Column("PMS_CODARTICULOSUST")]
        public int? PmsCodArticuloSust { get; set; }

        [Column("PMS_CODARTICULOANT")]
        public int? PmsCodArticuloAnt { get; set; }

        [Column("PMS_ESKIT", TypeName = "char(1)")]
        public string? PmsEsKit { get; set; }

        [Column("ULTIMACOMPRA", TypeName = "decimal(16,4)")]
        public decimal? UltimaCompra { get; set; }

        [Column("FECHAULTIMACOMPRA")]
        public DateTime? FechaUltimaCompra { get; set; }

        [Column("REMATE", TypeName = "varchar(1)")]
        public string? Remate { get; set; }

        //[Column("ABC", TypeName = "varchar(1)")]
        //public string Abc { get; set; }

        [Column("CODPARTIDA")]
        public int? CodPartida { get; set; }

        [Column("PRECIO_PROV_ORIGEN", TypeName = "decimal(14,6)")]
        public decimal? PrecioProvOrigen { get; set; }

        [Column("MONEDA_ORIGEN", TypeName = "varchar(15)")]
        public string? MonedaOrigen { get; set; }

        [Column("INCOTERM", TypeName = "varchar(5)")]
        public string? Incoterm { get; set; }

        [Column("ACTIVA_WEB", TypeName = "varchar(1)")]
        public string? ActivaWeb { get; set; }

        [Column("REGISTROSANITARIO", TypeName = "varchar(50)")]
        public string? RegistroSanitario { get; set; }

        [Column("CADUCIDAD_NSO")]
        public DateTime? CaducidadNSO { get; set; }

        [Column("AGOTAMIENTO_STOCK", TypeName = "varchar(1)")]
        public string? AgotamientoStock { get; set; }

        [Column("CADUCIDAD_AGOTAMIENTO")]
        public DateTime? CaducidadAgotamiento { get; set; }

        [Column("VENTAWEB", TypeName = "varchar(1)")]
        public string? VentaWeb { get; set; }

        // Navegación de claves foráneas
        
        
        //public virtual Empresa EmpresaCta { get; set; }
        public virtual GenEstados Estado { get; set; }
        //public virtual Agrupacion Agrupacion { get; set; }

        
        //public virtual Linea Linea { get; set; }
        //public virtual Sublinea Sublinea { get; set; }
        //public virtual GrupoTipo GrupoTipo { get; set; }
        //public virtual UsoAplicacionMat UsoAplicacionMat { get; set; }
    }


    //[Table("ARTICULOSXEMPRESA")]
    //public class ArticulosXEmpresa
    //{
    //    [Key]
    //    [Column("CODARTICULOEMPRESA")]
    //    public int CodArticuloEmpresa { get; set; }

    //    [Column("CODEMPRESA")]
    //    public int CodEmpresa { get; set; }

    //    [Column("CODARTICULO")]
    //    public int CodArticulo { get; set; }

    //    [Column("STOCKMINIMO")]
    //    public decimal StockMinimo { get; set; }

    //    [Column("SEINVENTARIA")]
    //    public string? SeInventaria { get; set; }

    //    [Column("VENTAPEDIDOS")]
    //    public string? VentaPedidos { get; set; }

    //    [Column("VENTAALMACENES")]
    //    public string? VentaAlmacenes { get; set; }

    //    [Column("COMPRABLE")]
    //    public string? Comprable { get; set; }

    //    [Column("VENDIBLE")]
    //    public string? Vendible { get; set; }

    //    [Column("CODMOTIVOARTICULO")]
    //    public int? CodMotivoArticulo { get; set; }

    //    [Column("PRECIOCOMPRA")]
    //    public decimal PrecioCompra { get; set; }

    //    [Column("PORCDSCTOCOMPRA")]
    //    public decimal PorcDsctoCompra { get; set; }

    //    [Column("PORCDSCTOVENTA")]
    //    public decimal? PorcDsctoVenta { get; set; }

    //    [Column("COSTOEMPRESA")]
    //    public decimal CostoEmpresa { get; set; }

    //    [Column("STOCKEMPRESA")]
    //    public decimal? StockEmpresa { get; set; }

    //    [Column("CODEMPRESACTA")]
    //    public int? CodEmpresaCta { get; set; }

    //    [Column("NUMCUENTA")]
    //    public string? NumCuenta { get; set; }

    //    [Column("CODESTADO")]
    //    public int CodEstado { get; set; }

    //    [Column("USUARIOCAMBIOESTADO")]
    //    public string? UsuarioCambioEstado { get; set; }

    //    [Column("FECHACAMBIOESTADO")]
    //    public DateTime? FechaCambioEstado { get; set; }

    //    [Column("NCPORVALOR")]
    //    public string? NcPorValor { get; set; }

    //    [Column("INCLUYEIVAVENTAS")]
    //    public string? IncluyeIvaVentas { get; set; }

    //    [Column("CODEMPRESAAGRUPACION")]
    //    public int? CodEmpresaAgrupacion { get; set; }

    //    [Column("CODAGRUPACION")]
    //    public int? CodAgrupacion { get; set; }

    //    [Column("IMPORTADO")]
    //    public string? Importado { get; set; }

    //    [Column("CALCULAMINMAX")]
    //    public string? CalculaMinMax { get; set; }

    //    [Column("PORCDSCTOMAXIMO")]
    //    public decimal? PorcDsctoMaximo { get; set; }

    //    [Column("LIQUIDAR")]
    //    public string? Liquidar { get; set; }

    //    [Column("CODEMPRESAMARCA")]
    //    public int? CodEmpresaMarca { get; set; }

    //    [Column("CODMARCA")]
    //    public int? CodMarca { get; set; }

    //    [Column("CODEMPRESALINEA")]
    //    public int? CodEmpresaLinea { get; set; }

    //    [Column("CODLINEA")]
    //    public int? CodLinea { get; set; }

    //    [Column("CODEMPRESASUBLINEA")]
    //    public int? CodEmpresaSubLinea { get; set; }

    //    [Column("CODSUBLINEA")]
    //    public int? CodSubLinea { get; set; }

    //    [Column("CODEMPRESASUBLINEADOS")]
    //    public int? CodEmpresaSubLineaDos { get; set; }

    //    [Column("CODSUBLINEADOS")]
    //    public int? CodSubLineaDos { get; set; }

    //    [Column("CODEMPRESAGRUPOTIPO")]
    //    public int? CodEmpresaGrupoTipo { get; set; }

    //    [Column("CODGRUPOTIPO")]
    //    public int? CodGrupoTipo { get; set; }

    //    [Column("CODEMPRESAUSOAPLICAMAT")]
    //    public int? CodEmpresaUsoAplicMat { get; set; }

    //    [Column("CODUSOAPLICAMAT")]
    //    public int? CodUsoAplicMat { get; set; }

    //    [Column("PMS_TIPOPRODUCTO")]
    //    public string? PmsTipoProducto { get; set; }

    //    [Column("PMS_RESURTIDO")]
    //    public string? PmsResurtido { get; set; }

    //    [Column("PMS_REABASTO")]
    //    public string? PmsReabasto { get; set; }

    //    [Column("PMS_PARAMETROS")]
    //    public string? PmsParametros { get; set; }

    //    [Column("PMS_ESREEMPLAZO")]
    //    public string? PmsEsReemplazo { get; set; }

    //    [Column("PMS_FECHAALTA")]
    //    public DateTime? PmsFechaAlta { get; set; }

    //    [Column("PMS_FECHAVIGENCIA")]
    //    public DateTime? PmsFechaVigencia { get; set; }

    //    [Column("PMS_VIGENCIAMESES")]
    //    public int? PmsVigenciaMeses { get; set; }

    //    [Column("PMS_CODARTICULOSUST")]
    //    public int? PmsCodArticuloSust { get; set; }

    //    [Column("PMS_CODARTICULOANT")]
    //    public int? PmsCodArticuloAnt { get; set; }

    //    [Column("PMS_ESKIT")]
    //    public string? PmsEsKit { get; set; }

    //    [Column("ULTIMACOMPRA")]
    //    public decimal? UltimaCompra { get; set; }

    //    [Column("FECHAULTIMACOMPRA")]
    //    public DateTime? FechaUltimaCompra { get; set; }

    //    [Column("REMATE")]
    //    public string? Remate { get; set; }

    //    //[Column("ABC")]
    //    //public string Abc { get; set; }

    //    [Column("CODPARTIDA")]
    //    public int? CodPartida { get; set; }

    //    [Column("PRECIO_PROV_ORIGEN")]
    //    public decimal? PrecioProvOrigen { get; set; }

    //    [Column("MONEDA_ORIGEN")]
    //    public string? MonedaOrigen { get; set; }

    //    [Column("INCOTERM")]
    //    public string? Incoterm { get; set; }

    //    [Column("ACTIVA_WEB")]
    //    public string? ActivaWeb { get; set; }

    //    [Column("REGISTROSANITARIO")]
    //    public string? RegistroSanitario { get; set; }

    //    [Column("CADUCIDAD_NSO")]
    //    public DateTime? CaducidadNSO { get; set; }

    //    [Column("AGOTAMIENTO_STOCK")]
    //    public string? AgotamientoStock { get; set; }

    //    [Column("CADUCIDAD_AGOTAMIENTO")]
    //    public DateTime? CaducidadAgotamiento { get; set; }

    //    [Column("VENTAWEB")]
    //    public string? VentaWeb { get; set; }
    //}
}
