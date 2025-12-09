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
        //DateTime? sync_date_since { get; set; }// => new DateTime(year, month, day);

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

        public AppSession appSession => Constants.Session;

        private string DbNameSqlite { get; set; }
        public ServerPuller() 
        {
            //sync_date_since = appSession.sync_date_since;

            //year = appSession.sync_date_since.Year;
            //month = appSession.sync_date_since.Month;
            //day = appSession.sync_date_since.Day;

            //DESACTIVADO PARA EFECTOS DE PRUEBA
            //Si ya hubo sincronizacion previa, se toma como limite de fecha la fecha de la ultima sincronizacion            
            //if(appSession.sync_date_since != appSession.CurrentUser.log_fec_sincro)
            //{
            //    year = appSession.CurrentUser.log_fec_sincro.Year;
            //    month = appSession.CurrentUser.log_fec_sincro.Month;
            //    day = appSession.CurrentUser.log_fec_sincro.Day;
            //}

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
                //await OnlineSyncStores();
                await OnlineSyncResCenter(false);
                await OnlineSyncStockWarehouse(false);

                await MotivoActividadDiaria(true);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Error en Pull: " + ex.Message);
                return false;
            }
            
            return true;
        }

        public async Task<bool> PullPromotions()
        {
            try
            {
                await OnlinePromotionBenefit(true);
               
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
                //return;
            }

            ApiManager.HubCompany hubCompany = new HubCompany(Constants.Session);
            var dataList = await hubCompany.GetAll();

            if (dataList!= null && dataList.result != null && dataList.result.Length > 0)
            {
                foreach (var companyItem in dataList.result)
                {
                    //if (companyItem.partner_id != null && companyItem.partner_id.Length > 0)
                    //{
                    //    companyItem.partner_id_ = companyItem.partner_id[0].id;
                    //}

                    //if (companyItem.check_journal_id != null && companyItem.check_journal_id.Length > 0)
                    //{
                    //    companyItem.check_journal_id_ = companyItem.check_journal_id[0].id;
                    //}

                    //if (companyItem.credit_note_journal_id != null && companyItem.credit_note_journal_id.Length > 0)
                    //{
                    //    companyItem.credit_note_journal_id_ = companyItem.credit_note_journal_id[0].id;
                    //}
                }
                await database.InsertBatchAsync(dataList.result);
            }
        }
    }
}
