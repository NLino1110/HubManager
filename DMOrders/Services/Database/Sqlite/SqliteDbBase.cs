using DMSA.Models.Odoo.DMCobranzas;
using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            
            string DatabasePath = Path.Combine(FileSystem.AppDataDirectory, DatabaseFilename);

            //Database = new SQLiteAsyncConnection(Constants.DatabasePath, Constants.Flags);
            Database = new SQLiteAsyncConnection(DatabasePath, Constants.Flags);
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
                string sql = $"SELECT MAX(WriteDate) FROM {tableName}";
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
            await Database.InsertAsync(item);
            return 0;
        }

        public async Task<int> InsertBatchAsync(IEnumerable<T> items)
        {
            await Init();
            await Database.InsertAllAsync(items, "OR REPLACE", true);
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
    }
}
