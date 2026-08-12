using ApiManager;
using ApiManagerOdoo.Accounting;
using ApiManagerOdoo.Sale;
using CommunityToolkit.Maui.Alerts;
using DMOrders.Services.Database.Sqlite;
using DMSA.Models.Odoo.Abstract;
using DMSA.Models.Odoo.DMOrders.promotions.abstractCustom;
using DMSA.Models.Odoo.General.Responses;
using DMSA.Models.Odoo.Native;
using DMSA.Models.Odoo.Promotions.Wizard;
using DMSA.Models.Odoo.Tareas;
using DMSA.Models.Odoo.Tools;
using DMSA.Models.Security;
using DMSA.Sync.Core.Controls;
using DMSA.Sync.Core.Database.Sqlite;
using DMSA.Sync.Core.Database.Sqlite.Sales;
using DMSA.Sync.Core.Database.Sqlite.tareas;
using Microsoft.Maui.Graphics;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Diagnostics;

namespace DMSA.Sync.Core.Update.Pusher
{
    public partial class SaleOrders
    {
        int year { get; set; }
        int month { get; set; }
        int day { get; set; }
        int limit { get; set; }

        DateTime? sync_date_since => new DateTime(year, month, day);
        public AppSession appSession => Constants.Session;
        public SaleOrders() 
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

            limit = Constants.Session.odooConnection.DbLimitDefault;
        }

        public async Task<bool> Push()
        {
            //await OnlineSyncCompany();
            //await OnlineSyncStores();
            return true;
        }

        private async Task<JObject> PreparePayLoad(sale_order sale_Order)
        {
            var settings = new JsonSerializerSettings
            {
                DateFormatString = "yyyy-MM-dd HH:mm:ss",
                ContractResolver = new IncludeJsonIgnoreResolver(),
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            };

            var json = JsonConvert.SerializeObject(sale_Order, settings);

            var fullObject = JObject.Parse(json);

            // Solo UI / solo lectura en sync de bajada: no enviar al create ni en external_payload
            JObjectExtensions.RemoveProperty(fullObject, "state_view");
            JObjectExtensions.RemoveProperty(fullObject, "free_order_state_view");
            JObjectExtensions.RemoveProperty(fullObject, "free_order_state");

            JObjectExtensions.RemovePropertyFromOrderLineItems(fullObject, "_order_id");
            JObjectExtensions.RemovePropertyFromOrderLineItems(fullObject, "_product_uom_category_id");
            JObjectExtensions.RemovePropertyFromOrderLineItems(fullObject, "product_code");
            JObjectExtensions.RemovePropertyFromOrderLineItems(fullObject, "product_display");
            JObjectExtensions.RemovePropertyFromOrderLineItems(fullObject, "uom_category_display");
            JObjectExtensions.RemovePropertyFromOrderLineItems(fullObject, "promotionDataList");
            JObjectExtensions.RemovePropertyFromOrderLineItems(fullObject, "promotion_data");

            foreach( var lineOrder in sale_Order.order_line)
            {
                var order_line_item = (sale_order_line)lineOrder[2];
                Debug.WriteLine(order_line_item.origin_gift_line_ids_offline);
            }

            var dto = new
            {
                version = 1,
                timestamp = DateTime.UtcNow,
                data = new SaleOrderDTO
                {
                    SaleOrder = fullObject,
                    PromoData = ""//JsonConvert.DeserializeObject(new object())
                }
            };

            return JObject.FromObject(dto);
        }


        public void RemoveGiftLines(ref sale_order sale_Order)
        {
            if (sale_Order.order_line == null || !sale_Order.order_line.Any())
                return;
            
            var orderLinesNotGifts = sale_Order.order_line.Where(ol => !((sale_order_line)ol[2]).is_gift).ToList();
            sale_Order.order_line = orderLinesNotGifts;
        }

        public void RemovePromoRulesData(ref sale_order saleOrder)
        {
            if (saleOrder?.order_line == null || !saleOrder.order_line.Any())
                return;

            foreach (var itemLine in saleOrder.order_line)
            {
                if (itemLine[2] is sale_order_line line)
                {
                    line.promotion_ids = [];
                    line.promotion_ids_json = "[]";
                    line.rule_ids = [];
                    line.rule_ids_json = "[]";
                }
            }
        }

        public async Task<SaleOrderSendResult> SendSaleOrder(sale_order sale_Order)
        {            
            SaleOrderDb saleOrderDb = new SaleOrderDb(Constants.Session.odooConnection.DbNameSqlite);
            string orderLabel = !string.IsNullOrWhiteSpace(sale_Order.id_referencia)
                ? sale_Order.id_referencia
                : $"id local {sale_Order.id}";

            if (!sale_Order.mobile_sync)
                sale_Order.mobile_sync = true;

            if(string.IsNullOrEmpty(sale_Order.external_guid))
            {
                sale_Order.external_guid = Guid.NewGuid().ToString("N");
                await saleOrderDb.UpdateAsync(sale_Order);
            }

            var dto = await PreparePayLoad(sale_Order);
            sale_Order.external_payload = dto;

            RemoveGiftLines(ref sale_Order);
            RemovePromoRulesData(ref sale_Order);

            HubSaleOrder hubSaleOrder = new HubSaleOrder(Constants.Session);
            ApiResponseOdooRpcT<int> resultTask = await hubSaleOrder.Create(sale_Order, false);

            if (resultTask != null && resultTask.error != null)
            {
                string serverMessage = resultTask.error.data?.message ?? resultTask.error.message ?? string.Empty;
                string serverDebug = resultTask.error.data?.debug ?? string.Empty;
                Debug.WriteLine(serverMessage);
                Debug.WriteLine(serverDebug);

                bool duplicateGuid = SaleOrderSyncErrorParser.IsDuplicateExternalGuid(serverMessage, serverDebug);
                if (duplicateGuid)
                {
                    var recoverResult = await RecoverSaleOrderFromErp(sale_Order, hubSaleOrder, saleOrderDb);
                    if (recoverResult.Ok)
                        return recoverResult;

                    string userMessage = SaleOrderSyncErrorParser.BuildUserMessage(serverMessage, true);
                    return SaleOrderSendResult.Fail(userMessage, duplicateGuid: true, orderLabel: orderLabel);
                }

                string genericMessage = SaleOrderSyncErrorParser.BuildUserMessage(serverMessage, false);
                return SaleOrderSendResult.Fail(genericMessage, orderLabel: orderLabel);
            }

            bool byPassExtras = true;

            if (resultTask != null && resultTask.result !=null && resultTask.result > 0)
            {
                await Toast.Make("Datos enviados correctamente").Show();

                sale_Order.erp_id = resultTask.result;
                sale_Order.is_synchronized = true;
                sale_Order.date_synchronized = DateTime.Now;
                await saleOrderDb.UpdateAsync(sale_Order);

                var ErpSaleOrder = await hubSaleOrder.GetById(sale_Order.erp_id);

                if (ErpSaleOrder != null && ErpSaleOrder.result.Length > 0)
                {
                    foreach(var erpOrderItem in ErpSaleOrder.result)
                    {
                        sale_Order.erp_name = erpOrderItem.name;
                        await saleOrderDb.UpdateAsync(sale_Order);
                    }                    
                }

                return SaleOrderSendResult.Success(erpName: sale_Order.erp_name ?? string.Empty);
            }

            return SaleOrderSendResult.Fail(
                "Es probable que no se haya sincronizado correctamente; el servidor devolvió un valor inválido.",
                orderLabel: orderLabel);
        }

        /// <summary>
        /// Busca en Odoo por external_guid y vincula erp_id / erp_name en local.
        /// </summary>
        public async Task<SaleOrderSendResult> RecoverSaleOrderFromErp(sale_order sale_Order)
        {
            SaleOrderDb saleOrderDb = new SaleOrderDb(Constants.Session.odooConnection.DbNameSqlite);
            HubSaleOrder hubSaleOrder = new HubSaleOrder(Constants.Session);

            if (string.IsNullOrWhiteSpace(sale_Order.external_guid))
            {
                return SaleOrderSendResult.Fail(
                    "Este pedido no tiene identificador externo (external_guid) para recuperar en el ERP.",
                    orderLabel: sale_Order.id_referencia ?? $"id local {sale_Order.id}");
            }

            var result = await RecoverSaleOrderFromErp(sale_Order, hubSaleOrder, saleOrderDb);
            if (result.Ok)
                await Toast.Make($"Pedido recuperado: {result.ErpName}").Show();

            return result;
        }

        private static async Task<SaleOrderSendResult> RecoverSaleOrderFromErp(
            sale_order sale_Order,
            HubSaleOrder hubSaleOrder,
            SaleOrderDb saleOrderDb)
        {
            string orderLabel = !string.IsNullOrWhiteSpace(sale_Order.id_referencia)
                ? sale_Order.id_referencia
                : $"id local {sale_Order.id}";

            var existing = await hubSaleOrder.GetByExternalGuid(sale_Order.external_guid);
            if (existing?.result == null || existing.result.Length == 0)
            {
                return SaleOrderSendResult.Fail(
                    "No se encontró en el ERP un pedido con este identificador externo.\n\n"
                    + "Si el pedido nunca se envió, use Reintentar envío.",
                    duplicateGuid: true,
                    orderLabel: orderLabel);
            }

            var erpOrder = existing.result[0];
            sale_Order.erp_id = erpOrder.id;
            sale_Order.erp_name = erpOrder.name;
            sale_Order.is_synchronized = true;
            sale_Order.date_synchronized = DateTime.Now;
            await saleOrderDb.UpdateAsync(sale_Order);

            return SaleOrderSendResult.Success(linkedExisting: true, erpName: erpOrder.name ?? string.Empty);
        }

        /// <summary>
        /// Reintento seguro: primero valida si ya existe en ERP (recupera); si no existe, crea con GUID nuevo.
        /// </summary>
        public async Task<SaleOrderSendResult> RetrySendAfterValidation(sale_order sale_Order)
        {
            SaleOrderDb saleOrderDb = new SaleOrderDb(Constants.Session.odooConnection.DbNameSqlite);
            HubSaleOrder hubSaleOrder = new HubSaleOrder(Constants.Session);

            if (!string.IsNullOrWhiteSpace(sale_Order.external_guid))
            {
                var recoverAttempt = await RecoverSaleOrderFromErp(sale_Order, hubSaleOrder, saleOrderDb);
                if (recoverAttempt.Ok)
                {
                    await Toast.Make($"El pedido ya existía en el ERP: {recoverAttempt.ErpName}").Show();
                    return recoverAttempt;
                }
            }

            sale_Order.external_guid = Guid.NewGuid().ToString("N");
            sale_Order.is_synchronized = false;
            sale_Order.erp_id = 0;
            sale_Order.erp_name = null;
            await saleOrderDb.UpdateAsync(sale_Order);

            return await SendSaleOrder(sale_Order);
        }

        [Obsolete("Use RetrySendAfterValidation")]
        public Task<SaleOrderSendResult> RegenerateExternalGuidAndSend(sale_order sale_Order) =>
            RetrySendAfterValidation(sale_Order);

        public async Task<(List<string> SyncedLabels, List<SaleOrderSendResult> Failures)> SendAllSaleOrders()
        {
            var syncedLabels = new List<string>();
            var failures = new List<SaleOrderSendResult>();
            var saleOrderLinesDb = new SaleOrderLineDb(Constants.Session.odooConnection.DbNameSqlite);
            SaleOrderDb saleOrderDb = new SaleOrderDb(Constants.Session.odooConnection.DbNameSqlite);
            var listOrders = await saleOrderDb.GetItemsAsync(Constants.Session.res_Company.id, false);

            if (listOrders == null || listOrders.Count == 0)
                return (syncedLabels, failures);

            foreach (var item in listOrders)
            {
                if (item.order_line == null)
                    item.order_line = new List<OrderLineWrapper>();

                var orderLines = await saleOrderLinesDb.GetItemsAsync(item.id);

                foreach (var line in orderLines)
                {
                    item.order_line.Add(new OrderLineWrapper(line));
                }

                var sendResult = await SendSaleOrder(item);
                if (sendResult.Ok)
                {
                    var label = !string.IsNullOrWhiteSpace(item.erp_name)
                        ? item.erp_name
                        : item.id_referencia;
                    if (string.IsNullOrWhiteSpace(label))
                        label = $"id local {item.id} (erp {item.erp_id})";
                    syncedLabels.Add(label);
                }
                else
                {
                    failures.Add(sendResult);
                }
            }

            return (syncedLabels, failures);
        }

        public async Task<int> CreateProjectTask(ProjectTask projectTask)
        {
            projectTask.user_ids = new int [] { projectTask.user_id };

            //TODO: Agregar validacion para ProjectTask existente
            ApiManager.HubProjectTask hubManager = new HubProjectTask(Constants.Session);

            var existingTasks = await hubManager.GetByNameUser(projectTask.name, Constants.Session.CurrentUserFront.uid);

            if (existingTasks != null && existingTasks.result != null && existingTasks.result.Length > 0)
            {
                Debug.WriteLine("La tarea ya existe en el servidor: " + projectTask.name);
                await Toast.Make("La tarea ya existe en el servidor: " + projectTask.name).Show();
                
                projectTask.is_synchronized = true;
                projectTask.date_synchronized = DateTime.Now;
                projectTask.id_sync = existingTasks.result[0].id;
                ProjectTaskDb projectTaskDb = new ProjectTaskDb(Constants.Session.odooConnection.DbNameSqlite);
                await projectTaskDb.UpdateAsync(projectTask);

                return projectTask.id_sync;
            }

            ApiResponseOdooRpcT<int> resultTask = await hubManager.Create(projectTask);

            if (resultTask != null && resultTask.error != null)
            {
                Debug.WriteLine(resultTask.error.data.message);
                Debug.WriteLine(resultTask.error.data.debug);
                await Toast.Make("Error:" + resultTask.error.data.message).Show();
                return 0;
            }

            if (resultTask != null && resultTask.result != null)
            {
                await Toast.Make("Datos enviados correctamente").Show();
                projectTask.is_synchronized = true;
                projectTask.date_synchronized = DateTime.Now;
                projectTask.id_sync = resultTask.result;
                ProjectTaskDb projectTaskDb = new ProjectTaskDb(Constants.Session.odooConnection.DbNameSqlite);
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

            AccountAnalyticLineDb projectTaskDb = new AccountAnalyticLineDb(Constants.Session.odooConnection.DbNameSqlite);
            var items = await projectTaskDb.GetItemsAsync(projectTask);
            foreach(var item in items)
            {
                if (item.project_id != Constants.Session.odooConnection.project_id)
                {
                    item.project_id = Constants.Session.odooConnection.project_id;                    
                    await projectTaskDb.UpdateAsync(item);
                }

                item.task_id_sync = projectTask.id_sync;
                await SendAccountAnalyticLine(item);
            }
        }

        public async Task SendAllProjectTask()
        {
            ProjectTaskDb saleOrderDb = new ProjectTaskDb(Constants.Session.odooConnection.DbNameSqlite);
            var listOrders = await saleOrderDb.GetItemsAsync(Constants.Session.res_Company.id, false);

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
            HubAccountAnalyticLine hubManager = new HubAccountAnalyticLine(Constants.Session);
            ApiResponseOdooRpcT<int> resultTask = await hubManager.Create(item);

            if (resultTask != null && resultTask.error != null)
            {
                Debug.WriteLine(resultTask.error.data.message);
                Debug.WriteLine(resultTask.error.data.debug);
                await Toast.Make("Error:" + resultTask.error.data.message).Show();
                return;
            }

            if (resultTask != null && resultTask.result != null)
            {
                await Toast.Make("Datos enviados correctamente").Show();

                item.is_synchronized = true;
                item.date_synchronized = DateTime.Now;
                item.id_sync = resultTask.result;
                AccountAnalyticLineDb projectTaskDb = new AccountAnalyticLineDb(Constants.Session.odooConnection.DbNameSqlite);
                await projectTaskDb.UpdateAsync(item);
            }
        }

        public async Task SendAllAccountAnalyticLine(ProgressBarAnimationBehaviorPage obj)
        {
            AccountAnalyticLineDb accountAnalyticLineDb = new AccountAnalyticLineDb(Constants.Session.odooConnection.DbNameSqlite);
            var listItems = await accountAnalyticLineDb.GetItemsAsync(Constants.Session.res_Company.id, false);

            if (listItems == null || listItems.Count == 0)
                return;

            int totalItems = listItems.Count;
            int itemIndex = 0;
            foreach (var item in listItems)
            {
                //TODO: Agregar validacion para ProjectTask existente
                itemIndex++;
                obj.SetTitle($"Sincronizando tareas ({itemIndex}/{totalItems})");
                await SendAccountAnalyticLine(item);
            }
        }
    }
}
