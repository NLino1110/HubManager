namespace ResourceBuilder.Handlers.Models
{
    public partial class User
    {
        public string UserName { get; set; }
        public string EmailAddress { get; set; }
        public string Password { get; set; }
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
        public DateTime HireDate { get; set; }
    }
}
