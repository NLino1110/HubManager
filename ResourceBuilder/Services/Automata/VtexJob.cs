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

namespace ResourceBuilder.Services.Automata
{
    public class VtexJob : IJob
    {
        //[Inject]
        //static IWebHostEnvironment WebHostEnvironment { get; set; }

        //[Inject]
        private TaskManager invoiceService { get; set; }

        public VtexJob(TaskManager _invoiceService)
        {
            invoiceService = _invoiceService;
        }

        //////static public void Setup(IWebHostEnvironment __WebHostEnvironment)
        //////{
        //////    WebHostEnvironment = __WebHostEnvironment;
        //////    //Lee todas las tareas configuradas
        //////    foreach (var taskItem in ConfigurationHelper.GetAppSettings().profile.Tasks)
        //////    {
        //////        if(taskItem.items != null && taskItem.enabled)
        //////        {
        //////            if(taskItem.name == "CacheBuilder")
        //////            {
        //////                List<ItemBuild> builds = new List<ItemBuild>();

        //////                builds = taskItem.items.ToList();

        //////                ThreadStart threadStart = () =>
        //////                {
        //////                    taskDataBuilder(builds);
        //////                };

        //////                string cronSchedule = taskItem.schedule;

        //////                cron_daemon.AddJob(cronSchedule, threadStart);

        //////                //Cada 0 horas
        //////                //cron_daemon.AddJob("0 0 * * *", threadStart);
        //////                //Cada minuto
        //////                //cron_daemon.AddJob("* * * * *", task);
        //////                //Cada hora
        //////                //cron_daemon.AddJob("0 * * * *", task);
        //////            }

        //////            if (taskItem.name == "OdooInvoicesImport")
        //////            {
        //////                List<ItemBuild> builds = new List<ItemBuild>();

        //////                builds = taskItem.items.ToList();
        //////                string cronSchedule = taskItem.schedule;

        //////                ThreadStart threadStart = () =>
        //////                {
        //////                    taskImportBuilder(builds, cronSchedule);
        //////                };

        //////                cron_daemon.AddJob(cronSchedule, threadStart);
        //////            }

        //////            if (taskItem.name == "VtexSyncIn")
        //////            {
        //////                List<ItemBuild> builds = new List<ItemBuild>();

        //////                builds = taskItem.items.ToList();
        //////                string cronSchedule = taskItem.schedule;

        //////                ThreadStart threadStart = () =>
        //////                {
        //////                    taskVtexSyncIn(builds, cronSchedule);
        //////                };

        //////                cron_daemon.AddJob(cronSchedule, threadStart);
        //////            }
        //////        }
        //////    }

        //////    cron_daemon.Start();
        //////}

        //////static async void taskDataBuilder(List<ItemBuild> builds)
        //////{
        //////    BuilderService builderService = new BuilderService();
        //////    Console.WriteLine("Iniciada la creación de cache:" + DateTime.Now.ToString());

        //////    foreach (var item in builds)
        //////    {
        //////        if (item.Process)
        //////        {
        //////            bool res1 = await builderService.SendRequestOdooChunks(item);
        //////        }
        //////    }

        //////    Console.WriteLine("Terminado el proceso de generación de cache!:" + DateTime.Now.ToString());
        //////}

        //////static async void taskImportBuilder(List<ItemBuild> builds, string cronSchedule)
        //////{
        //////    DataSourceManager.AppDbContext _appDbContext = new DataSourceManager.AppDbContext();

        //////    InvoicesService invoiceService = new InvoicesService(_appDbContext, WebHostEnvironment);

        //////    Console.WriteLine("Iniciada la importacion:" + DateTime.Now.ToString());

        //////    foreach (var item in builds)
        //////    {
        //////        if (item.Process)
        //////        {
        //////            int totalHeaders = 0;

        //////            //bool res1 = await builderService.SendRequestOdooChunks(item);

        //////            DateTime date_ini = DateTime.Now.AddMinutes(-15);
        //////            DateTime date_end = DateTime.Now;

        //////            Debug.WriteLine("Iniciando importación MBW -> Odoo    " + DateTime.Now);
        //////            Debug.WriteLine("date_ini    " + date_ini);
        //////            Debug.WriteLine("date_end    " + date_end);

        //////            Console.WriteLine("Iniciando importación MBW -> Odoo    " + DateTime.Now);
        //////            Console.WriteLine("date_ini    " + date_ini);
        //////            Console.WriteLine("date_end    " + date_end);

        //////            totalHeaders += await invoiceService.LaunchBatchSync(date_ini, date_end);

        //////            Debug.WriteLine("Fin: SyncManager" + DateTime.Now);
        //////        }
        //////    }

        //////    Console.WriteLine("Terminado el proceso de importación:" + DateTime.Now.ToString());
        //////}

        //////static async void taskVtexSyncIn(List<ItemBuild> builds, string cronSchedule)
        //////{
        //////    //DataSourceManager.AppDbContext _appDbContext = new DataSourceManager.AppDbContext();

        //////    Console.WriteLine("Iniciada la importación:" + DateTime.Now.ToString());

        //////    foreach (var item in builds)
        //////    {
        //////        if (item.Process)
        //////        {
        //////            //bool res1 = await builderService.SendRequestOdooChunks(item);

        //////            DateTime date_ini = DateTime.Now.AddMinutes(-15);
        //////            DateTime date_end = DateTime.Now;

        //////            Debug.WriteLine("Iniciando importación Vtex -> MBW    " + DateTime.Now);
        //////            Debug.WriteLine("date_ini    " + date_ini);
        //////            Debug.WriteLine("date_end    " + date_end);

        //////            invoiceService.SyncIn();

        //////            Debug.WriteLine("Fin: SyncManager" + DateTime.Now);
        //////        }
        //////    }

        //////    Console.WriteLine("Terminado el proceso de importación:" + DateTime.Now.ToString());
        //////}

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

                //if (itemsBuild == null || itemsBuild.Length == 0)
                //{
                //    Console.WriteLine("No se encontraron elementos válidos en 'itemsBuild'.");
                //}
                //else
                //{
                //    Console.WriteLine($"Se procesaron {itemsBuild.Length} elementos en 'itemsBuild'.");
                //}
            }
            catch (JsonException ex)
            {
                // Manejar errores de deserialización
                Console.WriteLine($"Error al deserializar 'itemsBuild': {ex.Message}");
            }

            invoiceService.SyncIn();
            return Task.CompletedTask;
        }
    }
}
