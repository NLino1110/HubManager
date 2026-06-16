using DMSA.Models.Odoo.Abstract.Server;
using DMSA.Models.Odoo.Abstract.Server.Dto;

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

        public async Task<List<Package>> GetAll(PackageFilterDto filter)
        {
            await Init();

            var query = Database.Table<Package>();

            if (filter != null)
            {
                if (!string.IsNullOrWhiteSpace(filter.Name))
                    query = query.Where(x => x.name == filter.Name);

                if (!string.IsNullOrWhiteSpace(filter.server))
                    query = query.Where(x => x.server == filter.server);

                if (!string.IsNullOrWhiteSpace(filter.database_name))
                    query = query.Where(x => x.database_name == filter.database_name);

                if (!string.IsNullOrWhiteSpace(filter.file_type))
                    query = query.Where(x => x.file_type == filter.file_type);

                if (!string.IsNullOrWhiteSpace(filter.mobile_app_id))
                    query = query.Where(x => x.mobile_app_id == filter.mobile_app_id);

                if (!string.IsNullOrWhiteSpace(filter.user_frontend))
                    query = query.Where(x => x.user_frontend == filter.user_frontend);

                if (!string.IsNullOrWhiteSpace(filter.external_guid))
                    query = query.Where(x => x.external_guid == filter.external_guid);

                if (filter.date_data_cutoff.HasValue)
                    query = query.Where(x => x.date_data_cutoff >= filter.date_data_cutoff.Value);

                if (filter.total_files_expected.HasValue)
                    query = query.Where(x => x.total_files_expected == filter.total_files_expected.Value);

                if (filter.total_file_size_expected.HasValue)
                    query = query.Where(x => x.total_file_size_expected == filter.total_file_size_expected.Value);

                if (filter.is_base.HasValue)
                    query = query.Where(x => x.is_base == filter.is_base.Value);
            }
            return await query.ToListAsync();
        }
    }
}