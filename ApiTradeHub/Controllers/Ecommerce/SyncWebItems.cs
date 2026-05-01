using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Data;
using Models.DMSA.Mbw.Sales;
using ApiTradeHub.Controllers.Security;
using ApiTradeHub.Services.Sales;
using Newtonsoft.Json;
using Models.DMSA.Mbw.Core;
using Models.DMSA.Mbw.Inventario;
using Models.DMSA.Mbw.Abstract;

namespace ApiTradeHub.Controllers.Ecommerce
{

    [Route("api/[controller]")]
    [ApiController]
    public class SyncWebItems : ControllerBase
    {
        //EcommerceService _ecommerceService;
        //DataSourceManager.AppDbContext _appDbContext = new DataSourceManager.AppDbContext();
        DataSourceManager.AppDbContext _appDbContext { get; set; }

        public SyncWebItems(DataSourceManager.AppDbContext appDbContext)
        {
            //_ecommerceService = ecommerceService;
            _appDbContext = appDbContext;
        }

        [HttpGet()]
        public async Task<ActionResult<dynamic>> Get(
            [FromBody] ParametersMode1 parametros,
            [FromHeader(Name = "X-API-Key")] string apiKey = null)
        {
            int codEmpresa = 2;
            string tmp_apikey = "9+7e3A7t4qI1Rl8XQ2GjKjs8KhZ9Y8p1MfbQvKlkmP4=";

            apiKey = tmp_apikey;

            if (!AccessValidator.IsValidApiKey(apiKey))
            {
                return Unauthorized(AccessValidator.BuildUnauthorized());
            }

            EcommerceService ecommerceService = new EcommerceService(_appDbContext);
            var articulosEnvio = await ecommerceService.MakeProducts(parametros);
            var jsonResult = JsonConvert.SerializeObject(articulosEnvio, Formatting.Indented, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore });

            return new ContentResult
            {
                Content = jsonResult,
                ContentType = "application/json",
                StatusCode = 200
            };
        }

        [Obsolete]
        //[HttpGet()]
        public async Task<ActionResult<dynamic>> __Get(
            [FromBody] ParametersMode1 parametros,
            [FromHeader(Name = "X-API-Key")] string apiKey = null)
        {
            int codEmpresa = 2;
            string tmp_apikey = "9+7e3A7t4qI1Rl8XQ2GjKjs8KhZ9Y8p1MfbQvKlkmP4=";

            apiKey = tmp_apikey;

            if (!AccessValidator.IsValidApiKey(apiKey))
            {
                return Unauthorized(AccessValidator.BuildUnauthorized());
            }

            //StrucGenParametro strucGenParametro = _appDbContext.GENPARAMETROS.Where(
            //    predicate: x => x.CODPARAMETRO.Equals("URL_SKU_ECOMMERCE_APIREST_MASIVO") &&
            //    x.CODEMPRESA == 2
            //    ).FirstOrDefault();

            List<GenEmpresa> empresas = _appDbContext.GENEMPRESAS.ToList();
            List<GenArticulos> articulos = new List<GenArticulos>();
            List<GenMarca> marcas = new List<GenMarca>();

            List<ArticulosXEmpresa> articulos_brand = new List<ArticulosXEmpresa>();
            List<ArticulosXEmpresa> articulos_articulos = new List<ArticulosXEmpresa>();

            if (parametros.brands.Count > 0)
            {
                marcas = _appDbContext.GENMARCAS.Where(x =>
                parametros.brands.Contains(x.CodMarca) && x.CodEmpresaMarca == codEmpresa).ToList();

                articulos_brand = _appDbContext.ARTICULOSXEMPRESA
                    .Include(x => x.Articulo)
                    .Include(x => x.Estado)
                    .Where(x =>
                parametros.brands.Contains(x.CodMarca) && (x.ActivaWeb == "S" ||
                x.ActivaWeb == "N" ||
                x.VentaAlmacenes == "S" ||
                x.VentaAlmacenes == "N")).ToList();
            }

            if (parametros.ids.Count > 0)
            {
                articulos = _appDbContext.GENARTICULOS.Where(x =>
                parametros.ids.Contains(x.CodArticulo)).ToList();

                articulos_articulos = _appDbContext.ARTICULOSXEMPRESA
                    .Include(x => x.Articulo)
                    .Include(x => x.Estado)
                    .AsEnumerable() // Esto forza que la evaluación se realice en el cliente
                    .Where(x => articulos.Any(articulo => articulo.CodArticulo == x.CodArticulo) &&
                                (x.ActivaWeb == "S" || x.ActivaWeb == "N" ||
                                 x.VentaAlmacenes == "S" || x.VentaAlmacenes == "N")
                                 && x.CodEmpresaMarca == codEmpresa)
                    .ToList();
            }

            //DateTime StartDate = DateTime.Now;
            //DateTime EndDate = DateTime.Now.AddDays(5);
            List<FacBonificadosXArticulo> dataSource_tmp = null;
            if (true)
            {
                dataSource_tmp = await _appDbContext.FACBONIFICADOSXARTICULO
                .Where(c => c.CodEmpresa == 2
                    && c.FechaInicio >= parametros.date_start
                    && c.FechaFin <= parametros.date_end
                    && c.CodEstado == 1)
                    //&& codArticulos.Contains(c.CodArticulo))
                    //.Take(2000)
                    .ToListAsync();

                var codArticulosTmp = dataSource_tmp.Select(articulo => articulo.CodArticulo).ToArray();

                for(int ib=0; ib < articulos_brand.Count; ib++)
                {
                    if (!codArticulosTmp.Contains(articulos_brand[ib].CodArticulo))
                        articulos_brand.RemoveAt(ib);                        
                }

                //if (articulos_brand!= null && articulos_brand.Count > 0)
                //{
                //    articulos_brand.RemoveAll(x => !codArticulosTmp.Contains( x.CodArticulo ));
                //}
            }

            articulos_articulos.AddRange(articulos_brand);

            double IVA = 0d;
            var pIVA = await _appDbContext.GENPARAMETROS.Where(x => x.CodEmpresa == codEmpresa && x.CodParametro == "IVA").FirstOrDefaultAsync();
            IVA = (pIVA != null) ? double.Parse(pIVA.Valor) : 0d;

            var pClienteWeb = await _appDbContext.GENPARAMETROS.Where(x => x.CodEmpresa == codEmpresa && x.CodParametro == "PRECIO_WEB").FirstOrDefaultAsync();
            long clienteWeb = (pClienteWeb != null) ? long.Parse(pClienteWeb.Valor) : 0l;

            var paramNivel = await _appDbContext.GENPARAMETROS.Where(x => x.CodEmpresa == codEmpresa && x.CodParametro == "TIPO_NIVEL_DEFAULT_WEB").FirstOrDefaultAsync();

            long nivelWeb = (paramNivel != null) ? long.Parse(paramNivel.Valor) : 0l;

            if (paramNivel == null)
                throw new Exception("No se ha configurado el parámetro TIPO_CLIENTE_DEFAULT_WEB");

            // Obtener la agencia matriz de la Empresa
            var agenciaMatriz = await _appDbContext.GENAGENCIAS.Where(x => x.CodEmpresa == codEmpresa
            && x.CodEstado == 1
            && x.TipoAgencia == "M")
                .OrderBy(o => o.CodAgencia)
                .FirstOrDefaultAsync();

            long codAgencia = 2;
            bool envioAdicional = true; 

            if (agenciaMatriz == null)
                throw new Exception("La empresa no tiene configurada Agencia Matriz");

            var articulosEnvio = new List<ArticuloDTO>();
            
            var articulosVE = new Dictionary<long, double>();
            var articulosAL = new Dictionary<long, double>();

            if (articulos_articulos.Count > 0)
            {
                //totalreg = articulos_articulos.Count;
                foreach (var art in articulos_articulos)
                {
                    string subCod = "";
                    subCod = art.Articulo.CodAlterno.Length >= 3 ? art.Articulo.CodAlterno.Trim().Substring(0, 3) : art.Articulo.CodAlterno.Trim();

                    //totalreg--;

                    if (!subCod.Equals("PADX", StringComparison.OrdinalIgnoreCase))
                    {
                        EcommerceService ecommerceService = new EcommerceService(_appDbContext);
                        
                        var articuloDto = await ecommerceService.BuildItemForSendStockPrice(art,
                            codEmpresa,
                            codAgencia,
                            IVA,
                            envioAdicional,
                            clienteWeb,
                            agenciaMatriz,
                            nivelWeb,
                            articulosVE,
                            articulosAL);

                        if (articuloDto != null)
                        {
                            bool vtaExterna = true;
                            //var precioAlmacen = await ecommerceService.BuildPrecio(
                            //    art,
                            //    parametros,
                            //    agenciaMatriz,
                            //    nivelWeb,
                            //    paramNivel,
                            //    vtaExterna,
                            //    IVA,
                            //    dataSource_tmp);
                            var pricesDiscounts = await ecommerceService.BuildDiscounts(art, parametros, agenciaMatriz, clienteWeb, paramNivel, vtaExterna, IVA);
                            articuloDto.Prices.AddRange(pricesDiscounts);
                            articulosEnvio.Add(articuloDto);
                            //count++;
                        }
                    }
                }
            }

            var jsonResult = JsonConvert.SerializeObject(articulosEnvio, Formatting.Indented, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore });

            return new ContentResult
            {
                Content = jsonResult,
                ContentType = "application/json",
                StatusCode = 200
            };
        }

        [HttpPost]
        public async Task<ActionResult<dynamic>> Post(
            [FromBody] ParametersMode1 parametros,
            [FromHeader(Name = "X-API-Key")] string apiKey = null
            )
        {
            int codEmpresa = 2;
            string tmp_apikey = "9+7e3A7t4qI1Rl8XQ2GjKjs8KhZ9Y8p1MfbQvKlkmP4=";

            apiKey = tmp_apikey;

            if (!AccessValidator.IsValidApiKey(apiKey))
            {
                return Unauthorized(AccessValidator.BuildUnauthorized());
            }

            GenParametros strucGenParametro = _appDbContext.GENPARAMETROS.Where(
                predicate: x => x.CodParametro.Equals("URL_SKU_ECOMMERCE_APIREST_MASIVO") &&
                x.CodEmpresa == 2
                ).FirstOrDefault();

            string endPointUrl = strucGenParametro.Valor;

            if (endPointUrl.Contains("(pruebas)"))
            {
                endPointUrl = endPointUrl.Replace("(pruebas)", "");

                //TODO: Url de produccion
                //endPointUrl = "http://api.dmujeres.ec/dmujeres/sku/bulk/";
            }

            //Articulo de prueba 22944
            //Se usa RestSharp.Method.Put porque es una actualización
            EcommerceService _ecommerceService = new EcommerceService(_appDbContext);
            var articulosEnvio = await _ecommerceService.MakeProducts(parametros);
            var jsonForSendResult = JsonConvert.SerializeObject(articulosEnvio, Formatting.Indented, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore });

            var resultData = await _ecommerceService.SendDataToMiddleware(
            jsonForSendResult,
                RestSharp.Method.Put,
                endPointUrl,
                _appDbContext);

            var jsonResult = JsonConvert.SerializeObject(resultData, Formatting.Indented, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore });

            return new ContentResult
            {
                Content = jsonResult,
                ContentType = "application/json",
                StatusCode = 200
            };
        }

        //[Obsolete]
        //[HttpPost]
        //public async Task<ActionResult<dynamic>> __Post(
        //    [FromBody] ParametersMode1 parametros,
        //    [FromHeader(Name = "X-API-Key")] string apiKey = null
        //    )
        //{
        //    int codEmpresa = 2;
        //    string tmp_apikey = "9+7e3A7t4qI1Rl8XQ2GjKjs8KhZ9Y8p1MfbQvKlkmP4=";
            
        //    apiKey = tmp_apikey;

        //    if (!AccessValidator.IsValidApiKey(apiKey))
        //    {
        //        return Unauthorized(AccessValidator.BuildUnauthorized());
        //    }
            
        //    GenParametros strucGenParametro = _appDbContext.GENPARAMETROS.Where(
        //        predicate: x => x.CodParametro.Equals("URL_SKU_ECOMMERCE_APIREST_MASIVO") &&
        //        x.CodEmpresa == 2
        //        ).FirstOrDefault();

        //    List<GenEmpresa> empresas = _appDbContext.GENEMPRESAS.ToList();
        //    List<GenArticulos> articulos = new List<GenArticulos>();
        //    List<GenMarca> marcas = new List<GenMarca>();

        //    List<ArticulosXEmpresa> articulos_brand = new List<ArticulosXEmpresa>();
        //    List<ArticulosXEmpresa> articulos_articulos = new List<ArticulosXEmpresa>();

        //    if (parametros.brands.Count > 0)
        //    {
        //        marcas = _appDbContext.GENMARCAS.Where(x =>
        //        parametros.brands.Contains(x.CodMarca) && x.CodEmpresaMarca == codEmpresa).ToList();

        //        articulos_brand = _appDbContext.ARTICULOSXEMPRESA
        //            .Include(x => x.Articulo)
        //            .Include(x => x.Estado)
        //            .Where(x =>
        //        parametros.brands.Contains(x.CodMarca) && (x.ActivaWeb == "S" ||
        //        x.ActivaWeb == "N" ||
        //        x.VentaAlmacenes == "S" ||
        //        x.VentaAlmacenes == "N")).ToList();
        //    }

        //    if (parametros.ids.Count > 0)
        //    {
        //        articulos = _appDbContext.GENARTICULOS.Where(x =>
        //        parametros.ids.Contains(x.CodArticulo)).ToList();

        //        articulos_articulos = _appDbContext.ARTICULOSXEMPRESA
        //            .Include(x => x.Articulo)
        //            .Include(x => x.Estado)
        //            .AsEnumerable() // Esto forza que la evaluación se realice en el cliente
        //            .Where(x => articulos.Any(articulo => articulo.CodArticulo == x.CodArticulo) &&
        //                        (x.ActivaWeb == "S" || x.ActivaWeb == "N" ||
        //                         x.VentaAlmacenes == "S" || x.VentaAlmacenes == "N"))
        //            .ToList();
        //    }

        //    articulos_articulos.AddRange(articulos_brand);

        //    //EcommerceService.Test();

        //    // Convertir los Ids a un arreglo de strings
        //    //string[] codArticulosArr = articulos.Select(obj => obj.CodArticulo.ToString()).ToArray();
        //    //string codArticulos = string.Join(",", codArticulosArr);
            
        //    string endPointUrl = strucGenParametro.Valor;

        //    if (endPointUrl.Contains("(pruebas)"))
        //    {
        //        endPointUrl = endPointUrl.Replace("(pruebas)", "");

        //        //TODO: Url de produccion
        //        //endPointUrl = "http://api.dmujeres.ec/dmujeres/sku/bulk/";
        //    }

        //    //Articulo de prueba 22944
        //    //Se usa RestSharp.Method.Put porque es una actualización
        //    EcommerceService _ecommerceService = new EcommerceService(_appDbContext);
        //    var resultData = await _ecommerceService.SendProcessProducts(2, 2,
        //        //codArticulos,
        //        articulos_articulos,
        //        endPointUrl,
        //        RestSharp.Method.Put, 
        //        "MODIFICAR",
        //        "MODIFICAR ARTICULO",
        //        true,
        //        false);

        //    //return new
        //    //{
        //    //    count = 1,
        //    //    create_id = 0,
        //    //    message = "",
        //    //    info = "{'data':''}",
        //    //    object_name = "",
        //    //    responseCode = 200,
        //    //    success = true,
        //    //    data = resultData
        //    //};

        //    var jsonResult = JsonConvert.SerializeObject(resultData, Formatting.Indented, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore});

        //    return new ContentResult
        //    {
        //        Content = jsonResult,
        //        ContentType = "application/json",
        //        StatusCode = 200
        //    };
        //}
        

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
                //_appDbContext.FACPEDIDODET.RemoveRange(pedido_det);
                //await _appDbContext.SaveChangesAsync();
            }
                        
            //_appDbContext.FACPEDIDOS.Remove(pedido);
            //await _appDbContext.SaveChangesAsync();

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
