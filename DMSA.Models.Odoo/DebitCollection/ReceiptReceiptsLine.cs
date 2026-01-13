using DMSA.Models.Odoo.Base;
using DMSA.Models.Odoo.Json.Converters;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMSA.Models.Odoo.DebitCollection
{
    [Table("receipt_receipts_line")]
    public class ReceiptReceiptsLine : OdooEntity
    {
        [PrimaryKey]        
        [Column("id")]
        [JsonProperty("id")]
        public int id { get; set; }

        [Ignore]
        [Column("company_id")]
        [JsonProperty("company_id")]
        public JToken company_id { get; set; }

        [JsonIgnore]
        [Column("_company_id")]
        public int _company_id
        {
            get => GetId(company_id);
            set => company_id = SetId(company_id, value);
        }

        [Ignore]
        [Column("receipt_receipts_id")]
        [JsonProperty("receipt_receipts_id")]
        public JToken receipt_receipts_id { get; set; }

        [Column("_receipt_receipts_id")]
        public int _receipt_receipts_id
        {
            get => GetId(receipt_receipts_id);
            set => receipt_receipts_id = SetId(receipt_receipts_id, value);
        }        

        public string name { get; set; }
        public int number_seq { get; set; }

        [JsonConverter(typeof(OdooNullableDateTimeConverter))]
        public DateTime? payment_date { get; set; }
        [JsonConverter(typeof(OdooNullableDateTimeConverter))]
        public DateTime? withdrawal_date { get; set; }
        [JsonConverter(typeof(OdooNullableDateTimeConverter))]
        public DateTime? reconciled_date { get; set; }
        public float amount { get; set; }
        public string amount_in_words { get; set; }
        public string note { get; set; }
        public string number_check_text { get; set; }
        [Ignore]
        public JToken payment_id { get; set; }
        
        [Ignore]
        public JToken sale_user_id { get; set; }

        [Column("_sale_user_id")]
        public int _sale_user_id
        {
            get => GetId(sale_user_id);
            set => sale_user_id = SetId(sale_user_id, value);
        }

        public string display_name { get; set; }

        [Column("create_date")]
        public DateTime create_date { get; set; }
        
        [Column("write_date")]
        public DateTime write_date { get; set; }

        [Column("state")]
        public string state { get; set; }
    }
}
