using DMSA.Models.Odoo.Abstract.Server;

namespace DMSA.Sync.Core.Database.Sqlite.Special
{
    public class PackageFileDb : SqliteDbBase<PackageFile>
    {
        public PackageFileDb(string db) : base(db) { }

        public async Task<List<PackageFile>> GetByPackage(int packageId)
        {
            await Init();
            return await Database.Table<PackageFile>()
                .Where(x => x.package_id == packageId)
                .ToListAsync();
        }
    }
}