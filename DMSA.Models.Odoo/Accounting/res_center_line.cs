using DMSA.Models.Odoo.Base;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SQLite;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMSA.Models.Odoo.Accounting
{
    [Table("res_center_line")]
    public class res_center_line : OdooEntity
    {
        [PrimaryKey]
        [Column("id")]
        public int id { get; set; }
       
        [Column("name")]
        public string? name { get; set; }

        [Column("code")]
        public string? code { get; set; }

        [Ignore]
        [Column("center_id")]
        public JToken? center_id { get; set; }
        
        [Column("center_id_")]
        public int center_id_
        {
            get => GetId(center_id);
            set => center_id = SetId(center_id, value);
        }

        [Ignore]
        [Column("doc_authorization_line_id")]
        public JToken? doc_authorization_line_id { get; set; }

        [Column("doc_authorization_line_id_json")]
        public string? doc_authorization_line_id_json { get; set; }

        [Column("display_name")]
        public string? display_name { get; set; }

        [Column("create_date")]
        public DateTime? create_date { get; set; }

        [Column("write_date")]
        public DateTime? write_date { get; set; }
    }
}
