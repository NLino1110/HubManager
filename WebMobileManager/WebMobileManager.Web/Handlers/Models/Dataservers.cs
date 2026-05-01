namespace WebMobileManager.Web.Handlers.Models
{
    public class Dataservers
    {
        public string IdConnection { get; set; }
        public string Server { get; set; }
        public string Port { get; set; }
        public string User { get; set; }
        public string Password { get; set; }
        public string Database { get; set; }
        public string Driver { get; set; }
        public string Type { get; set; }
        public bool default_setup { get; set; }
    }
}
