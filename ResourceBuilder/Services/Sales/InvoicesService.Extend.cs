using DataSourceManager;

using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
//using System.Data.Entity;
using System.Text;

using Models.DMSA.Mbw.Inventario;
using Models.DMSA.Mbw.Sales;
using Models.DMSA.Mbw.Core;
using Models.DMSA.Mbw.Query;
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
using System.Text.RegularExpressions;

namespace ResourceBuilder.Services.Sales
{
    public partial class InvoicesService
    {
        public static string CleanEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return string.Empty;

            // 1. Remover caracteres no válidos (como ¿, ¡, espacios extras)
            string cleanedEmail = Regex.Replace(email, @"[^\w.@+-]", "");

            // 2. Asegurar que solo hay un '@'
            int atIndex = cleanedEmail.IndexOf('@');
            if (atIndex == -1 || atIndex != cleanedEmail.LastIndexOf('@'))
                return string.Empty;

            // 3. Verificar que haya texto antes y después del '@'
            string[] parts = cleanedEmail.Split('@');
            if (parts.Length != 2 || parts[0].Length == 0 || parts[1].Length < 3)
                return string.Empty;

            // 4. Validar formato del dominio
            if (!Regex.IsMatch(parts[1], @"^[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$"))
                return string.Empty;

            return cleanedEmail;
        }

        public async Task<int> SyncCustomers(DateTime date_ini, DateTime date_end)
        {
            VtexStrucs.vTex_Account mAccount = new VtexStrucs.vTex_Account();
            mAccount.Account_Id = 1;
            mAccount.Account_Name = "dmujeresec";
            mAccount.Internal_Account_Name = "dmujeresec";
            mAccount.Environment = "vtexcommercestable";
            mAccount.vTexApiKey = "vtexappkey-dmujeresec-MKEUBV";
            mAccount.vTexApiToken = "NKRBHALIFNMCNIXYHWXLANZHZYLHYXLUMPQQSIDJNPOJYVWKNIZASFYOWRICWHKGFULEPNIFBCSIYKIDDOXSIDNSBSMBDOMDHVHYIVYIPPSKGBAIFGXPFGJFXBMLSFRH";
            
            int totalHeaders = 0;
           
            Console.WriteLine("Iniciando importación MBW -> Vtex    " + DateTime.Now);
            Console.WriteLine("date_ini    " + date_ini);
            Console.WriteLine("date_end    " + date_end);
                        
            var resHeaders = await GetInvoiceByProfesional(date_ini, date_end);
            var ids_headers = resHeaders.Select(doc => doc.Identificacion).ToArray();
            if (ids_headers.Length > 0)
            {
                Debug.WriteLine("Registros totales:[" + ids_headers.Length + "]");
                Console.WriteLine("Registros totales:[" + ids_headers.Length + "]");
                                
                for (int i = 0; i < ids_headers.Length; i ++)
                {
                    if (resHeaders[i].Email == null || resHeaders[i].Email.Equals(string.Empty))
                        continue;

                    resHeaders[i].Email = CleanEmail(resHeaders[i].Email);

                    VtexStrucs.Comm.Client vtexClient = new VtexStrucs.Comm.Client(mAccount);
                    //var data = await vtexClient.getByDocument(resHeaders[i].Identificacion);
                    var data = await vtexClient.getByEmail(resHeaders[i].Email);

                    //Console.WriteLine(data);
                    if(data.Content != null && data.Content.Length> 0)
                    {
                        try
                        {
                            var itemClient = JsonConvert.DeserializeObject<VtexStrucsGen2.Strucs.Vtex.Client[]>(data.Content);
                            if (itemClient != null && itemClient.Length > 0)
                            {
                                //itemClient[0].academy = itemClient[0].academy == null ? 0 : itemClient[0].academy;

                                if (!itemClient[0].professional)
                                {
                                    Console.WriteLine(resHeaders[i].Identificacion + " No es profesional en VTEX");
                                }

                                string gender = null;

                                if (resHeaders[i].Sexo.ToLower() == "m")
                                    gender = "male";
                                if (resHeaders[i].Sexo.ToLower() == "f")
                                    gender = "female";

                                if (resHeaders[i].Telefono1 != "000000000000")
                                {
                                    itemClient[0].phone = resHeaders[i].Telefono1;
                                }

                                itemClient[0].firstName = itemClient[0].firstName == null ? "" : itemClient[0].firstName;
                                itemClient[0].lastName = itemClient[0].lastName == null ? "" : itemClient[0].lastName;

                                //(itemClient[0].phone != resHeaders[i].Telefono1 && resHeaders[i].Telefono1.ToString() != "000000000000") ||

                                if (!itemClient[0].professional ||
                                    itemClient[0].email.ToLower().ToString() != resHeaders[i].Email.ToLower().ToString() ||
                                    itemClient[0].firstName.ToLower().ToString() != resHeaders[i].NombresCliente.ToLower().ToString() ||
                                    itemClient[0].lastName.ToLower().ToString() != resHeaders[i].ApellidosCliente.ToLower().ToString() ||
                                    itemClient[0].birthDate != resHeaders[i].FechaNacimiento ||
                                    itemClient[0].gender != gender)
                                {
                                    Console.WriteLine(resHeaders[i].Identificacion + " requiere actualizacion");

                                    Console.WriteLine(itemClient[0].professional);
                                    Console.WriteLine(itemClient[0].phone + " " + resHeaders[i].Telefono1);
                                    Console.WriteLine(itemClient[0].email + " " + resHeaders[i].Email);
                                    Console.WriteLine(itemClient[0].firstName + " " + resHeaders[i].NombresCliente);
                                    Console.WriteLine(itemClient[0].lastName + " " + resHeaders[i].ApellidosCliente);
                                    Console.WriteLine(itemClient[0].birthDate + " " + resHeaders[i].FechaNacimiento);
                                    Console.WriteLine(itemClient[0].gender + " " + gender);

                                    //if(itemClient[0].document == "0503953580")
                                    //{
                                    itemClient[0].professional = true;

                                    itemClient[0].email = resHeaders[i].Email.Trim();
                                    itemClient[0].firstName = resHeaders[i].NombresCliente.Trim();
                                    itemClient[0].lastName = resHeaders[i].ApellidosCliente.Trim();
                                    itemClient[0].birthDate = resHeaders[i].FechaNacimiento;
                                    itemClient[0].gender = gender;
                                    itemClient[0].academy = 0;

                                    var data_put = await vtexClient.createOrUpdateDocument(itemClient[0]);
                                    //Console.WriteLine(data_put);
                                    if (data_put.StatusCode == System.Net.HttpStatusCode.OK)
                                    {
                                        Console.WriteLine(resHeaders[i].Identificacion + " actualizado correctamente");
                                    }
                                    else
                                    {
                                        Console.WriteLine(resHeaders[i].Identificacion + " error en actualización");
                                    }
                                    //}
                                }
                            }
                            else
                            {
                                Console.WriteLine(resHeaders[i].Identificacion + " " + resHeaders[i].Email + " no encontrado en Vtex");
                            }
                        }
                        catch(Exception e)
                        {
                            Console.WriteLine("Error" + e.Message);
                            Console.WriteLine(data.Content);
                        }
                    }
                    else
                    {
                        Console.WriteLine(resHeaders[i].Identificacion + " " + resHeaders[i].Email + " no encontrado en Vtex");
                    }
                }
            }
            else
            {
                Debug.WriteLine("Datos no encontrados....");
                Console.WriteLine("Datos no encontrados....");
            }

            Debug.WriteLine("Fin: SyncManager" + DateTime.Now);
            Console.WriteLine("Fin: SyncManager" + DateTime.Now);

            return totalHeaders;
        }

        public async Task<List<ClienteProfesional>> GetInvoiceByProfesional(DateTime date_ini, DateTime date_end)
        {
            List<ClienteProfesional> response = new List<ClienteProfesional>();

            string rutaArchivo = Path.Combine(_webHostEnvironment.WebRootPath, "data/query/invoice_by_profesional.sql");

            if (!System.IO.File.Exists(rutaArchivo))
            {                
                throw new ArgumentException($"El archivo sql de busqueda no existe.");
            }

            string sql = await System.IO.File.ReadAllTextAsync(rutaArchivo);

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
                            var invoiceHeader = DataTools.MapToObject<ClienteProfesional>(reader);
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

            return response;
        }

        //public async Task<ApiResponseOdooRpcDynamic> __PutInvoiceHeader(List<ClienteProfesional> invoiceHeaders)
        //{
        //    AppSession _appSession = new AppSession();
        //    var appSetting = ConfigurationHelper.GetAppSettings();
        //    _appSession.EndPointServer = appSetting.profile.Odoo.ApiBaseAddressOdoo;

        //    int company_id = 1;

        //    _appSession.CurrentUser = new User()
        //    {
        //        api_key = appSetting.profile.Odoo.api_key,
        //        access_token = appSetting.profile.Odoo.access_token,
        //        uid = int.Parse(appSetting.profile.Odoo.uid),
        //        username = appSetting.profile.Odoo.User,
        //        codclave = appSetting.profile.Odoo.Password,
        //        databasename = appSetting.profile.Odoo.Database
        //    };

        //    //bool noSalir = true;

        //    ApiRequestOdoo_v1 apiRequest = new ApiRequestOdoo_v1();
        //    apiRequest.uid = int.Parse(appSetting.profile.Odoo.uid);
        //    apiRequest.password = appSetting.profile.Odoo.Password;
        //    apiRequest.databasename = appSetting.profile.Odoo.Database;
        //    apiRequest.dateIni = DateTime.Now;

        //    ApiManager.HubAccountMoveTransientLegacy hubmanager = new ApiManager.HubAccountMoveTransientLegacy(_appSession);
        //    ApiResponseOdooRpcDynamic resultCount = await hubmanager.Create(invoiceHeaders, company_id);

        //    return resultCount;
        //}
    }
}
