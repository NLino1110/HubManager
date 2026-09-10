using ApiManager;
using DMSA.Models.Security;
using DMSA.Sync.Core.Database.Sqlite;
using System.Diagnostics;

namespace DMSA.Sync.Core.Update
{
    public partial class ServerPuller
    {
        int year 
        { 
            get
            {
                return appSession.sync_date_since.Year;
            }                
        }
        int month 
        {
            get
            {
                return appSession.sync_date_since.Month;
            }
        }
        int day {
            get
            {
                return appSession.sync_date_since.Day;
            }
        }
        int limit { get; set; }
        
        DateTime? sync_date_since 
        { 
            get 
            {
                return appSession.sync_date_since;
            } 
        }

        DateTime? sync_date_since_lower
        {
            get
            {
                return appSession.sync_date_since.AddDays(-appSession.odooConnection.DataToleranceDays);
            }
        }

        int maxIndexExceeded { get; set; }

        /// <summary>
        /// Captura si la 1ra sync del día (cabeceras) ocurrió en esta sesión,
        /// para que el detalle use la misma fecha aunque ya se haya marcado el día.
        /// </summary>
        bool? _accountDocumentFirstSyncOfDay { get; set; }

        // REVERTIR Cobranzas Fase 2 (web_read saldos): poner en false.
        // Con false queda solo Fase 1 (search_read) como antes de este cambio.
        private const bool EnableResPartnerCobranzasSaldosWebRead = true;

        // Filtro Desde/Hasta por invoice_date en actualización masiva (Grupos 1 y 2).
        public const bool EnableInvoiceDateRangeSync = true;

        // Actualización rápida por ZIP en 1ra sync (paquete SQLite global con todos los res_partner).
        // Cobranzas: false → solo sync en línea filtrada por comercial. true → comportamiento legacy.
        public const bool EnablePipelineZipBootstrapSync = false;

        public AppSession appSession => Constants.Session;

        private string DbNameSqlite { get; set; }
        public ServerPuller() 
        {
            maxIndexExceeded = 600;
            
            if (appSession.odooConnection == null)
            {                
                return;
            }

            limit = appSession.odooConnection.DbLimitDefault;
            DbNameSqlite = appSession.odooConnection.DbNameSqlite;
        }

        public async Task<bool> Pull()
        {
            if (appSession.odooConnection == null)
            {
                return false;
            }

            try
            {
                await OnlineSyncCompany(false);
                await OnlineSyncResCenter(false);
                await OnlineSyncStockWarehouse(false);
                //await MotivoActividadDiaria(true);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Error en Pull: " + ex.Message);
                return false;
            }
            
            return true;
        }

        public async Task<bool> PullPromotions(Func<int, int, Task>? onProgress = null)
        {
            try
            {
                await OnlinePromotionBenefit(true, onProgress);
               
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Error en PullPromotions: " + ex.Message);
                return false;
            }
            return true;
        }

        public async Task OnlineSyncCompany(bool force)
        {
            var database = new CompanyDb(Constants.Session.odooConnection.DbNameSqlite);
            if( await database.GetCount() > 0)
            {
                //var listCmp = await database.GetItemsAsync();
                //Debug.WriteLine("Si existen!");
                //Ya se ha sincronizado previamente
                return;
            }

            HubResCompany hubCompany = new HubResCompany(Constants.Session);
            var dataList = await hubCompany.GetAll();

            if (dataList!= null && dataList.result != null && dataList.result.Length > 0)
            {
                foreach (var companyItem in dataList.result)
                {
                    if(companyItem.cuadratura_account_id_ == 0)
                    {
                        companyItem.cuadratura_account_id_ = 1064;
                    }

                    if (companyItem.credit_note_journal_id_ == 0)
                    {
                        companyItem.credit_note_journal_id_ = 9;
                    }
                }
                await database.InsertBatchAsync(dataList.result);
            }
        }
    }
}
