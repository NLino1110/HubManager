using DMSA.Models.Odoo.Abstract.Server;

namespace WebMobileManager.Web.Services.Sqlite
{
    public class PackageDb : SqliteDbBase<Package>
    {
        public PackageDb() : base() { }
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

        public async Task<List<PackageFile>> GetByPackageIds(List<int> ids)
        {
            await Init();

            return await Database.Table<PackageFile>()
                .Where(f => ids.Contains(f.package_id))
                .ToListAsync();
        }

        public async Task<List<Package>> GetPaged(int skip, int take)
        {
            await Init();

            return await Database.Table<Package>()
                .OrderByDescending(x => x.created_at)
                .Skip(skip)
                .Take(take)
                .ToListAsync();
        }

        public async Task<int> Count()
        {
            await Init();
            return await Database.Table<Package>().CountAsync();
        }


    }
}