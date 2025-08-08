using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Models.DMSA.Mbw.Clientes;
using System.Data;
using Models.DMSA.Shared.General;
using System.Text.RegularExpressions;
using System.Diagnostics;
using Oracle.ManagedDataAccess.Client;
using Models.DMSA.Mbw.Abstract;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;
using Ubiety.Dns.Core;
using DataSourceManager;
using Models.DMSA.Mbw.Query;
using System.Data.SqlClient;

namespace ApiTradeHub.Controllers.Customer
{
    //[ApiKey]
    [Route("api/[controller]")]
    [ApiController]
    public class MBWInvoiceController : ControllerBase
    {
        DataSourceManager.AppDbContext _appDbContext = new DataSourceManager.AppDbContext();

        private readonly IWebHostEnvironment _webHostEnvironment;

        public MBWInvoiceController(IWebHostEnvironment webHostEnvironment)
        {
            _webHostEnvironment = webHostEnvironment;
        }

        //53	No Autorizado
        //52	Autorizado
        //54	Por Autorizar
        [HttpGet("GetInvoiceHeader")]
        public async Task<ActionResult<ApiResponse_v1>> GetInvoiceHeader(DateTime date_ini, DateTime date_end)
        {
            ApiResponse_v1 apiResponse_V1 = new ApiResponse_v1();
            apiResponse_V1.success = true;
            apiResponse_V1.final = true;
            apiResponse_V1.exito = true;
            apiResponse_V1.mensaje = "Proceso realizado exitosamente";
            
            List<InvoiceHeader> response = new List<InvoiceHeader>();

            string rutaArchivo = Path.Combine(_webHostEnvironment.WebRootPath, "data/query/invoice_header.sql");
            
            if (!System.IO.File.Exists(rutaArchivo))
            {
                return NotFound("El archivo sql de busqueda no existe.");
            }

            // Lee el contenido del archivo de manera asíncrona
            string sql = await System.IO.File.ReadAllTextAsync(rutaArchivo);

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

                        command.Parameters.Add(new OracleParameter(":date_ini", OracleDbType.Date) { Value = date_ini });
                        command.Parameters.Add(new OracleParameter(":date_end", OracleDbType.Date) { Value = date_end });

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                var invoiceHeader = DataTools.MapToObject<InvoiceHeader>(reader);
                                response.Add(invoiceHeader);

                                //response.Add(new InvoiceHeader
                                //{
                                //    ApellidosCliente = reader.GetString("")
                                //});
                            }
                        }

                    }
                }
            }
            
            apiResponse_V1.data = response;
            apiResponse_V1.cantidad_registros = 0;
            return apiResponse_V1;
        }
    }
}
