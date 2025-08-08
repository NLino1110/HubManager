using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.DMSA.Mbw.Query
{
    [Obsolete]
    public class InvoiceDetails
    {
        [Key]
        public int Id { get; set; }

        [Column("CODAGENCIA")]
        public int CodAgencia { get; set; }

        [Column("CODTIPOCMPR")]
        public int CodTipoCmpr { get; set; }

        [Column("NUMCMPRVENTA")]
        [StringLength(10)]
        public string NumCmprVenta { get; set; }

        [Column("NUMDOCUMENTO")]
        [StringLength(20)]
        public string NumDocumento { get; set; }

        [Column("CODALTERNO")]
        [StringLength(20)]
        public string CodAlterno { get; set; }

        [Column("PRECIO")]
        public decimal Precio { get; set; }

        [Column("CANT")]
        public int Cant { get; set; }

        [Column("TOTAL")]
        public decimal Total { get; set; }
    }

}
