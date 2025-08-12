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

        public SqliteDbBase()
        {

        }

        protected async Task Init()
        {
            if (Database != null)
                return;

            Database = new SQLiteAsyncConnection(Constants.DatabasePath, Constants.Flags);
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

        public async Task<int> Truncate()
        {
            await Init();
            return await Database.DeleteAllAsync<T>();
        }
    }
}
