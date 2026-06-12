using SQLite;

namespace WebMobileManager.Web.Services.Sqlite;

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