using DMSA.Models.Odoo.Base;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SQLite;
using SQLiteNetExtensions.Attributes;

namespace DMSA.Models.Odoo.Accounting
{
    [Table("payment_check_invoice_link")]
    public class PaymentCheckInvoiceLink : OdooEntity
    {
        [JsonProperty("id")]
        public int id { get; set; }
        [JsonProperty("state")]
        public string state { get; set; }
        [JsonProperty("display_name")]
        public string display_name { get; set; }
        [JsonProperty("write_date")]
        public DateTime write_date { get; set; }
        [JsonProperty("create_date")]
        public DateTime create_date { get; set; }
    }
}
