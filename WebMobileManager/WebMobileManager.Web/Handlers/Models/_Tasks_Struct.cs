namespace WebMobileManager.Web.Handlers.Models
{
    public class _Tasks_Struct
    {
        public string name { get; set; }
        public string? description { get; set; }
        public string schedule { get; set; }
        public bool enabled { get; set; }
        public ItemBuild[] items { get; set; }
    }
}
