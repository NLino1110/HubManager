using CobranzasDMSA_Odoo.Models;
using Newtonsoft.Json;
using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DMSA.Models.Odoo.Native
{
    //Interpolation: Odoo -> account.move
    public class account_move_send
    {
        [AutoIncrement]
        [PrimaryKey]
        public int id { get; set; }

        [JsonIgnore]
        public int parent_id { get; set; }
        [JsonIgnore]
        public int sequence { get; set; }
        [JsonIgnore]
        public string ref_name { get; set; }
        [JsonIgnore]
        public string title { get; set; }

        [JsonIgnore]
        public int create_id { get; set; }
        public string move_type { get; set; }
        public DateTime invoice_date { get; set; }
        public int partner_id { get; set; }
        public int company_id { get; set; }
        public string document_type { get; set; }
        public int printer_id { get; set; }
        public int l10n_latam_document_type_id { get; set; }
        public int journal_id { get; set; }
        public int reversed_entry_id { get; set; }
        public string payment_reference { get; set; }

        public int module_id { get; set; }
        public int type_module_id { get; set; }

        [JsonProperty("ref")]
        public string _ref { get; set; }

        [JsonIgnore]
        public string name { get; set; }
        //No controlado por ODOO
        [JsonIgnore]
        public DateTime create_date { get; set; }
        [JsonIgnore]
        public int create_uid { get; set; }
        [JsonIgnore]
        public string partner_name { get; set; }
        [JsonIgnore]
        public string partner_email { get; set; }

        /*
         * pending (pendiente)
         * approved (aprobado)
         * rejected (rechazado)
        */

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

        [JsonIgnore]
        public string TIPONOTACREDITO { get; set;}

        [Ignore]
        [JsonIgnore]
        public account_move_line_send[] lines { get; set; }

        [Ignore]
        [JsonProperty]
        public List<object> invoice_line_ids { get; set; }

        [JsonIgnore]
        public string build_mode { get; set; }

        [Ignore]        
        public string? request_name { get; set; }

        [JsonIgnore]
        public bool was_odoo_synced { get; set; }
    }
}
