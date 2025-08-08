using Models.DMSA.Shared.Structs;
using Models.DMSA.Shared.Tools;
using Newtonsoft.Json;
using Quartz;

namespace ResourceBuilder.Services.Automata
{
    public static class ServiceJobs
    {
        public static string ConvertToQuartzCron(string unixCron)
        {
            // Divide la expresión cron Unix en partes
            var parts = unixCron.Split(' ');
            if (parts.Length != 5)
            {
                throw new ArgumentException("La expresión cron debe tener 5 campos.");
            }

            // Construye la expresión Quartz
            return $"0 {parts[0]} {parts[1]} {parts[2]} {parts[3]} ? *";
        }

        public static void SetJobs(WebApplicationBuilder builder) //, IServiceCollection services)
        {
            builder.Services.AddQuartz(q =>
            {
                // Usar el scheduler predeterminado
                q.UseMicrosoftDependencyInjectionJobFactory();

                foreach (var taskItem in ConfigurationHelper.GetAppSettings().profile.Tasks)
                {
                    if (taskItem.items != null && taskItem.enabled)
                    {
                        string schedule = ConvertToQuartzCron(taskItem.schedule);
                        var jobKey = new JobKey(taskItem.name);

                        if (taskItem.name == "VtexSyncIn")
                        {
                            string itemsBuild = JsonConvert.SerializeObject(taskItem.items);

                            // Registrar el Job                            
                            q.AddJob<VtexJob>(opts => opts
                                .WithIdentity(jobKey)
                                .UsingJobData("itemsBuild", itemsBuild)
                            );

                            // Configurar un disparador (trigger) usando una expresión CRON
                            q.AddTrigger(opts => opts
                                .ForJob(jobKey)
                                .WithIdentity("VtexJob-trigger")
                                .WithCronSchedule("0 * * ? * *")); // Cada min cuando el segundo sea 0
                                //.WithCronSchedule(taskItem.schedule));
                                //.WithCronSchedule("0/5 * * * * ?")); // Cada 5 segundos
                        }

                        if (taskItem.name == "CacheBuilder")
                        {
                            string itemsBuild = JsonConvert.SerializeObject(taskItem.items);
                                                        
                            q.AddJob<OdooCacheJob>(opts => opts
                                .WithIdentity(jobKey)
                                .UsingJobData("itemsBuild", itemsBuild)
                            );
                            
                            q.AddTrigger(opts => opts
                                .ForJob(jobKey)
                                .WithIdentity("CacheBuilder-trigger")
                                .WithCronSchedule(schedule));
                        }

                        if (taskItem.name == "OdooInvoicesImport")
                        {
                            string itemsBuild = JsonConvert.SerializeObject(taskItem.items);

                            q.AddJob<OdooInvoicesImportJob>(opts => opts
                                .WithIdentity(jobKey)
                                .UsingJobData("itemsBuild", itemsBuild)
                            );

                            q.AddTrigger(opts => opts
                                .ForJob(jobKey)
                                .WithIdentity("OdooInvoicesImport-trigger")
                                .WithCronSchedule(schedule));
                        }
                    }
                }
            });

            // Agregar el hosted service para ejecutar Quartz
            builder.Services.AddQuartzHostedService(options =>
            {
                options.WaitForJobsToComplete = true;
            });
        }
    }
}
