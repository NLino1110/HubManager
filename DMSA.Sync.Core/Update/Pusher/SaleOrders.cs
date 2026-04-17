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
using DMSA.Models.Security;
using DMSA.Sync.Core.Controls;
using DMSA.Sync.Core.Database.Sqlite;
using DMSA.Sync.Core.Database.Sqlite.Sales;
using DMSA.Sync.Core.Database.Sqlite.tareas;
using Microsoft.Maui.Graphics;
using Newtonsoft.Json;
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
                
        public async Task<bool> SendSaleOrder(sale_order sale_Order)
        {            
            SaleOrderDb saleOrderDb = new SaleOrderDb(Constants.Session.odooConnection.DbNameSqlite);

            if (!sale_Order.mobile_sync)
                sale_Order.mobile_sync = true;

            if(string.IsNullOrEmpty(sale_Order.external_guid))
            {
                sale_Order.external_guid = Guid.NewGuid().ToString("N");
                await saleOrderDb.UpdateAsync(sale_Order);
            }

            HubSaleOrder hubSaleOrder = new HubSaleOrder(Constants.Session);
            ApiResponseOdooRpcT<int> resultTask = await hubSaleOrder.Create(sale_Order, false);

            if (resultTask != null && resultTask.error != null)
            {
                Debug.WriteLine(resultTask.error.data.message);
                Debug.WriteLine(resultTask.error.data.debug);
                await Toast.Make("Error:" + resultTask.error.data.message).Show();
                return false;
            }

            if (resultTask != null && resultTask.result !=null)
            {
                await Toast.Make("Datos enviados correctamente").Show();

                sale_Order.erp_id = resultTask.result;
                sale_Order.is_synchronized = true;
                sale_Order.date_synchronized = DateTime.Now;
                
                await saleOrderDb.UpdateAsync(sale_Order);

                var resultDetailTask = await hubSaleOrder.GetLines(sale_Order.erp_id);

                if (resultDetailTask.result != null)
                {
                    SaleOrderLineDb saleOrderLineDb = new SaleOrderLineDb(Constants.Session.odooConnection.DbNameSqlite);
                    var lines = await saleOrderLineDb.GetItemsAsync(sale_Order.id);

                    bool new_name_order = false;
                    bool details_ok = false;

                    var linesIds = new List<SaleOrderPromotionWizardLineWrapper>();
                    var allGifts = new List<AllSaleOrderPromotionWizardGiftWrapper>();
                    var gift_line_Ids = new List<SaleOrderPromotionWizardGiftWrapper>();

                    foreach (var lineRcp in resultDetailTask.result)
                    {
                        var item = lines.FirstOrDefault(l => l.product_id == lineRcp._product_id && l.sequence == lineRcp.sequence);                        
                        if (item != null)
                        {
                            item.erp_id = lineRcp.id;
                            await saleOrderLineDb.UpdateAsync(item);

                            details_ok = true;

                            //Actualiza nombre de la orden aprovechando la consulta a los detalles
                            if (!new_name_order)
                            {
                                try
                                {
                                    var order_name = lineRcp.order_id?.ElementAtOrDefault(1)?.ToString();
                                    sale_Order.erp_name = order_name;
                                    await saleOrderDb.UpdateAsync(sale_Order);
                                    new_name_order = true;
                                }
                                catch (Exception ex)
                                {
                                    Debug.WriteLine("Error al obtener el nombre de la orden: " + ex.Message);
                                }
                            }
                        }
                    }

                    foreach (var item in lines.Where(x=>x.is_gift))// || (!x.is_gift && x.discount > 0)))
                    {
                        List<OriginPromoOrderLine> productSequenceApplyList = new List<OriginPromoOrderLine>();
                        
                        if (string.IsNullOrEmpty(item.origin_gift_line_ids_offline))
                            continue;

                        productSequenceApplyList = JsonConvert.DeserializeObject<List<OriginPromoOrderLine>>(item.origin_gift_line_ids_offline);

                        var items_found = lines
                                .Where(l => productSequenceApplyList.Any(p =>
                                        p.sequence == l.sequence &&
                                        p.product_id == l.product_id))
                                .Select(l => l.erp_id)
                                .ToList();

                        if (items_found != null && items_found.Any())
                        {
                            item.origin_gift_line_ids = items_found.ToArray();
                            await saleOrderLineDb.UpdateAsync(item);
                        }
                    }

                    foreach (var line in lines)
                    {
                        if (line.is_gift || line.discount > 0)
                        {
                            bool isLineDiscount = false;
                            decimal Qty = line.product_uom_qty;
                            int[] origin_gift_line_ids = line.origin_gift_line_ids;

                            if (line.discount > 0 && !line.is_gift)
                            {
                                Qty = line.discount;
                                isLineDiscount = true;
                                origin_gift_line_ids = new int[] { line.erp_id };
                            }

                            allGifts.Add(new AllSaleOrderPromotionWizardGiftWrapper(new AllSaleOrderPromotionWizardGift
                            {
                                Product_Id = line.product_tmpl_id,
                                Qty = Qty,
                                Promotion_Line_Id = 0, //Se determina cuando ya se haya creado padre
                                Stock = line.product_uom_qty,
                                Price = line.price_unit,
                                Approve = true,
                                Lines_Ids = new int[] { line.erp_id },                                
                                Discount = 0,
                                Obtained = true
                            }));


                            List<OriginPromoOrderLine> productSequenceApplyList = new List<OriginPromoOrderLine>();
                            productSequenceApplyList = JsonConvert.DeserializeObject<List<OriginPromoOrderLine>>(line.origin_gift_line_ids_offline);
                            int[] linesIdsArray = new int[] { line.erp_id };

                            foreach (var itemSequence in productSequenceApplyList)
                            {
                                linesIds.Add(new SaleOrderPromotionWizardLineWrapper(new SaleOrderPromotionWizardLine
                                {
                                    Promotion_Id = itemSequence.promo_id,
                                    Rule_Id = itemSequence.rule_id,
                                    Discount = 100,
                                    Rule_Value = itemSequence.total_allowed_gifts,
                                    Qty_Confirmation = true,
                                    Lines_Ids = linesIdsArray
                                }));
                            }

                            //////foreach(var originGiftLineId in origin_gift_line_ids)
                            //////{
                            //////    var groupedByPromotion = lines
                            //////        .Where(l =>
                            //////            l.erp_id == originGiftLineId
                            //////        )
                            //////        .SelectMany(l => l.promotion_ids.Select(promo => new
                            //////        {
                            //////            PromotionId = promo,
                            //////            Line = l
                            //////        }))
                            //////        .GroupBy(x => x.PromotionId);

                            //////    if(isLineDiscount)
                            //////    {
                            //////        groupedByPromotion = line.promotion_ids.Select(promo => new
                            //////        {
                            //////            PromotionId = promo,
                            //////            Line = line
                            //////        })
                            //////        .GroupBy(x => x.PromotionId);
                            //////    }

                            //////    foreach (var promoGroup in groupedByPromotion)
                            //////    {
                            //////        int promotionId = promoGroup.Key;

                            //////        var groupedByRule = promoGroup
                            //////            .SelectMany(x => x.Line.rule_ids.Select(rule => new
                            //////            {
                            //////                RuleId = rule,
                            //////                Line = x.Line
                            //////            }))
                            //////            .GroupBy(x => x.RuleId);

                            //////        foreach (var ruleGroup in groupedByRule)
                            //////        {
                            //////            int ruleId = ruleGroup.Key;

                            //////            int[] linesIdsArray = new int[] { line.erp_id };

                            //////            int totalAllowedGifts = 0;
                            //////            var linesGroup = ruleGroup
                            //////                .Select(x => x.Line)
                            //////                .Distinct()
                            //////                .ToArray();

                            //////            foreach (var lineItemGroup in linesGroup)
                            //////            {
                            //////                totalAllowedGifts = ruleGroup
                            //////                        .Select(x => x.Line)
                            //////                        .Distinct()
                            //////                        .Where(l => l.promotionDataList.Count > 0)
                            //////                        .Sum(line =>
                            //////                        {
                            //////                            var listPromotionData = lineItemGroup.promotionDataList;
                            //////                            return listPromotionData?
                            //////                                .SelectMany(p => p.RuleSet)
                            //////                                .Sum(r => r.AllowedGifts) ?? 0;
                            //////                        });
                            //////            }

                            //////            foreach (var item in ruleGroup)
                            //////            {
                            //////                linesIds.Add(new SaleOrderPromotionWizardLineWrapper(new SaleOrderPromotionWizardLine
                            //////                {
                            //////                    Promotion_Id = promoGroup.Key,
                            //////                    Rule_Id = ruleGroup.Key,
                            //////                    Discount = 100,
                            //////                    Rule_Value = totalAllowedGifts,
                            //////                    Qty_Confirmation = true,
                            //////                    Lines_Ids = linesIdsArray
                            //////                }));
                            //////            }
                            //////        }
                            //////    }
                        //}
                        }
                        else
                        {
                            
                        }
                    }
                    
                    var newSaleOrderPromotionWizard = new SaleOrderPromotionWizard
                    {
                        Order_Id = sale_Order.erp_id,
                        Line_Ids = linesIds,
                        Gift_Line_Ids = gift_line_Ids,
                        All_Gift_Line_Ids = allGifts,
                        Base = true
                    };

                    var hubSaleOrderPromotionWizard = new HubSaleOrderPromotionWizard(Constants.Session);
                    var createdPromotion = await hubSaleOrderPromotionWizard.Create(newSaleOrderPromotionWizard, true);
                    
                    await hubSaleOrder.WriteLines(lines);
                }

                return true;
            }

            return false;
        }

        public async Task SendAllSaleOrders()
        {           
            SaleOrderDb saleOrderDb = new SaleOrderDb(Constants.Session.odooConnection.DbNameSqlite);
            var listOrders = await saleOrderDb.GetItemsAsync(Constants.Session.res_Company.id, false);
            
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
