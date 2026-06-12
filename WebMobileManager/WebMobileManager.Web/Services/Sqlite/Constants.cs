using DMSA.Models.Security;

namespace WebMobileManager.Web.Services.Sqlite
{
    public static class Constants
    {
        public static AppSession Session { get; set; }

        public const string DatabaseFilename = "webmobilemanager.db3";

        public const SQLite.SQLiteOpenFlags Flags =            
            SQLite.SQLiteOpenFlags.ReadWrite |            
            SQLite.SQLiteOpenFlags.Create |            
            SQLite.SQLiteOpenFlags.SharedCache;

        public static string DatabasePath =>
            Path.Combine(Path.Combine(Directory.GetCurrentDirectory(),
                "wwwroot",
                "data",
                "sqlite"),
                DatabaseFilename);
    }
}
