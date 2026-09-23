using SQLite;
using System.Linq.Expressions;
using System.Reflection;
using static System.Runtime.InteropServices.Marshalling.IIUnknownCacheStrategy;

namespace DMSA.Sync.Core.Database.Sqlite
{
    public class SqliteDbBase<T> where T : new()
    {
        protected SQLiteAsyncConnection Database;

        protected virtual string TableName => typeof(T).Name;

        protected virtual string DatabaseFilename { get; set; }

        private bool _initialized = false;
        private readonly SemaphoreSlim _initLock = new(1, 1);

        public SqliteDbBase()
        {
            DatabaseFilename = Constants.DatabasePath;
        }

        public SqliteDbBase(string _DatabaseFilename)
        {
            DatabaseFilename = _DatabaseFilename;
        }

        public string GetDatabasePath()
        {
            return Path.Combine(FileSystem.AppDataDirectory, DatabaseFilename);
        }

        //protected async Task Init()
        //{
        //    if (Database != null)
        //        return;

        //    string DatabasePath = Path.Combine(FileSystem.AppDataDirectory, DatabaseFilename);

        //    Database = SqliteConnectionManager.GetConnection(DatabasePath, Constants.Flags);

        //    await Database.CreateTableAsync<T>();
        //}

        protected async Task Init()
        {
            if (_initialized)
                return;

            await _initLock.WaitAsync();

            try
            {
                if (_initialized)
                    return;

                if (Database == null)
                {
                    string DatabasePath = Path.Combine(FileSystem.AppDataDirectory, DatabaseFilename);
                    //Database = new SQLiteAsyncConnection(DatabasePath);
                    Database = SqliteConnectionManager.GetConnection(DatabasePath, Constants.Flags);
                    await Database.CreateTableAsync<T>();
                }

                await OnAfterInit();

                _initialized = true;
            }
            finally
            {
                _initLock.Release();
            }
        }

        protected virtual Task OnAfterInit()
        {
            return Task.CompletedTask;
        }

        public async Task<int> GetCount()
        {
            await Init();
            return (await Database.Table<T>().ToListAsync()).Count;
        }

        public async Task<List<T>> GetItemsAsync(Expression<Func<T, bool>> predicate)
        {
            await Init();
            return await Database.Table<T>().Where(predicate).ToListAsync();
        }

        public async Task<T> GetItemAsync(Expression<Func<T, bool>> predicate)
        {
            await Init();
            return await Database.Table<T>().Where(predicate).FirstOrDefaultAsync();
        }

        [Obsolete("No usar en movil")]
        public T GetItem(Func<T, bool> predicate)
        {
            Init().Wait(); // inicializa la base si no está lista
            return Database.GetConnection().Table<T>().ToList().FirstOrDefault(predicate);
        }

        /// <summary>
        /// MAX(write_date) o null si la tabla está vacía. Usado en reintento de detalle.
        /// </summary>
        public async Task<DateTime?> GetMaxWriteDateOrNullAsync()
        {
            await Init();

            var tableAttr = typeof(T).GetCustomAttributes(typeof(TableAttribute), true)
                .FirstOrDefault() as TableAttribute;
            string tableName = tableAttr?.Name ?? typeof(T).Name;

            try
            {
                return await Database.ExecuteScalarAsync<DateTime?>(
                    $"SELECT MAX(write_date) FROM {tableName}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error obteniendo MAX(write_date) de {tableName}: {ex.Message}");
                return null;
            }
        }

        public async Task<DateTime> GetLastWriteDateAsync(DateTime? defaultDate = null)
        {
            await Init();

            // Obtener el nombre de la tabla desde el atributo [Table]
            var tableAttr = typeof(T).GetCustomAttributes(typeof(TableAttribute), true)
                .FirstOrDefault() as TableAttribute;

            // Si no tiene el atributo, usa el nombre de la clase
            string tableName = tableAttr?.Name ?? typeof(T).Name;

            try
            {
                // Ejecutar consulta directa en SQLite
                string sql = $"SELECT MAX(write_date) FROM {tableName}";
                var result = await Database.ExecuteScalarAsync<DateTime?>(sql);

                // Si no hay resultados, usar el valor por defecto o fecha actual - 3 días
                if (result.HasValue)
                    return result.Value;

                return defaultDate ?? DateTime.UtcNow.AddDays(-3);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error obteniendo última WriteDate de {tableName}: {ex.Message}");
                return defaultDate ?? DateTime.UtcNow.AddDays(-3);
            }
        }

        public async Task<DateTime> GetSafeLastWriteDateAsync(int offset = 2)
        {
            await Init();

            var tableName = typeof(T)
                .GetCustomAttribute<TableAttribute>()?.Name
                ?? typeof(T).Name;

            try
            {
                var items = await Database.QueryAsync<T>(
                    $"SELECT * FROM {tableName} WHERE write_date IS NOT NULL ORDER BY write_date DESC LIMIT 1000");

                var prop = typeof(T).GetProperty("write_date",
                    BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);

                if (prop == null)
                    throw new Exception($"La entidad {typeof(T).Name} no tiene write_date");

                var dates = items
                    .Select(x =>
                    {
                        var value = prop.GetValue(x);
                        if (value == null) return (DateTime?)null;
                        var date = (DateTime) value;
                        return date.Date;
                    })
                    .Where(d => d.HasValue)
                    .Select(d => d.Value)
                    .Distinct()
                    .OrderByDescending(d => d)
                    .ToList();

                if (dates.Count > offset)
                    return dates[offset];

                if (dates.Count > 0)
                    return dates.Last();

                return DateTime.UtcNow.AddDays(-3);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error en {tableName}: {ex.Message}");
                return DateTime.UtcNow.AddDays(-3);
            }
        }

        public async Task<int> InsertAsync(T item)
        {
            await Init();
            return await Database.InsertAsync(item);
        }

        public async Task<int> InsertOrReplaceAsync(T item)
        {
            await Init();
            return await Database.InsertAsync(item, "OR REPLACE");
        }

        public async Task<int> InsertBatchAsync(IEnumerable<T> items)
        {
            await Init();
            await Database.InsertAllAsync(items, "OR REPLACE", true);
            return 0;
        }

        public async Task<int> InsertBatchControlAsync(IEnumerable<T> items)
        {
            await Init();

            foreach (var item in items)
            {
                try
                {
                    await Database.InsertOrReplaceAsync(item);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error insertando {typeof(T).Name}: {ex.Message}");
                }
            }

            return 0;
        }

        public async Task<int> UpdateAsync(T item)
        {
            await Init();
            return await Database.UpdateAsync(item);
        }

        public async Task<int> UpdateBatchAsync(IEnumerable<T> items)
        {
            await Init();
            return await Database.UpdateAllAsync(items, true);
        }

        public async Task<int> Truncate()
        {
            await Init();
            int deleted = await Database.DeleteAllAsync<T>();
            return deleted;
        }

        public async Task<int> DeleteAsync(T item)
        {
            await Init();
            return await Database.DeleteAsync(item);
        }

        public async Task<int> DeleteAllAsync(Expression<Func<T, bool>> predicate)
        {
            await Init();

            var matches = await Database.Table<T>().Where(predicate).ToListAsync();
            if (!matches.Any())
                return 0;

            int count = 0;

            await Database.RunInTransactionAsync(tran =>
            {
                foreach (var item in matches)
                {
                    tran.Delete(item);
                    count++;
                }
            });

            return count;
        }

        public async Task<IEnumerable<T>> QueryAsync(string query, object[] args)
        {
            await Init();
            return await Database.QueryAsync<T>(query, args);
        }

        public static async Task CloseDatabaseAsync()
        {
            //await SqliteConnectionManager.CloseAsync();
            await SqliteConnectionManager.CloseAllAsync();
        }

        public static async Task CloseDatabaseAsync(string dbPath)
        {
            //await SqliteConnectionManager.CloseAsync();
        }

        public async Task<IEnumerable<T>> SearchByColumnAsync(
            string columnName,
            string searchText)
        {
            await Init();
        
            var tableName = typeof(T)
                .GetCustomAttribute<TableAttribute>()?.Name
                ?? typeof(T).Name;

            var search = $"%{searchText}%";

            var query = $"SELECT * FROM {tableName} " +
                        $"WHERE {columnName} LIKE ? COLLATE NOCASE";

            return await Database.QueryAsync<T>(
                query,
                new object[] { search }
            );
        }

        public async Task<IEnumerable<T>> SearchByColumnAsync(
            string columnName,
            string searchText,
            int limit = 50)
        {
            await Init();

            var tableName = typeof(T)
                .GetCustomAttribute<TableAttribute>()?.Name
                ?? typeof(T).Name;

            var search = $"%{searchText}%";

            var query = $"SELECT * FROM {tableName} " +
                $"WHERE {columnName} LIKE ? COLLATE NOCASE " +
                $"LIMIT ?";

            return await Database.QueryAsync<T>(
                query,
                search,
                limit
            );
        }

        public async Task<IEnumerable<T>> SearchAsync(
            string searchText,
            params string[] columns)
        {
            await Init();

            var tableName = typeof(T)
                .GetCustomAttribute<TableAttribute>()?.Name
                ?? typeof(T).Name;

            var search = $"%{searchText}%";

            var where = string.Join(
                " OR ",
                columns.Select(c => $"{c} LIKE ? COLLATE NOCASE")
            );

            var args = columns.Select(_ => (object)search).ToArray();

            var query = $"SELECT * FROM {tableName} WHERE {where}";

            return await Database.QueryAsync<T>(query, args);
        }

        public async Task DropTableAsync()
        {
            await Init();
            var tableAttr = typeof(T)
                .GetCustomAttribute<TableAttribute>();
            string tableName = tableAttr?.Name ?? typeof(T).Name;
            string sql = $"DROP TABLE IF EXISTS {tableName}";
            await Database.ExecuteAsync(sql);
            Database = null;
        }

        public async Task Vaccum()
        {
            await Init();
            //await Database.ExecuteAsync("PRAGMA wal_checkpoint(TRUNCATE);");
            await Database.ExecuteAsync("VACUUM;");
            await Database.ExecuteAsync("ANALYZE;");
        }

        public void InvalidateConnection()
        {
            Database = null;
            _initialized = false;
        }

        public async Task<List<string>> GetTablesAsync()
        {
            await Init();
            var result = await Database.QueryAsync<TableInfo>(
                "SELECT name FROM sqlite_master WHERE type='table' AND name NOT LIKE 'sqlite_%';"
            );

            return result.Select(x => x.name).ToList();
        }

        public async Task DropTableAsync(string tableName)
        {
            await Init();
            await Database.ExecuteAsync($"DROP TABLE IF EXISTS {tableName}");            
        }
    }

    public class TableInfo
    {
        public string name { get; set; }
    }
}
