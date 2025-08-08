using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using DataSourceManager;
using DataSourceManager.MySql;
using DMSA.Models.Odoo.General.Responses;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using ResourceBuilder.Services.Sales;
using VtexStrucs;

namespace ResourceBuilder.Services.Sync
{
    public partial class TaskManager
    {
        [Inject]
        private Processor processorService { get; set; }

        private readonly AppDbContext _appDbContext;
        private readonly MySqlDbContext _mysqlDbContext;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public TaskManager(MySqlDbContext context,
            AppDbContext appContext,
            IWebHostEnvironment webHostEnvironment)
        {
            _mysqlDbContext = context;
            _webHostEnvironment = webHostEnvironment;
            _appDbContext = appContext;
            processorService = new Processor(context, appContext, webHostEnvironment);
        }

        public void SyncIn()
        {
            if (TaskStatus.SyncInWorking)
            {
                //LogManager.Log().LogInformation("TaskStatus.SyncInWorking " + TaskStatus.SyncInWorking.ToString());
                Console.WriteLine("TaskStatus.SyncInWorking " + TaskStatus.SyncInWorking.ToString());
                return;
            }

            //LogManager.Log().LogInformation("SyncIn start at " + DateTime.Now.ToString());
            Console.WriteLine("SyncIn start at " + DateTime.Now.ToString());
            TaskStatus.SyncInWorking = true;
            //ConnectionCredentials Ecuador = new ConnectionCredentials { Host = "129.200.8.1", Nombre = "Siscom Ecuador", Password = "moscasenlamesa", UserName = "SISCOM", Port = 1521, Schema = "SISCOM", SID = "SISCOM" };
            //ConnectionCredentials Panama = new ConnectionCredentials { Host = "129.200.8.1", Nombre = "Siscom Panama", Password = "chevere", UserName = "SISCOMPA", Port = 1521, Schema = "SISCOM", SID = "SISCOM" };
            //List<ConnectionCredentials> credentials = new List<ConnectionCredentials>();
            //credentials.Add(Ecuador);
            //credentials.Add(Panama);

            //foreach (ConnectionCredentials credencial in credentials)
            //{
                //DataManager data = new DataManager(credencial);
                //DataTable dtvTexAccount = data.ExecuteDataTable($"Select * from {credencial.UserName}.vtex_account"); //where account_id=1

                //if (dtvTexAccount == null)
                //{
                    //TaskStatus.SyncInWorking = false;
                    //LogManager.Log().LogInformation("SyncIn error al obtener los datos de la cuenta");
                    //Console.WriteLine("SyncIn error al obtener los datos de la cuenta");
                    //return;
                //}

                //foreach (DataRow row in dtvTexAccount.Rows)
                {
                vTex_Account cuenta = new vTex_Account();
                //SiscomDataTransport.RowToEntidad(row, cuenta);
                //Processor act = new Processor(cuenta, data);

                cuenta.Account_Name = "dmujeresec";
                cuenta.Internal_Account_Name = "dmujeresec";
                cuenta.Account_Id = 1;
                cuenta.vTexApiKey = "vtexappkey-dmujeresec-MKEUBV";
                cuenta.vTexApiToken = "NKRBHALIFNMCNIXYHWXLANZHZYLHYXLUMPQQSIDJNPOJYVWKNIZASFYOWRICWHKGFULEPNIFBCSIYKIDDOXSIDNSBSMBDOMDHVHYIVYIPPSKGBAIFGXPFGJFXBMLSFRH";
                
                processorService.SetAccount(cuenta);

                //campos para prueba
                if (cuenta.Internal_Account_Name.ToLower().Equals("dmujeres"))
                {
                    //continue;

                    //int skuItem = 5976;
                    //act.for_test_id = $"WHERE Codigo_Siscom  = '251921' ";
                    //act.allSkus_for_test = new string[] { skuItem.ToString() };
                    //act.whereForTest = " and prod_vtex_sku = " + skuItem.ToString();
                }

                    new Task(async () => {
                        //await processorService.QueueAnalyzer();
                        await QueueAnalyzer(cuenta);
                        TaskStatus.SyncInWorking = false;
                    }).RunSynchronously();
                }
            //}            
        }

        public void SyncOut()
        {
            //if (TaskStatus.SyncOutWorking)
            //{
            //    LogManager.Log().LogInformation("TaskStatus.SyncOutWorking " + TaskStatus.SyncOutWorking.ToString());
            //    return;
            //}

            //LogManager.Log().LogInformation("SyncOut start at " + DateTime.Now.ToString());
            Console.WriteLine("SyncOut start at " + DateTime.Now.ToString());
            TaskStatus.SyncOutWorking = true;
            //ConnectionCredentials Ecuador = new ConnectionCredentials { Host = "129.200.8.1", Nombre = "Siscom Ecuador", Password = "moscasenlamesa", UserName = "SISCOM", Port = 1521, Schema = "SISCOM", SID = "SISCOM" };
            //ConnectionCredentials Panama = new ConnectionCredentials { Host = "129.200.8.1", Nombre = "Siscom Panama", Password = "chevere", UserName = "SISCOMPA", Port = 1521, Schema = "SISCOM", SID = "SISCOM" };
            //List<ConnectionCredentials> credentials = new List<ConnectionCredentials>();
            //credentials.Add(Ecuador);
            //credentials.Add(Panama);
            //foreach (ConnectionCredentials credencial in credentials)
            {
                //DataManager data = new DataManager(credencial);
                //DataTable dtvTexAccount = data.ExecuteDataTable($"Select * from {credencial.UserName}.vtex_account"); //where account_id=1
                
                //if (dtvTexAccount == null)
                //{
                //    TaskStatus.SyncOutWorking = false;
                //    LogManager.Log().LogInformation("SyncOut error al obtener los datos de la cuenta");
                //    return;
                //}

                //foreach (DataRow row in dtvTexAccount.Rows)
                {
                    vTex_Account cuenta = new vTex_Account();
                    //SiscomDataTransport.RowToEntidad(row, cuenta);
                    //Processor act = new Processor(cuenta, data);

                    if (cuenta.Internal_Account_Name.ToLower().Equals("dmujeres"))
                    {
                        //continue;
                        //int skuItem = 3124;
                        //act.for_test_id = $"WHERE Codigo_Siscom  = '832381.3' ";
                        //act.allSkus_for_test = new string[] { skuItem.ToString() };
                        //act.whereForTest = " and prod_vtex_sku = " + skuItem.ToString();
                    }                    

                    new Task(async () => {
                        await processorService.VtexDataFixer();
                        //TaskStatus.SyncOutWorking = false;
                    }).RunSynchronously();

                }//fin de for de cuentas, 
            }            
        }
    }
}
