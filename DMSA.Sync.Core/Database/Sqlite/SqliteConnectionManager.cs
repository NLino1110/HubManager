using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMSA.Sync.Core.Database.Sqlite
{
    public static class SqliteConnectionManager
    {
        private static SQLiteAsyncConnection? _connection;
        private static string? _dbPath;

        public static SQLiteAsyncConnection GetConnection(string dbPath, SQLiteOpenFlags flags)
        {
            if (_connection == null || _dbPath != dbPath)
            {
                _dbPath = dbPath;
                _connection = new SQLiteAsyncConnection(dbPath, flags);
            }

            return _connection;
        }

        public static async Task CloseAsync()
        {
            if (_connection != null)
            {
                try
                {
                    await _connection.CloseAsync();
                }
                catch
                {
                    // ignorar: SQLite en Android a veces ya está cerrada
                }
                finally
                {
                    _connection = null;
                    _dbPath = null;
                }
            }
        }
    }

}
