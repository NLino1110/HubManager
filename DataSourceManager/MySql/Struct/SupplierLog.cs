using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel.DataAnnotations;

namespace DataSourceManager.MySql.Struct
{
    public class SupplierLog
    {
        public int? id_vtex { get; set; }
        public string access_app { get; set; }
        public string access_ip { get; set; }
        public DateTime? aud_ins_date { get; set; }
    }
}
