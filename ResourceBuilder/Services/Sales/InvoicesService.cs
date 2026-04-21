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
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
//using Models.DMSA.Mbw.Query;
using Oracle.ManagedDataAccess.Client;
using System.Data;
using DMSA.Models.Clientes.Aprobaciones.Responses;
using DMSA.Models.General.Requests;
using System.Text.Json.Nodes;
using DMSA.Models.Security;
using DMSA.Models.Odoo.Import;
using Models.DMSA.Shared.Tools;
using System.Collections.Generic;
using DMSA.Models.Odoo.General.Requests;
using DMSA.Models.Odoo.General.Responses;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;
using System.Diagnostics;
using System.Data.Common;

namespace ResourceBuilder.Services.Sales
{
    public partial class InvoicesService
    {
        string connection_string = string.Empty;

        private readonly AppDbContext _appDbContext;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public InvoicesService(AppDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _appDbContext = context;
            _webHostEnvironment = webHostEnvironment;
        }

        private void ShowError(string NameSource, Exception e)
        {
            Console.WriteLine(NameSource);
            Console.WriteLine("==========================================");
            Debug.WriteLine("Error:" + e.Message);
            Console.WriteLine("Erro:" + e.Message);
            Console.WriteLine("==========================================");
        }

        public async Task<int> LaunchBatchSync(DateTime date_ini, DateTime date_end)
        {
            int totalHeaders = 0;
            //Debug.WriteLine("Iniciando importación MBW -> Odoo    " + DateTime.Now);
            //Debug.WriteLine("date_ini    " + date_ini);
            //Debug.WriteLine("date_end    " + date_end);

            Console.WriteLine("Iniciando importación MBW -> Odoo    " + DateTime.Now);
            Console.WriteLine("date_ini    " + date_ini);
            Console.WriteLine("date_end    " + date_end);
            
            var resHeaders = await GetInvoiceHeader(date_ini, date_end);
            var ids_headers = resHeaders.Select(doc => doc.NumCmprVenta).ToArray();
            if (ids_headers.Length > 0)
            {
                Debug.WriteLine("Cabeceras totales:[" + ids_headers.Length + "]");
                Console.WriteLine("Cabeceras totales:[" + ids_headers.Length + "]");

                const int batchSize = 1000; // Tamaño del bloque
                for (int i = 0; i < ids_headers.Length; i += batchSize)
                {
                    // Tomar el siguiente bloque de 1000 elementos
                    var batch = ids_headers.Skip(i).Take(batchSize).ToArray();
                    totalHeaders += batch.Length;

                    Debug.WriteLine($"Procesando bloque de {batch.Length} registros (desde {i} hasta {i + batch.Length - 1} de {ids_headers.Length})");
                    Console.WriteLine($"Procesando bloque de {batch.Length} registros (desde {i} hasta {i + batch.Length - 1} de {ids_headers.Length})");

                    Console.WriteLine($"Obteniendo datos...");
                    var resDetails = await GetInvoiceDetails(batch);
                    Debug.WriteLine("Detalles:[" + resDetails.Count + "]");
                    Console.WriteLine("Detalles:[" + resDetails.Count + "]");

                    var resPayments = await GetInvoicePayments(batch);
                    Debug.WriteLine("Pagos:[" + resPayments.Count + "]");
                    Console.WriteLine("Pagos:[" + resPayments.Count + "]");

                    Console.WriteLine($"Insertando datos...");
                    var resInsHeaders01 = await PutInvoiceHeader(resHeaders.Where(r => batch.Contains(r.NumCmprVenta)).ToList());
                    var resInsDetails01 = await PutInvoiceDetails(resDetails);
                    var resInsPayments01 = await PutInvoicePayments(resPayments);
                }
            }
            else
            {
                Debug.WriteLine("Datos de cabecera no encontrados....");
                Console.WriteLine("Datos de cabecera no encontrados....");
            }

            Debug.WriteLine("Fin: SyncManager" + DateTime.Now);

            return totalHeaders;
        }

        public async Task<List<InvoiceHeader>> GetInvoiceHeader(DateTime date_ini, DateTime date_end)
        {
            List<InvoiceHeader> response = new List<InvoiceHeader>();

            string rutaArchivo = Path.Combine(_webHostEnvironment.WebRootPath, "data/query/invoice_header.sql");

            if (!System.IO.File.Exists(rutaArchivo))
            {                
                throw new ArgumentException($"El archivo sql de busqueda no existe.");
            }

            string sql = await System.IO.File.ReadAllTextAsync(rutaArchivo);

            //var connectionString = _appDbContext.Database.GetDbConnection().ConnectionString;
            //var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
            //optionsBuilder.UseOracle(connectionString);

            //using (var newContext = new AppDbContext(optionsBuilder.Options))
            //{
            DbConnection connection = null;
            if (connection_string!= string.Empty)
            {
                connection = new OracleConnection(connection_string);
            }
            else
            {
                connection = _appDbContext.Database.GetDbConnection();
            }
            
            //using (connection)
            //{
                await connection.OpenAsync();

                try
                {
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
                                //break;
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    ShowError("GetInvoiceHeader", e);
                }
                finally
                {
                    await connection.CloseAsync();
                }
                //}
                //}
            //}

            return response;
        }

        public async Task<List<InvoiceDetails>> GetInvoiceDetails(DateTime date_ini, DateTime date_end)
        {
            List<InvoiceDetails> response = new List<InvoiceDetails>();

            string rutaArchivo = Path.Combine(_webHostEnvironment.WebRootPath, "data/query/invoice_details.sql");

            if (!System.IO.File.Exists(rutaArchivo))
            {
                throw new ArgumentException($"El archivo sql de busqueda no existe.");
            }

            string sql = await System.IO.File.ReadAllTextAsync(rutaArchivo);

            //var connectionString = _appDbContext.Database.GetDbConnection().ConnectionString;
            //var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
            //optionsBuilder.UseOracle(connectionString);

            //using (var newContext = new AppDbContext(optionsBuilder.Options))
            //{
            var connection = _appDbContext.Database.GetDbConnection();
            await connection.OpenAsync();
                    try 
                    {
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
                                    var invoiceHeader = DataTools.MapToObject<InvoiceDetails>(reader);
                                    response.Add(invoiceHeader);
                                    //break;
                                }
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        ShowError("GetInvoiceDetails", e);
                    }
                    finally
                    {
                        await connection.CloseAsync();
                    }
                //}
            //}

            return response;
        }

        public async Task<List<InvoiceDetails>> GetInvoiceDetails(string[] ids)
        {
            List<InvoiceDetails> response = new List<InvoiceDetails>();

            string rutaArchivo = Path.Combine(_webHostEnvironment.WebRootPath, "data/query/invoice_details_ids.sql");

            if (!System.IO.File.Exists(rutaArchivo))
            {
                throw new ArgumentException($"El archivo sql de busqueda no existe.");
            }

            string sql = await System.IO.File.ReadAllTextAsync(rutaArchivo);

            //var connectionString = _appDbContext.Database.GetDbConnection().ConnectionString;
            //var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
            //optionsBuilder.UseOracle(connectionString);

            //using (var newContext = new AppDbContext(optionsBuilder.Options))
            //{
            
            DbConnection connection = null;
            if (connection_string != string.Empty)
            {
                connection = new OracleConnection(connection_string);
            }
            else
            {
                connection = _appDbContext.Database.GetDbConnection();
            }

            try
                    {
                await connection.OpenAsync();
                using (var command = connection.CreateCommand())
                        {                        
                            command.CommandType = CommandType.Text;
                            sql = sql.Replace("[ids]", string.Join(",", ids));
                            command.CommandText = sql;
                            //command.Parameters.Add(new OracleParameter(":ids", OracleDbType.NVarchar2) { Value = string.Join(",", ids) });
                            //command.Parameters.Add(new OracleParameter(":ids", OracleDbType.Array) { Value = string.Join(",", ids) });

                            using (var reader = await command.ExecuteReaderAsync())
                            {
                                while (await reader.ReadAsync())
                                {
                                    try
                                    {
                                        var invoiceHeader = DataTools.MapToObject<InvoiceDetails>(reader);
                                        response.Add(invoiceHeader);
                                    }
                                    catch(Exception e)
                                    {                                    
                                        Console.WriteLine("Error de conversión: " + e.Message);
                                        //Console.WriteLine(reader.ToString());
                                    }
                                
                                    //break;
                                }
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        ShowError("GetInvoiceDetails", e);
                    }
                    finally
                    {
                        await connection.CloseAsync();
                    }

                //}
            //}

            return response;
        }
        

        public async Task<List<InvoicePayments>> GetInvoicePayments(string[] ids)
        {
            List<InvoicePayments> response = new List<InvoicePayments>();

            string rutaArchivo = Path.Combine(_webHostEnvironment.WebRootPath, "data/query/invoice_payments_ids.sql");

            if (!System.IO.File.Exists(rutaArchivo))
            {
                throw new ArgumentException($"El archivo sql de busqueda no existe.");
            }

            string sql = await System.IO.File.ReadAllTextAsync(rutaArchivo);

            //var connectionString = _appDbContext.Database.GetDbConnection().ConnectionString;
            //var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
            //optionsBuilder.UseOracle(connectionString);

            //using (var newContext = new AppDbContext(optionsBuilder.Options))
            //{
            //var connection = _appDbContext.Database.GetDbConnection();

            DbConnection connection = null;
            if (connection_string != string.Empty)
            {
                connection = new OracleConnection(connection_string);
            }
            else
            {
                connection = _appDbContext.Database.GetDbConnection();
            }

            await connection.OpenAsync();

                    try
                    { 
                        using (var command = connection.CreateCommand())
                        {
                            command.CommandType = CommandType.Text;
                            sql = sql.Replace("[ids]", string.Join(",", ids));
                            command.CommandText = sql;

                            using (var reader = await command.ExecuteReaderAsync())
                            {
                                while (await reader.ReadAsync())
                                {
                                    var invoiceHeader = DataTools.MapToObject<InvoicePayments>(reader);
                                    response.Add(invoiceHeader);
                                    //break;
                                }
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        ShowError("GetInvoicePayments", e);
                    }
                    finally
                    {
                        await connection.CloseAsync();
                    }
                //}
            //}

            return response;
        }

        public async Task<List<InvoicePayments>> GetInvoicePayments(DateTime date_ini, DateTime date_end)
        {
            List<InvoicePayments> response = new List<InvoicePayments>();

            string rutaArchivo = Path.Combine(_webHostEnvironment.WebRootPath, "data/query/invoice_payments.sql");

            if (!System.IO.File.Exists(rutaArchivo))
            {
                throw new ArgumentException($"El archivo sql de busqueda no existe.");
            }

            string sql = await System.IO.File.ReadAllTextAsync(rutaArchivo);

            //var connectionString = _appDbContext.Database.GetDbConnection().ConnectionString;
            //var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
            //optionsBuilder.UseOracle(connectionString);

            //using (var newContext = new AppDbContext(optionsBuilder.Options))
            //{
            var connection = _appDbContext.Database.GetDbConnection();
            await connection.OpenAsync();

                    try
                    { 
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
                                    var invoiceHeader = DataTools.MapToObject<InvoicePayments>(reader);
                                    response.Add(invoiceHeader);
                                    //break;
                                }
                            }
                        }
                    }
                    catch(Exception e)
                    {
                        ShowError("GetInvoicePayments", e);
                    }
                    finally
                    {                        
                        await connection.CloseAsync();
                    }
                //}
            //}

            return response;
        }

        public async Task<ApiResponseOdooRpcT<List<int>>> PutInvoiceHeader(List<InvoiceHeader> invoiceHeaders)
        {
            AppSession _appSession = new AppSession();
            var appSetting = ConfigurationHelper.GetAppSettings();
            _appSession.odooConnection.Host = appSetting.profile.Odoo.Host;

            int company_id = 1;

            _appSession.CurrentUser = new User()
            {
                api_key = appSetting.profile.Odoo.api_key,
                access_token = appSetting.profile.Odoo.access_token,
                uid = int.Parse(appSetting.profile.Odoo.uid),
                username = appSetting.profile.Odoo.User,
                password = appSetting.profile.Odoo.Password,
                databasename = appSetting.profile.Odoo.Database
            };


            ApiManager.HubAccountMoveTransientLegacy hubmanager = new ApiManager.HubAccountMoveTransientLegacy(_appSession);
            var resultCount = await hubmanager.Create(invoiceHeaders, company_id);

            return resultCount;
        }

        public async Task<ApiResponseOdooRpcT<List<int>>> PutInvoiceDetails(List<InvoiceDetails> invoiceDetails)
        {
            AppSession _appSession = new AppSession();
            var appSetting = ConfigurationHelper.GetAppSettings();
            _appSession.odooConnection.Host = appSetting.profile.Odoo.Host;

            int company_id = 1;

            _appSession.CurrentUser = new User()
            {
                api_key = appSetting.profile.Odoo.api_key,
                access_token = appSetting.profile.Odoo.access_token,
                uid = int.Parse(appSetting.profile.Odoo.uid),
                username = appSetting.profile.Odoo.User,
                password = appSetting.profile.Odoo.Password,
                databasename = appSetting.profile.Odoo.Database
            };
            

            ApiManager.HubAccountMoveTransientLegacy hubmanager = new ApiManager.HubAccountMoveTransientLegacy(_appSession);
            var resultCount = await hubmanager.CreateDetails(invoiceDetails, company_id);

            return resultCount;
        }

        public async Task<ApiResponseOdooRpcT<List<int>>> PutInvoicePayments(List<InvoicePayments> invoiceDetails)
        {
            AppSession _appSession = new AppSession();
            var appSetting = ConfigurationHelper.GetAppSettings();
            _appSession.odooConnection.Host = appSetting.profile.Odoo.Host;

            int company_id = 1;

            _appSession.CurrentUser = new User()
            {
                api_key = appSetting.profile.Odoo.api_key,
                access_token = appSetting.profile.Odoo.access_token,
                uid = int.Parse(appSetting.profile.Odoo.uid),
                username = appSetting.profile.Odoo.User,
                password = appSetting.profile.Odoo.Password,
                databasename = appSetting.profile.Odoo.Database
            };
                        

            ApiManager.HubAccountMoveTransientLegacy hubmanager = new ApiManager.HubAccountMoveTransientLegacy(_appSession);
            var resultCount = await hubmanager.CreatePayments(invoiceDetails, company_id);

            return resultCount;
        }

        internal void setConnectionString(string _connection_string)
        {
            connection_string = _connection_string;
        }
    }
}
