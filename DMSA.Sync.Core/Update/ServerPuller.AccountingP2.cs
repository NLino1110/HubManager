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

namespace DMSA.Sync.Core.Update
{
    public partial class ServerPuller
    {
        [UpdateAction("Actualizar Facturas Por Cliente")]
        public async Task<bool> OnlineSyncAccountMoveByResPartner(int res_partner, Func<int, int, Task>? onProgress = null)
        {
            DateTime dateTimeIni = DateTime.Now;

            var database = new AccountMoveDb(Constants.Session.odooConnection.DbNameSqlite);

            HubAccountMove hubmanager = new HubAccountMove(Constants.Session);
            var resultCount = await hubmanager.GetCountByResPartner(res_partner);

            Debug.WriteLine(resultCount.result);

            if (resultCount.result == 0)
            {
                return false;
            }

            //int limit = Constants.Session.odooConnection.DbLimitDefault;
            int totalPages = (int)Math.Ceiling((double)resultCount.result / limit);

            for (int indice = 0; indice <= totalPages; indice++)
            {
                var responseAll = await hubmanager.GetAccountMovesByResPartner(res_partner, limit, indice);

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

            return true;
        }

        [UpdateAction("Actualizar Detalles de Facturas por Cliente")]
        public async Task<bool> OnlineSyncAccountMoveLineByIds(int[] linesIds, Func<int, int, Task>? onProgress = null)
        {
            DateTime dateTimeIni = DateTime.Now;
            var databaseDet = new AccountMoveLineDb(Constants.Session.odooConnection.DbNameSqlite);

            Debug.WriteLine("Iniciando proceso:" + " " + DateTime.Now.ToString());

            HubAccountMoveLine hubmanager = new HubAccountMoveLine(Constants.Session);            

            var resultCount = await hubmanager.GetCountByIds(linesIds);

            Debug.WriteLine(resultCount.result);

            if (resultCount.result == 0)
            {
                return false;
            }
                        
            int totalPages = (int)Math.Ceiling((double)resultCount.result / limit);

            for (int indice = 0; indice <= totalPages; indice++)
            {
                var responseAll = await hubmanager.GetAccountMoveLineByIds(linesIds, limit, indice);

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

            return true;
        }

        [UpdateAction("Actualizar Detalles de Facturas por Cliente")]
        public async Task<bool> OnlineSyncAccountMoveLineByMove(int move_id, Func<int, int, Task>? onProgress = null)
        {
            DateTime dateTimeIni = DateTime.Now;
            var databaseDet = new AccountMoveLineDb(Constants.Session.odooConnection.DbNameSqlite);

            Debug.WriteLine("Iniciando proceso:" + " " + DateTime.Now.ToString());

            HubAccountMoveLine hubmanager = new HubAccountMoveLine(Constants.Session);

            var resultCount = await hubmanager.GetCountByMove(move_id);

            Debug.WriteLine(resultCount.result);

            if (resultCount.result == 0)
            {
                return false;
            }

            int totalPages = (int)Math.Ceiling((double)resultCount.result / limit);

            for (int indice = 0; indice <= totalPages; indice++)
            {
                var responseAll = await hubmanager.GetAccountMoveLinesByMove(move_id, limit, indice);

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

            return true;
        }
    }
}
