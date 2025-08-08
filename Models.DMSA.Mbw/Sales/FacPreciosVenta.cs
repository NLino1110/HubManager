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
    [Table("FACPRECIOSVENTA")]
    public class FacPreciosVenta
    {
        [Key]
        [Column("CODAGENCIA")]
        public int CodAgencia { get; set; }

        [Key]
        [Column("NUMPRECIOVENTA")]
        public int NumPrecioVenta { get; set; }

        [Key]
        [Column("CODTIPOCLIENTE")]
        public int CodTipoCliente { get; set; }

        [Key]
        [Column("CODARTICULO")]
        public int CodArticulo { get; set; }

        [Column("CODUNIDADMEDIDA")]
        [StringLength(15)]
        public string? CodUnidadMedida { get; set; }

        [Column("PRECIO", TypeName = "decimal(16,4)")]
        public decimal Precio { get; set; }

        [Column("FECHAMODIFICACION")]
        public DateTime? FechaModificacion { get; set; }

        [Column("CODUSUARIOMODIFICA")]
        [StringLength(15)]
        public string? CodUsuarioModifica { get; set; }

        [Column("CODUSUARIO")]
        [StringLength(15)]
        public string? CodUsuario { get; set; }

        [Column("FECHA")]
        public DateTime Fecha { get; set; }

        [Column("PRECIOANTERIOR", TypeName = "decimal(16,4)")]
        public decimal? PrecioAnterior { get; set; }
    }

}
