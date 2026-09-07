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
                app_version = ResolveAppVersion(),
                timestamp = DateTime.UtcNow,
                data = new SaleOrderDTO
                {
                    SaleOrder = fullObject,
                    PromoData = ""//JsonConvert.DeserializeObject(new object())
                }
            };

            return JObject.FromObject(dto);
        }

        /// <summary>
        /// Versión del APK en sesión. Si no hay (apps viejas / default 0.0.0), usa 1.0.56.
        /// </summary>
        private static string ResolveAppVersion()
        {
            var version = Constants.Session?.AppVersion;
            if (string.IsNullOrWhiteSpace(version) || version == "0.0.0")
                return "1.0.56";

            return version;
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

        /// <summary>
        /// Odoo create va sin regalos ni rule/promo ids; el pedido en memoria debe
        /// conservarlos para que Reintentar envío arme de nuevo el payload completo.
        /// </summary>
        private static List<(sale_order_line Line, int[] PromotionIds, int[] RuleIds)> SnapshotLinePromoRules(sale_order saleOrder)
        {
            var snapshot = new List<(sale_order_line, int[], int[])>();
            if (saleOrder?.order_line == null)
                return snapshot;

            foreach (var itemLine in saleOrder.order_line)
            {
                if (itemLine[2] is sale_order_line line)
                {
                    snapshot.Add((
                        line,
                        line.promotion_ids?.ToArray() ?? [],
                        line.rule_ids?.ToArray() ?? []));
                }
            }

            return snapshot;
        }

        private static void RestoreLinePromoRules(List<(sale_order_line Line, int[] PromotionIds, int[] RuleIds)> snapshot)
        {
            if (snapshot == null)
                return;

            foreach (var item in snapshot)
            {
                item.Line.promotion_ids = item.PromotionIds;
                item.Line.promotion_ids_json = item.PromotionIds == null
                    ? "[]"
                    : JsonConvert.SerializeObject(item.PromotionIds);
                item.Line.rule_ids = item.RuleIds;
                item.Line.rule_ids_json = item.RuleIds == null
                    ? "[]"
                    : JsonConvert.SerializeObject(item.RuleIds);
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

            var originalOrderLines = sale_Order.order_line;
            var originalPromoRules = SnapshotLinePromoRules(sale_Order);

            HubSaleOrder hubSaleOrder = new HubSaleOrder(Constants.Session);
            ApiResponseOdooRpcT<int> resultTask = null;
            try
            {
                RemoveGiftLines(ref sale_Order);
                RemovePromoRulesData(ref sale_Order);
                resultTask = await hubSaleOrder.Create(sale_Order, false);
            }
            finally
            {
                sale_Order.order_line = originalOrderLines;
                RestoreLinePromoRules(originalPromoRules);
            }

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

        public async Task<int> CreateProjectTask(ProjectTask projectTask, bool showNotifications = true)
        {
            projectTask.user_ids = new int [] { projectTask.user_id };

            ApiManager.HubProjectTask hubManager = new HubProjectTask(Constants.Session);

            var existingTasks = await hubManager.GetByNameUser(projectTask.name, Constants.Session.CurrentUserFront.uid);

            if (existingTasks != null && existingTasks.result != null && existingTasks.result.Length > 0)
            {
                Debug.WriteLine("La tarea ya existe en el servidor: " + projectTask.name);
                if (showNotifications)
                    await Toast.Make("La tarea ya existe en el servidor: " + projectTask.name).Show();

                projectTask.id_sync = existingTasks.result[0].id;
                if (showNotifications)
                {
                    projectTask.is_synchronized = true;
                    projectTask.date_synchronized = DateTime.Now;
                }

                ProjectTaskDb projectTaskDb = new ProjectTaskDb(Constants.Session.odooConnection.DbNameSqlite);
                await projectTaskDb.UpdateAsync(projectTask);

                return projectTask.id_sync;
            }

            ApiResponseOdooRpcT<int> resultTask = await hubManager.Create(projectTask);

            if (resultTask != null && resultTask.error != null)
            {
                Debug.WriteLine(resultTask.error.data.message);
                Debug.WriteLine(resultTask.error.data.debug);
                if (showNotifications)
                    await Toast.Make("Error:" + resultTask.error.data.message).Show();
                return 0;
            }

            if (resultTask != null && resultTask.result != null)
            {
                if (showNotifications)
                    await Toast.Make("Datos enviados correctamente").Show();

                projectTask.id_sync = resultTask.result;
                if (showNotifications)
                {
                    projectTask.is_synchronized = true;
                    projectTask.date_synchronized = DateTime.Now;
                }

                ProjectTaskDb projectTaskDb = new ProjectTaskDb(Constants.Session.odooConnection.DbNameSqlite);
                await projectTaskDb.UpdateAsync(projectTask);
                return projectTask.id_sync;
            }

            return 0;
        }

        public async Task<ProjectTaskSendResult> SendProjectTask(ProjectTask projectTask, bool allowAutoRetry = false)
        {
            var firstPass = await SendProjectTaskPassAsync(projectTask, autoRetried: false);

            if (allowAutoRetry && firstPass.IsPartial && firstPass.PendingCount > 0)
            {
                var retryPass = await SendProjectTaskPassAsync(projectTask, autoRetried: true);
                await ApplyProjectTaskSyncStateAsync(projectTask, retryPass);
                return retryPass;
            }

            await ApplyProjectTaskSyncStateAsync(projectTask, firstPass);
            return firstPass;
        }

        public Task<ProjectTaskSendResult> ReprocessPendingProjectTask(ProjectTask projectTask) =>
            SendProjectTask(projectTask, allowAutoRetry: false);

        private async Task<ProjectTaskSendResult> SendProjectTaskPassAsync(ProjectTask projectTask, bool autoRetried)
        {
            var dbName = Constants.Session.odooConnection.DbNameSqlite;
            var lineDb = new AccountAnalyticLineDb(dbName);
            var failures = new List<ProjectTaskLineFailure>();
            var taskLabel = !string.IsNullOrWhiteSpace(projectTask.name)
                ? projectTask.name
                : $"id local {projectTask.id}";

            projectTask.last_sync_attempt = DateTime.Now;

            if (projectTask.project_id_ != Constants.Session.odooConnection.project_id)
                projectTask.project_id_ = Constants.Session.odooConnection.project_id;

            if (projectTask.id_sync <= 0)
            {
                int projectId = await CreateProjectTask(projectTask, showNotifications: false);
                if (projectId <= 0)
                {
                    failures.Add(new ProjectTaskLineFailure
                    {
                        LocalId = projectTask.id,
                        Label = taskLabel,
                        ErrorMessage = "No se pudo crear o vincular la cabecera en el ERP."
                    });

                    return ProjectTaskSendResult.Build(
                        taskLabel,
                        total: 0,
                        synced: 0,
                        failed: 1,
                        headerFailed: true,
                        autoRetried: autoRetried,
                        failures: failures);
                }
            }

            var allLines = await lineDb.GetItemsAsync(projectTask);
            var pendingLines = allLines
                .Where(x => ProjectTaskSyncValidation.IsLinePendingSync(x, projectTask.id_sync))
                .ToList();

            foreach (var item in pendingLines)
            {
                if (item.project_id != Constants.Session.odooConnection.project_id)
                {
                    item.project_id = Constants.Session.odooConnection.project_id;
                    await lineDb.UpdateAsync(item);
                }

                var lineResult = await SendOrRepairAccountAnalyticLine(item, projectTask.id_sync, showNotifications: false);
                if (!lineResult.ok)
                {
                    failures.Add(new ProjectTaskLineFailure
                    {
                        LocalId = item.id,
                        Label = string.IsNullOrWhiteSpace(item.name) ? $"Detalle {item.id}" : item.name,
                        ErrorMessage = lineResult.errorMessage
                    });
                }
            }

            allLines = await lineDb.GetItemsAsync(projectTask);
            int total = allLines.Count;
            int synced = ProjectTaskSyncValidation.CountEffectivelySyncedLines(allLines, projectTask.id_sync);
            int failed = failures.Count;

            if (total > 0 && projectTask.id_sync <= 0)
            {
                failures.Insert(0, new ProjectTaskLineFailure
                {
                    LocalId = projectTask.id,
                    Label = taskLabel,
                    ErrorMessage = "La cabecera no quedó vinculada al ERP."
                });

                return ProjectTaskSendResult.Build(
                    taskLabel,
                    total,
                    synced,
                    failures.Count,
                    headerFailed: true,
                    autoRetried: autoRetried,
                    failures: failures);
            }

            return ProjectTaskSendResult.Build(
                taskLabel,
                total,
                synced,
                failed,
                headerFailed: false,
                autoRetried: autoRetried,
                failures: failures);
        }

        private async Task ApplyProjectTaskSyncStateAsync(ProjectTask projectTask, ProjectTaskSendResult result)
        {
            var dbName = Constants.Session.odooConnection.DbNameSqlite;
            var projectTaskDb = new ProjectTaskDb(dbName);

            projectTask.sync_status = result.SyncStatus;
            projectTask.sync_message = result.Message;
            projectTask.sync_ok_count = result.SyncedCount;
            projectTask.sync_total_count = result.TotalCount;
            projectTask.last_sync_attempt = DateTime.Now;
            projectTask.is_synchronized = result.Ok;

            if (result.Ok)
                projectTask.date_synchronized = DateTime.Now;

            await projectTaskDb.UpdateAsync(projectTask);
        }

        public async Task<List<ProjectTaskSendResult>> SendAllProjectTask(bool allowAutoRetry = true)
        {
            var results = new List<ProjectTaskSendResult>();
            ProjectTaskDb saleOrderDb = new ProjectTaskDb(Constants.Session.odooConnection.DbNameSqlite);
            var listOrders = await saleOrderDb.GetItemsPendingSyncAsync(Constants.Session.res_Company.id);

            if (listOrders == null || listOrders.Count == 0)
                return results;

            foreach (var item in listOrders)
            {
                var sendResult = await SendProjectTask(item, allowAutoRetry);
                results.Add(sendResult);
            }

            return results;
        }

        private async Task<(bool ok, string errorMessage)> SendOrRepairAccountAnalyticLine(
            AccountAnalyticLine item,
            int erpTaskId,
            bool showNotifications = true)
        {
            if (item.id_sync > 0 && erpTaskId > 0)
                return await LinkAccountAnalyticLineToTask(item, erpTaskId, showNotifications);

            if (erpTaskId > 0)
                item.task_id_sync = erpTaskId;

            return await SendAccountAnalyticLine(item, showNotifications);
        }

        private async Task<(bool ok, string errorMessage)> LinkAccountAnalyticLineToTask(
            AccountAnalyticLine item,
            int erpTaskId,
            bool showNotifications = true)
        {
            HubAccountAnalyticLine hubManager = new HubAccountAnalyticLine(Constants.Session);
            ApiResponseOdooRpcT<bool> resultTask = await hubManager.WriteTaskId(item.id_sync, erpTaskId);

            if (resultTask != null && resultTask.error != null)
            {
                var error = resultTask.error.data.message ?? "Error al vincular detalle con la tarea ERP.";
                Debug.WriteLine(resultTask.error.data.message);
                Debug.WriteLine(resultTask.error.data.debug);
                if (showNotifications)
                    await Toast.Make("Error:" + error).Show();
                return (false, error);
            }

            if (resultTask?.result == true)
            {
                item.task_id_sync = erpTaskId;
                item.is_synchronized = true;
                item.date_synchronized = DateTime.Now;

                AccountAnalyticLineDb lineDb = new AccountAnalyticLineDb(Constants.Session.odooConnection.DbNameSqlite);
                await lineDb.UpdateAsync(item);

                if (showNotifications)
                    await Toast.Make("Detalle vinculado correctamente con la tarea ERP").Show();

                return (true, string.Empty);
            }

            return (false, "No se pudo vincular el detalle existente con la tarea ERP.");
        }

        private async Task<(bool ok, string errorMessage)> SendAccountAnalyticLine(AccountAnalyticLine item, bool showNotifications = true)
        {
            HubAccountAnalyticLine hubManager = new HubAccountAnalyticLine(Constants.Session);
            ApiResponseOdooRpcT<int> resultTask = await hubManager.Create(item);

            if (resultTask != null && resultTask.error != null)
            {
                Debug.WriteLine(resultTask.error.data.message);
                Debug.WriteLine(resultTask.error.data.debug);
                var error = resultTask.error.data.message ?? "Error al enviar detalle.";
                if (showNotifications)
                    await Toast.Make("Error:" + error).Show();
                return (false, error);
            }

            if (resultTask != null && resultTask.result != null)
            {
                if (showNotifications)
                    await Toast.Make("Datos enviados correctamente").Show();

                item.is_synchronized = true;
                item.date_synchronized = DateTime.Now;
                item.id_sync = resultTask.result;
                AccountAnalyticLineDb projectTaskDb = new AccountAnalyticLineDb(Constants.Session.odooConnection.DbNameSqlite);
                await projectTaskDb.UpdateAsync(item);
                return (true, string.Empty);
            }

            return (false, "Respuesta vacía del servidor al enviar detalle.");
        }

        [Obsolete("Use SendAccountAnalyticLine(item, showNotifications)")]
        public async Task SendAccountAnalyticLine(AccountAnalyticLine item)
        {
            await SendAccountAnalyticLine(item, showNotifications: true);
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
