namespace WebMobileManager.Web.Handlers.Models
{
    public class MobileAppDto
    {
        public string Name { get; set; }
        public string PackageId { get; set; }
        public string Version { get; set; }
        public bool IsActive { get; set; }

        public List<AppFileDto> Files { get; set; } = new();
    }

    public class AppFileDto
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public PlatformType Platform { get; set; }
        public string FileName { get; set; }
    }

    public enum PlatformType
    {
        Android,
        iOS,
        Windows,
    }
}
