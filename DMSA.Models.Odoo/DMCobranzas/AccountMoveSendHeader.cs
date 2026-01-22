using DMSA.Models.Odoo.Native;
using Newtonsoft.Json;
using SQLite;
using System.ComponentModel.DataAnnotations;

namespace DMSA.Models.Odoo.DMCobranzas
{
    //cabeceraCobro
    public class AccountMoveSendHeader
    {
        [Key]
        [PrimaryKey]
        [AutoIncrement]
        [NotNull]
        public int id { get; set; }
        
        public int company_id { get; set; }
        public string request_name { get; set; }
        public DateTime create_datetime { get; set; }
        public int uid { get; set; }
        public int partner_id { get; set; }
        public string partner_name { get; set; }
        public decimal return_amount { get; set; }
        public string request_status { get; set; }
        //public string request_mode { get; set; }
        public DateTime modification_datetime { get; set; }

        [Ignore]
        [JsonIgnore]
        public string partner_display
        {
            get => string.Concat(partner_id.ToString(), " - ", partner_name);
        }

        [Ignore]
        public string autosend { get; set; }
        [Ignore]
        public string? username { get; set; }
        [Ignore]
        public string? emailCustomer { get; set; }
        [Ignore]
        public bool? isGroup { get; set; }

        [Ignore]
        public string manufacturer { get; set; }
        [Ignore]
        public string model { get; set; }
        [Ignore]
        public string serialNumber { get; set; }

        [JsonIgnore]
        public bool was_odoo_synced { get; set; }

        [JsonIgnore]
        [Ignore]
        public account_move_send[] account_moves { get; set; }
    }
}
