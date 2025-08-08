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
    [Table("INVSTOCK")]
    public class InvStock
    {
        [Column("CODBODEGAAGENCIA")]
        public long CodBodegaAgencia { get; set; }

        [Column("CODARTICULO")]
        public long CodArticulo { get; set; }

        [Column("CODALTERNO")]
        [StringLength(20)]
        public string? CodAlterno { get; set; }

        [Column("CANTIDAD", TypeName = "decimal(16,4)")]
        public decimal Cantidad { get; set; } = 0;

        [Column("FECHAULTINGRESO")]
        public DateTime? FechaUltIngreso { get; set; }

        [Column("FECHAULTEGRESO")]
        public DateTime? FechaUltEgreso { get; set; }

        [Column("CANTIDADRESERVADA", TypeName = "decimal(16,4)")]
        public decimal CantidadReservada { get; set; } = 0;

        [Column("CANTIDADTRANSITO", TypeName = "decimal(16,4)")]
        public decimal CantidadTransito { get; set; } = 0;

        [Column("CANTIDADLOGICA", TypeName = "decimal(16,4)")]
        public decimal CantidadLogica { get; set; } = 0;

        [Column("RESERVATMP", TypeName = "decimal(16,4)")]
        public decimal? ReservaTmp { get; set; }

        [Column("CONTROLCICLICO")]
        [StringLength(1)]
        public string? ControlCiclico { get; set; }
    }

}
