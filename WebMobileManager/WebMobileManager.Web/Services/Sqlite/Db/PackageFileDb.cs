using DMSA.Models.Odoo.Abstract.Server;

namespace WebMobileManager.Web.Services.Sqlite
{
    public class PackageFileDb : SqliteDbBase<PackageFile>
    {
        public PackageFileDb() : base() { }
        public PackageFileDb(string db) : base(db) { }

        public async Task<List<PackageFile>> GetByPackage(int packageId)
        {
            await Init();
            return await Database.Table<PackageFile>()
                .Where(x => x.package_id == packageId)
                .ToListAsync();
        }

        public async Task<List<PackageFile>> GetByPackageIds(List<int> ids)
        {
            await Init();

            return await Database.Table<PackageFile>()
                .Where(f => ids.Contains(f.package_id))
                .ToListAsync();
        }
    }
}