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
using Models.DMSA.Mbw.Clientes;
using System.Data;
//using Entidades.SyncTask;
using System.IO;
using Models.DMSA.Mbw.Sales;
using ApiTradeHub.Controllers.Security;
using Models.DMSA.Shared.Structs;
using ApiTradeHub.Services.Sales;

namespace ApiTradeHub.Controllers.Sales
{
    [Route("api/[controller]")]
    [ApiController]
    public class FacPedidoDetController : ControllerBase
    {
        DataSourceManager.AppDbContext appDbContext = new DataSourceManager.AppDbContext();

        [HttpGet()]
        public async Task<ActionResult<ApiResponseFacPedidoDet>> Get(
            [FromQuery] int? CodAgencia = null,
            [FromQuery] string? TipoPedido = null,
            [FromQuery] int? NumPedido = null,
            [FromHeader(Name = "X-API-Key")] string apiKey = null)
        {
            if (!AccessValidator.IsValidApiKey(apiKey))
            {   
                return Unauthorized(AccessValidator.BuildUnauthorized());
            }

            var query = appDbContext.FACPEDIDODET.AsQueryable();

            if (CodAgencia.HasValue)
            {
                query = query.Where(p => p.CodAgencia == CodAgencia.Value);
            }

            if (!string.IsNullOrEmpty(TipoPedido))
            {
                query = query.Where(p => p.TipoPedido == TipoPedido);
            }

            if (NumPedido.HasValue)
            {
                query = query.Where(p => p.NumPedido == NumPedido.Value);
            }

            var lresult = await query.Take(100).ToListAsync();

            return new ApiResponseFacPedidoDet()
            {
                count = lresult.Count,
                create_id = 0,
                message = "",
                info = "{'data':''}",
                object_name = "",
                responseCode = 200,
                success = true,
                data = lresult.ToArray()
            };
        }
                
        [HttpPost]
        public async Task<ActionResult<ApiResponseFacPedido>> Post(
            [FromBody] FacPedido document,
            [FromHeader(Name = "X-API-Key")] string apiKey = null)
        {
            if (!AccessValidator.IsValidApiKey(apiKey))
            {
                return Unauthorized(AccessValidator.BuildUnauthorized());
            }

            FacPedidoService facPedidoService = new FacPedidoService(appDbContext);

            document.NumPedido = facPedidoService.ObtenerNumeroProformaAsync(document.CodAgencia);
            document.FechaRegistro = DateTime.Now;

            //Agregar lógica para los lotes

            await appDbContext.FACPEDIDOS.AddAsync(document);
            await appDbContext.SaveChangesAsync();
            //return document;

            return new ApiResponseFacPedido()
            {
                count = 1,
                create_id = document.NumPedido,
                message = "",
                info = "{'data':''}",
                object_name = "",
                responseCode = 200,
                success = true,
                data = [document]
            };
        }

        [HttpPut()]
        public async Task<ActionResult<FacPedido>> Put([FromBody] FacPedido document)
        {
            //appDbContext.FACPEDIDOS.Update(document);
            //await appDbContext.SaveChangesAsync();
            return document;
        }

        [HttpDelete("{CodAgencia}/{NumPedido}")]
        public async Task<ActionResult<ApiResponseFacPedido>> Delete(
            int NumPedido, int CodAgencia, 
            [FromHeader(Name = "X-API-Key")] string apiKey = null)
        {
            // Validar la API key
            if (!AccessValidator.IsValidApiKey(apiKey))
            {
                return Unauthorized(AccessValidator.BuildUnauthorized());
            }

            // Buscar el pedido por el ID
            var pedido = await appDbContext.FACPEDIDOS.FirstOrDefaultAsync(p => p.NumPedido == NumPedido && p.CodAgencia == CodAgencia);

            // Si el pedido no existe, devolver un error 404
            if (pedido == null)
            {
                return NotFound(new ApiResponseFacPedido
                {
                    success = false,
                    message = "Pedido no encontrado",
                    responseCode = 404
                });
            }

            // Eliminar el pedido
            appDbContext.FACPEDIDOS.Remove(pedido);
            await appDbContext.SaveChangesAsync();

            // Devolver respuesta exitosa
            return new ApiResponseFacPedido
            {
                success = true,
                message = "Pedido eliminado exitosamente",
                responseCode = 200,
                count = 1,
                data = new[] { pedido }
            };
        }
    }
}
