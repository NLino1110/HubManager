using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

namespace DMSA.Models.Odoo.Import
{
    //[Obsolete]
    //[Table("TuNombreDeTabla")]
    public class InvoicePayments
    {
        [Key]
        [Column("CODAGENCIA")]
        [JsonProperty("codagencia")]
        public int CodAgencia { get; set; }

        [Column("CODTIPOCMPR")]
        [JsonProperty("codtipocmpr")]
        public string CodTipoCmpr { get; set; }

        [Column("NUMCMPRVENTA")]
        [JsonProperty("numcmprventa")]
        public string NumCmprVenta { get; set; }

        [Column("NUMMOVIMIENTODET")]
        [JsonProperty("ord")]
        public string NumMovimientoDet { get; set; }

        [Column("NUMDOCUMENTO")]
        [JsonProperty("numdocumento")]
        public string NumDocumento { get; set; }

        [Column("CODFORMAPAGO")]
        [JsonProperty("codformapago")]
        public string CodFormaPago { get; set; }

        [Column("CODTARJETA")]
        [JsonProperty("codtarjeta")]
        public string CodTarjeta { get; set; }

        [Column("CODBANCO")]
        [JsonProperty("codbanco")]
        public string CodBanco { get; set; }

        [Column("NUMTARJETACREDITO")]
        [JsonProperty("numtarjetacredito")]
        public string NumTarjetaCredito { get; set; }

        [Column("CUOTAS_DIFERIDO")]
        [JsonProperty("cuotas_diferido")]
        public int? CuotasDiferido { get; set; } // Asumido nullable

        [Column("TIPO_DIFERIDO")]
        [JsonProperty("tipo_diferido")]
        public string TipoDiferido { get; set; }

        [Column("VALOR")]
        [JsonProperty("valor")]
        public decimal Valor { get; set; }
    }
    
}
