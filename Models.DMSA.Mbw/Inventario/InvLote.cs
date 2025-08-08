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

    [Table("INVLOTES")]
    public class InvLotes
    {
        [Key]
        [Column("CODIGOLOTE")]
        public long CodigoLote { get; set; }

        [Column("LOTE")]
        [StringLength(30)]
        public string? Lote { get; set; }

        [Column("FECHAREGISTRO")]
        public DateTime? FechaRegistro { get; set; }

        [Column("FECFABRICA")]
        public DateTime? FechaFabrica { get; set; }

        [Column("FECCADUCIDAD")]
        public DateTime? FechaCaducidad { get; set; }

        [Column("CODIGOBARRAS")]
        [StringLength(50)]
        public string? CodigoBarras { get; set; }
    }

}
