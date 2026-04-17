using System;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
using SQLite;

namespace DMSA.Models.Odoo.Inventory
{
    [Table("uom_uom")]
    public class uom_uom
    {
        [PrimaryKey]
        [Column("id")]
        public int id { get; set; }
        [Column("name")]
        public string? name { get; set; }

        [Column("factor")]
        public double? factor { get; set; }

        [Column("factor_inv")]
        public double? factor_inv { get; set; }

        [Column("rounding")]
        public double? rounding { get; set; }

        [Column("active")]
        public bool? active { get; set; }

        [Column("uom_type")]
        public string? uom_type { get; set; }

        [Column("ratio")]
        public double? ratio { get; set; }

        [Column("color")]
        public int? color { get; set; }

        [Column("display_name")]
        public string? display_name { get; set; }

        [Column("clave_externa")]
        public string? clave_externa { get; set; }

        [Column("fiscal_country_codes")]
        public string? fiscal_country_codes { get; set; }

        [Column("create_date")]
        public DateTime? create_date { get; set; }
               
        [Column("write_date")]
        public DateTime? write_date { get; set; }
    }
}