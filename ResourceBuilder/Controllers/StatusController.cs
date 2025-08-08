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

namespace ResourceBuilder.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StatusController : ControllerBase
    {
        //DataSourceManager.AppDbContext appDbContext = new DataSourceManager.AppDbContext();

        [HttpGet("checkonline")]
        public async Task<ActionResult> CheckOnline()
        {
            var result = new
            {
                responseCode = 200,
                success = true,
                message = "Everything ok",
                data = new[]
                {
                    new {
                        status = "online",
                        descripcion = "Builder Server is running"
                    }
                }
            };

            return Ok(result);
        }
    }
}
