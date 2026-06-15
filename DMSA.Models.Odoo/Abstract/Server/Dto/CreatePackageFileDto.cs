using System;
using System.Collections.Generic;
using System.Text;

namespace DMSA.Models.Odoo.Abstract.Server.Dto
{
    public class CreatePackageFileDto
    {
        //public IFormFile File { get; set; }
        public string file_name { get; set; }
        public string? file_type { get; set; }
        public string? url { get; set; }
        public int? total_file_size_expected { get; set; }
        public bool? success_upload { get; set; }
        public string? error { get; set; }
    }
}
