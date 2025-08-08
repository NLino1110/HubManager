using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using System.Text;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Security.Cryptography;
using Models.DMSA.Mbw.Clientes;
using System.Data;
//using Entidades.SyncTask;
using Models.DMSA.Shared.General;
using System.Data.SqlTypes;
using System.Text.RegularExpressions;
using System.Diagnostics;
using Models.DMSA.Shared.General.v3;
using ApiTradeHub.Attributes;
using ApiTradeHub.Middleware;
using System.Reflection.Metadata;
using Oracle.ManagedDataAccess.Client;

namespace ApiTradeHub.Controllers.AppStore
{
    //[ApiKey]
    [Route("api/[controller]")]
    [ApiController]
    public class StorageController : ControllerBase
    {
        DataSourceManager.AppDbContext appDbContext = new DataSourceManager.AppDbContext();

        [HttpGet("GetById/{id}")]
        public async Task<ActionResult<ApiResponse_v1>> GetById(string id)
        {
            ApiResponse_v1 apiResponse_V1 = new ApiResponse_v1();
            apiResponse_V1.success = true;
            apiResponse_V1.final = true;
            apiResponse_V1.exito = true;
            apiResponse_V1.mensaje = "Proceso realizado exitosamente";
            List<ClienteAprobacion> lresult = null;
            lresult = await appDbContext.GENCLIENTEAPROBACION.Where(c => c.IDENTIFICACION == id).ToListAsync();
            apiResponse_V1.data = lresult;
            apiResponse_V1.cantidad_registros = lresult.Count;
            return apiResponse_V1;
        }
                
        //52	Autorizado
        //54	Por Autorizar
        [HttpGet("GetByStatus/{status:int}")] 
        public async Task<ActionResult<ApiResponse_v1>> GetByStatus(int status)
        {
            ApiResponse_v1 apiResponse_V1 = new ApiResponse_v1();
            apiResponse_V1.success = true;
            apiResponse_V1.final = true;
            apiResponse_V1.exito = true;
            apiResponse_V1.mensaje = "Proceso realizado exitosamente";
            List<ClienteAprobacion> lresult = null;
            lresult = await appDbContext.GENCLIENTEAPROBACION.Where(c => c.CODESTADO == status).ToListAsync();
            apiResponse_V1.data = lresult;
            apiResponse_V1.cantidad_registros = lresult.Count;
            return apiResponse_V1;
        }

        [HttpGet]
        public async Task<ActionResult<ApiResponse_v1>> Get()
        {
            //this.ControllerContext.HttpContext

            ApiResponse_v1 apiResponse_V1 = new ApiResponse_v1();
            apiResponse_V1.success = true;
            apiResponse_V1.final = true;
            apiResponse_V1.exito = true;
            apiResponse_V1.mensaje = "Proceso realizado exitosamente";
            List<ClienteAprobacion> lresult = null;
            lresult = await appDbContext.GENCLIENTEAPROBACION.ToListAsync();
            apiResponse_V1.data = lresult;
            apiResponse_V1.cantidad_registros = lresult.Count;
            return apiResponse_V1;
        }

        [HttpPost]
        public async Task<ActionResult<ApiResponse_v1>> Post([FromBody] ClienteAprobacion document)
        {
            ApiResponse_v1 apiResponse_Save_V3 = new ApiResponse_v1();
            apiResponse_Save_V3.cantidad_registros = 0;
            apiResponse_Save_V3.final = true;
            apiResponse_Save_V3.success = true;
            apiResponse_Save_V3.exito = true;
            apiResponse_Save_V3.mensaje = "Proceso realizado exitosamente";

            document.TIPOIDENTIFICACION = document.TIPOIDENTIFICACION?.ToUpper();
            document.NOMBRESCLIENTE = document.NOMBRESCLIENTE?.ToUpper();
            document.APELLIDOSCLIENTE = document.APELLIDOSCLIENTE?.ToUpper();
            document.DIRECCIONCLIENTE = document.DIRECCIONCLIENTE?.ToUpper();
            document.EMAILCLIENTE = document.EMAILCLIENTE?.ToLower();
            document.TELEFONOCLIENTE = document.TELEFONOCLIENTE;
            document.FECHACAMBIOESTADO = DateTime.Now;
            apiResponse_Save_V3.data = document;

            try
            {
                var foundItem = appDbContext.GENCLIENTEAPROBACION.AsNoTracking().Where(i=>i.IDENTIFICACION == document.IDENTIFICACION).FirstOrDefault();
                
                if (foundItem!=null)
                {
                    appDbContext.GENCLIENTEAPROBACION.Update(document);
                    await appDbContext.SaveChangesAsync();
                }
                else
                {
                    await appDbContext.GENCLIENTEAPROBACION.AddAsync(document);
                    await appDbContext.SaveChangesAsync();
                }
                
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
        public async Task<ActionResult<ApiResponse_v1>> Put([FromBody] ClienteAprobacion document)
        {
            ApiResponse_v1 apiResponse_Save_V3 = new ApiResponse_v1();
            apiResponse_Save_V3.cantidad_registros = 0;
            apiResponse_Save_V3.final = true;
            apiResponse_Save_V3.success = true;
            apiResponse_Save_V3.exito = true;
            apiResponse_Save_V3.mensaje = "Proceso realizado exitosamente";

            try
            {
                document.TIPOIDENTIFICACION = document.TIPOIDENTIFICACION?.ToUpper();
                document.NOMBRESCLIENTE = document.NOMBRESCLIENTE?.ToUpper();
                document.APELLIDOSCLIENTE = document.APELLIDOSCLIENTE?.ToUpper();
                document.DIRECCIONCLIENTE = document.DIRECCIONCLIENTE?.ToUpper();
                document.EMAILCLIENTE = document.EMAILCLIENTE?.ToLower();
                document.FECHACAMBIOESTADO = DateTime.Now;
                apiResponse_Save_V3.data = document;
            }
            catch (Exception ex)
            {
                apiResponse_Save_V3.success = false;
                apiResponse_Save_V3.mensaje = "" + ex.InnerException?.Message;
                apiResponse_Save_V3.data = null;
            }

            appDbContext.GENCLIENTEAPROBACION.Update(document);
            await appDbContext.SaveChangesAsync();
            return apiResponse_Save_V3;
        }
                
        //public async Task UpdateSpecial(ClienteAprobacion document)
        //{
        //    ApiResponse_v1 apiResponse_Save_V3 = new ApiResponse_v1();
        //    apiResponse_Save_V3.cantidad_registros = 0;
        //    apiResponse_Save_V3.final = true;
        //    apiResponse_Save_V3.success = true;
        //    apiResponse_Save_V3.exito = true;
        //    apiResponse_Save_V3.mensaje = "Proceso realizado exitosamente";

        //    try
        //    {
        //        document.TIPOIDENTIFICACION = document.TIPOIDENTIFICACION?.ToUpper();
        //        document.NOMBRESCLIENTE = document.NOMBRESCLIENTE?.ToUpper();
        //        document.APELLIDOSCLIENTE = document.APELLIDOSCLIENTE?.ToUpper();
        //        document.DIRECCIONCLIENTE = document.DIRECCIONCLIENTE?.ToUpper();
        //        document.EMAILCLIENTE = document.EMAILCLIENTE?.ToLower();
        //        document.FECHACAMBIOESTADO = DateTime.Now;
        //        apiResponse_Save_V3.data = document;
        //    }
        //    catch (Exception ex)
        //    {
        //        apiResponse_Save_V3.success = false;
        //        apiResponse_Save_V3.mensaje = "" + ex.InnerException?.Message;
        //        apiResponse_Save_V3.data = null;
        //    }

        //    appDbContext.GENCLIENTEAPROBACION.Update(document);
        //    await appDbContext.SaveChangesAsync();
            

        //}
    }
}
