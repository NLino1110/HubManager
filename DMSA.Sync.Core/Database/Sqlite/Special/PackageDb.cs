using DMSA.Models.Odoo.Abstract.Server;
using DMSA.Models.Odoo.Specials;

namespace DMSA.Sync.Core.Database.Sqlite.Special
{
    public class PackageDb : SqliteDbBase<Package>
    {
        public PackageDb(string db) : base(db) { }

        public async Task<Package> GetByName(string name)
        {
            await Init();
            return await Database.Table<Package>()
                .Where(x => x.name == name)
                .FirstOrDefaultAsync();
        }

        public async Task<List<Package>> GetAll()
        {
            await Init();
            return await Database.Table<Package>().ToListAsync();
        }

        public async Task<Package> GetLastUpdate()
        {
            await Init();
            var q = Database.Table<Package>();
            q.OrderByDescending(x => x.date_data_cutoff);
            return await q.FirstOrDefaultAsync();
        }

        public async Task<Package> GetLastUpdate(string dbPath)
        {
            await Init();
            var q = Database.Table<Package>().Where(x => x.file_name == dbPath);
            q.OrderByDescending(x => x.date_data_cutoff);
            return await q.FirstOrDefaultAsync();
        }
    }
}