using ApiTradeHub.Controllers.Ecommerce;
using DataSourceManager;
using Models.DMSA.Mbw.Core;
using Models.DMSA.Mbw.Inventario;
using Models.DMSA.Mbw.Sales;
using Newtonsoft.Json.Linq;
using RestSharp;
using System.Net;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query.Internal;
using System.Xml;
using Newtonsoft.Json;
using System.DirectoryServices.Protocols;
//using ApiTradeHub.Services.Sales.Models;
using System.Security.Policy;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Data;
using static System.Net.WebRequestMethods;
using Org.BouncyCastle.Utilities;
using System.Text.RegularExpressions;
using Models.DMSA.Mbw.Query;

namespace ApiTradeHub.Services.Sales
{
    public class DbService
    {
        DataSourceManager.AppDbContext _appDbContext = new DataSourceManager.AppDbContext();

        public async Task<bool> GetDBTableSizes(long codEmpresa, long codAgencia, ArticulosXEmpresa art)
        {            
            try
            {
                // Consulta para obtener el stock del artículo WMS
                string sql = $@"SELECT 
                    segment_name AS table_name,
                    ROUND(SUM(bytes) / (1024 * 1024), 2) AS size_mb
                FROM
                    user_segments
                WHERE
                    segment_type = 'TABLE'

                GROUP BY
                    segment_name; ";

                // Ejecuta la consulta SQL                                
                //var minMaxArti = _appDbContext.Set<StockResult>().FromSqlRaw(sql).FirstOrDefault();
                //var precios = _appDbContext.Database.SqlQuery<FacPrecioDTO>(sql).ToList();



                var minMaxArti = new StockResult();

                var connectionString = _appDbContext.Database.GetDbConnection().ConnectionString;
                var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
                optionsBuilder.UseOracle(connectionString);

                using (var newContext = new AppDbContext(optionsBuilder.Options))
                {
                    using (var connection = newContext.Database.GetDbConnection())
                    {
                        await connection.OpenAsync();

                        using (var command = connection.CreateCommand())
                        {
                            command.CommandText = sql;
                            command.CommandType = CommandType.Text;

                            using (var reader = await command.ExecuteReaderAsync())
                            {
                                while (await reader.ReadAsync())
                                {
                                    minMaxArti = new StockResult
                                    {
                                        CodBodegaAgencia = reader.GetInt64(0),
                                        NombreBodega = reader.GetString(1),
                                        CantDisponibleWms = reader.GetDouble(2),
                                        MinimoVtaWeb = reader.GetDouble(3),
                                        CantidadReservada = reader.GetDouble(4)
                                    };
                                    //resultados.Add(minMaxArti);
                                    break;
                                }
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
            return true;
        }


    }
}
