using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMSA.Models.Odoo
{
    public class SyncDetails
    {
        public string? model_name {  get; set; }
        public int year { get; set; }
        public int month { get; set; }
        public int day { get; set; }
        public DateTime date_sync { get; set; }
        public string? status { get; set; }
    }
}
