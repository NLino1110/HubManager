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
using Models.DMSA.Mbw.Clientes;
using Models.DMSA.Shared.General.v3;

namespace ResourceBuilder.Controllers.Customer
{
    [Route("api/[controller]")]
    [ApiController]
    public class OdooLopdpController : ControllerBase
    {
        DataSourceManager.AppDbContext appDbContext = new DataSourceManager.AppDbContext();
        
        [HttpGet]
        public async Task<ActionResult<IEnumerable<OdooLopdp>>> Get()
        {
            List<OdooLopdp> lresult = null;
            lresult = await appDbContext.ODDO_LOPDP.ToListAsync();
            return lresult;
        }
                
        [HttpPost]
        public async Task<ActionResult<ApiResponse_save>> Post([FromBody] OdooLopdp document)
        {
            ApiResponse_save apiResponse_Save_V3 = new ApiResponse_save();
            apiResponse_Save_V3.data = document;
            document.FECHAREGISTRO = DateTime.Now;

            try
            {
                await appDbContext.ODDO_LOPDP.AddAsync(document);
                await appDbContext.SaveChangesAsync();                
            }
            catch (Exception ex)
            {
                apiResponse_Save_V3.success = false;
                apiResponse_Save_V3.mensaje = "" + ex.Message;
                apiResponse_Save_V3.data = null;
            }
            return apiResponse_Save_V3;
        }

        [HttpPut()]
        public async Task<ActionResult<OdooLopdp>> Put([FromBody] OdooLopdp document)
        {
            appDbContext.ODDO_LOPDP.Update(document);
            await appDbContext.SaveChangesAsync();
            return document;
        }
    }
}
