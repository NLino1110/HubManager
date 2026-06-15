using WebMobileManager.Web.Components.Pages.Packages;

namespace WebMobileManager.Web.Services.Interfaces
{
    public interface IPackageService
    {
        Task<(List<PackageViewDto> items, int total)> GetPackages(int page, int pageSize);
        Task DeletePackage(int id);
    }
}
