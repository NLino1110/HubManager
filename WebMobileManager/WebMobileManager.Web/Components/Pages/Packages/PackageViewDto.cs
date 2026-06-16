using WebMobileManager.Web.Services.Sqlite;

namespace WebMobileManager.Web.Components.Pages.Packages
{
    public class PackageViewDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string database_name { get; set; }
        public string file_name { get; set; }
        public bool success_upload { get; set; }
        public string processing_state { get; set; }

        public int total_files_expected { get; set; }
        public int total_file_size_expected { get; set; }

        public DateTime created_at { get; set; }

        public List<PackageFileDto> Files { get; set; } = new();
    }
}
