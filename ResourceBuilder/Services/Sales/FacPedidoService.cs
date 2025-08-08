using DataSourceManager;

using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
//using System.Data.Entity;
using System.Text;

using Models.DMSA.Mbw.Inventario;
using Models.DMSA.Mbw.Sales;
using Models.DMSA.Mbw.Core;
using System.Data.Entity;


namespace ResourceBuilder.Services.Sales
{
    public class FacPedidoService
    {
        private readonly AppDbContext _context;

        public FacPedidoService(AppDbContext context)
        {
            _context = context;
        }

        public const string CABECERA_XML = "<?xml version='1.0' encoding='ISO-8859-1'?>";

        public static string EstandarizarCadena(string cadena)
        {
            cadena = cadena ?? string.Empty;
            return "<![CDATA[" + cadena + "]]>";
        }

        public static string EstandarCadenaJSON(string cadena)
        {
            cadena = cadena ?? string.Empty;
            cadena = cadena
                .Replace("'", "\\u0027")
                .Replace("\"", "\\u0022")
                .Replace(",", "\\u002c")
                .Replace("\n", "\\n");
            return cadena;
        }

        private double RedondearDecimal(double valor, int precision)
        {
            return Math.Round(valor, precision);
        }

        public static double RedondeoDecimalDouble(double valor, int decimales)
        {
            try
            {                
                decimal valorDecimal = (decimal)valor;                
                decimal valorRedondeado = Math.Round(valorDecimal, decimales, MidpointRounding.AwayFromZero);                
                return (double)valorRedondeado;
            }
            catch (Exception e)
            {                
                Console.WriteLine(e.Message);
                throw;
            }
        }

        public FacRangoDescuentoLote AplicarRangoDescuentoAsync(long codigoEmpresa, long difference, long codTipoPromocion)
        {
            try
            {
                // Construcción de la consulta utilizando LINQ
                var rangoDescuento = _context.FACRANGODESCUENTOLOTE
                    .Where(rd => rd.CodEmpresa == codigoEmpresa &&
                                 rd.ValorInicial <= difference &&
                                 rd.ValorFinal >= difference &&
                                 rd.CodEstado == GenEstados_ENUM.ESTADO_ACTIVO &&
                                 rd.CodTipoPromocion == codTipoPromocion)
                    .FirstOrDefault();

                return rangoDescuento;
            }
            catch (Exception e)
            {
                // Manejo de excepciones
                Console.WriteLine(e.Message);
                throw;
            }
        }

        public async Task<string> ObtenerLotesArticulosJSON(long codigoEmpresa,
                                            long codBodegaAgencia,
                                            long codigoArticulo,
                                            bool verSinStock,
                                            bool crearLoteAuto)
        {
            var retorno = new StringBuilder();
            var descuentoLote = string.Empty;
            long diff = 0L;
            var sdf = "dd/MM/yyyy";

            try
            {
                Console.WriteLine($"Articulo: {codigoArticulo} codbodegaagencia: {codBodegaAgencia}");

                var stock = ObtenerInvStockxBodegaAsync(codigoArticulo, codBodegaAgencia);
                if (stock == null)
                    throw new Exception("No se pudo obtener el registro de Stock para el Artículo.");

                var listaLotes = ObtenerLotesArticulos(codigoEmpresa, codBodegaAgencia, codigoArticulo, false);
                InvLotes lote = null;
                var itemArticulo = _context.GENARTICULOS.Where(x => x.CodArticulo == codigoArticulo).FirstOrDefault();

                double cantidadLote = 0d;
                int contador = 1;

                foreach (var stockLote in listaLotes)
                {
                    // Si no existe cadena en el lote
                    if (!verSinStock && stockLote.Cantidad <= 0)
                        continue;


                    // Obtener Cabecera Principal Lotes                    
                    lote = _context.INVLOTES.Where(x => x.CodigoLote == stockLote.CodigoLote).FirstOrDefault();

                    if (lote == null)
                        throw new Exception("No se pudo obtener el lote");

                    if (contador > 1)
                        retorno.Append(",");

                    // Obtener descuento por Fecha de caducidad
                    var fechaActual = DateTime.Now;
                    var expirationDateFormat = lote.FechaCaducidad.Value.ToString(sdf);
                    var expirationDate = DateTime.ParseExact(expirationDateFormat, sdf, null);

                    diff = (long)(expirationDate - fechaActual).TotalMilliseconds;
                    var difference = (long)(diff / TimeSpan.FromDays(1).TotalMilliseconds);

                    var rangoLote = AplicarRangoDescuentoAsync(codigoEmpresa, difference, FacPromociones_ENUM.PROMO_DESCUENTOS);
                    
                    // Generar registro Lote
                    var loteJson = new JObject
                    {
                        ["cantidad"] = stockLote.Cantidad.ToString(),
                        ["stocklote"] = stockLote.Cantidad.ToString(),
                        ["crecibida"] = "0",
                        ["numlote"] = lote.CodigoLote,
                        ["lote"] = lote.Lote ?? string.Empty,
                        ["fcaducidad"] = lote.FechaCaducidad.Value.ToString("dd-MM-yyyy"),
                        ["unidad"] = itemArticulo.CodUnidadMedida, //stockLote.Id.Genarticulos.Genunidadesmedida.Codunidadmedida,
                        ["promolote"] = new JArray
                        {
                            new JObject
                            {
                                ["esdctolote"] = rangoLote != null ? "S" : "N",
                                ["eseditableporc"] = rangoLote != null ? "S" : "N",
                                ["codrangolote"] = rangoLote?.CodRangoLote , //rangoLote?.CodRangoLote ?? string.Empty,
                                ["descripcionlote"] = rangoLote != null ? EstandarCadenaJSON(rangoLote.Descripcion.Replace("\"", "").Replace("&", "").Replace("'", "")) : string.Empty,
                                ["porcinicial"] = rangoLote?.PorcInicial.ToString() ?? "0",
                                ["porcfinal"] = rangoLote?.PorcFinal.ToString() ?? "0",
                                ["difference"] = difference.ToString()
                            }
                        },
                        ["bloqueado"] = "S"
                    };

                    retorno.Append(loteJson.ToString(Formatting.None));
                    cantidadLote += (double) stockLote.Cantidad;
                    contador++;
                }

                // Asignación de Lote Genérico
                if (crearLoteAuto)
                {
                    if (stock.Cantidad > (decimal) cantidadLote)
                    {
                        if (contador > 1)
                            retorno.Append(",");

                        var loteGenericoJson = new JObject
                        {
                            ["cantidad"] = RedondeoDecimalDouble((int) stock.Cantidad - cantidadLote, 0).ToString(),
                            ["crecibida"] = "0",
                            ["numlote"] = "0",
                            ["lote"] = "GENERICO",
                            ["fcaducidad"] = "31-12-" + DateTime.Now.Year,
                            ["unidad"] = itemArticulo.CodUnidadMedida, // stock.Id.Genarticulos.Genunidadesmedida.Codunidadmedida,
                            ["bloqueado"] = "N"
                        };

                        retorno.Append(loteGenericoJson.ToString(Formatting.None));
                    }
                }

                return $"{{\"lotes\":[{retorno}]}}";
            }
            catch (Exception e)
            {
                // Manejo de excepciones
                Console.WriteLine(e.Message);
                return "{\"lotes\":[]}";
            }
        }

        public async Task<string> AsignarLoteAutomatico(long codigoEmpresa, long codBodegaAgencia, GenArticulos articulo, double cantidad, double factorConversion, DbContextTransaction transaction = null)
        {
            var respuesta = new System.Text.StringBuilder();

            try
            {               
                string perecible = articulo.Perecible ?? "N";

                if (perecible == "S")
                {
                    var jsonLotes = await ObtenerLotesArticulosJSON(codigoEmpresa, codBodegaAgencia, articulo.CodArticulo, false, false);

                    double cantLotes = 0;
                    int contador = 1;
                    var newDetalleLotes = new List<JObject>();
                    double cantAsigna = 0;
                    double cantTmp = cantidad * factorConversion;

                    var detalleLote = JObject.Parse(jsonLotes);
                    var detalleLotes = detalleLote["lotes"] as JArray;
                    foreach (var item in detalleLotes)
                    {
                        var lote = (JObject)item;

                        if (lote.Value<double>("cantidad") > 0)
                        {
                            cantAsigna = 0;

                            if (cantTmp == 0) break;

                            if (cantTmp <= lote.Value<double>("cantidad"))
                            {
                                cantAsigna = cantTmp;
                                cantTmp = 0;
                            }
                            else
                            {
                                cantAsigna = lote.Value<double>("cantidad");
                                cantTmp -= lote.Value<double>("cantidad");
                                cantTmp = RedondearDecimal(cantTmp, 2);
                            }

                            if (cantAsigna > 0)
                            {
                                var newLote = new JObject
                                {
                                    ["cantidad"] = cantAsigna,
                                    ["crecibida"] = lote.Value<string>("crecibida"),
                                    ["numlote"] = lote.Value<string>("numlote"),
                                    ["lote"] = lote.Value<string>("lote"),
                                    ["fcaducidad"] = lote.Value<string>("fcaducidad"),
                                    ["unidad"] = lote.Value<string>("unidad"),
                                    ["bloqueado"] = lote.Value<string>("bloqueado")
                                };

                                newDetalleLotes.Add(newLote);
                                contador++;
                                cantLotes += cantAsigna;
                            }
                        }
                    }

                    var detalleLotesJson = JsonConvert.SerializeObject(newDetalleLotes);                    

                    respuesta.Append($"{{\"success\":true,\"exito\":true,\"detallelote\": {detalleLotesJson},\"cantlotes\":\"{cantLotes}\"}}");
                }
            }
            catch (Exception e)
            {
                var mensajeError = e.Message ?? "Error al momento de aplicar lote automático.";
                mensajeError = mensajeError.Replace("'", "").Replace("\n", "<br>").Trim();
                respuesta.Clear();
                respuesta.Append($"{{\"success\":true,\"exito\":false,\"mensaje\":\"Lote automático: {mensajeError}\"}}");
            }

            return respuesta.ToString();
        }

        public int ObtenerNumeroProformaAsync(int codAgencia)
        {
            try
            {
                // Realizamos la consulta utilizando LINQ
                var resultado = _context.FACPEDIDOS
                    .Where(p => p.CodAgencia == codAgencia && p.TipoPedido == "PRO")
                    .Max(p => (int?)p.NumPedido);

                // Si no hay resultado, retornamos 1, de lo contrario incrementamos en 1
                //return (resultado.HasValue ? resultado.Value + 1 : 1);
                return (resultado.HasValue ? resultado.Value + 1 : 1);
            }
            catch (Exception ex)
            {
                // En caso de excepción, retornamos null o gestionamos el error
                Console.WriteLine(ex.Message);
                return 0;
            }
        }

        public InvStock ObtenerInvStockxBodegaAsync(long codigoArticulo, long codigoBodegaxAgencia)
        {
            var registroBodega = _context.INVSTOCKS
                .Where(i => i.CodArticulo == codigoArticulo && i.CodBodegaAgencia == codigoBodegaxAgencia)
                .FirstOrDefault();

            return registroBodega;
        }

        public List<InvstockLotes> ObtenerLotesArticulos(
        long codigoEmpresa,
        long codBodegaAgencia,
        long codigoArticulo,
        bool verConStock)
        {
            try
            {
                var query = _context.INVSTOCKLOTES.AsQueryable();

                query = query.Where(sl => sl.CodEmpresa == codigoEmpresa &&
                                          sl.CodBodegaAgencia == codBodegaAgencia &&
                                          sl.CodArticulo == codigoArticulo);

                if (verConStock)
                {
                    query = query.Where(sl => sl.Cantidad > 0);
                }

                var listaRetorno = query
                    .OrderBy(sl => sl.FechaUltModifica) // Reemplaza esto por la columna adecuada para ordenamiento si es necesario
                    .ToList();

                return listaRetorno;
            }
            catch (Exception ex)
            {
                // Manejo de excepciones
                Console.WriteLine(ex.Message);
                throw;
            }
        }
    }
}
