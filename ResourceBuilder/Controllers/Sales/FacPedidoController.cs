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
using ResourceBuilder.Controllers.Security;
using ResourceBuilder.Services.Sales;
using DMSA.Models.Odoo.Update;
using Newtonsoft.Json.Linq;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Models.DMSA.Mbw.Sales;

namespace ResourceBuilder.Controllers.Sales
{
    [Route("api/[controller]")]
    [ApiController]
    public class FacPedidoController : ControllerBase
    {
        DataSourceManager.AppDbContext _appDbContext = new DataSourceManager.AppDbContext();

        //private readonly DataSourceManager.AppDbContext _appDbContext;
        //private readonly ILogger<FacPedidoController> _logger;

        //public FacPedidoController(DataSourceManager.AppDbContext appDbContext, ILogger<FacPedidoController> logger)
        //{
        //    _appDbContext = appDbContext;
        //    _logger = logger;
        //}

        [HttpGet()]
        public async Task<ActionResult<ApiResponseFacPedido>> Get(
            [FromQuery] int? NumPedido = null,
            [FromQuery] int? CodAgencia = null,
            [FromQuery] int? CodCliente = null,
            [FromQuery] int? CodEstado = null,
            [FromHeader(Name = "X-API-Key")] string apiKey = null)
        {
            if (!AccessValidator.IsValidApiKey(apiKey))
            {   
                return Unauthorized(AccessValidator.BuildUnauthorized());
            }

            //List<FacPedido> lresult = null;
            //lresult = await appDbContext.FACPEDIDOS.Take(100).ToListAsync();
            //return lresult;

            var query = _appDbContext.FACPEDIDOS.AsQueryable();

            if (NumPedido.HasValue)
            {
                query = query.Where(p => p.NumPedido == NumPedido.Value);
            }

            if (CodAgencia.HasValue)
            {
                query = query.Where(p => p.CodAgencia == CodAgencia.Value);
            }

            if (CodCliente.HasValue)
            {
                query = query.Where(p => p.CodCliente == CodCliente.Value);
            }

            if (CodEstado.HasValue)
            {
                query = query.Where(p => p.CodEstado == CodEstado);
            }

            var lresult = await query.Take(100).ToListAsync();

            return new ApiResponseFacPedido()
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

            FacPedidoService facPedidoService = new FacPedidoService(_appDbContext);

            document.NumPedido = facPedidoService.ObtenerNumeroProformaAsync(document.CodAgencia);
            document.FechaRegistro = DateTime.Now;

            await _appDbContext.FACPEDIDOS.AddAsync(document);
            
            int resultInsert = await _appDbContext.SaveChangesAsync();

            if (resultInsert == 1)
            {
                foreach (var itemProducto in document.detalle_productos)
                {
                    if (itemProducto.Cantidad > 0)
                    {
                        //Console.WriteLine(itemProducto.CodArticulo);
                        FacPedidoService pedidoService = new FacPedidoService(_appDbContext);
                        var itemArticulo = await _appDbContext.GENARTICULOS.Where(x => x.CodArticulo == itemProducto.CodArticulo).FirstOrDefaultAsync();
                        //var unidadPresentacion = await appDbContext.GENUNIDADESMEDIDA.Where(x => x.CodUnidadMedida == itemArticulo.CodUnidadMedida).FirstOrDefaultAsync();
                        var equivalenciaUniMedidas = await _appDbContext.GENEQUIVALENCIAUNIMEDIDAS.Where(x => x.Codunidadorigen == itemProducto.CodUnidadMedida &&
                        x.Codunidaddestino == itemArticulo.CodUnidadMedida).FirstOrDefaultAsync();

                        var loteJsonData = await pedidoService.AsignarLoteAutomatico(
                            (long) itemProducto.CodEmpresa,
                            (long) itemProducto.CodBodegaAgencia,
                            itemArticulo,
                            (double) itemProducto.Cantidad,
                            (double) equivalenciaUniMedidas.Factorconversion);

                        int cantLotes = 0;

                        try
                        {
                            JObject jsonLoteAut = JObject.Parse(loteJsonData);
                            cantLotes = jsonLoteAut["cantlotes"].ToObject<int>();
                            string detallesLotes = jsonLoteAut["detallelote"].ToString(Formatting.None);

                            if (cantLotes > 0)
                            {
                                itemProducto.CadenaLotes = detallesLotes;
                                itemProducto.NumPedido = document.NumPedido;
                                itemProducto.CodAgencia = document.CodAgencia;
                                await _appDbContext.FACPEDIDODET.AddAsync(itemProducto);
                                await _appDbContext.SaveChangesAsync();
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("Error: " + ex.Message);
                        }
                    }
                }
            }

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
        public async Task<ActionResult<ApiResponseFacPedido>> Delete(int NumPedido, int CodAgencia, [FromHeader(Name = "X-API-Key")] string apiKey = null)
        {
            // Validar la API key
            if (!AccessValidator.IsValidApiKey(apiKey))
            {
                return Unauthorized(AccessValidator.BuildUnauthorized());
            }

            // Buscar el pedido por el ID
            var pedido = await _appDbContext.FACPEDIDOS.FirstOrDefaultAsync(p => p.NumPedido == NumPedido && p.CodAgencia == CodAgencia);

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

            var pedido_det = _appDbContext.FACPEDIDODET.Where(p => p.NumPedido == NumPedido && p.CodAgencia == CodAgencia);
                        
            if (pedido_det != null)
            {
                _appDbContext.FACPEDIDODET.RemoveRange(pedido_det);
                await _appDbContext.SaveChangesAsync();
            }
                        
            _appDbContext.FACPEDIDOS.Remove(pedido);
            await _appDbContext.SaveChangesAsync();

            // Devolver respuesta exitosa
            return new ApiResponseFacPedido
            {
                success = true,
                message = "Pedido eliminado exitosamente",
                responseCode = 200,
                count = 1,
                data = [ pedido ]
            };
        }
    }
}
