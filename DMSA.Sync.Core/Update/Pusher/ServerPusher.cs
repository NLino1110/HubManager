using ApiManager;
using DMOrders.Services.Database.Sqlite;
using DMSA.Models.General.Requests;
using DMSA.Models.Odoo.DMOrders.tareas;
using DMSA.Models.Odoo.General.Responses;
using DMSA.Models.Odoo.Native;
using DMSA.Models.Security;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using static System.Net.Mime.MediaTypeNames;

namespace DMOrders.Services.Update.Pusher
{
    public partial class ServerPusher
    {
        int year { get; set; }
        int month { get; set; }
        int day { get; set; }
        int limit { get; set; }

        DateTime? sync_date_since => new DateTime(year, month, day);
        public AppSession appSession { get; set; }
        public ServerPusher() 
        {
            year = appSession.sync_date_since.Year;
            month = appSession.sync_date_since.Month;
            day = appSession.sync_date_since.Day;

            //DESACTIVADO PARA EFECTOS DE PRUEBA
            //Si ya hubi sincronizacion previa, se toma como limite de fecha la fecha de la ultima sincronizacion            
            //if(appSession.sync_date_since != appSession.CurrentUser.log_fec_sincro)
            //{
            //    year = appSession.CurrentUser.log_fec_sincro.Year;
            //    month = appSession.CurrentUser.log_fec_sincro.Month;
            //    day = appSession.CurrentUser.log_fec_sincro.Day;
            //}

            limit = appSession.odooConnection.DbLimitDefault;
        }

        public async Task<bool> Push()
        {
            //await OnlineSyncCompany();
            //await OnlineSyncStores();
            return true;
        }
                
        public async Task<bool> SendSaleOrder(sale_order sale_Order)
        {   
            //object[] _args_ = new object[] {
            //    sale_Order
            //};

            //object kargs = new object[] {};

            ApiManager.HubSaleOrder hubStore = new HubSaleOrder(appSession);
            ApiResponseOdooRpcT<int> resultTask = await hubStore.Create(sale_Order);

            if (resultTask != null && resultTask.error != null)
            {
                Debug.WriteLine(resultTask.error.data.message);
                Debug.WriteLine(resultTask.error.data.debug);
                //await Toast.Make("Error:" + resultTask.error.data.message).Show();
                return false;
            }

            if (resultTask != null && resultTask.result !=null)
            {
                //await Toast.Make("Datos enviados correctamente").Show();

                sale_Order.erp_id = resultTask.result;
                sale_Order.is_synchronized = true;
                sale_Order.date_synchronized = DateTime.Now;
                SaleOrderDb saleOrderDb = new SaleOrderDb(appSession.odooConnection.DbNameSqlite);
                await saleOrderDb.UpdateAsync(sale_Order);
                return true;
            }

            return false;
        }

        public async Task SendAllSaleOrders(object obj)
        {           
            SaleOrderDb saleOrderDb = new SaleOrderDb(appSession.odooConnection.DbNameSqlite);
            var listOrders = await saleOrderDb.GetItemsAsync(appSession.res_Company.id, false);
            
            if(listOrders == null || listOrders.Count == 0)
                return;

            int totalItems = listOrders.Count;
            int itemIndex = 0;
            foreach (var item in listOrders)
            {
                itemIndex++;
                //obj.SetTitle($"Sincronizando pedidos ({itemIndex}/{totalItems})");
                await SendSaleOrder(item);
            }            
        }

        public async Task<int> CreateProjectTask(ProjectTask projectTask)
        {
            projectTask.user_ids = new int [] { projectTask.user_id };

            //TODO: Agregar validacion para ProjectTask existente
            ApiManager.HubProjectTask hubManager = new HubProjectTask(appSession);

            var existingTasks = await hubManager.GetByNameUser(projectTask.name, appSession.CurrentUserFront.uid);

            if (existingTasks != null && existingTasks.result != null && existingTasks.result.Length > 0)
            {
                Debug.WriteLine("La tarea ya existe en el servidor: " + projectTask.name);
                //await Toast.Make("La tarea ya existe en el servidor: " + projectTask.name).Show();
                
                projectTask.is_synchronized = true;
                projectTask.date_synchronized = DateTime.Now;
                projectTask.id_sync = existingTasks.result[0].id;
                ProjectTaskDb projectTaskDb = new ProjectTaskDb(appSession.odooConnection.DbNameSqlite);
                await projectTaskDb.UpdateAsync(projectTask);

                return projectTask.id_sync;
            }

            ApiResponseOdooRpcT<int> resultTask = await hubManager.Create(projectTask);

            if (resultTask != null && resultTask.error != null)
            {
                Debug.WriteLine(resultTask.error.data.message);
                Debug.WriteLine(resultTask.error.data.debug);
                //await Toast.Make("Error:" + resultTask.error.data.message).Show();
                return 0;
            }

            if (resultTask != null && resultTask.result != null)
            {
                //await Toast.Make("Datos enviados correctamente").Show();
                projectTask.is_synchronized = true;
                projectTask.date_synchronized = DateTime.Now;
                projectTask.id_sync = resultTask.result;
                ProjectTaskDb projectTaskDb = new ProjectTaskDb(appSession.odooConnection.DbNameSqlite);
                await projectTaskDb.UpdateAsync(projectTask);
                return projectTask.id_sync;
            }

            return 0;

        }

        public async Task SendProjectTask(ProjectTask projectTask)
        {
            if(!projectTask.is_synchronized)
            {
                int projectId = await CreateProjectTask(projectTask);
                Debug.WriteLine(projectId);
            }
            else
            {
                Debug.WriteLine(projectTask.id + " ya ha sido sincronizado");
            }

            //Debug.WriteLine(projectTask.id_sync);

            AccountAnalyticLineDb projectTaskDb = new AccountAnalyticLineDb(appSession.odooConnection.DbNameSqlite);
            var items = await projectTaskDb.GetItemsAsync(projectTask);
            foreach(var item in items)
            {
                item.task_id_sync = projectTask.id_sync;
                await SendAccountAnalyticLine(item);
            }
        }

        public async Task SendAllProjectTask(object obj)
        {
            ProjectTaskDb saleOrderDb = new ProjectTaskDb(appSession.odooConnection.DbNameSqlite);
            var listOrders = await saleOrderDb.GetItemsAsync(appSession.res_Company.id, false);

            if (listOrders == null || listOrders.Count == 0)
                return;

            int totalItems = listOrders.Count;
            int itemIndex = 0;
            foreach (var item in listOrders)
            {
                itemIndex++;
                //obj.SetTitle($"Sincronizando tareas ({itemIndex}/{totalItems})");
                await SendProjectTask(item);
            }
        }

        public async Task SendAccountAnalyticLine(AccountAnalyticLine item)
        {
            ApiManager.HubAccountAnalyticLine hubManager = new HubAccountAnalyticLine(appSession);
            ApiResponseOdooRpcT<int> resultTask = await hubManager.Create(item);

            if (resultTask != null && resultTask.error != null)
            {
                Debug.WriteLine(resultTask.error.data.message);
                Debug.WriteLine(resultTask.error.data.debug);
                //await Toast.Make("Error:" + resultTask.error.data.message).Show();
                return;
            }

            if (resultTask != null && resultTask.result != null)
            {
                //await Toast.Make("Datos enviados correctamente").Show();

                item.is_synchronized = true;
                item.date_synchronized = DateTime.Now;
                item.id_sync = resultTask.result;
                AccountAnalyticLineDb projectTaskDb = new AccountAnalyticLineDb(appSession.odooConnection.DbNameSqlite);
                await projectTaskDb.UpdateAsync(item);
            }
        }

        public async Task SendAllAccountAnalyticLine(object obj)
        {
            AccountAnalyticLineDb accountAnalyticLineDb = new AccountAnalyticLineDb(appSession.odooConnection.DbNameSqlite);
            var listItems = await accountAnalyticLineDb.GetItemsAsync(appSession.res_Company.id, false);

            if (listItems == null || listItems.Count == 0)
                return;

            int totalItems = listItems.Count;
            int itemIndex = 0;
            foreach (var item in listItems)
            {
                //TODO: Agregar validacion para ProjectTask existente
                itemIndex++;
                //obj.SetTitle($"Sincronizando tareas ({itemIndex}/{totalItems})");
                await SendAccountAnalyticLine(item);
            }
        }
    }
}
