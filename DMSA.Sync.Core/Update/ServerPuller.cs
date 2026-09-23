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

        /// <summary>
        /// Esta sesión ya importó cabecera+detalle desde el ZIP admin (evita 2da descarga).
        /// </summary>
        bool _adminDocumentPackImported;

        // REVERTIR Cobranzas Fase 2 (web_read saldos): poner en false.
        // Con false queda solo Fase 1 (search_read) como antes de este cambio.
        private const bool EnableResPartnerCobranzasSaldosWebRead = true;

        // Filtro Desde/Hasta por invoice_date en actualización masiva (Grupos 1 y 2).
        // Reintento (solo si falló cabecera/detalle): cabecera invoice_date >= MAX local;
        // detalle write_date >= MAX local. Si cabecera se recupera y sigue detalle → Desde/Hasta del UI.
        public const bool EnableInvoiceDateRangeSync = true;

        // Actualización rápida por ZIP en 1ra sync (paquete SQLite global con todos los res_partner).
        // Cobranzas: false → solo sync en línea filtrada por comercial. true → comportamiento legacy.
        public const bool EnablePipelineZipBootstrapSync = false;

        // Benchmark ZIP cabecera/detalle (account_move / account_move_line) por rango Desde/Hasta.
        // false → oculta checkboxes y no ejecuta prueba. true → muestra checkboxes de benchmark.
        public const bool EnableAccountMoveLineZipBenchmarkSync = false;

        // Admin Mobile App (218), 1ra sync del día: descarga ZIP de mnsa.mobile.document.pack
        // (cron 03:00, último año), descomprime e inserta. Si no hay pack → search_read HTTP.
        // Vendedor: no aplica. Reintento (resume): no aplica.
        public const bool EnableAdminDocumentPackZipSync = true;

        // Pruebas: checkbox “Forzar 1ra sync del día” en Actualizar datos.
        // true → permite repetir la baja del ZIP Odoo (cabecera + detalle) el mismo día.
        // false → oculta el check y no borra account_move_sync_day_*. No usar en producción.
        public const bool EnableForceFirstSyncOfDayUi = false;

        // Cobranzas sync masivo: cabecera, detalle, clientes (Fase 1) y saldos (Fase 2).
        // DbLimitDefault global sigue en 300 para el resto de procesos.
        public const int CobranzasSyncPageSize = 1000;

        /// <summary>
        /// Page size elevado para admin Mobile App (218) en pruebas de peso/volumen por página.
        /// </summary>
        public const int CobranzasSyncPageSizeAdmin = 2000;

        /// <summary>
        /// Mobile App = Administrador Apps Móviles (res.users sel_groups_218).
        /// Admin: partners/cabecera/detalle sin filtro comercial; no SyncMissingPartnerSaleIds.
        /// </summary>
        protected static bool IsCurrentUserMobileAppAdmin()
            => Constants.Session?.CurrentUserFront?.IsMobileAppAdmin == true;

        /// <summary>Pruebas: olvida el ZIP de esta sesión para volver a tratarla como 1ra del día.</summary>
        public void ResetAdminDocumentPackDayState()
        {
            _accountDocumentFirstSyncOfDay = null;
            _adminDocumentPackImported = false;
        }

        /// <summary>
        /// Vendedor: <see cref="CobranzasSyncPageSize"/> (1000).
        /// Admin Mobile App: <see cref="CobranzasSyncPageSizeAdmin"/> (2000) para pruebas de peso.
        /// </summary>
        protected static int ResolveCobranzasSyncPageSize()
            => IsCurrentUserMobileAppAdmin() ? CobranzasSyncPageSizeAdmin : CobranzasSyncPageSize;

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
