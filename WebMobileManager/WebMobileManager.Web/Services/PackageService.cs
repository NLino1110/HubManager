using DMSA.Models.Odoo.Abstract.Server;
using WebMobileManager.Web.Components.Pages.Packages;
using WebMobileManager.Web.Services.Interfaces;
using WebMobileManager.Web.Services.Sqlite;

namespace WebMobileManager.Web.Services
{
    public class PackageService : IPackageService
    {
        private readonly PackageDb _packageDb;
        private readonly PackageFileDb _fileDb;

        public PackageService(PackageDb packageDb, PackageFileDb fileDb)
        {
            _packageDb = packageDb;
            _fileDb = fileDb;
        }

        public async Task<(List<PackageViewDto>, int)> GetPackages(int page, int pageSize)
        {
            var skip = (page - 1) * pageSize;

            // ahora el servicio NO toca SQLite directamente
            var packages = await _packageDb.GetPaged(skip, pageSize);
            var total = await _packageDb.Count();

            var ids = packages.Select(x => x.id).ToList();

            var files = await _fileDb.GetByPackageIds(ids);

            var result = packages.Select(p => new PackageViewDto
            {
                Id = p.id,
                Name = p.name,
                database_name = p.database_name,
                file_name = p.file_name,
                success_upload = p.success_upload,
                processing_state = p.processing_state,
                created_at = p.created_at,

                Files = files
                    .Where(f => f.package_id == p.id)
                    .Select(f => new PackageFileDto
                    {
                        file_name = f.file_name,
                        file_type = f.file_type,
                        status = f.status,
                        success_upload = f.success_upload
                    }).ToList()
            }).ToList();

            return (result, total);
        }

        public async Task DeletePackage(int id)
        {            
            var files = await _fileDb.GetItemsAsync(x=> x.package_id == id);
            var related = files.Where(f => f.package_id == id).ToList();

            foreach (var file in related)
            {
                await _fileDb.DeleteAsync(file);
            }
            
            var pkg = (await _packageDb.GetItemsAsync(x=> x.id == id)).FirstOrDefault();

            if (pkg != null)
            {
                var folderPath = Path.Combine(
                        Directory.GetCurrentDirectory(),
                        "wwwroot",
                        "uploads",
                        "zips",
                        pkg.name
                    );

                try
                {
                    if (Directory.Exists(folderPath))
                    {
                        Directory.Delete(folderPath, true);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error borrando carpeta: {ex.Message}");
                }

                await _packageDb.DeleteAsync(pkg);
            }
        }
    }
}
