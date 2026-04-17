using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SQLite;

namespace DMSA.Models.Odoo.Accounting
{
    [Table("doc_tax_sustent")]
    public class doc_tax_sustent
    {
        [PrimaryKey]
        [Column("id")]
        public int id { get; set; }
       
        [Column("name")]
        public string? name { get; set; }

        [Column("code")]
        public string? code { get; set; }

        [Column("description")]
        public string? description { get; set; }

        [Column("display_name")]
        public string? display_name { get; set; }

        [Column("active")]
        public bool active { get; set; }

        [Column("create_date")]
        public DateTime? create_date { get; set; }

        [Column("write_date")]
        public DateTime? write_date { get; set; }
    }
}
