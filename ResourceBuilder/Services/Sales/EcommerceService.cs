using DataSourceManager;
using Microsoft.EntityFrameworkCore;
using Models.DMSA.Mbw.Abstract;
using Models.DMSA.Mbw.Core;
using Models.DMSA.Mbw.Inventario;
using Models.DMSA.Mbw.Sales;
using Newtonsoft.Json;
using RestSharp;
using System.Data;
using System.Data.Common;
using System.Diagnostics;

namespace ResourceBuilder.Services.Sales
{
    public class EcommerceService
    {
        DataSourceManager.AppDbContext _appDbContext { get; set; }
        public EcommerceService(DataSourceManager.AppDbContext appDbContext)
        {
            _appDbContext = appDbContext;
        }

        private static T GetValueOrDefault<T>(DbDataReader reader, string columnName)
        {
            try
            {
                int colIndex = reader.GetOrdinal(columnName);

                if (reader.IsDBNull(colIndex))
                {
                    Debug.WriteLine($"INFO: La columna '{columnName}' contiene un valor NULL. Se devolverá el valor por defecto.");
                    return default(T);
                }

                // Obtenemos el valor como un objeto genérico para evitar errores de casting directo.
                object value = reader.GetValue(colIndex);

                // Obtenemos el tipo de destino. Si es un tipo anulable (ej: int?), 
                // necesitamos obtener su tipo subyacente (ej: int).
                var targetType = Nullable.GetUnderlyingType(typeof(T)) ?? typeof(T);

                // Usamos Convert.ChangeType para manejar de forma segura las conversiones numéricas,
                // como la de Decimal (de Oracle) a Int64/long o Double.
                return (T)Convert.ChangeType(value, targetType);
            }
            catch (IndexOutOfRangeException)
            {
                Debug.WriteLine($"ERROR: La columna '{columnName}' no fue encontrada en el resultado de la consulta SQL.");
                throw new ArgumentException($"La columna '{columnName}' no fue encontrada en el resultado de la consulta SQL.");
            }
            catch (InvalidCastException ex)
            {
                // Este bloque nos dará información muy útil si la conversión falla por alguna razón.
                object originalValue = reader.GetValue(reader.GetOrdinal(columnName));
                Debug.WriteLine($"ERROR DE CASTING: No se pudo convertir el valor '{originalValue}' (tipo: {originalValue.GetType()}) de la columna '{columnName}' al tipo {typeof(T)}. Error: {ex.Message}");
                throw; // Relanzamos la excepción para no ocultar el problema.
            }
        }

        public async Task<List<Models.DMSA.Mbw.Abstract.Inventory>> BuildStock(long codEmpresa, 
            long codAgencia, 
            ArticulosXEmpresa art,
            ParametersMode1 parametros)
        {
            int TotalHoursBefore = 6;
            bool FullStock = parametros.with_full_stock;

            DateTime fechaBefore = DateTime.Now.AddMinutes(-15);
            //fechaHace15Minutos = DateTime.Now.AddMinutes(-120);
            string fechaFormateada = fechaBefore.ToString("dd/MM/yyyy HH:mm:ss");

            string withDateDiff = $"and v.fechaultegreso >= to_date('{fechaFormateada}','dd/mm/yyyy hh24:mi:ss')";
            if(true)
            {
                withDateDiff = "";
            }

            string extra_stores = "";

            if (parametros!=null && parametros.stores != null && parametros.stores.Length > 0)
            {
                extra_stores = " and x.codbodegaagencia in (" + string.Join(",", parametros.stores) + ")";
            }

            // Inicializa la respuesta
            List<Models.DMSA.Mbw.Abstract.Inventory> response = new List<Models.DMSA.Mbw.Abstract.Inventory>();

            try
            {
                // Consulta para obtener el stock del artículo WMS
                string sql = $"select s.codbodegaagencia, x.nombodega, s.cantdisponiblewms, s.minimovtaweb, v.cantidadreservada, " +
                             $"v.fechaultingreso,v.fechaultegreso " +
                             $"from vw_emp_age_bod_bxa x " +
                             $"inner join Invminmaxarticulos s on s.codbodegaagencia = x.codbodegaagencia " +
                             $"and s.codarticulo = {art.Articulo.CodArticulo} " +
                             $"inner join invstock v on v.codbodegaagencia = x.codbodegaagencia and v.codarticulo = {art.Articulo.CodArticulo} " +
                             $"where x.codempresa = {codEmpresa} " +
                             $"and NVL(x.procesawms, 'X') = 'S' " +
                             $"and NVL(x.controldisponiblewms, 'X') = 'S' " +
                             $"and NVL(x.web, 'X') = 'S' " +
                             $"and NVL(x.envioecommerce, 'X') = 'S' " +
                             $"and x.codagencia = {codAgencia} " +
                             $"{extra_stores}" +
                             //$"and v.fechaultegreso  >= to_date('23/12/2024 16:00:00','dd/mm/yyyy hh24:mi:ss')";
                             //$"and v.fechaultegreso >= (SYSDATE - INTERVAL '15' MINUTE)";
                             withDateDiff;

                // Ejecuta la consulta SQL
                //var minMaxArti = _appDbContext.Set<StockResult>().FromSqlRaw(sql).FirstOrDefault();
                //var precios = _appDbContext.Database.SqlQuery<FacPrecioDTO>(sql).ToList();

                Models.DMSA.Mbw.Query.StockResult minMaxArti = null;

                var connection = _appDbContext.Database.GetDbConnection();

                if(connection.State != ConnectionState.Open)
                    await connection.OpenAsync();

                using (var command = connection.CreateCommand())
                {
                    command.CommandText = sql;
                    command.CommandType = CommandType.Text;

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            DateTime? _fechaultingreso = GetValueOrDefault<DateTime?>(reader, "fechaultingreso");
                            DateTime? _fechaultegreso = GetValueOrDefault<DateTime?>(reader, "fechaultegreso");
                            
                            minMaxArti = new Models.DMSA.Mbw.Query.StockResult
                            {
                                CodBodegaAgencia = reader.GetInt64(0), //GetValueOrDefault<long>(reader, "codbodegaagencia"),
                                NombreBodega = reader.GetString(1), //GetValueOrDefault<string>(reader, "nombodega"),
                                CantDisponibleWms = reader.GetDouble(2), //GetValueOrDefault<double>(reader, "cantdisponiblewms"),
                                MinimoVtaWeb = reader.GetDouble(3), //GetValueOrDefault<double>(reader, "minimovtaweb"),
                                CantidadReservada = reader.GetDouble(4), //GetValueOrDefault<double>(reader, "cantidadreservada"),
                                fechaultingreso = _fechaultingreso, //GetValueOrDefault<DateTime?>(reader, "fechaultingreso"),
                                fechaultegreso = _fechaultegreso, //GetValueOrDefault<DateTime?>(reader, "fechaultegreso"),
                            };
                            //resultados.Add(minMaxArti);
                            break;
                        }
                    }
                }

                if (minMaxArti != null)
                {
                    // Validar mínimo de venta WEB
                    if (!(minMaxArti.CantDisponibleWms > minMaxArti.MinimoVtaWeb))
                    {
                        minMaxArti.CantDisponibleWms = 0;
                    }

                    var ahora = DateTime.Now;

                    bool ingresoReciente = minMaxArti.fechaultingreso.HasValue &&
                        (ahora - minMaxArti.fechaultingreso.Value).TotalHours <= TotalHoursBefore;

                    bool egresoReciente = minMaxArti.fechaultegreso.HasValue &&
                        (ahora - minMaxArti.fechaultegreso.Value).TotalHours <= TotalHoursBefore;

                    //Evalua si las actualizaciones de stock son recientes
                    
                    if (ingresoReciente || egresoReciente || FullStock)
                    {
                        // Agregar al inventario
                        response.Add(new Models.DMSA.Mbw.Abstract.Inventory
                        {
                            Warehouse = new Warehouse
                            {
                                ExternalId = minMaxArti.CodBodegaAgencia.ToString(),
                                Name = minMaxArti.NombreBodega
                            },
                            Stock = minMaxArti.CantDisponibleWms,
                            Reserved = minMaxArti.CantidadReservada != null ? minMaxArti.CantidadReservada : 0,
                            fechaultingreso = minMaxArti.fechaultingreso,
                            fechaultegreso = minMaxArti.fechaultegreso
                        });
                    }
                    
                }

                // Consulta para obtener el stock de agencias
                sql = $"select v.codbodegaagencia, x.nombodega, v.cantidad, v.cantidadreservada, " +
                        $"v.fechaultingreso, v.fechaultegreso " +
                        $"from vw_emp_age_bod_bxa x " +
                        $"inner join invstock v on v.codbodegaagencia = x.codbodegaagencia " +
                        $"inner join genagencias g on g.codagencia = x.codagencia and NVL(g.envioecommerce, 'X') = 'S' " +
                        $"where x.codempresa = {codEmpresa} " +
                        $"and NVL(x.envioecommerce, 'X') = 'S' " +
                        $"and NVL(x.usaptoventa, 'X') = 'S' " +
                        $"and NVL(x.ubicacion, 'X') = 'EX' " +
                        $"and NVL(x.controldisponiblewms, 'X') = 'N' " +
                        $"and v.codarticulo = {art.Articulo.CodArticulo} " +
                        $"{extra_stores}" +
                        withDateDiff;

                // Ejecuta la consulta SQL                                
                //var stockAgencia = _appDbContext.Set<StockResult>().FromSqlRaw(sql).ToList();

                var stockAgencia = new List<Models.DMSA.Mbw.Query.StockResult>();

                using (var command = connection.CreateCommand())
                {
                    command.CommandText = sql;
                    command.CommandType = CommandType.Text;

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            DateTime? _fechaultingreso = GetValueOrDefault<DateTime?>(reader, "fechaultingreso");
                            DateTime? _fechaultegreso = GetValueOrDefault<DateTime?>(reader, "fechaultegreso");

                            var ahora = DateTime.Now;

                            bool ingresoReciente = _fechaultingreso.HasValue &&
                                (ahora - _fechaultingreso.Value).TotalHours <= TotalHoursBefore;

                            bool egresoReciente = _fechaultegreso.HasValue &&
                                (ahora - _fechaultegreso.Value).TotalHours <= TotalHoursBefore;

                            //Evalua si las actualizaciones de stock son recientes

                            if (ingresoReciente || egresoReciente || FullStock)
                            {
                                int Merchant = 1;
                                string ExternalId = reader.GetInt64(0).ToString();
                                if(ExternalId == "100")
                                {
                                    Merchant = 7;
                                }

                                response.Add(new Models.DMSA.Mbw.Abstract.Inventory
                                {
                                    Warehouse = new Warehouse
                                    {
                                        Merchant = Merchant,  //TODO: 1-Vtex , 7-Mercadolibre
                                        ExternalId = ExternalId, //GetValueOrDefault<long>(reader, "codbodegaagencia").ToString(),
                                        Name = reader.GetString(1) //GetValueOrDefault<string>(reader, "nombodega")
                                    },
                                    Stock = reader.GetDouble(2), //GetValueOrDefault<double>(reader, "cantidad"),
                                    Reserved = reader.GetDouble(3) != null ? reader.GetDouble(3) : 0, //GetValueOrDefault<double>(reader, "cantidadreservada"),
                                    fechaultingreso = _fechaultingreso, //GetValueOrDefault<DateTime?>(reader, "fechaultingreso"),
                                    fechaultegreso = _fechaultegreso, //GetValueOrDefault<DateTime?>(reader, "fechaultegreso"),
                                });
                            }
                            
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine("Error:" + "ObtenerStock:" + e.Message);
                // Manejo de excepciones: podrías registrar la excepción o manejarla de otra manera.
            }

            // Devuelve la respuesta con la lista de inventarios
            return response;
        }

        static public void Test()
        {
            DataSourceManager.AppDbContext _appDbContext = new DataSourceManager.AppDbContext();
            List<string> cadenaArticulos = new List<string>();
            cadenaArticulos.Add("4111");
            cadenaArticulos.Add("11667");
            cadenaArticulos.Add("10556");
            cadenaArticulos.Add("10816");

            //var articulosConMarcas = _appDbContext.ARTICULOSXEMPRESA
            //    .Include(a => a.Marca) 
            //    .Where(a => a.Empresa.CodEmpresa == 1) 
            //    .Select(a => new
            //    {
            //        ArticuloId = a.CodArticulo,
            //        ArticuloNombre = a.CodArticulo,
            //        MarcaId = a.Marca.CodMarca, 
            //        MarcaNombre = a.Marca.Descripcion,
            //        EmpresaId = a.Empresa.CodEmpresa,
            //        EmpresaNombre = a.Empresa.Nombre
            //    })
            //    .ToList();

            var listArticulos = _appDbContext.ARTICULOSXEMPRESA.Include(m => m.Empresa).Where((x) =>
                    (x.Empresa.CodEmpresa.Equals(2)
                    //|| (x.ActivaWeb != "S" && x.ActivaWeb != "N" && x.VentaAlmacenes != "S" && x.VentaAlmacenes != "N" && x.AgotamientoStock == "S")
                    //|| x.Marca.Empresa.CodEmpresa == 2
                    && cadenaArticulos.Contains(x.Articulo.CodArticulo.ToString()))).ToList();

            //var listArticulos = _appDbContext.ARTICULOSXEMPRESA.Include(m => m.Empresa).Where(x=>x.RegistroSanitario == "NSOC17495-14PE").ToList();
            //var listArticulos = _appDbContext.ARTICULOSXEMPRESA.Where(x=>x.CodArticulo == 292).ToList();

            var aticulos_list = _appDbContext.GENARTICULOS.ToList();

            var marcas_list = _appDbContext.GENMARCAS.ToList();

            var empresas_list = _appDbContext.GENEMPRESAS.ToList();

            var Estado_list = _appDbContext.GENESTADOS.ToList();

            //Console.WriteLine(listArticulos.Count);

            //var marcas = from m in _appDbContext.GENMARCAS
            //             join e in _appDbContext.GENEMPRESAS on m.CodEmpresaMarca equals e.CodEmpresa
            //             select new
            //             {
            //                 MarcaNombre = m.Descripcion,
            //                 EmpresaNombre = e.Nombre
            //             };

            var marcas = _appDbContext.GENMARCAS.Include(m => m.Empresa).Where( x=> x.Empresa.CodEmpresa == 1).ToList();

            Console.WriteLine(marcas);
        }

        public async Task<ResponseSkuBulk[]?> SendToMiddleware(
            List<ArticuloDTO> payload,
            RestSharp.Method method,
            string urlApiRest,
            DbContext context)
        {
            var jsonDatos = Newtonsoft.Json.JsonConvert.SerializeObject(payload,
                Newtonsoft.Json.Formatting.Indented,
                new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore });

            ResponseSkuBulk[]? responseData = null;
            //Console.WriteLine(payload);

            string requestPort = "";

            RestClient client = new RestClient(urlApiRest);
            RestRequest request = new RestRequest(requestPort, method);
            request.Timeout = TimeSpan.FromSeconds(0);

            DateTime startDate = DateTime.Now;

            try
            {   
                request.AddBody(jsonDatos);

                //Console.WriteLine("Envio Data: " + jsonDatos);
                //Console.WriteLine("Metodo: " + method);

                var response = client.Execute(request);
                //Console.WriteLine("status: " + response.StatusCode);
                //Console.WriteLine("responseMsg: " + response.Content);

                if (response.IsSuccessful)
                {
                    responseData = JsonConvert.DeserializeObject<ResponseSkuBulk[]?>(response.Content);
                    Debug.WriteLine(responseData);
                }
                else
                {
                    //throw new Exception($"Server returned non-OK status: {response.StatusCode}, message: {response.ErrorMessage}\nServer Response:\n{response.Content}");
                    responseData = new ResponseSkuBulk[]
                    {
                        new ResponseSkuBulk()
                        {
                            external_id = null,
                            inventory = null,
                            non_field_errors = new string[] { response.ErrorException.Message },
                            status_code = (int) response.StatusCode, //response.StatusCode
                        }
                    };
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("catch de envioDataMasivoApiRest : " + e);

                responseData = new ResponseSkuBulk[]
                {
                    new ResponseSkuBulk()
                    {
                        external_id = null,
                        inventory = null,
                        non_field_errors = null,
                        status_code = null
                    }
                };

                string messageCatch = (e.Message != null ? e.Message.Replace("\"", "") : "NULL MESSAGE");
                Console.WriteLine("ERROR: " + messageCatch);                
            }

            return responseData;
        }

        public async Task<ResponseSkuBulk[]?> SendDataToMiddleware(
            string jsonDatos,
            RestSharp.Method method,
            string urlApiRest,
            DbContext context)
        {
            ResponseSkuBulk[]? responseData = null;
            //Console.WriteLine(payload);

            string requestPort = "";

            RestClient client = new RestClient(urlApiRest);
            RestRequest request = new RestRequest(requestPort, method);
            request.AddHeader("Content-Type", "application/json; charset=utf-8");
            request.AddHeader("Accept", "application/json");
            request.Timeout = TimeSpan.FromSeconds(60);

            DateTime startDate = DateTime.Now;

            try
            {
                request.AddBody(jsonDatos);

                var response = await client.ExecuteAsync(request);

                if (response.IsSuccessful)
                {
                    responseData = JsonConvert.DeserializeObject<ResponseSkuBulk[]?>(response.Content);
                    //Debug.WriteLine(responseData);
                }
                else
                {
                    //throw new Exception($"Server returned non-OK status: {response.StatusCode}, message: {response.ErrorMessage}\nServer Response:\n{response.Content}");
                    responseData = new ResponseSkuBulk[]
                    {
                        new ResponseSkuBulk()
                        {
                            external_id = null,
                            inventory = null,
                            non_field_errors = new string[] { response.ErrorException.Message },
                            status_code = (int) response.StatusCode, //response.StatusCode
                        }
                    };
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("catch de SendDataToMiddleware : " + e);

                responseData = new ResponseSkuBulk[]
                {
                    new ResponseSkuBulk()
                    {
                        external_id = null,
                        inventory = null,
                        non_field_errors = null,
                        status_code = null
                    }
                };

                string messageCatch = (e.Message != null ? e.Message.Replace("\"", "") : "NULL MESSAGE");
                Console.WriteLine("ERROR: " + messageCatch);
            }

            return responseData;
        }

        [Obsolete("Debe ser eliminado........")]
        public async Task<List<PrecioDTO>> BuildPrecio(
            ArticulosXEmpresa art,
            ParametersMode1 parametros,
            GenAgencias agenciaMatriz, 
            long nivelWeb,
            GenParametros paramNivel,
             bool vtaExterna,
            double IVA,
            List<FacBonificadosXArticulo> bonificados)
        {
            double precioConDescuento = 0d;
            double precio = 0d;

            List<PrecioDTO> prices = new List<PrecioDTO>();

            var precioDTO = await GetPriceAlmacen(art, agenciaMatriz, long.Parse(paramNivel.Valor), IVA);

            precio = precioDTO.Value;

            foreach (var fbxa in bonificados)
            {
                //restanteArt--;
                //precio = (double) fbxa.Precio;

                precioConDescuento = precio - (precio * ((double)fbxa.PorcDescuento / 100));

                PrecioDTO price = new PrecioDTO
                {
                    ExternalId = (int) fbxa.CodBonificadoArticulo,
                    Store = new Store
                    {
                        ExternalId = fbxa.GenAgencias.CodAgencia,
                        Name = fbxa.GenAgencias.Nombre,
                        Ecommerce = vtaExterna
                    },
                    Type = "discount",
                    Minimum = (double)fbxa.MinimoAplicaDscto,
                    Value = precioConDescuento,
                    //Start = parametros.date_start.ToString("yyyy-MM-dd") + "T00:00:00",
                    //End = parametros.date_end.ToString("yyyy-MM-dd") + "T00:00:00",

                    Start = fbxa.FechaInicio.ToString("yyyy-MM-dd") + "T00:00:00",
                    End = fbxa.FechaFin.ToString("yyyy-MM-dd") + "T00:00:00",

                    Status = fbxa.CodEstado == 1 //GenestadosDAOEXT.ESTADO_ACTIVO
                };

                prices.Add(price);
            }

            return prices;
        }

        public async Task<PrecioDTO> GetPriceAlmacen(ArticulosXEmpresa art, GenAgencias agenciaMatriz, long nivelWeb, double IVA)
        {
            var precioAlm = new PrecioDTO();

            var facPrecioAlmacen = await _appDbContext.FACPRECIOSALMACEN.Where(x => x.CodEmpresa == art.CodEmpresa &&
            x.CodArticulo == art.Articulo.CodArticulo &&
            x.CodUnidadMedida == art.Articulo.CodUnidadPresentacion &&
            x.CodNivel == nivelWeb).FirstOrDefaultAsync();

            double precioAlmacen = 0;

            if (facPrecioAlmacen != null)
            {
                if (art.IncluyeIvaVentas.Equals("S"))
                {
                    precioAlmacen = ((double) facPrecioAlmacen.Precio / (1 + (IVA / 100)));
                }
                else
                {
                    precioAlmacen = (double) facPrecioAlmacen.Precio;
                }

                precioAlm.ExternalId = facPrecioAlmacen.CodArticulo;

                //articulosAL.TryAdd(art.CodArticulo, precioAlmacen);
                precioAlm.Value = precioAlmacen;
            }

            return precioAlm;
        }

        public async Task<PrecioDTO> GetPriceVenta(ArticulosXEmpresa art, GenAgencias agenciaMatriz, long nivelWeb, long clienteWeb, double IVA)
        {
            var precioDto = new PrecioDTO();

            var precioItem = await _appDbContext.FACPRECIOSVENTA.Where(x => x.CodArticulo == art.Articulo.CodArticulo &&
                x.CodTipoCliente == clienteWeb &&
                x.CodAgencia == agenciaMatriz.CodAgencia).FirstOrDefaultAsync();

            //Facpreciosventa precioItem = null;

            //string sqlPrecio = $"select p from Facpreciosventa p " +
            //                   $"where p.genarticulos.codarticulo = {art.Genarticulos.Codarticulo} " +
            //                   $"and p.gentiposclientes.codtipocliente = {clienteWeb} " +
            //                   $"and p.id.genagencias.codagencia = {agenciaMatriz.Codagencia}";

            ////var queryPrecio = objSesion.CreateQuery(sqlPrecio);
            //precioItem = queryPrecio.UniqueResult<Facpreciosventa>();
            double precio = 0;
            if (precioItem != null)
            {
                precio = art.IncluyeIvaVentas.Equals("S") ?
                    ((double)precioItem.Precio / (1 + (IVA / 100))) :
                    (double)precioItem.Precio;

                //precioDto.ExternalId = precioItem.Id.Numprecioventa.ToString();
                precioDto.ExternalId = precioItem.NumPrecioVenta;
                precioDto.Type = "price";
                precioDto.Value = precio;
                //articuloDto.Prices.Add(precioDto);

                //articulosEnvio.Add(articuloDto);

                //articulosVE.TryAdd(art.CodArticulo, precio);
            }

            return precioDto;
        }


        public async Task<List<PrecioDTO>> BuildPrices(
            ArticulosXEmpresa art,            
            long codEmpresa,
            long codAgencia,
            double IVA,
            bool envioAdicional,
            long clienteWeb,
            GenAgencias agenciaMatriz,
            long nivelWeb,
            Dictionary<long, double> articulosVE,
            Dictionary<long, double> articulosAL)
        {
            List<PrecioDTO> pricesList = new List<PrecioDTO>();
            //var articuloDto = new ArticuloDTO();

            //var articuloWeb = await _appDbContext.GENARTICULOSWEB
            //    .Where(x => x.CodArticulo == art.CodArticulo).FirstOrDefaultAsync();

            //if (articuloWeb != null &&
            //    articuloWeb.ArticuloVariable != null &&
            //    articuloWeb.ArticuloVariable.Equals("S", StringComparison.OrdinalIgnoreCase))
            //{
            //    var articuloPadre = await _appDbContext.ARTICULOSXEMPRESA
            //        .Include(a => a.Articulo)
            //        .Where(x => x.CodArticulo == articuloWeb.CodArticuloPadre).FirstOrDefaultAsync();
            //    articuloDto.Product = new ArticuloDTOProduct();
            //    articuloDto.Product.ExternalId = articuloPadre.Articulo.CodArticulo.ToString();
            //    articuloDto.Product.Name = !string.IsNullOrEmpty(articuloPadre.Articulo.DescripcionCorta) ?
            //        articuloPadre.Articulo.DescripcionCorta.Trim() :
            //        articuloPadre.Articulo.Descripcion.Trim();
            //    articuloDto.Product.Reference = articuloPadre.Articulo.CodAlterno;
            //}

            //articuloDto.ExternalId = art.Articulo.CodArticulo.ToString();
            //articuloDto.Name = !string.IsNullOrEmpty(art.Articulo.DescripcionCorta) ?
            //        art.Articulo.DescripcionCorta.Trim() :
            //        art.Articulo.Descripcion.Trim();
            //articuloDto.Reference = art.Articulo.CodAlterno;
            //articuloDto.Weight = art.Articulo.MedidaPeso != null ? (double)art.Articulo.MedidaPeso : 0;
            //articuloDto.Width = art.Articulo.MedidaFrente != null ? (double)art.Articulo.MedidaFrente : 0;
            //articuloDto.Height = art.Articulo.MedidaAlto != null ? (double)art.Articulo.MedidaAlto : 0;
            //articuloDto.Length = art.Articulo.MedidaFondo != null ? (double)art.Articulo.MedidaFondo : 0;
            //articuloDto.Status = art.Estado.CodEstado.Equals(1);
            //articuloDto.ShowWeb = art.ActivaWeb.Equals("S");
            //articuloDto.ShowStore = art.VentaAlmacenes.Equals("S");
            //articuloDto.UnidadPresentacion = art.Articulo.CodUnidadPresentacion;

            //count++;

            if (envioAdicional)
            {
                var precioDto = new PrecioDTO();

                var precioItem = await _appDbContext.FACPRECIOSVENTA.Where(x => x.CodArticulo == art.Articulo.CodArticulo &&
                x.CodTipoCliente == clienteWeb &&
                x.CodAgencia == agenciaMatriz.CodAgencia).FirstOrDefaultAsync();

                //Facpreciosventa precioItem = null;

                //string sqlPrecio = $"select p from Facpreciosventa p " +
                //                   $"where p.genarticulos.codarticulo = {art.Genarticulos.Codarticulo} " +
                //                   $"and p.gentiposclientes.codtipocliente = {clienteWeb} " +
                //                   $"and p.id.genagencias.codagencia = {agenciaMatriz.Codagencia}";

                ////var queryPrecio = objSesion.CreateQuery(sqlPrecio);
                //precioItem = queryPrecio.UniqueResult<Facpreciosventa>();

                if (precioItem != null)
                {
                    double precio = art.IncluyeIvaVentas.Equals("S") ?
                        ((double)precioItem.Precio / (1 + (IVA / 100))) :
                        (double)precioItem.Precio;

                    //precioDto.ExternalId = precioItem.Id.Numprecioventa.ToString();
                    precioDto.ExternalId = precioItem.NumPrecioVenta;
                    precioDto.Type = "price";
                    precioDto.Value = precio;
                    //articuloDto.Prices.Add(precioDto);
                    //articulosEnvio.Add(articuloDto);

                    articulosVE.TryAdd(art.CodArticulo, precio);
                    pricesList.Add(precioDto);
                }

                var precioAlm = new PrecioDTO();

                var facPrecioAlmacen = await _appDbContext.FACPRECIOSALMACEN.Where(x => x.CodEmpresa == codEmpresa &&
                x.CodArticulo == art.Articulo.CodArticulo &&
                x.CodUnidadMedida == art.Articulo.CodUnidadPresentacion &&
                x.CodNivel == nivelWeb).FirstOrDefaultAsync();

                double precioAlmacen = 0;

                if (facPrecioAlmacen != null)
                {
                    if (art.IncluyeIvaVentas.Equals("S"))
                    {
                        precioAlmacen = ((double)facPrecioAlmacen.Precio / (1 + (IVA / 100)));
                    }
                    else
                    {
                        precioAlmacen = (double)facPrecioAlmacen.Precio;
                    }

                    precioAlm.ExternalId = facPrecioAlmacen.CodArticulo;

                    articulosAL.TryAdd(art.CodArticulo, precioAlmacen);
                }
            }
            return pricesList;
        }

        public async  Task<List<PrecioDTO>> BuildDiscounts(
            ArticulosXEmpresa art,
            ParametersMode1 parametros,
            //FacBonificadosXArticulo fbxa,
            GenAgencias agenciaMatriz,
            long clienteWeb,
            GenParametros paramNivel,
            bool vtaExterna,
            double IVA)
        {
            List<FacBonificadosXArticulo> bonificados = new List<FacBonificadosXArticulo>();
            string sql = "";

            DateTime today = DateTime.Today;
            DateTime fechaExclusion = new DateTime(today.Year, 12, 31);

            if (vtaExterna)
            {
                bonificados = await _appDbContext
                    .FACBONIFICADOSXARTICULO
                    .Include(y => y.GenAgencias)
                    .Where(
                    predicate: x => x.CodArticulo.Equals(art.CodArticulo)
                    && x.CodEmpresa == art.CodEmpresa
                    && x.CodAgencia == agenciaMatriz.CodAgencia
                    && x.GenAgencias.EnvioEcommerce == "S"
                    && x.CodTipoCliente == clienteWeb
                    && (today <= x.FechaFin && x.FechaFin != fechaExclusion)
                    && x.CodEstado == 1
                    )
                    .OrderByDescending(x => x.PorcDescuento)
                    .ToListAsync();
            }            
            else
            {
                // Solo se envia Descuentos con Unidad de Presentacion
                //bonificados = await _appDbContext
                //    .FACBONIFICADOSXARTICULO
                //    .Include(y => y.GenAgencias)
                //    .Where(
                //    predicate: x => x.CodArticulo.Equals(art.CodArticulo)
                //    && x.CodEmpresa == art.CodEmpresa
                //    && x.GenAgencias.EnvioEcommerce == "S"
                //    && x.CodNivel == decimal.Parse(paramNivel.Valor)
                //    && (today <= x.FechaFin && x.FechaFin != fechaExclusion)
                //    && x.CodEstado == 1)
                //    .OrderByDescending(x => x.PorcDescuento)
                //    .ToListAsync();

                bonificados = await _appDbContext.FACBONIFICADOSXARTICULO
                    .Include(y => y.GenAgencias)
                    .Include(y => y.Articulo)
                    .Include(y => y.GenUnidadesMedida)
                    .Include(y => y.FacNivelesPreciosFk)
                    .Where(x =>
                        x.Articulo.CodArticulo == art.CodArticulo &&
                        x.Articulo.CodUnidadPresentacion == x.GenUnidadesMedida.CodUnidadMedida &&
                        x.CodEmpresa == art.CodEmpresa &&
                        (x.GenAgencias.EnvioEcommerce ?? "X") == "S" &&
                        x.CodEstado == 1 &&
                        x.FacNivelesPreciosFk.CodNivel == decimal.Parse(paramNivel.Valor) &&
                        x.FacNivelesPreciosFk.CodEmpresa == art.CodEmpresa
                        && (today <= x.FechaFin && x.FechaFin != fechaExclusion)
                    )
                    .OrderByDescending(x => x.PorcDescuento)
                    .ToListAsync();
            }

            List<ArticuloDTO> articles = new List<ArticuloDTO>();
            List<PrecioDTO> prices = new List<PrecioDTO>();

            double precioConDescuento = 0d;
            double precio = 0d;

            //DiscountPayload payload = new DiscountPayload();
            int countDsct = 0;
            bool datos = false;

            //double restanteArt = articulosWeb.Count;

            if (bonificados.Count > 0)
            {
                //var article = new ArticuloDTO
                //{
                //    ExternalId = art.CodArticuloEmpresa.ToString()
                //};

                countDsct = 0;

                var precioDTO = await GetPriceAlmacen(art, agenciaMatriz, long.Parse(paramNivel.Valor), IVA);

                precio = precioDTO.Value;

                foreach (var fbxa in bonificados)
                {
                    //restanteArt--;
                    //precio = (double) fbxa.Precio;
                    if (fbxa.CodEstado != 1) continue;

                    precioConDescuento = precio - (precio * ((double)fbxa.PorcDescuento / 100));

                    PrecioDTO price = new PrecioDTO
                    {
                        ExternalId = (int) fbxa.CodBonificadoArticulo,
                        Store = new Store
                        {
                            ExternalId = fbxa.GenAgencias.CodAgencia,
                            Name = fbxa.GenAgencias.Nombre,
                            Ecommerce = vtaExterna
                        },
                        Type = "discount",
                        Minimum = (double) fbxa.MinimoAplicaDscto,
                        Value = precioConDescuento,
                        Start = fbxa.FechaInicio.ToString("yyyy-MM-ddT00:00:00"),
                        End = fbxa.FechaFin.ToString("yyyy-MM-ddT23:59:59"),
                        Status = fbxa.CodEstado == 1 //GenestadosDAOEXT.ESTADO_ACTIVO
                    };

                    prices.Add(price);
                }
            }
            
            countDsct++;
            datos = true;

            return prices;
        }

        public async Task<List<ArticuloDTO>> MakeProducts(ParametersMode1 parametros)
        {
            bool buildBodyProduct = false;

            int codEmpresa = 2;
            string tmp_apikey = "9+7e3A7t4qI1Rl8XQ2GjKjs8KhZ9Y8p1MfbQvKlkmP4=";

            var pClienteWeb = await _appDbContext.GENPARAMETROS.Where(x => x.CodEmpresa == codEmpresa && x.CodParametro == "PRECIO_WEB").FirstOrDefaultAsync();
            long clienteWeb = (pClienteWeb != null) ? long.Parse(pClienteWeb.Valor) : 0l;

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
            
            List<FacBonificadosXArticulo> dataSource_tmp = null;
            //if (true)
            //{
            //parametros.date_start = DateTime.Now.Date;
            //parametros.date_end = DateTime.Now.Date.AddDays(1).AddSeconds(-1);

            dataSource_tmp = await _appDbContext.FACBONIFICADOSXARTICULO
                .Where(c => c.CodEmpresa == 2
                    && c.FechaInicio >= parametros.date_start
                    && c.FechaFin <= parametros.date_end
                    && c.CodTipoCliente == clienteWeb
                    && c.CodEstado == 1)
                    //&& codArticulos.Contains(c.CodArticulo))
                    //.Take(2000)
                    .ToListAsync();

                var codArticulosTmp = dataSource_tmp.Select(articulo => articulo.CodArticulo).ToArray();

                for (int ib = 0; ib < articulos_brand.Count; ib++)
                {
                    if (!codArticulosTmp.Contains(articulos_brand[ib].CodArticulo))
                        articulos_brand.RemoveAt(ib);
                }
            //}

            articulos_articulos.AddRange(articulos_brand);

            double IVA = 0d;
            var pIVA = await _appDbContext.GENPARAMETROS.Where(x => x.CodEmpresa == codEmpresa && x.CodParametro == "IVA").FirstOrDefaultAsync();
            IVA = (pIVA != null) ? double.Parse(pIVA.Valor) : 0d;

            

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

                        var articuloDto = new ArticuloDTO();

                        var articuloWeb = await _appDbContext.GENARTICULOSWEB
                            .Where(x => x.CodArticulo == art.CodArticulo).FirstOrDefaultAsync();

                        if (articuloWeb != null &&
                            articuloWeb.ArticuloVariable != null &&
                            articuloWeb.ArticuloVariable.Equals("S", StringComparison.OrdinalIgnoreCase))
                        {
                            var articuloPadre = await _appDbContext.ARTICULOSXEMPRESA
                                .Include(a => a.Articulo)
                                .Where(x => x.CodArticulo == articuloWeb.CodArticuloPadre).FirstOrDefaultAsync();
                            articuloDto.Product = new ArticuloDTOProduct();
                            articuloDto.Product.ExternalId = articuloPadre.Articulo.CodArticulo.ToString();
                            articuloDto.Product.Name = !string.IsNullOrEmpty(articuloPadre.Articulo.DescripcionCorta) ?
                                articuloPadre.Articulo.DescripcionCorta.Trim() :
                                articuloPadre.Articulo.Descripcion.Trim();
                            articuloDto.Product.Reference = articuloPadre.Articulo.CodAlterno;
                        }

                        articuloDto.ExternalId = art.Articulo.CodArticulo.ToString();
                        
                        if (buildBodyProduct)
                        {
                            articuloDto.Name = !string.IsNullOrEmpty(art.Articulo.DescripcionCorta) ?
                                    art.Articulo.DescripcionCorta.Trim() :
                                    art.Articulo.Descripcion.Trim();
                            articuloDto.Reference = art.Articulo.CodAlterno;
                            articuloDto.Weight = art.Articulo.MedidaPeso != null ? (double)art.Articulo.MedidaPeso : 0;
                            articuloDto.Width = art.Articulo.MedidaFrente != null ? (double)art.Articulo.MedidaFrente : 0;
                            articuloDto.Height = art.Articulo.MedidaAlto != null ? (double)art.Articulo.MedidaAlto : 0;
                            articuloDto.Length = art.Articulo.MedidaFondo != null ? (double)art.Articulo.MedidaFondo : 0;
                            articuloDto.Status = art.Estado.CodEstado.Equals(1);
                            articuloDto.ShowWeb = art.ActivaWeb.Equals("S");
                            articuloDto.ShowStore = art.VentaAlmacenes.Equals("S");
                            articuloDto.UnidadPresentacion = art.Articulo.CodUnidadPresentacion;
                        }

                        if(parametros.with_prices)
                        {
                            var pricesList = await ecommerceService.BuildPrices(
                                art,
                                //articuloDto,
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
                                if (pricesList != null)
                                {
                                    articuloDto.Prices = pricesList;
                                }
                            }
                        }

                        if(parametros.with_discount)
                        {
                            bool vtaExterna = true;
                            var pricesDiscounts = await ecommerceService.BuildDiscounts(art, parametros, agenciaMatriz, clienteWeb, paramNivel, vtaExterna, IVA);
                            //var pricesDiscounts2 = await ecommerceService.BuildDiscounts(art, parametros, agenciaMatriz, clienteWeb, paramNivel, !vtaExterna, IVA);

                            articuloDto.Prices.AddRange(pricesDiscounts);
                            //articuloDto.Prices.AddRange(pricesDiscounts2);
                        }

                        if (parametros.with_stock)
                        {
                            if (articuloDto != null)
                            {
                                var stockResponse = await BuildStock(codEmpresa, codAgencia, art, parametros);

                                if (stockResponse != null && stockResponse.Count > 0)
                                {
                                    articuloDto.InventoryList = stockResponse;
                                }
                            }
                        }

                        if (articuloDto != null)
                        {                            
                            articulosEnvio.Add(articuloDto);                            
                            //count++;
                        }
                    }
                }
            }

            return articulosEnvio;            
        }

        public async Task<List<ArticuloDTO>> BuildItemForSendDiscounts(
            int codempresa,
            Dictionary<long, double> articulosWeb,
            bool vtaExterna,
            string url,
            bool guardaBitacora,
            GenAgencias agenciaMatriz,
            long clienteWeb,
            GenParametros paramNivel)
        {

            List<ArticuloDTO> articles = new List<ArticuloDTO>();

            double precioConDescuento = 0d;
            double precio = 0d;

            //DiscountPayload payload = new DiscountPayload();
            int countDsct = 0;
            bool datos = false;

            try
            {
                double restanteArt = articulosWeb.Count;
                Console.WriteLine($"Total de Articulos para enviar descuentos: {restanteArt} {(vtaExterna ? "Venta Externa" : "Venta Almacen")}");
                
                foreach (var entry in articulosWeb)
                {
                    restanteArt--;

                    //List<ArticuloDTO> jsonObject = new List<ArticuloDTO>();                    

                    List<FacBonificadosXArticulo> bonificados = new List<FacBonificadosXArticulo>();
                    string sql = "";

                    if (vtaExterna)
                    {
                        bonificados = await _appDbContext.FACBONIFICADOSXARTICULO.Where(
                            predicate: x => x.CodArticulo.Equals(entry.Key)
                            && x.CodEmpresa == codempresa
                            && x.CodAgencia == agenciaMatriz.CodAgencia
                            && x.CodTipoCliente == clienteWeb
                            && x.FechaInicio >= new DateTime(2024, 10, 1))
                            .OrderByDescending(x => x.PorcDescuento)
                            .ToListAsync();

                        //sql = $"select s from Facbonificadosxarticulo s where s.genarticulos.codarticulo = {entry.Key} " +
                        //      $"and s.genempresas.codempresa = {codempresa} " +
                        //      $"and s.genagencias.codagencia = {agencia.Codagencia} " +
                        //      $"and s.gentiposclientes.codtipocliente = {clienteWeb} " +
                        //      $"and trunc(s.fechainicio) >= to_date('01/10/2024', 'dd/mm/yyyy') order by s.porcdescuento desc";
                    }
                    else
                    {
                        // Solo se envia Descuentos con Unidad de Presentacion

                        bonificados = await _appDbContext.FACBONIFICADOSXARTICULO.Where(
                            predicate: x => x.CodArticulo.Equals(entry.Key)
                            && x.CodEmpresa == codempresa
                            && x.GenAgencias.EnvioEcommerce == "S"
                            && x.CodNivel == decimal.Parse(paramNivel.Valor)
                            && x.FechaInicio >= new DateTime(2024, 10, 1))
                            .OrderByDescending(x => x.PorcDescuento)
                            .ToListAsync();

                        //sql = $"select s from Facbonificadosxarticulo s where s.genarticulos.codarticulo = {entry.Key} " +
                        //      $"and s.genarticulos.codunidadpresentacion = s.genunidadesmedida.codunidadmedida " +
                        //      $"and s.genempresas.codempresa = {codempresa} " +
                        //      $"and NVL(s.genagencias.envioecommerce, 'X') = 'S' " +
                        //      $"and s.facnivelesprecios.id.codnivel = {paramNivel.VALOR} " +
                        //      $"and s.facnivelesprecios.id.genempresas.codempresa = {codempresa} " +
                        //      $"and trunc(s.fechainicio) >= to_date('01/10/2024', 'dd/mm/yyyy') order by s.porcdescuento desc";
                    }

                    //IQuery queryDescuento = session.CreateQuery(sql);
                    //bonificados = queryDescuento.List<Facbonificadosxarticulo>().ToList();
                    Console.WriteLine($"ITEM {entry.Key} TOTAL DE DESCUENTOS: {bonificados.Count}");

                    if (bonificados.Count > 0)
                    {
                        var article = new ArticuloDTO
                        {
                            ExternalId = entry.Key.ToString()
                        };

                        countDsct = 0;

                        foreach (var fbxa in bonificados)
                        {
                            string fechainicio = fbxa.FechaInicio.ToString("yyyy-MM-dd") + "T00:00:00";
                            string fechafin = fbxa.FechaFin.ToString("yyyy-MM-dd") + "T23:59:59";

                            precio = entry.Value;
                            precioConDescuento = precio - (precio * ((double)fbxa.PorcDescuento / 100));

                            PrecioDTO price = new PrecioDTO
                            {
                                ExternalId = (int) fbxa.CodBonificadoArticulo,
                                Store = new Store
                                {
                                    ExternalId = fbxa.GenAgencias.CodAgencia,
                                    Name = fbxa.GenAgencias.Nombre,
                                    Ecommerce = vtaExterna
                                },
                                Type = "discount",
                                Minimum = (double)fbxa.MinimoAplicaDscto,
                                Value = precioConDescuento,
                                Start = fechainicio,
                                End = fechafin,
                                Status = fbxa.CodEstado == 1 //GenestadosDAOEXT.ESTADO_ACTIVO
                            };

                            article.Prices.Add(price);
                            countDsct++;
                            datos = true;
                        }

                        //payload.Articles.Add(article);
                        articles.Add(article);
                    }

                    //if ((payload.Articles.Count == 10 || restanteArt == 0) && datos)
                    //{
                    //    datos = false;
                    //    SendToMiddleware(codempresa, agencia.CodAgencia, payload, Method.Put, url, "ADICIONALES", "GUARDAR DESCUENTOS POR PRECIO", true, guardaBitacora, _appDbContext);
                    //    payload.Articles.Clear();
                    //}
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            return articles;
        }

        public void EnvioDescuentoXCambioPrecio(
            int codempresa,
            Dictionary<long, double> articulosWeb,
            bool vtaExterna,
            string url,
            bool guardaBitacora)
        {
            double precioConDescuento = 0d;
            double precio = 0d;

            DiscountPayload payload = new DiscountPayload();
            int countDsct = 0;
            bool datos = false;

            try
            {
                double restanteArt = articulosWeb.Count;
                Console.WriteLine($"Total de Articulos para enviar descuentos: {restanteArt} {(vtaExterna ? "Venta Externa" : "Venta Almacen")}");

                var param = _appDbContext.GENPARAMETROS.Where(
                    predicate: x => x.CodParametro.Equals("TIPO_CLIENTE_DEFAULT_WEB") &&
                    x.CodEmpresa == codempresa
                    ).FirstOrDefault();

                if (param == null)
                    throw new Exception("No se ha configurado el parámetro TIPO_CLIENTE_DEFAULT_WEB");

                var paramNivel = _appDbContext.GENPARAMETROS.Where(
                    predicate: x => x.CodParametro.Equals("TIPO_NIVEL_DEFAULT_WEB") &&
                    x.CodEmpresa == codempresa
                ).FirstOrDefault();

                if (paramNivel == null)
                    throw new Exception("No se ha configurado el parámetro TIPO_NIVEL_DEFAULT_WEB");

                long clienteWeb = long.Parse(param.Valor);

                var agencia = _appDbContext.GENAGENCIAS.Where(
                    predicate: x => x.CodEmpresa == codempresa
                    ).FirstOrDefault();

                if (agencia == null)
                    throw new Exception("No existe agencia matriz para la empresa.");

                foreach (var entry in articulosWeb)
                {
                    restanteArt--;

                    //List<ArticuloDTO> jsonObject = new List<ArticuloDTO>();                    

                    List<FacBonificadosXArticulo> bonificados = new List<FacBonificadosXArticulo>();
                    string sql = "";

                    if (vtaExterna)
                    {
                        bonificados = _appDbContext.FACBONIFICADOSXARTICULO.Where(
                            predicate: x => x.CodArticulo.Equals(entry.Key)
                            && x.CodEmpresa == codempresa
                            && x.CodAgencia == agencia.CodAgencia
                            && x.CodTipoCliente == clienteWeb
                            && x.FechaInicio >= new DateTime(2024, 10, 1))
                            .OrderByDescending(x => x.PorcDescuento)
                            .ToList();

                        //sql = $"select s from Facbonificadosxarticulo s where s.genarticulos.codarticulo = {entry.Key} " +
                        //      $"and s.genempresas.codempresa = {codempresa} " +
                        //      $"and s.genagencias.codagencia = {agencia.Codagencia} " +
                        //      $"and s.gentiposclientes.codtipocliente = {clienteWeb} " +
                        //      $"and trunc(s.fechainicio) >= to_date('01/10/2024', 'dd/mm/yyyy') order by s.porcdescuento desc";
                    }
                    else
                    {
                        // Solo se envia Descuentos con Unidad de Presentacion

                        bonificados = _appDbContext.FACBONIFICADOSXARTICULO.Where(
                            predicate: x => x.CodArticulo.Equals(entry.Key)                            
                            && x.CodEmpresa == codempresa
                            && x.GenAgencias.EnvioEcommerce == "S"
                            && x.CodNivel == decimal.Parse(paramNivel.Valor)
                            && x.FechaInicio >= new DateTime(2024, 10, 1))
                            .OrderByDescending(x => x.PorcDescuento)
                            .ToList();

                        //sql = $"select s from Facbonificadosxarticulo s where s.genarticulos.codarticulo = {entry.Key} " +
                        //      $"and s.genarticulos.codunidadpresentacion = s.genunidadesmedida.codunidadmedida " +
                        //      $"and s.genempresas.codempresa = {codempresa} " +
                        //      $"and NVL(s.genagencias.envioecommerce, 'X') = 'S' " +
                        //      $"and s.facnivelesprecios.id.codnivel = {paramNivel.VALOR} " +
                        //      $"and s.facnivelesprecios.id.genempresas.codempresa = {codempresa} " +
                        //      $"and trunc(s.fechainicio) >= to_date('01/10/2024', 'dd/mm/yyyy') order by s.porcdescuento desc";
                    }

                    //IQuery queryDescuento = session.CreateQuery(sql);
                    //bonificados = queryDescuento.List<Facbonificadosxarticulo>().ToList();
                    Console.WriteLine($"ITEM {entry.Key} TOTAL DE DESCUENTOS: {bonificados.Count}");

                    if (bonificados.Count > 0)
                    {
                        ArticuloDTO article = new ArticuloDTO
                        {
                            ExternalId = entry.Key.ToString()
                        };

                        countDsct = 0;

                        foreach (var fbxa in bonificados)
                        {
                            string fechainicio = fbxa.FechaInicio.ToString("yyyy-MM-dd") + "T00:00:00";
                            string fechafin = fbxa.FechaFin.ToString("yyyy-MM-dd") + "T23:59:59";

                            precio = entry.Value;
                            precioConDescuento = precio - (precio * ((double) fbxa.PorcDescuento / 100));

                            PrecioDTO price = new PrecioDTO
                            {
                                ExternalId = (int) fbxa.CodBonificadoArticulo,
                                Store = new Store
                                {
                                    ExternalId = fbxa.GenAgencias.CodAgencia,
                                    Name = fbxa.GenAgencias.Nombre,
                                    Ecommerce = vtaExterna
                                },
                                Type = "discount",
                                Minimum = (double) fbxa.MinimoAplicaDscto,
                                Value = precioConDescuento,
                                Start = fechainicio,
                                End = fechafin,
                                Status = fbxa.GenEstados.CodEstado == 1 //GenestadosDAOEXT.ESTADO_ACTIVO
                            };

                            article.Prices.Add(price);
                            countDsct++;
                            datos = true;
                        }

                        payload.Articles.Add(article);
                    }

                    if ((payload.Articles.Count == 10 || restanteArt == 0) && datos)
                    {
                        datos = false;
                        //SendToMiddleware(codempresa, agencia.CodAgencia, payload, Method.Put, url, "ADICIONALES", "GUARDAR DESCUENTOS POR PRECIO", true, guardaBitacora, _appDbContext);
                        payload.Articles.Clear();
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
    }
}
