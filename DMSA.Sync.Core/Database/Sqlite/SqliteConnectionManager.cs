using SQLite;

////namespace DMSA.Sync.Core.Database.Sqlite
////{
////    public static class SqliteConnectionManager
////    {
////        private static SQLiteAsyncConnection? _connection;
////        private static string? _dbPath;

////        public static SQLiteAsyncConnection GetConnection(string dbPath, SQLiteOpenFlags flags)
////        {
////            if (_connection == null || _dbPath != dbPath)
////            {
////                _dbPath = dbPath;
////                _connection = new SQLiteAsyncConnection(dbPath, flags);
////            }

////            return _connection;
////        }

////        public static async Task CloseAsync()
////        {
////            if (_connection != null)
////            {
////                try
////                {
////                    await _connection.CloseAsync();
////                }
////                catch
////                {
////                    // ignorar: SQLite en Android a veces ya está cerrada
////                }
////                finally
////                {
////                    _connection = null;
////                    _dbPath = null;
////                }
////            }
////        }
////    }
////}



public static class SqliteConnectionManager
{
    private static readonly Dictionary<string, SQLiteAsyncConnection> _connections = new();

    public static SQLiteAsyncConnection GetConnection(string dbPath, SQLiteOpenFlags flags)
    {
        if (!_connections.TryGetValue(dbPath, out var conn))
        {
            conn = new SQLiteAsyncConnection(dbPath, flags);
            _connections[dbPath] = conn;
        }

        return conn;
    }

    public static async Task CloseAllAsync()
    {
        foreach (var conn in _connections.Values)
        {
            try
            {
                await conn.CloseAsync();
            }
            catch
            {
                // ignorar
            }
        }

        _connections.Clear();
    }

    public static async Task CloseAsync(string dbPath)
    {
        if (_connections.TryGetValue(dbPath, out var conn))
        {
            try
            {
                await conn.CloseAsync();
            }
            catch { }

            _connections.Remove(dbPath);
        }
    }
}