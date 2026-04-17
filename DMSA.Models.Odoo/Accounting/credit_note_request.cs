using Newtonsoft.Json;
using SQLite;

namespace DMSA.Models.Odoo.Accounting
{
    [Table("credit_note_request")]
    public class credit_note_request
    {
        [AutoIncrement]
        [PrimaryKey]
        public int id { get; set; }

        [JsonIgnore]
        public int parent_id { get; set; }
        [JsonIgnore]
        public int sequence { get; set; }

        public string move_type_nc { get; set; }

        public string state_sync { get; set; }
        public string state { get; set; }
        public int picking_id { get; set; }

        public int company_id { get; set; }
        public string tipo_nc { get; set; }

        public string move_type { get; set; }

        public string modo_genera_nc { get; set; }
        public int partner_id { get; set; }

        public DateTime request_date { get; set; }
        public int center_id { get; set; }
        public string docnum_mask { get; set; }

        public int res_center_line_id { get; set; }
        public int doc_authorization_line_id { get; set; }      

        public int doc_tax_sustent_id { get; set; }
        public int journal_id { get; set; }
        public bool reimprime_factura { get; set; }
        public int parent_nc_id { get; set; }
        public int type_module_id { get; set; }
        public string? reason { get; set; }
        public bool nc_sin_sustento { get; set; }
        public string docnum_mask_sustent { get; set; }
        public string authorization { get; set; }
        public string request_type { get; set; }
        public int? mainAccountMove { get; set; }
        public decimal amount_discount { get; set; }
        public decimal return_amount { get; set; }
        public string obs_2 { get; set; }
        public string obs_approved { get; set; }

        [Ignore]
        [JsonProperty("accountMovesProducts")]
        public List<object> accountMovesProducts { get; set; }        

        [JsonIgnore]
        public string? title { get; set; }

        [JsonIgnore]
        public int create_id { get; set; }


        [Obsolete]
        public int reversed_entry_id { get; set; }
        [Obsolete]
        public string payment_reference { get; set; }
        //[Obsolete]
        //public int module_id { get; set; }
       

        [JsonProperty("ref")]
        public string? _ref { get; set; }

        [JsonIgnore]
        public string name { get; set; }
        //No controlado por ODOO
        [JsonIgnore]
        public DateTime create_date { get; set; }
        [JsonIgnore]
        public int create_uid { get; set; }

        [JsonProperty("external_create_uid")]
        public int external_create_uid { get; set; }
        [JsonProperty("external_guid")]
        public string external_guid { get; set; }

        [JsonIgnore]
        public string partner_name { get; set; }
        [JsonIgnore]
        public string partner_email { get; set; }

        [JsonIgnore]
        public string doc_status { get; set; }

        [JsonIgnore]
        public DateTime send_date { get; set; }

        /*
         * open
         * closed
         */
        [JsonIgnore]
        public string group_status { get; set; }

        [JsonIgnore]
        public int group_id { get; set; }        

        [Ignore]
        [JsonIgnore]
        public credit_note_request_detail[] lines { get; set; }

        [Ignore]
        [JsonProperty]
        public List<object> invoice_line_ids { get; set; }

        [Obsolete]
        [JsonIgnore]
        public string build_mode { get; set; }

        [Ignore]
        public string? request_name { get; set; }

        [JsonIgnore]
        public bool was_odoo_synced { get; set; }

        [Ignore]
        [JsonIgnore]
        public string? display_parent_nc { get; set; }
        [Ignore]
        [JsonIgnore]
        public string? display_type_module { get; set; }
    }
}
