using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel.DataAnnotations;

namespace ResourceBuilder.Data.Structs.SyncTask
{
    public class SupplierLog
    {
        public int? id_vtex { get; set; }
        public string access_app { get; set; }
        public string access_ip { get; set; }
        public DateTime? aud_ins_date { get; set; }
    }
}
