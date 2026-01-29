using DMSA.Models.Odoo.Base;
using DMSA.Models.Odoo.Native;
using Newtonsoft.Json;
using SQLite;

namespace DMSA.Models.Odoo.DebitCollection
{
    public class MultipleCobrosInvoiceLineCuadraturaWrapper : List<object>
    {
        public MultipleCobrosInvoiceLineCuadraturaWrapper(MultipleCobrosInvoiceLineCuadratura line)
        {
            Add(0);
            Add(0);
            Add(line);
        }
    }

    [Table("multiple_cobros_invoice_line_cuadratura")]
    public class MultipleCobrosInvoiceLineCuadratura : OdooEntity
    {
        [PrimaryKey]
        [AutoIncrement]
        [JsonProperty("id")]
        [Column("id")]
        public int Id { get; set; }

        [JsonProperty("name")]
        [Column("name")]
        public string name { get; set; }

        [JsonProperty("account_cuadre_id")]
        [Column("account_cuadre_id")]
        public int account_cuadre_id { get; set; }

        [JsonProperty("aplica_analitica")]
        [Column("aplica_analitica")]
        public bool aplica_analitica { get; set; }

        [JsonProperty("analitica_requerido")]
        [Column("analitica_requerido")]
        public bool analitica_requerido { get; set; }

        [JsonProperty("analytic_account_id")]
        [Column("analytic_account_id")]
        public int analytic_account_id { get; set; }

        [JsonProperty("partner_id")]
        [Column("partner_id")]
        public int partner_id { get; set; }
        [JsonProperty("amount")]
        [Column("amount")]
        public decimal? amount { get; set; }
        

        [JsonProperty("multiple_cobros_invoice_line_id")]
        [Column("multiple_cobros_invoice_line_id")]
        public int multiple_cobros_invoice_line_id { get; set; }
        


    }
}
