using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMSA.Models.Odoo.Import
{
    //public class InvoiceHeader
    //{
    //    [Key]
    //    [Column("CODAGENCIA")]
    //    public int CodAgencia { get; set; }

    //    [Column("CODTIPOCMPR")]
    //    public string CodTipoCmpr { get; set; }

    //    [Column("NUMCMPRVENTA")]
    //    public string NumCmprVenta { get; set; }

    //    [Column("CODCLIENTE")]
    //    public int CodCliente { get; set; }

    //    [Column("TIPOIDENTIFICACION")]
    //    public string TipoIdentificacion { get; set; }

    //    [Column("APELLIDOSCLIENTE")]
    //    public string ApellidosCliente { get; set; }

    //    [Column("NOMBRESCLIENTE")]
    //    public string NombresCliente { get; set; }

    //    [Column("DIRECCION")]
    //    public string Direccion { get; set; }

    //    [Column("TELEFONO")]
    //    public string Telefono { get; set; }

    //    [Column("EMAIL")]
    //    public string Email { get; set; }

    //    [Column("IDENTIFICACION")]
    //    public string Identificacion { get; set; }

    //    [Column("ESTAB")]
    //    public string Estab { get; set; }

    //    [Column("PTOEMI")]
    //    public string PtoEmi { get; set; }

    //    [Column("SECUENCIAL")]
    //    public string Secuencial { get; set; }

    //    [Column("NUMDOCUMENTO")]
    //    public string NumDocumento { get; set; }

    //    [Column("FECHAREGISTROCREA")]
    //    public DateTime FechaRegistroCrea { get; set; }

    //    [Column("FECHAREGISTROAPRO")]
    //    public DateTime FechaRegistroApro { get; set; }

    //    [Column("NOMAGENCIA")]
    //    public string NomAgencia { get; set; }

    //    [Column("DSCTIPOCLIENTEVTA")]
    //    public string DscTipoClienteVta { get; set; }

    //    [Column("CODVENDEDOR")]
    //    public int CodVendedor { get; set; }

    //    [Column("NOMVENDEDOR")]
    //    public string NomVendedor { get; set; }

    //    [Column("NULL")]
    //    public string Null { get; set; }

    //    [Column("SUBTOTAL")]
    //    public decimal Subtotal { get; set; }

    //    [Column("DESCUENTO")]
    //    public decimal Descuento { get; set; }

    //    [Column("IMPUESTO")]
    //    public decimal Impuesto { get; set; }

    //    [Column("TOTAL")]
    //    public decimal Total { get; set; }
    //}

    using Newtonsoft.Json;
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public class InvoiceHeader
    {
        [Key]
        [Column("CODAGENCIA")]
        [JsonProperty("codagencia")]
        public int CodAgencia { get; set; }

        //[Column("CUSTOMHASH")]
        //[JsonProperty("customhash")]
        //public string CustomHash { get; set; }

        [Column("ORIGIN")]
        [JsonProperty("origin")]
        public string Origin { get; set; }


        [Column("CODTIPOCMPR")]
        [JsonProperty("codtipocmpr")]
        public string CodTipoCmpr { get; set; }

        [Column("NUMCMPRVENTA")]
        [JsonProperty("numcmprventa")]
        public string NumCmprVenta { get; set; }

        //[Column("CODCLIENTE")]
        //[JsonProperty("codcliente")]
        //public int CodCliente { get; set; }

        [Column("TIPOIDENTIFICACION")]
        [JsonProperty("tipoidentificacion")]
        public string TipoIdentificacion { get; set; }

        [Column("APELLIDOSCLIENTE")]
        [JsonProperty("apellidoscliente")]
        public string ApellidosCliente { get; set; }

        [Column("NOMBRESCLIENTE")]
        [JsonProperty("nombrescliente")]
        public string NombresCliente { get; set; }

        [Column("DIRECCION")]
        [JsonProperty("direccion")]
        public string Direccion { get; set; }

        [Column("TELEFONO")]
        [JsonProperty("telefono")]
        public string Telefono { get; set; }

        [Column("EMAIL")]
        [JsonProperty("email")]
        public string Email { get; set; }

        [Column("IDENTIFICACION")]
        [JsonProperty("identificacion")]
        public string Identificacion { get; set; }

        [Column("ESTAB")]
        [JsonProperty("estab")]
        public string Estab { get; set; }

        [Column("PTOEMI")]
        [JsonProperty("ptoemi")]
        public string PtoEmi { get; set; }

        [Column("SECUENCIAL")]
        [JsonProperty("secuencial")]
        public string Secuencial { get; set; }

        [Column("NUMDOCUMENTO")]
        [JsonProperty("numdocumento")]
        public string NumDocumento { get; set; }

        [Column("FECHAREGISTROCREA")]
        [JsonProperty("fecharegistrocrea")]
        public DateTime FechaRegistroCrea { get; set; }

        [Column("FECHAREGISTROAPRO")]
        [JsonProperty("fecharegistroapro")]
        public DateTime FechaRegistroApro { get; set; }

        [Column("NOMAGENCIA")]
        [JsonProperty("nomagencia")]
        public string NomAgencia { get; set; }

        [Column("DSCTIPOCLIENTEVTA")]
        [JsonProperty("dsctipoclientevta")]
        public string DscTipoClienteVta { get; set; }

        [Column("CODVENDEDOR")]
        [JsonProperty("codvendedor")]
        public int CodVendedor { get; set; }

        [Column("NOMVENDEDOR")]
        [JsonProperty("nomvendedor")]
        public string NomVendedor { get; set; }

        [Column("NULL")]
        [JsonProperty("null")]
        public string Null { get; set; }

        [Column("SUBTOTAL")]
        [JsonProperty("subtotal")]
        public decimal Subtotal { get; set; }

        [Column("DESCUENTO")]
        [JsonProperty("descuento")]
        public decimal Descuento { get; set; }

        [Column("IMPUESTO")]
        [JsonProperty("impuesto")]
        public decimal Impuesto { get; set; }

        [Column("TOTAL")]
        [JsonProperty("total")]
        public decimal Total { get; set; }
    }

}
