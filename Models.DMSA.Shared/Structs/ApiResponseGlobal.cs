using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.DMSA.Shared.Structs
{
    public class ApiResponseGlobal
    {
        public bool success { get; set; }
        public string message { get; set; }
        public int responseCode { get; set; }
        public string object_name { get; set; }
        public int count { get; set; }
        public int create_id { get; set; }
        public int pages_total { get; set; }
        public int page { get; set; }
        public string info { get; set; }
        public DateTime date { get; } = DateTime.Now;
    }
}
