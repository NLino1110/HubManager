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
    [Table("doc_authorization_line")]
    public class doc_authorization_line : OdooEntity
    {
        [PrimaryKey]
        [Column("id")]
        public int id { get; set; }
       
        [Column("name")]
        public string? name { get; set; }

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
        [Column("center_line_id")]
        public JToken? center_line_id { get; set; }

        [Column("center_line_id_")]
        public int center_line_id_
        {
            get => GetId(center_line_id);
            set => center_line_id = SetId(center_line_id, value);
        }

        [Ignore]
        [Column("document_type_id")]
        public JToken? document_type_id { get; set; }

        [Column("document_type_id_")]
        public int document_type_id_
        {
            get => GetId(document_type_id);
            set => document_type_id = SetId(document_type_id, value);
        }

        [Ignore]
        [Column("doc_authorization_id")]
        public JToken? doc_authorization_id { get; set; }

        [Column("doc_authorization_line_id_json")]
        public string? doc_authorization_line_id_json { get; set; }

        [Column("display_name")]
        public string? display_name { get; set; }


        [Column("starting_number")]
        public int? starting_number { get; set; }


        [Column("ending_number")]
        public int? ending_number { get; set; }

        [Column("next_number")]
        public int? next_number { get; set; }

        [Column("active")]
        public bool active { get; set; }

        [Column("type")]
        public string type { get; set; }

        [Column("create_date")]
        public DateTime? create_date { get; set; }

        [Column("write_date")]
        public DateTime? write_date { get; set; }
    }
}
