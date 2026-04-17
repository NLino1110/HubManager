using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace DMSA.Models.Odoo.Import
{
    //[Obsolete]
    public class InvoiceDetails
    {
        [Column("CODAGENCIA")]
        [JsonProperty("codagencia")]
        public int CodAgencia { get; set; }

        [Column("CODTIPOCMPR")]
        [JsonProperty("codtipocmpr")]
        public string CodTipoCmpr { get; set; }

        [Column("NUMCMPRVENTA")]
        [StringLength(10)]
        [JsonProperty("numcmprventa")]
        public string NumCmprVenta { get; set; }

        [Column("NUMCMPRVENTADET")]
        [StringLength(10)]
        [JsonProperty("ord")]
        public string NumCmprVentaDet { get; set; }

        [Column("NUMDOCUMENTO")]
        [StringLength(20)]
        [JsonProperty("numdocumento")]
        public string NumDocumento { get; set; }

        [Column("CODALTERNO")]
        [StringLength(20)]
        [JsonProperty("codalterno")]
        public string CodAlterno { get; set; }

        [Column("PRECIO")]
        [JsonProperty("precio")]
        public decimal Precio { get; set; }

        [Column("CANT")]
        [JsonProperty("cant")]
        public decimal Cant { get; set; }

        [Column("TOTAL")]
        [JsonProperty("total")]
        public decimal Total { get; set; }
    }

}
