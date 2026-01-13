using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMSA.Models.Odoo.General.Responses
{
    [Obsolete]
    public class ApiResponseOdoo
    {
        public bool success { get; set; }
        public string message { get; set; }
        public int responseCode { get; set; }
        public string api_key { get; set; }
        public string fields { get; set; }
        public string domain { get; set; }
        public string object_name { get; set; }
        public int count { get; set; }
        public Permisssions permisssions { get; set; }
        
        public int model_id { get; set; }
        
        //[Ignore]
        //[JsonIgnore]
        public int create_id { get; set; }
    }

    public class Permisssions
    {
        public bool read { get; set; }
        public bool write { get; set; }
        public bool delete { get; set; }
        public bool create { get; set; }
    }
}
