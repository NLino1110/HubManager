using SQLite;

namespace DMSA.Models.Odoo.Abstract.Server
{
    [Table("packages")]
    public class Package
    {
        [PrimaryKey, AutoIncrement]
        public int id { get; set; }

        [Indexed]
        public string name { get; set; }
        public string server { get; set; }
        public string database_name { get; set; }
        public string file_name { get; set; }
        public string file_type { get; set; }
        public DateTime date_data_cutoff { get; set; }
        public string mobile_app_id { get; set; }
        public string user_frontend { get; set; }
        public int total_files_expected { get; set; }
        public int total_file_size_expected { get; set; }
        public int uploaded_files { get; set; }
        public bool success_upload { get; set; }
        public string external_guid { get; set; }

        //total_file_size

        public DateTime created_at { get; set; }

        public string processing_state { get; set; }        

        public string last_error { get; set; }
    }
}
