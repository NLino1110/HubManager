using Newtonsoft.Json;
using SQLite;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMSA.Models.Odoo.DebitCollection
{
    [Table("multiple_cobros_invoice")]
    public class MultipleCobrosInvoice
    {

        [Key]
        [PrimaryKey]
        [AutoIncrement]
        [Column("id")]
        public int id { get; set; }
        [Column("company_id")]
        public int company_id { get; set; }
        [Column("name")]
        public string name { get; set; }
        [Column("center_id")]
        public int center_id { get; set; }
        [Column("state")]
        public string state { get; set; }
        [Column("partner_id")]
        public int partner_id { get; set; }
        [Column("date")]
        public DateTime date { get; set; }
        [Column("amount_depositos")]
        public float amount_depositos { get; set; }
        [Column("amount_efectivo")]
        public float amount_efectivo { get; set; }
        [Column("amount_cheques_post")]
        public float amount_cheques_post { get; set; }
        [Column("amount_cheques_dia")] 
        public float amount_cheques_dia { get; set; }
        [Column("amount_tc")] 
        public float amount_tc { get; set; }
        [Column("amount")] 
        public float amount { get; set; }
        [Column("partner_type")] 
        public string partner_type { get; set; }
        [Column("payment_type")] 
        public string payment_type { get; set; }
        [Column("type_invoice")] 
        public string type_invoice { get; set; }
        [Column("user_id")] 
        public int user_id { get; set; }
        [Column("bank_macro_id")] 
        public int bank_macro_id { get; set; }
        [Column("res_partner_bank_id")] 
        public int res_partner_bank_id { get; set; }
        [Column("report_file")] 
        public bool report_file { get; set; }
        [Column("file_name")] 
        public bool file_name { get; set; }
        [Column("journal_id")] 
        public int journal_id { get; set; }
        [Column("grouped")] 
        public bool grouped { get; set; }
        [Column("note")] 
        public string note { get; set; }
        [Column("pagos_count")] 
        public int pagos_count { get; set; }
        [Column("depositos_count")] 
        public int depositos_count { get; set; }
        [Column("cheques_count")] 
        public int cheques_count { get; set; }
        [Column("subclasificacion_gasto_id")] 
        public int subclasificacion_gasto_id { get; set; }
        [Column("receipt_receipts_id")] 
        public int receipt_receipts_id { get; set; }

        [Column("receipt_receipts_line_id")]
        public int receipt_receipts_line_id { get; set; }

        [Column("depositos_id")] 
        public int depositos_id { get; set; }
        [Column("display_name")] 
        public string display_name { get; set; }
        [Column("create_uid")] 
        public int create_uid { get; set; }
        [Column("create_date")] 
        public DateTime create_date { get; set; }
        [Column("write_uid")] 
        public int write_uid { get; set; }
        [Column("write_date")] 
        public DateTime write_date { get; set; }
        [Column("proceso_caja_id")] 
        public int proceso_caja_id { get; set; }
        //[Column("partner_retail_id")] 
        //public int partner_retail_id { get; set; }

        public string recipe_name { get; set; }
        public string guid { get; set; }
        public string payment_status { get; set; }
        public string device_app_version { get; set; }
        public string device_idiom { get; set; }
        public string device_model { get; set; }
        public string device_manufacturer { get; set; }        
        public string device_serial { get; set; }
        public string origin_mobile_app { get; set; }
        public string sync_mode { get; set; }

        [Ignore]
        [JsonIgnore]
        public MultipleCobrosInvoiceLine[] lines { get; set; }
        [JsonIgnore]
        public string partner_name { get; set; }
        [JsonIgnore]
        public float total_due { get; set; }

        [JsonIgnore]
        public string partner_email { get; set; }

        [JsonIgnore]
        public string CERRADO { get; set; }

        [JsonIgnore]
        public string user_name { get; set; }
    }
}
