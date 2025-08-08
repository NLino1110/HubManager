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

using System.Security.Cryptography;
using Models.DMSA.Mbw.Clientes;
using System.Data;
//using Entidades.SyncTask;
using System.IO;
using Models.DMSA.Mbw.Sales;
using ApiTradeHub.Controllers.Security;
using Models.DMSA.Shared.Structs;
using ApiTradeHub.Services.Sales;
using Newtonsoft.Json.Linq;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Models.DMSA.Mbw.Inventario;
using Microsoft.OpenApi.Models;
using static System.Runtime.InteropServices.JavaScript.JSType;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using System.Collections;
using System.Diagnostics;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Drawing.Printing;
using Microsoft.AspNetCore.OData.Query;

namespace ApiTradeHub.Controllers.Sales
{
    [Route("odata/[controller]")]
    [ApiController]
    public class OPromocionesController : ODataController
    {
        private readonly DataSourceManager.AppDbContext _context;

        public OPromocionesController(DataSourceManager.AppDbContext context)
        {
            _context = context;
        }

        private int? ConvertToNullableInt(object value)
        {
            if (value == null)
                return null;

            // Intenta convertir directamente si es ya un entero
            if (value is int intValue)
                return intValue;

            // Si es un string o algún otro formato, intenta convertir
            if (int.TryParse(value.ToString(), out int parsedValue))
                return parsedValue;

            // Retorna null si no se pudo convertir
            return null;
        }

        [EnableQuery]
        [HttpGet("get-bonificados")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<ArticulosXEmpresa>))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetBonificados(
            [FromBody] ApiRequestGlobal<FacBonificadosXArticulo> parameters = null,
            //[FromBody] Models.DMSA.Mbw.Abstract.ParametersMode1? parameters_post = null,
            [FromHeader(Name = "X-API-Key")] string apiKey = null
            )
        {
            //ApiResponseGlobal apiResponseGlobal = new ApiResponseGlobal();
            //OpenApiResponse openApiResponse = new OpenApiResponse();

            if (!AccessValidator.IsValidApiKey(apiKey))
            {   
                return Unauthorized(AccessValidator.BuildUnauthorized());
            }

            // Validar valores de entrada
            if (parameters.Page < 1) parameters.Page = 1;
            if (parameters.PageSize < 1) parameters.PageSize = 100;

            int skip = (parameters.Page - 1) * parameters.PageSize;

            List<object> ids = new List<object>();
            List<object> brands = new List<object>();
            DateTime? date_start = null;
            DateTime? date_end = null;

            // Aplicar filtros dinámicos
            foreach (var filter in parameters.Filters)
            {
                //query = query.Where(e =>
                //    EF.Property<object>(e, filter.Key).ToString() == filter.Value.ToString());
                if (filter.Key == "ids")
                {                    
                    JArray arrayValues = JsonConvert.DeserializeObject<JArray>(filter.Value.ToString());

                    //Debug.WriteLine(arrayValues);
                    foreach (var itemValue in arrayValues)
                    {
                        ids.Add(itemValue.ToString());
                    }
                }

                if (filter.Key == "ids" && filter.Value is IEnumerable<object> enumerable)
                {
                    ids = enumerable.ToList();
                }

                if (filter.Key == "brands" && filter.Value is IEnumerable<object> enumerableb)
                {
                    brands = enumerableb.ToList();
                }

                if (filter.Key == "date_start")
                {
                    date_start = DateTime.Parse( filter.Value.ToString() );
                }

                if (filter.Key == "date_end")
                {
                    date_end = DateTime.Parse(filter.Value.ToString());
                }
            }

            //string input_search_param_var = "";
            //string SelectedCompany = "";

            List<int?> CodeListProducts = ids
                .Select(id => ConvertToNullableInt(id))
                .ToList();

            List<int?> CodeListBrands = brands
                .Select(brand => ConvertToNullableInt(brand))
                .ToList();

            int CodEmpresa = 2;

            List<FacBonificadosXArticulo> dataSource_tmp = null;

            try
            {
                int[] codArticulos = new int[] { };

                List<GenArticulos> genArticulo = null;
                List<ArticulosXEmpresa> artXEmpresa = null;
                if (CodeListProducts.Count > 0)
                {
                    //SE BUSCAN LOS CODIGOS DE LOS ARTICULOS QUE SE HAN ENVIADO COMO PARAMETROS
                    //if (int.TryParse(input_search_param_var, out int numero))
                    //{
                        //Console.WriteLine("El valor ingresado es un número: " + numero);

                        //genArticulo = await _appDbContext.GENARTICULOS.Where(a =>
                        //a.CodArticulo == numero || a.CodAlterno.Contains(input_search_param_var)).ToListAsync();
                    genArticulo = await _context.GENARTICULOS.Where(a =>
                        CodeListProducts.Contains(a.CodArticulo)).ToListAsync();
                    //}
                    //else // if (input_search_param_var.All(char.IsLetter))
                    //{
                    //    Console.WriteLine("El valor ingresado contiene solo caracteres.");
                    //    genArticulo = await _appDbContext.GENARTICULOS
                    //        .Where(a => a.CodAlterno == input_search_param_var.Trim()
                    //        || a.Descripcion.Contains(input_search_param_var.Trim())).ToListAsync();
                    //}

                    if (genArticulo == null)
                    {
                        //toastService.ShowError("Datos de artículo no encontrados!");
                        return NotFound();
                    }

                    codArticulos = genArticulo.Select(articulo => articulo.CodArticulo).ToArray();
                }
                else
                {
                    //SE BUSCAN TODOS LOS ARTICULOS RELACIONADOS CON LA EMPRESA Y MARCA SELECCIONADAS
                    if (CodeListBrands.Count > 0)
                    {
                        artXEmpresa = await _context.ARTICULOSXEMPRESA
                            .Include(z => z.Articulo)
                            .Include(y => y.Marca)
                            .Where(Data =>
                        Data.CodEmpresa == CodEmpresa
                        && CodeListBrands.Contains(Data.CodMarca) // == SelectedBrand.CodMarca
                        && Data.CodEstado == 1
                        &&
                        (
                        Data.ActivaWeb == "N" ||
                        Data.VentaAlmacenes == "S" ||
                        Data.VentaAlmacenes == "N"
                        )
                        )?
                        .Skip(skip)
                        .Take(parameters.PageSize)
                        .ToListAsync();
                    }
                    else
                    {
                        //SE BUSCAN TODOS LOS ARTICULOS RELACIONADOS CON LA EMPRESA SELECCIONADA
                        artXEmpresa = await _context.ARTICULOSXEMPRESA
                            .Include(z => z.Articulo)
                            .Include(y => y.Marca)
                            .Where(Data =>
                        Data.CodEmpresa == CodEmpresa
                        && Data.CodEstado == 1)?
                        .Skip(skip)
                        .Take(parameters.PageSize)
                        .ToListAsync();
                    }

                    codArticulos = artXEmpresa.Select(articulo => articulo.CodArticulo).ToArray();
                    genArticulo = artXEmpresa.Select(articulo => articulo.Articulo).ToList();
                }

                List<GenMarca> brandTemp = new List<GenMarca>();

                dataSource_tmp = await _context.FACBONIFICADOSXARTICULO
                    .Where(c => c.CodEmpresa == CodEmpresa
                        && c.FechaInicio >= date_start
                        && c.FechaFin <= date_end
                        && c.CodEstado == 1
                        && codArticulos.Contains(c.CodArticulo))
                    //.Skip(skip)
                    //.Take(parameters.PageSize)
                    .ToListAsync();

                if (artXEmpresa != null && dataSource_tmp.Count > 0)
                {
                    //foreach (var itemSource in artXEmpresa)
                    for (int i = 0; i < dataSource_tmp.Count; i++)
                    {
                        var itemSource = dataSource_tmp[i];

                        var ArticulosXEmpresaFound = artXEmpresa.
                            Where(x =>
                            x.CodArticulo == dataSource_tmp[i].CodArticulo
                            && x.CodEmpresa == dataSource_tmp[i].CodEmpresa)
                            .FirstOrDefault();

                        dataSource_tmp[i].ArticulosXEmpresa = ArticulosXEmpresaFound;
                    }

                    //dataSource = dataSource_tmp.AsQueryable();
                }
                else
                {

                    dataSource_tmp = new List<FacBonificadosXArticulo>();

                    foreach (var articulo in genArticulo)
                    {
                        FacBonificadosXArticulo facBonificadosXArticulo = new FacBonificadosXArticulo();
                        facBonificadosXArticulo.Articulo = articulo;
                        facBonificadosXArticulo.CodBonificadoArticulo = 0;
                        facBonificadosXArticulo.CodArticulo = articulo.CodArticulo;
                        facBonificadosXArticulo.CodAgencia = 0;
                        facBonificadosXArticulo.ArticulosXEmpresa = new ArticulosXEmpresa()
                        {
                            Marca = new GenMarca()
                            {
                                Descripcion = ""
                            }
                        };
                        facBonificadosXArticulo.FechaInicio = DateTime.Now.AddYears(-1000);
                        facBonificadosXArticulo.FechaFin = DateTime.Now.AddYears(-1000);
                        facBonificadosXArticulo.MinimoAplicaDscto = 0;
                        facBonificadosXArticulo.PorcDescuento = 0;
                        facBonificadosXArticulo.Precio = 0;
                        facBonificadosXArticulo.ValorDescuento = 0;

                        dataSource_tmp.Add(facBonificadosXArticulo);
                    }

                    //dataSource = dataSource_tmp.AsQueryable();

                    //toastService.ShowError("No se encontró en bonificados: " + 
                    //    genArticulo[0].CodArticulo + " - " +
                    //    genArticulo[0].CodAlterno + " - " +
                    //    genArticulo[0].Descripcion);

                    //toastService.ShowError("No se encontró en bonificados");

                    //return NotFound();
                }

            }
            catch (Exception e)
            {
                Console.WriteLine($"Error: {e.Message}");
                Console.WriteLine($"Error: {e}");
            }

            //return Ok(dataSource_tmp);
            int? PageSize = 10;
            int Page = 1;

            var totalRecords = _context.FACBONIFICADOSXARTICULO.Count();
            var metadata = new
            {
                TotalRecords = totalRecords,
                TotalPages = (int)Math.Ceiling(totalRecords / (double) PageSize.Value),
                CurrentPage = Page,
                PageSize = PageSize
            };

            return Ok(new { Metadata = metadata, Data = dataSource_tmp });
        }   
    }
}
