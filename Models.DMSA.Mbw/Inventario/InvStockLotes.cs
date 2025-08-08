using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;

namespace Models.DMSA.Mbw.Inventario
{
   
    [Table("INVSTOCKLOTES")]
    public class InvstockLotes
    {
        [Column("CODEMPRESA")]
        public long CodEmpresa { get; set; }

        [Column("CODBODEGAAGENCIA")]
        public long CodBodegaAgencia { get; set; }

        [Column("CODARTICULO")]
        public long CodArticulo { get; set; }

        [Column("CODIGOLOTE")]
        public long? CodigoLote { get; set; }

        [Column("CANTIDAD", TypeName = "decimal(16,4)")]
        public decimal Cantidad { get; set; } = 0;

        [Column("CANTIDADRESERVADA", TypeName = "decimal(16,4)")]
        public decimal? CantidadReservada { get; set; } = 0;

        [Column("FECHAULTMODIFICA")]
        public DateTime? FechaUltModifica { get; set; }

        [Column("USUARIOMODIFICA")]
        public string? UsuarioModifica { get; set; }
    }

}
