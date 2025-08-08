using DataSourceManager;
using Microsoft.EntityFrameworkCore;
using Models.DMSA.Mbw.Inventario;
using System.Data.Entity;

namespace ResourceBuilder.Services.Inventory
{
    public class BrandService
    {
        private readonly AppDbContext _context;
        //public BrandService(AppDbContext context)
        //{
        //    _context = context;
        //}

        public BrandService()
        {
            _context = new DataSourceManager.AppDbContext();
        }

        public async Task<IEnumerable<GenMarca>> GetDataAsync()
        {
            var dataSource = _context.GENMARCAS.Where(c => c.Descripcion.Contains("") && c.CodEmpresaMarca == 1).AsEnumerable();

            return dataSource;
        }
    }
}
