using ApiManager;
using ApiManagerOdoo.Accounting;
using CommunityToolkit.Maui.Core;
using DMSA.Models.Odoo.Accounting;
using DMSA.Models.Odoo.DebitCollection;
using DMSA.Models.Odoo.Tools;
using DMSA.Sync.Core.Controls;
using DMSA.Sync.Core.Database.Sqlite;
using DMSA.Sync.Core.Database.Sqlite.Accounting;
using DMSA.Sync.Core.Database.Sqlite.Benefits;
using DMSA.Sync.Core.Database.Sqlite.DebitCollection;
using DMSA.Sync.Core.Database.Sqlite.Payments;
using DMSA.Sync.Core.Sys;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Diagnostics;
using System.Linq;

namespace DMSA.Sync.Core.Update
{
    public partial class ServerPuller
    {
        public async Task<bool> OnlineCreditNotesRelated(Func<int, int, Task>? onProgress = null)
        {
            await TypeNcData(onProgress);
            await TypeParentNcData(onProgress);
            await AccountAccountData(onProgress);
            await GetAccountAccount(onProgress);
            return true;
        }

        public async Task<bool> GetAccountAccount(Func<int, int, Task>? onProgress = null)
        {
            var stopwatch = Stopwatch.StartNew();
            
            var database = new AccountAccountDb(DbNameSqlite);
            DateTime? lastDate = await database.GetLastWriteDateAsync(sync_date_since);

            var hubmanager = new ApiManager.HubAccountAccount(Constants.Session);
            string names =  "ANTICIPO" ;
            var resultCount = await hubmanager.GetCountByNames(names);

            if (resultCount.result == 0)
            {
                return false;
            }

            int totalPages = (int)Math.Ceiling((double)resultCount.result / limit);

            for (int indice = 0; indice <= totalPages; indice++)
            {
                Debug.WriteLine("GetAccountAccount Página:" + indice);

                var responseAll = await hubmanager.GetItemsByNames(names);

                if (responseAll.result != null && responseAll.result.Length > 0)
                {
                    await database.InsertBatchAsync(responseAll.result);
                }

                if (onProgress != null)
                    await onProgress(indice + 1, totalPages);

                if (indice >= maxIndexExceeded)
                {
                    Debug.WriteLine("Página " + indice + ": Se terminará el proceso.");
                    break;
                }
            }

            stopwatch.Stop();

            Debug.WriteLine(String.Format("Lapso transcurrido: {0} days, {1} hours, {2} minutes, {3} seconds",
                stopwatch.Elapsed.Days, stopwatch.Elapsed.Hours, stopwatch.Elapsed.Minutes, stopwatch.Elapsed.Seconds));

            return true;
        }

        public async Task<bool> AccountAccountData(Func<int, int, Task>? onProgress = null)
        {
            var stopwatch = Stopwatch.StartNew();

            var databaseTypeNc = new TypeNcDb(DbNameSqlite);
            var database = new AccountAccountDb(DbNameSqlite);
            DateTime? lastDate = await database.GetLastWriteDateAsync(sync_date_since);

            int[] account_ids = await databaseTypeNc.GetAllAccountIds();

            var hubmanager = new ApiManager.HubAccountAccount(Constants.Session);
            var resultCount = await hubmanager.GetCountByIds(account_ids);

            if (resultCount.result == 0)
            {
                return false;
            }

            int totalPages = (int)Math.Ceiling((double)resultCount.result / limit);

            for (int indice = 0; indice <= totalPages; indice++)
            {
                Debug.WriteLine("AccountAccountData Página:" + indice);

                var responseAll = await hubmanager.GetItemsById(account_ids);

                if (responseAll.result != null && responseAll.result.Length > 0)
                {
                    await database.InsertBatchAsync(responseAll.result);
                }

                if (onProgress != null)
                    await onProgress(indice + 1, totalPages);

                if (indice >= maxIndexExceeded)
                {
                    Debug.WriteLine("Página " + indice + ": Se terminará el proceso.");
                    break;
                }
            }

            stopwatch.Stop();

            Debug.WriteLine(String.Format("Lapso transcurrido: {0} days, {1} hours, {2} minutes, {3} seconds",
                stopwatch.Elapsed.Days, stopwatch.Elapsed.Hours, stopwatch.Elapsed.Minutes, stopwatch.Elapsed.Seconds));

            return true;
        }

        public async Task<bool> TypeParentNcData(Func<int, int, Task>? onProgress = null)
        {
            var stopwatch = Stopwatch.StartNew();

            var database = new TypeParentNcDb(DbNameSqlite);
            DateTime? lastDate = await database.GetLastWriteDateAsync(sync_date_since);

            ApiManager.HubTypeParentNc hubmanager = new ApiManager.HubTypeParentNc(Constants.Session);
            var resultCount = await hubmanager.GetCount(lastDate.Value);

            if (resultCount.result == 0)
            {
                return false;
            }

            int totalPages = (int)Math.Ceiling((double)resultCount.result / limit);

            for (int indice = 0; indice <= totalPages; indice++)
            {
                Debug.WriteLine("TypeParentNcData Página:" + indice);

                var responseAll = await hubmanager.GetItems(lastDate.Value, limit, indice);

                if (responseAll.result != null && responseAll.result.Length > 0)
                {
                    await database.InsertBatchAsync(responseAll.result);
                }

                if (onProgress != null)
                    await onProgress(indice + 1, totalPages);

                if (indice >= maxIndexExceeded)
                {
                    Debug.WriteLine("Página " + indice + ": Se terminará el proceso.");
                    break;
                }
            }

            stopwatch.Stop();

            Debug.WriteLine(String.Format("Lapso transcurrido: {0} days, {1} hours, {2} minutes, {3} seconds",
                stopwatch.Elapsed.Days, stopwatch.Elapsed.Hours, stopwatch.Elapsed.Minutes, stopwatch.Elapsed.Seconds));

            return true;
        }

        public async Task<bool> TypeNcData(Func<int, int, Task>? onProgress = null)
        {
            var stopwatch = Stopwatch.StartNew();

            var database = new TypeNcDb(DbNameSqlite);
            DateTime? lastDate = await database.GetLastWriteDateAsync(sync_date_since);

            HubTypeNc hubmanager = new HubTypeNc(Constants.Session);
            var resultCount = await hubmanager.GetCount(lastDate.Value);

            if (resultCount.result == 0)
            {
                return false;
            }

            int totalPages = (int)Math.Ceiling((double)resultCount.result / limit);

            for (int indice = 0; indice <= totalPages; indice++)
            {
                Debug.WriteLine("TypeNcData Página:" + indice);

                var responseAll = await hubmanager.GetItems(lastDate.Value, limit, indice);

                if (responseAll.result != null && responseAll.result.Length > 0)
                {
                    await database.InsertBatchAsync(responseAll.result);
                }

                if (onProgress != null)
                    await onProgress(indice + 1, totalPages);

                if (indice >= maxIndexExceeded)
                {
                    Debug.WriteLine("Página " + indice + ": Se terminará el proceso.");
                    break;
                }
            }

            stopwatch.Stop();

            Debug.WriteLine(String.Format("Lapso transcurrido: {0} days, {1} hours, {2} minutes, {3} seconds",
                stopwatch.Elapsed.Days, stopwatch.Elapsed.Hours, stopwatch.Elapsed.Minutes, stopwatch.Elapsed.Seconds));

            return true;
        }

        public async Task<bool> OnlineAccountTaxes(Func<int, int, Task>? onProgress = null)
        {
            var stopwatch = Stopwatch.StartNew();

            var productProductDb = new ProductProductDb(DbNameSqlite);

            var idstaxes = await productProductDb.GetAllTaxesIdsAsync();
            
            //string idsTaxesStr = string.Join(",", idstaxes.Distinct());

            ApiManager.HubAccountTax hubmanager = new ApiManager.HubAccountTax(Constants.Session);
            var resultCount = await hubmanager.GetCount(idstaxes);

            if (resultCount.result == 0)
            {
                return false;
            }

            int totalPages = (int)Math.Ceiling((double)resultCount.result / limit);

            var database = new AccountTaxDb(DbNameSqlite);

            for (int indice = 0; indice <= totalPages; indice++)
            {
                Debug.WriteLine("Página:" + indice);

                var responseAll = await hubmanager.GetAll(idstaxes);

                if (responseAll.result != null && responseAll.result.Length > 0)
                {
                    await database.InsertBatchAsync(responseAll.result);
                }

                if (onProgress != null)
                    await onProgress(indice + 1, totalPages);

                if (indice >= maxIndexExceeded)
                {
                    Debug.WriteLine("Página " + indice + ": Se terminará el proceso.");
                    break;
                }
            }

            stopwatch.Stop();

            Debug.WriteLine(String.Format("Lapso transcurrido: {0} days, {1} hours, {2} minutes, {3} seconds",
                stopwatch.Elapsed.Days, stopwatch.Elapsed.Hours, stopwatch.Elapsed.Minutes, stopwatch.Elapsed.Seconds));

            return true;
        }

        public async Task<bool> OnlineCalificacionCrediticia(Func<int, int, Task>? onProgress = null)
        {
            var stopwatch = Stopwatch.StartNew();

            ApiManager.HubCalificacionCrediticia hubmanager = new ApiManager.HubCalificacionCrediticia(Constants.Session);
            var resultCount = await hubmanager.GetCount();

            if (resultCount.result == 0)
            {
                return false;
            }

            int totalPages = (int)Math.Ceiling((double)resultCount.result / limit);

            var database = new CalificacionCrediticiaDb(DbNameSqlite);

            for (int indice = 0; indice <= totalPages; indice++)
            {
                Debug.WriteLine("Página:" + indice + " de " + totalPages);

                var responseAll = await hubmanager.GetByCreateDate(limit, indice, year, month, day);

                if (responseAll.result != null && responseAll.result.Length > 0)
                {
                    await database.InsertBatchAsync(responseAll.result);
                }

                if (indice >= maxIndexExceeded)
                {
                    Debug.WriteLine("Página " + indice + ": Se terminará el proceso.");
                    break;
                }
            }

            stopwatch.Stop();

            Debug.WriteLine(String.Format("Lapso transcurrido: {0} days, {1} hours, {2} minutes, {3} seconds",
                stopwatch.Elapsed.Days, stopwatch.Elapsed.Hours, stopwatch.Elapsed.Minutes, stopwatch.Elapsed.Seconds));

            return true;
        }

        //public async Task OnlineSyncAccountModule()
        //{
        //    ApiManager.HubAccountModule hubAccountModule = new HubAccountModule(Constants.Session);
        //    var dataList = await hubAccountModule.GetAll();

        //    if (dataList != null && dataList.result != null && dataList.result.Length > 0)
        //    {
        //        var database = new AccountModuleDb(Constants.Session.odooConnection.DbNameSqlite);
        //        await database.InsertBatchAsync(dataList.result);
        //    }
        //}

        //public async Task OnlineSyncAccountTypeModule()
        //{
        //    ApiManager.HubAccountTypeModule hubAccountModule = new HubAccountTypeModule(Constants.Session);
        //    var dataList = await hubAccountModule.GetAll();

        //    if (dataList != null && dataList.result != null && dataList.result.Length > 0)
        //    {
        //        var database = new AccountTypeModuleDb(Constants.Session.odooConnection.DbNameSqlite);

        //        foreach (var user in dataList.result)
        //        {
        //            if (user.module_id != null && user.module_id.Length > 0)
        //            {
        //                user._module_id = user.module_id[0].id;
        //            }
        //        }
        //        await database.InsertBatchAsync(dataList.result);
        //    }
        //}

        [UpdateAction(
            "Actualizar documentos (facturas, notas de débito)",
            "Descarga cabeceras de facturas y notas de débito.")]
        public async Task<bool> OnlineSyncAccountMove(
            Func<int, int, Task>? onProgress = null,
            DateTime? invoiceDateFrom = null,
            DateTime? invoiceDateTo = null)
        {
            DateTime dateTimeIni = DateTime.Now;

            var database = new AccountMoveDb(Constants.Session.odooConnection.DbNameSqlite);
            HubAccountMove hubmanager = new HubAccountMove(Constants.Session);

            bool useInvoiceDateRange = EnableInvoiceDateRangeSync
                && invoiceDateFrom.HasValue
                && invoiceDateTo.HasValue;

            if (useInvoiceDateRange)
            {
                var dateFrom = invoiceDateFrom!.Value.Date;
                var dateTo = invoiceDateTo!.Value.Date;

                // ANTES: count/search_read solo por rango invoice_date (todos los partners).
                // DESPUÉS: filtra partner_id IN ids de res_partner local (cartera del comercial).
                var partnerDb = new ResPartnerDb(Constants.Session.odooConnection.DbNameSqlite);
                int commercialPartnerId = Constants.Session.CurrentUserFront.partner_id;
                int[] partnerIds = await partnerDb.GetAllPartnerIdsByAdicComercialAsync(commercialPartnerId);

                if (partnerIds.Length == 0)
                {
                    Debug.WriteLine("AccountMove sync: sin partners del comercial en res_partner local, se omite rango invoice_date.");
                    return false;
                }

                var headerCount = await hubmanager.GetHeaderCountByInvoiceDateRange(dateFrom, dateTo, partnerIds);
                Debug.WriteLine(headerCount?.result);
                Debug.WriteLine($"AccountMove sync modo: rango invoice_date {dateFrom:yyyy-MM-dd} .. {dateTo:yyyy-MM-dd}, partners={partnerIds.Length}");

                if (headerCount == null || headerCount.result == 0)
                    return false;

                int headerPages = (int)Math.Ceiling((double)headerCount.result / limit);

                for (int indice = 0; indice <= headerPages; indice++)
                {
                    var responseAll = await hubmanager.GetAccountMovesByInvoiceDateRange(dateFrom, dateTo, limit, indice, partnerIds);

                    if (responseAll?.result != null && responseAll.result.Length > 0)
                    {
                        responseAll.result = FixAccountMove(responseAll.result);
                        await database.InsertBatchAsync(responseAll.result);
                    }

                    Debug.WriteLine("AccountMove Página:" + indice + "/" + headerPages);

                    if (onProgress != null)
                        await onProgress(indice + 1, headerPages);

                    if (indice >= maxIndexExceeded)
                    {
                        Debug.WriteLine("Página " + indice + ": Se terminará el proceso.");
                        break;
                    }
                }

                TimeSpan spanRange = (DateTime.Now - dateTimeIni);
                Debug.WriteLine(String.Format("Lapso transcurrido: {0} days, {1} hours, {2} minutes, {3} seconds",
                    spanRange.Days, spanRange.Hours, spanRange.Minutes, spanRange.Seconds));

                database.MarkAccountMoveSyncCompletedToday();
                return true;
            }

            bool firstSyncOfDay = database.IsFirstAccountMoveSyncOfDay();
            _accountDocumentFirstSyncOfDay = firstSyncOfDay;

            DateTime lastDate = firstSyncOfDay
                ? DateTime.Now.Date
                : await database.GetLastWriteDateAsync(sync_date_since_lower);

            var resultCount = await hubmanager.GetHeaderCount(
                lastDate.Year,
                lastDate.Month,
                lastDate.Day,
                firstSyncOfDay);

            Debug.WriteLine(resultCount.result);
            Debug.WriteLine($"AccountMove sync modo: {(firstSyncOfDay ? "write_date <= (1ra del día)" : "write_date >= (incremental)")}");

            if (resultCount.result == 0)
            {
                database.MarkAccountMoveSyncCompletedToday();
                return false;
            }

            //int limit = Constants.Session.odooConnection.DbLimitDefault;
            int totalPages = (int)Math.Ceiling((double)resultCount.result / limit);

            for (int indice = 0; indice <= totalPages; indice++)
            {
                var responseAll = await hubmanager.GetAccountMoves(lastDate, limit, indice, firstSyncOfDay);

                if (responseAll.result != null && responseAll.result.Length > 0)
                {
                    responseAll.result = FixAccountMove(responseAll.result);
                    await database.InsertBatchAsync(responseAll.result);
                }

                Debug.WriteLine("AccountMove Página:" + indice + "/" + totalPages.ToString());

                if (onProgress != null)
                    await onProgress(indice + 1, totalPages);

                if (indice >= maxIndexExceeded)
                {
                    Debug.WriteLine("Página " + indice + ": Se terminará el proceso.");
                    break;
                }
            }

            TimeSpan span = (DateTime.Now - dateTimeIni);

            Debug.WriteLine(String.Format("Lapso transcurrido: {0} days, {1} hours, {2} minutes, {3} seconds",
                span.Days, span.Hours, span.Minutes, span.Seconds));

            database.MarkAccountMoveSyncCompletedToday();

            return true;
        }


        private account_move[] FixAccountMove(account_move[] account_Moves)
        {
            foreach (var amItem in account_Moves)
            {
                //if (amItem.reversed_entry_id != null && amItem.reversed_entry_id.Length > 0)
                //{
                //    amItem._reversed_entry_id = amItem.reversed_entry_id[0].id;
                //}

                //if (amItem.partner_id != null && amItem.partner_id.Length > 0)
                //{
                //    amItem._partner_id = amItem.partner_id[0].id;
                //}

                //if (amItem.journal_id != null && amItem.journal_id.Length > 0)
                //{
                //    amItem._journal_id = amItem.journal_id[0].id;
                //}

                //if (amItem.l10n_latam_document_type_id != null && amItem.l10n_latam_document_type_id.Length > 0)
                //{
                //    amItem._l10n_latam_document_type_id = amItem.l10n_latam_document_type_id[0].id;
                //}

                //if (amItem.invoice_user_id != null && amItem.invoice_user_id.Length > 0)
                //{
                //    amItem._invoice_user_id = amItem.invoice_user_id[0].id;
                //}

                //if (amItem.printer_id != null && amItem.printer_id.Length > 0)
                //{
                //    amItem._printer_id = amItem.printer_id[0].id;
                //}

                //if (amItem.printer_id != null && amItem.printer_id.Length > 0)
                //{
                //    amItem._printer_id = amItem.printer_id[0].id;
                //}

                //if (amItem.company_id != null && amItem.company_id.Length > 0)
                //{
                //    amItem._company_id = amItem.company_id[0].id;
                //}

                //if (amItem.team_id != null && amItem.team_id.Length > 0)
                //{
                //    amItem._team_id = amItem.team_id[0].id;
                //}
            }

            return account_Moves;
        }
        public async Task OnlineSyncBank(Func<int, int, Task>? onProgress = null)
        {
            BankDb bankDb = new BankDb(Constants.Session.odooConnection.DbNameSqlite);
            await bankDb.Truncate();

            List<string> bank_ids_list = new List<string>();
            HubResPartnerBank hubCuentas = new HubResPartnerBank(Constants.Session);            
            var cuentasDeLista = await hubCuentas.GetAll();

            if (cuentasDeLista != null && cuentasDeLista.result != null && cuentasDeLista.result.Length > 0)
            {
                foreach (var pbItem in cuentasDeLista.result)
                {
                    pbItem._bank_id = 0;
                    pbItem._partner_id = 0;
                    bank_ids_list.Add(pbItem._bank_id.ToString());
                    
                    if (pbItem.acc_holder_name.Trim().Equals("false"))
                    {
                        pbItem.acc_holder_name = "-";
                    }
                }

                PartnerBankDb parnetBankDb = new PartnerBankDb(Constants.Session.odooConnection.DbNameSqlite);
                await parnetBankDb.InsertBatchAsync(cuentasDeLista.result);
            }

            HubBank hubBancos = new HubBank(Constants.Session);            
            DateTime dateTimeIni = DateTime.Now;

            var resultCount = await hubBancos.GetCount(dateTimeIni);

            Debug.WriteLine(resultCount.result);

            if (resultCount.result == 0)
            {
                return;
            }

            int limit = Constants.Session.odooConnection.DbLimitDefault;
            int countTotal = resultCount.result / limit;

            for (int indice = 0; indice <= countTotal; indice++)
            {
                var bancosDeLista = await hubBancos.GetAll(dateTimeIni, limit, indice);

                if (bancosDeLista != null && bancosDeLista.result != null && bancosDeLista.result.Length > 0)
                {
                    await bankDb.InsertBatchAsync(bancosDeLista.result);
                }
            }
        }

        public async Task<bool> OnlineSyncJournal(Func<int, int, Task>? onProgress = null)
        {
            var stopwatch = Stopwatch.StartNew();

            var database = new AccountJournalDb(Constants.Session.odooConnection.DbNameSqlite);

            DateTime? lastDate = await database.GetLastWriteDateAsync(sync_date_since);

            if (lastDate.HasValue)
            {
                lastDate = lastDate.Value.AddMonths(-2);
            }

            HubAccountJournal hubmanager = new HubAccountJournal(appSession);
            var resultCount = await hubmanager.GetCount(lastDate.Value.Year, lastDate.Value.Month, lastDate.Value.Day);

            Debug.WriteLine(resultCount.result);

            if (resultCount.result == 0)
            {
                return false;
            }

            int totalPages = (int)Math.Ceiling((double)resultCount.result / limit);

            for (int indice = 0; indice <= totalPages; indice++)
            {
                var responseAll = await hubmanager.GetItems(lastDate.Value, limit, indice);

                if (responseAll != null && responseAll.result != null && responseAll.result.Length > 0)
                {
                    await database.InsertBatchAsync(responseAll.result);
                    //await database.InsertBatchControlAsync(responseAll.result);
                }

                Console.WriteLine("Journal Página:" + indice + " de " + totalPages);

                if (onProgress != null)
                    await onProgress(indice + 1, totalPages);

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

        [UpdateAction(
            "Actualizar detalles de documentos (facturas, notas de débito)",
            "Descarga líneas de facturas y notas de débito.")]
        public async Task<bool> OnlineSyncAccountMoveLine(
            Func<int, int, Task>? onProgress = null,
            DateTime? invoiceDateFrom = null,
            DateTime? invoiceDateTo = null)
        {
            DateTime dateTimeIni = DateTime.Now;
            var databaseDet = new AccountMoveLineDb(Constants.Session.odooConnection.DbNameSqlite);

            Debug.WriteLine("Iniciando proceso:" + " " + DateTime.Now.ToString());

            HubAccountMoveLine hubmanager = new HubAccountMoveLine(Constants.Session);

            bool useInvoiceDateRange = EnableInvoiceDateRangeSync
                && invoiceDateFrom.HasValue
                && invoiceDateTo.HasValue;

            if (useInvoiceDateRange)
            {
                var dateFrom = invoiceDateFrom!.Value.Date;
                var dateTo = invoiceDateTo!.Value.Date;

                // ANTES: count/search_read solo por move_id.invoice_date (sin acotar a cabeceras locales).
                // DESPUÉS: move_id IN ids de account_move local (mismo rango Desde/Hasta) + filtro invoice_date.
                var moveDb = new AccountMoveDb(Constants.Session.odooConnection.DbNameSqlite);
                int[] moveIds = await moveDb.GetIdsByInvoiceDateRangeAsync(dateFrom, dateTo);

                if (moveIds.Length == 0)
                {
                    Debug.WriteLine("AccountMoveLine sync: sin account_move local en rango, se omite detalle.");
                    return false;
                }

                var lineCount = await hubmanager.GetDetailCountByInvoiceDateRange(dateFrom, dateTo, moveIds);
                Debug.WriteLine(lineCount?.result);
                Debug.WriteLine($"AccountMoveLine sync modo: rango invoice_date {dateFrom:yyyy-MM-dd} .. {dateTo:yyyy-MM-dd}, moves={moveIds.Length}");

                if (lineCount == null || lineCount.result == 0)
                    return false;

                int linePages = (int)Math.Ceiling((double)lineCount.result / limit);

                for (int indice = 0; indice <= linePages; indice++)
                {
                    var responseAll = await hubmanager.GetAccountMoveLinesByInvoiceDateRange(dateFrom, dateTo, limit, indice, moveIds);

                    if (responseAll?.result != null && responseAll.result.Length > 0)
                        await databaseDet.InsertBatchAsync(responseAll.result);

                    Debug.WriteLine("AccountMoveLine Página:" + indice + "/" + linePages);

                    if (onProgress != null)
                        await onProgress(indice + 1, linePages);
                }

                TimeSpan spanRange = (DateTime.Now - dateTimeIni);
                Debug.WriteLine(String.Format("Lapso transcurrido: {0} days, {1} hours, {2} minutes, {3} seconds",
                    spanRange.Days, spanRange.Hours, spanRange.Minutes, spanRange.Seconds));

                await SyncMissingPartnerSaleIdsFromAccountMoveAsync(dateFrom, dateTo);
                return true;
            }

            var syncStateDb = new AccountMoveDb(Constants.Session.odooConnection.DbNameSqlite);
            bool firstSyncOfDay = _accountDocumentFirstSyncOfDay ?? syncStateDb.IsFirstAccountMoveSyncOfDay();

            DateTime lastDate = firstSyncOfDay
                ? DateTime.Now.Date
                : await databaseDet.GetLastWriteDateAsync(sync_date_since_lower);

            var resultCount = await hubmanager.GetDetailCount(lastDate, firstSyncOfDay);

            Debug.WriteLine(resultCount.result);
            Debug.WriteLine($"AccountMoveLine sync modo: {(firstSyncOfDay ? "write_date <= (1ra del día)" : "write_date >= (incremental)")}");

            if (resultCount.result == 0)
            {
                return false;
            }
                        
            int totalPages = (int)Math.Ceiling((double)resultCount.result / limit);

            for (int indice = 0; indice <= totalPages; indice++)
            {
                var responseAll = await hubmanager.GetAccountMoveLines(lastDate, limit, indice, firstSyncOfDay);

                if (responseAll.result != null && responseAll.result.Length > 0)
                {
                    await databaseDet.InsertBatchAsync(responseAll.result);
                }

                Debug.WriteLine("AccountMoveLine Página:" + indice + "/" + totalPages);

                if (onProgress != null)
                    await onProgress(indice + 1, totalPages);
            }

            TimeSpan span = (DateTime.Now - dateTimeIni);

            Debug.WriteLine(String.Format("Lapso transcurrido: {0} days, {1} hours, {2} minutes, {3} seconds",
                span.Days, span.Hours, span.Minutes, span.Seconds));

            await SyncMissingPartnerSaleIdsFromAccountMoveAsync(null, null);
            return true;
        }

        // Tras sync de detalle: partner_sale_id distintos en account_move → res.partner faltantes en local.
        private async Task SyncMissingPartnerSaleIdsFromAccountMoveAsync(
            DateTime? invoiceDateFrom,
            DateTime? invoiceDateTo)
        {
            var moveDb = new AccountMoveDb(Constants.Session.odooConnection.DbNameSqlite);
            var partnerDb = new ResPartnerDb(Constants.Session.odooConnection.DbNameSqlite);
            var hubPartner = new HubResPartner(appSession);

            int[] saleIds = await moveDb.GetDistinctPartnerSaleIdsAsync(invoiceDateFrom, invoiceDateTo);
            if (saleIds.Length == 0)
            {
                Debug.WriteLine("SyncPartnerSaleIds: sin partner_sale_id en account_move.");
                return;
            }

            int[] missingIds = await partnerDb.FilterMissingPartnerIdsAsync(saleIds);
            if (missingIds.Length == 0)
            {
                Debug.WriteLine("SyncPartnerSaleIds: todos los partner_sale_id ya existen en res_partner.");
                return;
            }

            Debug.WriteLine($"SyncPartnerSaleIds: descargando {missingIds.Length} de {saleIds.Length} partners...");

            int batchSize = Math.Max(limit, 50);
            for (int offset = 0; offset < missingIds.Length; offset += batchSize)
            {
                var batch = missingIds.Skip(offset).Take(batchSize).ToArray();
                var response = await hubPartner.GetByIds(batchSize, 0, batch);

                if (response?.result != null && response.result.Length > 0)
                    await partnerDb.InsertBatchAsync(response.result);
            }
        }

        //public void set_to_token(JToken token, int value)
        //{
        //    if (token is JArray array && array.Count > 0)
        //    {
        //        array[0] = value;
        //    }
        //    else if (token is JValue)
        //    {
        //        token = new JArray { value };
        //    }
        //    else
        //    {
        //        token = new JArray { value };
        //    }
        //}

        //public int get_from_token(JToken token)
        //{
        //    if (token is JArray array && array.Count > 0)
        //    {
        //        return array[0].Type == JTokenType.Integer ? (int)array[0] : 0;
        //    }

        //    else if (token is JValue value && value.Type == JTokenType.Boolean)
        //    {
        //        return 0;
        //    }
        //    return 0;
        //}

        public async Task<bool> OnlineSyncAccountPaymentDaily(Func<int, int, Task>? onProgress = null)
        {            
            DateTime dateTimeIni = DateTime.Now;

            var database = new AccountPaymentDailyDb(Constants.Session.odooConnection.DbNameSqlite);
            DateTime? lastDate = await database.GetLastWriteDateAsync(sync_date_since_lower);


            var hubmanager = new ApiManager.HubAccountPaymentDaily(Constants.Session);
            var resultCount = await hubmanager.GetCountByUser(Constants.Session.CurrentUserFront.uid, lastDate.Value);

            Debug.WriteLine(resultCount.result);

            if (resultCount.result == 0)
            {
                return false;
            }
                        
            int totalPages = (int)Math.Ceiling((double)resultCount.result / limit);                       

            for (int indice = 0; indice <= totalPages; indice++)
            {
                var responseAll = await hubmanager.GetByUser(Constants.Session.CurrentUserFront.uid, lastDate.Value);

                if (responseAll != null && responseAll.result != null && responseAll.result.Length > 0)
                {
                    foreach (var itemPaymentDaily in responseAll.result)
                    {
                        var foundItem = await database.GetItemAsync(itemPaymentDaily.company_id, itemPaymentDaily.closing_id);
                        if (foundItem != null)
                        {
                            continue;
                        }
                        itemPaymentDaily.was_odoo_synced = true;
                        await database.InsertAsync(itemPaymentDaily);
                    }
                }

                if (onProgress != null)
                    await onProgress(indice + 1, totalPages);

                Console.WriteLine("AccountPaymentDaily Página:" + indice);

                if (indice >= maxIndexExceeded)
                {
                    Console.WriteLine("Página " + indice + ": Se terminará el proceso.");
                    break;
                }
            }

            TimeSpan span = (DateTime.Now - dateTimeIni);

            Console.WriteLine(String.Format("Lapso transcurrido: {0} days, {1} hours, {2} minutes, {3} seconds",
                span.Days, span.Hours, span.Minutes, span.Seconds));

            return true;
        }

        public async Task OnlineSyncFacturas(Func<int, int, Task>? onProgress = null)
        {            
            await OnlineSyncAccountMove(onProgress);
            await OnlineSyncAccountMoveLine(onProgress);
            await OnlineSyncUsers();
            Debug.WriteLine("Importación en linea account.move terminada");
        }

        public async Task OnlineSyncUsers()
        {
            var database = new UserDb(Constants.Session.odooConnection.DbNameSqlite);
            ApiManager.HubUser hubProductBrand = new HubUser(Constants.Session);
            DateTime? lastDate = await database.GetLastWriteDateAsync(sync_date_since_lower);
            var dataList = await hubProductBrand.GetItems();

            if (dataList != null && dataList.result != null && dataList.result.Length > 0)
            {
                
                //foreach (var user in dataList.result)
                //{
                    //if (user.company_id != null && user.company_id.Length > 0)
                    //{
                    //    user._company_id = user.company_id[0].id;
                    //}

                    //if (user.partner_id != null && user.partner_id.Length > 0)
                    //{
                    //    user._partner_id = user.partner_id[0].id;
                    //    user.partner_name = user.partner_id[0].name;
                    //}

                    //if (user.sale_team_id != null && user.sale_team_id.Length > 0)
                    //{
                    //    user._sale_team_id = user.sale_team_id[0].id;
                    //}
                //}

                await database.InsertBatchAsync(dataList.result);
            }
        }

        public async Task<bool> OnlineSyncPaymentHeader(Func<int, int, Task>? onProgress = null)
        {
            int uid = Constants.Session.CurrentUserFront.uid;

            JsonSerializerSettings settings = new JsonSerializerSettings();            
            settings.ContractResolver = new IncludeJsonIgnoreResolver();

            DateTime dateIni = DateTime.Now.AddDays(-150);
            DateTime dateEnd = DateTime.Now;

            HubMultipleCobrosInvoice hubmanager = new HubMultipleCobrosInvoice(Constants.Session);
            var resultCount = await hubmanager.GetCount(uid, dateIni);

            Debug.WriteLine(resultCount.result);

            if (resultCount.result == 0)
            {
                return false;
            }

            int totalPages = (int)Math.Ceiling((double)resultCount.result / limit);

            var database = new MultipleCobrosInvoiceDb(Constants.Session.odooConnection.DbNameSqlite);
            var databasePay = new MultipleCobrosInvoiceLineDb(Constants.Session.odooConnection.DbNameSqlite);
            var databaseInvoLine = new MultipleCobrosInvoiceLineAiDb(Constants.Session.odooConnection.DbNameSqlite);

            for (int indice = 0; indice <= totalPages; indice++)
            {                
                var responseAll = await hubmanager.GetItemsFull(uid, dateIni, limit, indice);

                if (responseAll.result != null && responseAll.result.Length > 0)
                {                    
                    foreach (var headerItem in responseAll.result)
                    {
                        if (string.IsNullOrWhiteSpace(headerItem.external_guid))
                            continue;

                        var foundHeader = await database.GetItemAsync(x => x.external_guid == headerItem.external_guid);
                        if (foundHeader != null)
                        {
                            Debug.WriteLine("Registro ya existe en la base de datos!");
                            Debug.WriteLine(foundHeader.external_guid);
                            Debug.WriteLine(foundHeader.receipt_name);

                            if (ApplyOdooPaymentHeaderState(foundHeader, headerItem))
                                await database.UpdateAsync(foundHeader);

                            continue;
                        }

                        //OMITIDO TEMPORALMENTE
                        //INSERSION DE REGISTRO COMPLETO (HEADER, LINES Y LINEAS DE DETALLE)
                        //////string jsonHeaderItem = JsonConvert.SerializeObject(headerItem);
                        //////var newHeaderItem = JsonConvert.DeserializeObject<MultipleCobrosInvoice>(jsonHeaderItem);

                        //////var itemFound = await database.GetItemAsync(x => x.external_guid == headerItem.external_guid);

                        //////if (itemFound != null) continue;

                        //////newHeaderItem.was_odoo_synced = true;
                        //////int newHeaderId = await database.InsertAsync(newHeaderItem);

                        //////if (headerItem.lines != null)
                        //////{
                        //////    foreach (var paymentItem in headerItem.lines)
                        //////    {
                        //////        paymentItem.MultipleCobrosInvoiceId = newHeaderItem.id;

                        //////        string jsonPaymentItem = JsonConvert.SerializeObject(paymentItem, settings);
                        //////        var newPaymentItem = JsonConvert.DeserializeObject<MultipleCobrosInvoiceLine>(jsonPaymentItem, settings);

                        //////        int newPayId = await databasePay.InsertAsync(newPaymentItem);

                        //////        if (paymentItem.lines != null)
                        //////        {
                        //////            foreach (var lineItem in paymentItem.lines)
                        //////            {
                        //////                lineItem.multiple_cobros_invoice_line_id = newPaymentItem.Id;
                        //////                string jsonLineItem = JsonConvert.SerializeObject(lineItem, settings);
                        //////                var newLineItem = JsonConvert.DeserializeObject<MultipleCobrosInvoiceLineAi>(jsonLineItem, settings);
                        //////                await databaseInvoLine.InsertAsync(newLineItem);
                        //////            }
                        //////        }
                        //////    }
                        //////}
                    }

                    if (onProgress != null)
                        await onProgress(indice + 1, totalPages);
                }

                Console.WriteLine("OnlineSyncPaymentHeader Página:" + indice);

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

        private static bool ApplyOdooPaymentHeaderState(MultipleCobrosInvoice local, MultipleCobrosInvoice fromOdoo)
        {
            bool changed = false;

            if (!string.Equals(local.state, fromOdoo.state, StringComparison.Ordinal))
            {
                local.state = fromOdoo.state;
                changed = true;
            }

            if (!string.Equals(local.state_applied, fromOdoo.state_applied, StringComparison.Ordinal))
            {
                local.state_applied = fromOdoo.state_applied;
                changed = true;
            }

            string? mappedPaymentStatus = MapPaymentStatusFromOdooState(fromOdoo.state);
            if (mappedPaymentStatus != null &&
                !string.Equals(local.payment_status, mappedPaymentStatus, StringComparison.Ordinal))
            {
                local.payment_status = mappedPaymentStatus;
                changed = true;
            }

            return changed;
        }

        private static string? MapPaymentStatusFromOdooState(string? odooState)
        {
            return odooState switch
            {
                "cancel" => CobrosEstados.CANCELADO,
                "done" => CobrosEstados.APLICADO,
                _ => null
            };
        }

    }
}
