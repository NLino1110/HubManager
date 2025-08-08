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
    public class OdooCacheJob : IJob
    {
        //[Inject]
        //static IWebHostEnvironment WebHostEnvironment { get; set; }

        //[Inject]
        private BuilderService builderService { get; set; }

        public OdooCacheJob(BuilderService _builderService)
        {
            builderService = _builderService;
        }

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
                    if (item.Process)
                    {
                        bool res1 = builderService.SendRequestOdooChunks(item).GetAwaiter().GetResult();
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
    }
}
