using SQLite;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace DMSA.Models.Odoo.Abstract
{
    [Table("global_settings")]
    public class GlobalSettings
    {
        [PrimaryKey]
        [Column("id")]
        public int Id {  get; set; }
        
        [Required]
        [Column("push_server")]
        public string PushServer { get; set; }

        [Required]
        [Column("package_server")]
        public string PackageServer { get; set; }        
    }
}
