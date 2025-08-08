using SQLite;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace CobranzasDMSA_Odoo.Models
{
    //cabeceraCobro
    public class AccountPaymentHeader
    {
        [Key]
        [PrimaryKey]
        [AutoIncrement]
        [NotNull]
        public int id { get; set; }
        public string guid { get; set; }
        public string recipe_name { get; set; }
        //[DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        //public int ROWID { get; set; }
        public int company_id { get; set; }
        //[Key]
        //[PrimaryKey]
        //[NotNull]
        public DateTime create_datetime { get; set; }
        public int uid { get; set; }
        public int partner_id { get; set; }
        public string partner_name { get; set; }
        public decimal payment_amount { get; set; }
        public decimal total_due { get; set; }
        public string payment_status { get; set; }
        public DateTime modification_datetime { get; set; }

        [Ignore]
        [JsonIgnore]
        public string partner_display
        {
            get => string.Concat(partner_id.ToString(), " - ", partner_name);
        }

        /// <summary>
        /// Almacena los detalles de los pagos
        /// </summary>
        //public string account_payment_json { get; set; }
        /// <summary>
        /// Almacena los detalles de las facturas pagadas
        /// </summary>
        //public string account_payment_invoice_json { get; set; }

        public bool autosend { get; set; }
        [JsonIgnore]
        public bool was_odoo_synced { get; set;}
        [Ignore]
        public string manufacturer { get; set; }
        [Ignore]
        public string model { get; set; }
        [Ignore]
        public string serial { get; set; }
        //[Ignore]
        //public string ENVIOAUTOMATICO { get; set; }

        //NOTMAPPED
        //[NotMapped]
        //[IgnoreDataMember]
        //public string? ID { get; set; }
        [Ignore]
        [JsonIgnore]
        public string? NOMBREUSUARIO { get; set; }
        [Ignore]
        [JsonIgnore]
        public string? EMAILCLIENTE { get; set; }
        [Ignore]
        [JsonIgnore]
        public string? CERRADO { get; set; }
        [Ignore]
        [JsonIgnore]
        public bool? isGroup { get; set; }
    }
}
