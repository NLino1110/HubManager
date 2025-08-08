using Blazored.Toast.Services;
using ResourceBuilder.Data.Structs;
using ResourceBuilder.Data;
using System.Threading.Tasks;

using Models.DMSA.Shared.Tools;
using Models.DMSA.Shared.Structs;
using ResourceBuilder.Services.Sales;
using Microsoft.AspNetCore.Hosting;
using System.Diagnostics;
using Microsoft.AspNetCore.Components;
using ResourceBuilder.Services.Sync;
using Quartz;
using Newtonsoft.Json;
using Oracle.ManagedDataAccess.Client;
using DataSourceManager;
using Microsoft.EntityFrameworkCore;

namespace ResourceBuilder.Services.Automata
{
    public class OdooInvoicesImportJob : IJob
    {
        private readonly InvoicesService _invoiceService;
        private readonly AppDbContext _appDbContext;

        [Inject]
        private InvoicesService invoiceService { get; set; }

        [Inject]
        static IWebHostEnvironment WebHostEnvironment { get; set; }

        //[Inject]
        //static TaskManager invoiceService { get; set; }

        public OdooInvoicesImportJob(InvoicesService _invoiceService, AppDbContext appDbContext,  IWebHostEnvironment _WebHostEnvironment)
        {
            //builderService = _builderService;
            invoiceService = _invoiceService;
            _appDbContext = appDbContext;
            WebHostEnvironment = _WebHostEnvironment;
        }

        //public async Task Execute(IJobExecutionContext context)
        //{
        //    try
        //    {
        //        // Probar la conexión manualmente
        //        using (var connection = new OracleConnection(_appDbContext.Database.GetDbConnection().ConnectionString))
        //        {
        //            await connection.OpenAsync();
        //            Console.WriteLine("Conexión abierta exitosamente.");
        //            await connection.CloseAsync();
        //        }

        //        // Aquí puedes llamar a tu método LaunchBatchSync o cualquier otro método necesario
        //        // await LaunchBatchSync(date_ini, date_end);
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine("Error al abrir la conexión manualmente: " + ex.Message);
        //    }
        //}

        public Task Execute(IJobExecutionContext context)
        {
            string itemsBuildJson = context.JobDetail.JobDataMap.GetString("itemsBuild");

            if (string.IsNullOrEmpty(itemsBuildJson))
            {
                Console.WriteLine("El parámetro 'itemsBuild' no está definido o es nulo.");
                //return Task.CompletedTask;
            }

            try
            {
                ItemBuild[] itemsBuild = JsonConvert.DeserializeObject<ItemBuild[]>(itemsBuildJson);

                foreach (var item in itemsBuild)
                {
                    Console.WriteLine($"Item {item.Name}");
                    if (item.Process && item.Name.Equals("InvoicesImport"))
                    {
                        SyncInit();
                    }
                }                    
               
            }
            catch (JsonException ex)
            {
                // Manejar errores de deserialización
                Console.WriteLine($"Error al deserializar 'itemsBuild': {ex.Message}");
            }
            
            return Task.CompletedTask;
        }

        private async Task SyncInit()
        {
            //DataSourceManager.AppDbContext _appDbContext = new DataSourceManager.AppDbContext();
            var connection_string = _appDbContext.Database.GetDbConnection().ConnectionString;

            //        {
            //            await connection.OpenAsync();
            //            Console.WriteLine("Conexión abierta exitosamente.");
            //            await connection.CloseAsync();
            //        }
            //InvoicesService invoiceService = new InvoicesService(_appDbContext, WebHostEnvironment);

            invoiceService.setConnectionString(connection_string);

            int totalHeaders = 0;
            
            DateTime date_ini = DateTime.Now.AddMinutes(-15);
            DateTime date_end = DateTime.Now;

            //var resHeaders = await invoiceService.GetInvoiceHeader(date_ini, date_end);

            int CountHour = 0;

            CountHour++;

            if (date_end > date_end)
            {
                date_end = date_end;
            }

            // Llamar a la función
            Console.WriteLine($"Calling LaunchBatchSync with range: {date_ini} - {date_end}");
            totalHeaders += await invoiceService.LaunchBatchSync(date_ini, date_end);

            // Mover al siguiente rango de 1 hora
            date_ini = date_ini.AddHours(1);

            Console.WriteLine("Total cabeceras: " + totalHeaders);
            Console.WriteLine($"Finished process:" + DateTime.Now);
            Console.WriteLine($"==========================================================================");
        }
    }
}
