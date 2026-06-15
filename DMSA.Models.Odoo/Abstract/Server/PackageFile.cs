using SQLite;

namespace DMSA.Models.Odoo.Abstract.Server
{
    [Table("package_files")]
    public class PackageFile
    {
        [PrimaryKey, AutoIncrement]
        public int id { get; set; }

        [Indexed]
        public int package_id { get; set; }

        public string file_name { get; set; }
        public string file_type { get; set; }

        public string file_path { get; set; }
        public string url { get; set; }
        public int total_file_size_expected { get; set; }
        public bool success_upload { get; set; }

        public DateTime uploaded_at { get; set; }

        public string status { get; set; } // Uploaded, Failed

        public string error { get; set; }
    }
}
