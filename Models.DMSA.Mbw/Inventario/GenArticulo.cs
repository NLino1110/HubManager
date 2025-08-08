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
    [Table("GENARTICULOS")]
    public class GenArticulos
    {
        [Key]
        [Column("CODARTICULO")]
        public int CodArticulo { get; set; }

        [Column("CODALTERNO")]
        [Required]
        [StringLength(20)]
        public string? CodAlterno { get; set; }

        [Column("CODBARRA")]
        [StringLength(20)]
        public string? CodBarra { get; set; }

        [Column("DESCRIPCION")]
        [Required]
        [StringLength(250)]
        public string? Descripcion { get; set; }

        [Column("CODTIPOPRODSRI")]
        [Required]
        [StringLength(5)]
        public string? CodTipoProdSri { get; set; }

        [Column("ACEPTADECIMALES")]
        [Required]
        [StringLength(1)]
        public string? AceptaDecimales { get; set; }

        [Column("ESCOMBO")]
        [Required]
        [StringLength(1)]
        public string? EsCombo { get; set; }

        [Column("CALCULAIVA")]
        [Required]
        [StringLength(1)]
        public string? CalculaIva { get; set; }

        [Column("CODUNIDADMEDIDA")]
        [Required]
        [StringLength(15)]
        public string? CodUnidadMedida { get; set; }

        [Column("INCLUYEIVACOMPRAS")]
        [Required]
        [StringLength(1)]
        public string? IncluyeIvaCompras { get; set; }

        [Column("CODUSUARIO")]
        [Required]
        [StringLength(15)]
        public string? CodUsuario { get; set; }

        [Column("FECHA")]
        [Required]
        public DateTime Fecha { get; set; }

        [Column("CODUNIDADPRESENTACION")]
        [Required]
        [StringLength(15)]
        public string? CodUnidadPresentacion { get; set; }

        [Column("CODUNIDADPROVEEDOR")]
        [StringLength(15)]
        public string? CodUnidadProveedor { get; set; }

        [Column("IMAGEN")]
        public byte[]? Imagen { get; set; }

        [Column("DESCRIPCIONCORTA")]
        [StringLength(250)]
        public string? DescripcionCorta { get; set; }

        [Column("CODTIPO")]
        public long? CodTipo { get; set; }

        [Column("CODCLASE")]
        public long? CodClase { get; set; }

        [Column("CALCULAIVACOMPRAS")]
        [StringLength(1)]
        public string? CalculaIvaCompras { get; set; }

        [Column("USUARIOMODIFICACION")]
        [StringLength(15)]
        public string? UsuarioModificacion { get; set; }

        [Column("FECHAMODIFICACION")]
        public DateTime? FechaModificacion { get; set; }

        [Column("IMAGENCARGADA")]
        [StringLength(1)]
        public string? ImagenCargada { get; set; }

        [Column("FECHAACTIMAGEN")]
        public DateTime? FechaActImagen { get; set; }

        [Column("USUARIOACTIMAGEN")]
        [StringLength(15)]
        public string? UsuarioActImagen { get; set; }

        [Column("MEDIDAFRENTE", TypeName = "decimal(16,4)")]
        public decimal? MedidaFrente { get; set; }

        [Column("MEDIDAALTO", TypeName ="decimal(16,4)")]
        public decimal? MedidaAlto { get; set; }

        [Column("MEDIDAFONDO",TypeName ="decimal(16,4)")]
        public decimal? MedidaFondo { get; set; }

        [Column("MEDIDAPESO", TypeName = "decimal(16,4)")]
        public decimal? MedidaPeso { get; set; }

        [Column("VOLUMETRIA")]
        [StringLength(1)]
        public string? Volumetria { get; set; }

        [Column("PERECIBLE")]
        [StringLength(1)]
        public string? Perecible { get; set; }

        [Column("CODUNIDADMASTER")]
        [StringLength(15)]
        public string? CodUnidadMaster { get; set; }

        [Column("EAN14")]
        [StringLength(20)]
        public string? Ean14 { get; set; }

        [Column("CODALTERNOPROV")]
        [StringLength(20)]
        public string? CodAlternoProv { get; set; }

        [Column("CODMASTERVENTA")]
        [StringLength(15)]
        public string? CodMasterVenta { get; set; }

        [Column("ESCUPON")]
        [StringLength(1)]
        public string? EsCupon { get; set; }

        [Column("IMPRIMIBLE")]
        [StringLength(2)]
        public string? Imprimible { get; set; }

        [Column("ESREEMBOLSO")]
        [StringLength(1)]
        public string? EsReembolso { get; set; }
    }

}
