using DMSA.Sync.Core;
using SQLite;

namespace DMSA.Sync.Core
{
    public class FileSystemClient
    {
        static public string AppDataDirectory { get; set; }
    }
}

namespace DMOrders.Services.Database.Sqlite
{
    public class SqliteDbBase<T> where T : new()
    {
        protected SQLiteAsyncConnection Database;

        protected virtual string TableName => typeof(T).Name;

        protected virtual string DatabaseFilename { get; set; }

        public SqliteDbBase(string _DatabaseFilename)
        {
            DatabaseFilename = _DatabaseFilename;
        }

        protected async Task Init()
        {
            if (Database != null)
                return;
            
            string DatabasePath = Path.Combine(FileSystemClient.AppDataDirectory, DatabaseFilename);

            //Database = new SQLiteAsyncConnection(Constants.DatabasePath, Constants.Flags);
            Database = new SQLiteAsyncConnection(DatabasePath, Constants.Flags);

            //try
            //{
            //    await Database.ExecuteAsync("PRAGMA journal_mode=WAL;");
            //    await Database.ExecuteAsync("PRAGMA synchronous=NORMAL;");
            //    await Database.ExecuteAsync("PRAGMA temp_store=MEMORY;");
            //    await Database.ExecuteAsync("PRAGMA foreign_keys=ON;"); // por si usas claves foráneas
            //}
            //catch (Exception ex)
            //{
            //    Console.WriteLine($"Error aplicando PRAGMAs en SQLite: {ex.Message}");
            //}

            await Database.CreateTableAsync<T>();
        }

        public async Task<int> GetCount()
        {
            await Init();
            return (await Database.Table<T>().ToListAsync()).Count;
        }

        public async Task<List<T>> GetItemsAsync(Func<T, bool> predicate)
        {
            await Init();
            return (await Database.Table<T>().ToListAsync()).Where(predicate).ToList();
        }

        public async Task<T> GetItemAsync(Func<T, bool> predicate)
        {
            await Init();
            return (await Database.Table<T>().ToListAsync()).FirstOrDefault(predicate);
        }

        [Obsolete("No usar en movil")]
        public T GetItem(Func<T, bool> predicate)
        {
            Init().Wait(); // inicializa la base si no está lista
            return Database.GetConnection().Table<T>().ToList().FirstOrDefault(predicate);
        }

        //public async Task<DateTime?> GetLastWriteDateAsync(Func<T, bool>? predicate = null)
        //{
        //    await Init();
        //    var table = await Database.Table<T>().ToListAsync();

        //    if (predicate != null)
        //        table = table.Where(predicate).ToList();

        //    // Si la clase T tiene una propiedad WriteDate
        //    var lastDate = table
        //        .Select(x => (DateTime?)typeof(T).GetProperty("WriteDate")?.GetValue(x))
        //        .Where(x => x.HasValue)
        //        .OrderByDescending(x => x.Value)
        //        .FirstOrDefault();

        //    return lastDate;
        //}

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

        public async Task<int> InsertAsync(T item)
        {
            await Init();
            return await Database.InsertAsync(item);
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

        public async Task<int> Truncate()
        {
            await Init();
            return await Database.DeleteAllAsync<T>();
        }

        public async Task<int> DeleteAsync(T item)
        {
            await Init();
            return await Database.DeleteAsync(item);
        }
    }
}
