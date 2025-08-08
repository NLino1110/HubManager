using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMSA.Models.Odoo.Update
{
    public class update_pack_info
    {
        public DateTime pack_base_date { get; set; }
        public string description { get; set; }
        public Detail[] details { get; set; }
    }

    public class Detail
    {
        public string model { get; set; }
        public int total_files { get; set; }
        public fileData[] files { get; set; }        
    }

    public class fileData
    {
        public string name { get; set; }
        public string hash { get; set; }
        public DateTime create_date { get; set; }
        public int year { get; set; }
        public int month { get; set; }
        public int day { get; set; }
    }
}
