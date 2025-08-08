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
    public class InvoiceHeader
    {
        [Key]
        [Column("CODAGENCIA")]
        public int CodAgencia { get; set; }

        [Column("CODTIPOCMPR")]
        public string CodTipoCmpr { get; set; }

        [Column("NUMCMPRVENTA")]
        public string NumCmprVenta { get; set; }

        [Column("CODCLIENTE")]
        public int CodCliente { get; set; }

        [Column("TIPOIDENTIFICACION")]
        public string TipoIdentificacion { get; set; }

        [Column("APELLIDOSCLIENTE")]
        public string ApellidosCliente { get; set; }

        [Column("NOMBRESCLIENTE")]
        public string NombresCliente { get; set; }

        [Column("DIRECCION")]
        public string Direccion { get; set; }

        [Column("TELEFONO")]
        public string Telefono { get; set; }

        [Column("EMAIL")]
        public string Email { get; set; }

        [Column("IDENTIFICACION")]
        public string Identificacion { get; set; }

        [Column("ESTAB")]
        public string Estab { get; set; }

        [Column("PTOEMI")]
        public string PtoEmi { get; set; }

        [Column("SECUENCIAL")]
        public string Secuencial { get; set; }

        [Column("NUMDOCUMENTO")]
        public string NumDocumento { get; set; }

        [Column("FECHAREGISTROCREA")]
        public DateTime FechaRegistroCrea { get; set; }

        [Column("FECHAREGISTROAPRO")]
        public DateTime FechaRegistroApro { get; set; }

        [Column("NOMAGENCIA")]
        public string NomAgencia { get; set; }

        [Column("DSCTIPOCLIENTEVTA")]
        public string DscTipoClienteVta { get; set; }

        [Column("CODVENDEDOR")]
        public int CodVendedor { get; set; }

        [Column("NOMVENDEDOR")]
        public string NomVendedor { get; set; }

        [Column("NULL")]
        public string Null { get; set; }

        [Column("SUBTOTAL")]
        public decimal Subtotal { get; set; }

        [Column("DESCUENTO")]
        public decimal Descuento { get; set; }

        [Column("IMPUESTO")]
        public decimal Impuesto { get; set; }

        [Column("TOTAL")]
        public decimal Total { get; set; }
    }
}
