using ApiManager;
using ApiManagerOdoo.Accounting;
using DMSA.Models.General.Requests;
using DMSA.Models.Odoo.Accounting;
using DMSA.Models.Odoo.Tools;
using DMSA.Sync.Core.Database.Sqlite;
using DMSA.Sync.Core.Database.Sqlite.Payments;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Linq;

namespace DMSA.Sync.Core.Update
{
    public partial class ServerPuller
    {
        public async Task<bool> DownloadAccountMoveRefund(Func<int, int, Task>? onProgress = null)
        {
            JsonSerializerSettings settings = new JsonSerializerSettings();
            settings.ContractResolver = new IncludeJsonIgnoreResolver();

            DateTime dateIni = DateTime.Now.AddDays(-150);
            DateTime dateEnd = DateTime.Now;

            var hubmanager = new HubCreditNoteRequestGroup(appSession);
            var resultCount = await hubmanager.GetHeaderCount(dateIni);

            Debug.WriteLine(resultCount.result);

            if (resultCount.result == 0)
            {
                return false;
            }

            int totalPages = (int)Math.Ceiling((double)resultCount.result / limit);

            var databaseHeader = new CreditNoteRequestGroupDb(Constants.Session.odooConnection.DbNameSqlite);
            var databaseMoveSend = new CreditNoteRequestDb(Constants.Session.odooConnection.DbNameSqlite);
            var databaseMoveLineSend = new CreditNoteRequestDetailDb(Constants.Session.odooConnection.DbNameSqlite);

            for (int indice = 0; indice <= totalPages; indice++)
            {
                var responseAll = await hubmanager.GetItemsFull(dateIni, indice);

                if (responseAll != null && responseAll.Length > 0)
                {                    
                    foreach (var headerItem in responseAll)
                    {                        
                        var foundHeader = await databaseHeader.GetByRequestName(headerItem.request_name);
                        if (foundHeader != null)
                        {
                            Debug.WriteLine("Registro ya existe en la base de datos!, no se sincronizará");
                            Debug.WriteLine(foundHeader.request_name);                            
                            continue;
                        }

                        string jsonHeaderItem = JsonConvert.SerializeObject(headerItem);
                        var newHeaderItem = JsonConvert.DeserializeObject<CreditNoteRequestGroup>(jsonHeaderItem);

                        newHeaderItem.was_odoo_synced = true;
                        int newHeaderId = await databaseHeader.InsertAsync(newHeaderItem);

                        if (headerItem.account_moves != null)
                        {
                            foreach (var paymentItem in headerItem.account_moves)
                            {
                                paymentItem.parent_id = newHeaderItem.id;
                                paymentItem.was_odoo_synced = true;
                                string jsonPaymentItem = JsonConvert.SerializeObject(paymentItem, settings);
                                var newPaymentItem = JsonConvert.DeserializeObject<credit_note_request>(jsonPaymentItem, settings);

                                int newPayId = await databaseMoveSend.InsertAsync(newPaymentItem);

                                if (paymentItem.lines != null)
                                {
                                    foreach (var lineItem in paymentItem.lines)
                                    {
                                        lineItem.parent_id = newPaymentItem.id;
                                        lineItem.was_odoo_synced = true;
                                        string jsonLineItem = JsonConvert.SerializeObject(lineItem, settings); //, settings);
                                        var newLineItem = JsonConvert.DeserializeObject<credit_note_request_detail>(jsonLineItem, settings);
                                        await databaseMoveLineSend.InsertAsync(newLineItem);
                                    }
                                }
                            }
                        }

                        if (onProgress != null)
                            await onProgress(indice + 1, totalPages);
                    }
                }

                Console.WriteLine("DownloadAccountMoveRefund Página:" + indice);

                if (indice >= maxIndexExceeded)
                {
                    Console.WriteLine("Página " + indice + ": Se terminará el proceso.");
                    break;
                }
            }

            TimeSpan span = (DateTime.Now - dateIni);

            Console.WriteLine(String.Format("Lapso transcurrido: {0} days, {1} hours, {2} minutes, {3} seconds",
                span.Days, span.Hours, span.Minutes, span.Seconds));

            return true;
        }

        public async Task<bool> OnlineSyncResPartner()
        {
            var stopwatch = Stopwatch.StartNew();

            var database = new ResPartnerDb(Constants.Session.odooConnection.DbNameSqlite);
            
            int partner_id = Constants.Session.CurrentUserFront.partner_id;

            DateTime? lastDate = await database.GetLastWriteDateAsync(sync_date_since);
            
            ApiManager.HubResPartner hubmanager = new ApiManager.HubResPartner(appSession);
            var resultCount = await hubmanager.GetCountBySeller(lastDate.Value.Year, lastDate.Value.Month, lastDate.Value.Day, partner_id);

            Debug.WriteLine(resultCount.result);

            if (resultCount.result == 0)
            {
                return false;
            }

            int countTotal = resultCount.result / limit;
            
            for (int indice = 0; indice <= countTotal; indice++)
            {
                var responseAll = await hubmanager.GetByWriteDateBySeller(lastDate.Value, limit, indice, partner_id);

                if (responseAll != null && responseAll.result != null && responseAll.result.Length > 0)
                {
                    await database.InsertBatchAsync(responseAll.result);

                    //obtenemos los campos child_ids para los contactos asociados
                    for (int i = 0; i < responseAll.result.Length; i++)
                    {
                        var item = responseAll.result[i];

                        if (string.IsNullOrEmpty(item.childs_ids_json) || item.childs_ids_json.Equals("[]"))
                            continue;

                        var _childs_ids = JsonConvert.DeserializeObject<int[]>(item.childs_ids_json);

                        if (_childs_ids != null && _childs_ids.Length > 0)
                        {
                            var responseChild = await hubmanager.GetByIds(limit, 0, _childs_ids);
                            if (responseChild != null && responseChild.result != null && responseChild.result.Length > 0)
                            {
                                await database.InsertBatchAsync(responseChild.result);
                            }
                        }
                    }
                }

                Console.WriteLine("Página:" + indice);

                if (indice >= maxIndexExceeded)
                {
                    Console.WriteLine("Página " + indice + ": Se terminará el proceso.");
                    break;
                }
            }

            stopwatch.Stop();

            Debug.WriteLine(String.Format("Lapso transcurrido: {0} days, {1} hours, {2} minutes, {3} seconds",
                stopwatch.Elapsed.Days, stopwatch.Elapsed.Hours, stopwatch.Elapsed.Minutes, stopwatch.Elapsed.Seconds));

            return true;
        }


        public async Task<bool> OnlineSyncResPartnerFull(Func<int, int, Task>? onProgress = null)
        {
            var stopwatch = Stopwatch.StartNew();

            var database = new ResPartnerDb(Constants.Session.odooConnection.DbNameSqlite);

            DateTime? lastDate = await database.GetLastWriteDateAsync(sync_date_since);

            HubResPartner hubmanager = new HubResPartner(appSession);
            var resultCount = await hubmanager.GetCount(lastDate.Value.Year, lastDate.Value.Month, lastDate.Value.Day);

            Debug.WriteLine(resultCount.result);

            if (resultCount.result == 0)
            {
                return false;
            }

            //int countTotal = resultCount.result / limit;
            int totalPages = (int)Math.Ceiling((double)resultCount.result / limit);

            for (int indice = 0; indice <= totalPages; indice++)
            {
                var responseAll = await hubmanager.GetByWriteDate(lastDate.Value, limit, indice);

                if (responseAll != null && responseAll.result != null && responseAll.result.Length > 0)
                {
                    //var itemsConCero = responseAll.result
                    //    .Where(x => x._product_pricelist_id == 0)
                    //    .ToList();

                    //if (itemsConCero.Any())
                    //{
                    //    Debug.WriteLine($"Se encontraron {itemsConCero.Count} items con _product_pricelist_id = 0");

                    //    foreach (var item in itemsConCero)
                    //    {
                    //        Debug.WriteLine($"Item ID: {item.id} - {item.name}");
                    //    }
                    //}

                    //var itemsDataTest = responseAll.result
                    //    .Where(x => x.facturacion_dias_credito_limite > 0)
                    //    .ToList();

                    //foreach(var item in itemsDataTest)
                    //{
                    //    Debug.WriteLine($"Item ID: {item.id} - {item.name}   {item.facturacion_dias_credito_limite}");
                    //}

                    await database.InsertBatchAsync(responseAll.result);
                    //await database.InsertBatchControlAsync(responseAll.result);
                }

                Console.WriteLine("ResPartnerFull Página:" + indice + " de " + totalPages);

                if (onProgress != null)
                    await onProgress(indice, totalPages);

                if (indice >= maxIndexExceeded)
                {
                    Console.WriteLine("Página " + indice + ": Se terminará el proceso.");
                    break;
                }
            }

            await database.RemoveOldDataAsync();

            stopwatch.Stop();

            Debug.WriteLine(String.Format("Lapso transcurrido: {0} days, {1} hours, {2} minutes, {3} seconds",
                stopwatch.Elapsed.Days, stopwatch.Elapsed.Hours, stopwatch.Elapsed.Minutes, stopwatch.Elapsed.Seconds));

            return true;
        }

        // ANTES (Cobranzas usaba el mismo de Órdenes): OnlineSyncResPartnerFull
        //   search_count/search_read con write_date >= última sync.
        //   No actualizaba saldos si el partner no se había escrito.
        // DESPUÉS (solo Cobranzas): OnlineSyncResPartnerCobranzasAll
        //   Fase 1: search_count/search_read de res.partner filtrados por adic_comercial_id
        //     o adic_comercial_secundarios_ids (= partner_id de sesión), sin write_date.
        //     Incluye activos e inactivos (misc_estado). Para omitir inactivos en Odoo, ver
        //     BuildAdicComercialDomain en HubResPartner (filtro misc_estado=activo, no activo hoy).
        //   Mobile App Admin (218): GetCountAll/GetAll sin filtro comercial (todos los partners).
        //   Page size admin: ResolveCobranzasSyncPageSize() → 2000 (pruebas de peso); vendedor → 1000.
        //   Fase 2: web_read por IDs + UPDATE parcial de saldos en res_partner.
        //   Órdenes sigue usando OnlineSyncResPartnerFull; no se toca.
        // REVERTIR Fase 2: EnableResPartnerCobranzasSaldosWebRead = false en ServerPuller.cs
        public async Task<bool> OnlineSyncResPartnerCobranzasAll(
            Func<int, int, Task>? onProgress = null,
            Func<int, int, Task>? onSaldosProgress = null)
        {
            var stopwatch = Stopwatch.StartNew();
            int pageLimit = ResolveCobranzasSyncPageSize();

            var database = new ResPartnerDb(Constants.Session.odooConnection.DbNameSqlite);
            var hubmanager = new HubResPartner(appSession);

            bool isMobileAppAdmin = IsCurrentUserMobileAppAdmin();

            // partner_id se obtiene al autenticarse (Login → CurrentUserFront.partner_id).
            // Vendedor: solo cartera (adic_comercial principal o secundario).
            int partner_id = Constants.Session.CurrentUserFront.partner_id;

            if (!isMobileAppAdmin && partner_id <= 0)
            {
                Debug.WriteLine("OnlineSyncResPartnerCobranzasAll: partner_id inválido en sesión, se omite sync de clientes.");
                return false;
            }

            // ANTES: hubmanager.GetCountAll() — count de todos los partners.
            // Admin Mobile App (218): GetCountAll sin filtro comercial.
            var resultCount = isMobileAppAdmin
                ? await hubmanager.GetCountAll()
                : await hubmanager.GetCountAllByAdicComercial(partner_id);

            Debug.WriteLine(isMobileAppAdmin
                ? "OnlineSyncResPartnerCobranzasAll count: " + resultCount.result + " (modo admin Mobile App, sin filtro comercial)"
                : "OnlineSyncResPartnerCobranzasAll count: " + resultCount.result + " (partner_id=" + partner_id + ")");

            if (resultCount.result == 0)
            {
                return false;
            }

            int totalPages = (int)Math.Ceiling((double)resultCount.result / pageLimit);
            var downloadedPartnerIds = new List<int>();

            // --- Fase 1: datos maestros del partner (search_read filtrado por comercial) ---
            for (int indice = 0; indice <= totalPages; indice++)
            {
                // ANTES: hubmanager.GetAll(limit, indice) — traía todos los partners.
                // Vendedor: GetAllByAdicComercial. Admin Mobile App: GetAll (todos los partners).
                var responseAll = isMobileAppAdmin
                    ? await hubmanager.GetAll(pageLimit, indice)
                    : await hubmanager.GetAllByAdicComercial(pageLimit, indice, partner_id);

                if (responseAll != null && responseAll.result != null && responseAll.result.Length > 0)
                {
                    await database.InsertBatchFromCobranzasSyncAsync(responseAll.result);
                    downloadedPartnerIds.AddRange(responseAll.result.Select(p => p.id));
                }

                Console.WriteLine("ResPartnerCobranzasAll Fase1 Página:" + indice + " de " + totalPages);

                if (onProgress != null)
                    await onProgress(indice, totalPages);

                if (indice >= maxIndexExceeded)
                {
                    Console.WriteLine("Página " + indice + ": Se terminará el proceso Fase 1.");
                    break;
                }
            }

            // --- Fase 2: saldos correctos vía web_read (revertible) ---
            if (EnableResPartnerCobranzasSaldosWebRead)
            {
                // ANTES: web_read sobre todos los res_partner locales (incluía cartera antigua).
                // DESPUÉS: solo IDs descargados en Fase 1 (cartera del comercial logueado).
                await RefreshResPartnerSaldosWebReadAsync(
                    database,
                    hubmanager,
                    downloadedPartnerIds.Distinct().ToArray(),
                    onSaldosProgress ?? onProgress);
            }

            stopwatch.Stop();

            Debug.WriteLine(String.Format("Lapso transcurrido: {0} days, {1} hours, {2} minutes, {3} seconds",
                stopwatch.Elapsed.Days, stopwatch.Elapsed.Hours, stopwatch.Elapsed.Minutes, stopwatch.Elapsed.Seconds));

            return true;
        }

        // ANTES: paginaba todos los res_partner de SQLite (GetPartnerIdsPageAsync).
        // DESPUÉS: partnerIds = IDs descargados en Fase 1; web_read solo para esa cartera.
        private async Task RefreshResPartnerSaldosWebReadAsync(
            ResPartnerDb database,
            HubResPartner hubmanager,
            int[] partnerIds,
            Func<int, int, Task>? onProgress)
        {
            if (partnerIds == null || partnerIds.Length == 0)
            {
                Debug.WriteLine("RefreshResPartnerSaldosWebRead: sin IDs de partners descargados.");
                return;
            }

            int totalPartners = partnerIds.Length;

            Debug.WriteLine("RefreshResPartnerSaldosWebRead count descargados: " + totalPartners);

            int saldosBatchSize = ResolveCobranzasSyncPageSize();
            int totalSaldosPages = (int)Math.Ceiling((double)totalPartners / saldosBatchSize);

            for (int pageIndex = 0; pageIndex < totalSaldosPages; pageIndex++)
            {
                var ids = partnerIds
                    .Skip(pageIndex * saldosBatchSize)
                    .Take(saldosBatchSize)
                    .ToArray();

                if (ids.Length == 0)
                {
                    continue;
                }

                var responseSaldos = await hubmanager.WebReadSaldos(ids);

                if (responseSaldos != null && responseSaldos.result != null && responseSaldos.result.Length > 0)
                {
                    await database.UpdateSaldosBatchAsync(responseSaldos.result);
                }

                Console.WriteLine("ResPartnerCobranzasAll Fase2 Saldos:" + (pageIndex + 1) + " de " + totalSaldosPages);

                if (onProgress != null)
                    await onProgress(pageIndex + 1, totalSaldosPages);

                if (pageIndex >= maxIndexExceeded)
                {
                    Console.WriteLine("Página saldos " + pageIndex + ": Se terminará el proceso Fase 2.");
                    break;
                }
            }
        }
    }
}
