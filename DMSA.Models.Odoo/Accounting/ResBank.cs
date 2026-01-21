using Newtonsoft.Json;
using SQLite;

namespace DMSA.Models.Odoo.Accounting
{
    [Table("res_bank")]
    public class ResBank
    {
        [PrimaryKey]
        public int id { get; set; }
        public string name { get; set; }

        public string bic { get; set; }        

        [JsonProperty("create_date")]
        [Column("create_date")]
        public DateTime? create_date { get; set; }

        [JsonProperty("write_date")]
        [Column("write_date")]
        public DateTime? write_date { get; set; }
    }
}
