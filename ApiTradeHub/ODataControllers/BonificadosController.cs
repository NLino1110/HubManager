using DataSourceManager;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Results;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Models.DMSA.Mbw.Sales;
using Models.DMSA.Shared.Structs;

namespace ApiTradeHub.ODataControllers
{
    [Microsoft.AspNetCore.Components.Route("odata/[controller]")]
    public class BonificadosController : ODataController
    {
        private readonly AppDbContext _context;

        public BonificadosController(AppDbContext context)
        {
            _context = context;
        }

        [EnableQuery]
        public IActionResult Get([FromQuery] int? Page = 1, [FromQuery] int? PageSize = 10)
        {
            if (Page <= 0 || PageSize <= 0)
            {
                return BadRequest("Page and PageSize must be greater than zero.");
            }

            var skip = (Page.Value - 1) * PageSize.Value;

            var query = _context.FACBONIFICADOSXARTICULO.Skip(skip).Take(PageSize.Value);

            var totalRecords = _context.FACBONIFICADOSXARTICULO.Count();
            var metadata = new
            {
                TotalRecords = totalRecords,
                TotalPages = (int)Math.Ceiling(totalRecords / (double)PageSize.Value),
                CurrentPage = Page,
                PageSize = PageSize
            };
            
            return Ok(new { Metadata = metadata, Data = query });
        }
    }
}
