using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
//using DataSource.Db;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using System.Text;
using System.Security.Claims;
//using System.IdentityModel.Tokens.Jwt;
//using Microsoft.IdentityModel.Tokens;
using System.Security.Cryptography;
using System.Data;
using System.IO;
using Models.DMSA.Mbw.Clientes;

namespace ResourceBuilder.Controllers.Customer
{
    [Route("api/[controller]")]
    [ApiController]
    public class AprobacionesController : ControllerBase
    {
        DataSourceManager.AppDbContext appDbContext = new DataSourceManager.AppDbContext();

        [HttpGet("GetByStatus/{status:int}")]
        public async Task<ActionResult<IEnumerable<ClienteAprobacion>>> GetByStatus(int status)
        {
            List<ClienteAprobacion> lresult = null;
            lresult = await appDbContext.GENCLIENTEAPROBACION.Where(c=>c.CODESTADO == status).ToListAsync();
            return lresult;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ClienteAprobacion>>> Get()
        {
            List<ClienteAprobacion> lresult = null;
            lresult = await appDbContext.GENCLIENTEAPROBACION.ToListAsync();
            return lresult;
        }
                
        [HttpPost]
        public async Task<ActionResult<ClienteAprobacion>> Post([FromBody] ClienteAprobacion document)
        {
            await appDbContext.GENCLIENTEAPROBACION.AddAsync(document);
            await appDbContext.SaveChangesAsync();
            return document;
        }

        [HttpPut()]
        public async Task<ActionResult<ClienteAprobacion>> Put([FromBody] ClienteAprobacion document)
        {
            appDbContext.GENCLIENTEAPROBACION.Update(document);
            await appDbContext.SaveChangesAsync();
            return document;
        }
    }
}
