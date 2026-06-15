using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace DMSA.Models.Odoo.Abstract.Server.Dto
{
    public class CreatePackageDto
    {
        [JsonProperty("name")]
        public string Name { get; set; }
        public string? server { get; set; }
        public string? database_name { get; set; }
        public string file_name { get; set; }
        public string file_type { get; set; }
        public DateTime? date_data_cutoff { get; set; }
        public string? mobile_app_id { get; set; }
        public string? user_frontend { get; set; }
        public int? total_files_expected { get; set; }
        public int? total_file_size_expected { get; set; }
        public string external_guid { get; set; }
    }
}
