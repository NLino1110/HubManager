using System;
using System.Collections.Generic;
using System.Text;

namespace DMSA.Models.Odoo.Abstract.Server.Dto
{
    public class PackageFileResponseDto
    {
        public int id { get; set; }
        public string file_name { get; set; }
        public string file_type { get; set; }
        public string url { get; set; }
        public DateTime uploaded_at { get; set; }
        public string status { get; set; }
    }
}
