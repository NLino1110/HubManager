using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CobranzasDMSA;

namespace ResourceBuilder.Data.Sqlite
{
    public static class Constants
    {
        public const string DatabaseFilename = "ResourceBuilder.db3";

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
