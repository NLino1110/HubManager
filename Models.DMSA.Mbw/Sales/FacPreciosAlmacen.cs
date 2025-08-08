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
    [Table("FACPRECIOSALMACEN")]
    public class FacPreciosAlmacen
    {
        [Key]
        [Column("CODEMPRESA")]
        public int CodEmpresa { get; set; }

        [Key]
        [Column("CODARTICULO")]
        public int CodArticulo { get; set; }

        [Key]
        [Column("CODEMPRESANIVEL")]
        public int CodEmpresaNivel { get; set; }

        [Key]
        [Column("CODNIVEL")]
        public int CodNivel { get; set; }

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
