using ApiManager;
using DMOrders.Services.Database.Sqlite;
using DMSA.Models.General.Requests;
using DMSA.Models.Odoo.General.Responses;
using DMSA.Models.Odoo.Native;
using DMSA.Models.Security;
using System.Diagnostics;

namespace DMOrders.Services.Update
{
    public partial class ServerPuller
    {
        int year { get; set; }
        int month { get; set; }
        int day { get; set; }
        int limit { get; set; }

        int maxIndexExceeded { get; set; }

        public AppSession appSession => App.Session;

        private string DbNameSqlite { get; set; }
        public ServerPuller() 
        {
            year = appSession.sync_date_since.Year;
            month = appSession.sync_date_since.Month;
            day = appSession.sync_date_since.Day;

            //DESACTIVADO PARA EFECTOS DE PRUEBA
            //Si ya hubo sincronizacion previa, se toma como limite de fecha la fecha de la ultima sincronizacion            
            //if(appSession.sync_date_since != appSession.CurrentUser.log_fec_sincro)
            //{
            //    year = appSession.CurrentUser.log_fec_sincro.Year;
            //    month = appSession.CurrentUser.log_fec_sincro.Month;
            //    day = appSession.CurrentUser.log_fec_sincro.Day;
            //}

            maxIndexExceeded = 600;
            limit = appSession.odooConnection.DbLimitDefault;
            DbNameSqlite = appSession.odooConnection.DbNameSqlite;
        }

        public async Task<bool> Pull()
        {
            try
            {
                await OnlineSyncCompany(false);
                //await OnlineSyncStores();
                await OnlineSyncResCenter(false);
                await OnlineSyncStockWarehouse(false);

                await OnlineMotivoActividadDiaria(true);
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

        private async Task OnlineSyncCompany(bool force)
        {
            var database = new CompanyDb();
            if( await database.GetCount() > 0)
            {
                //var listCmp = await database.GetItemsAsync();
                //Debug.WriteLine("Si existen!");
                //Ya se ha sincronizado previamente
                //return;
            }

            ApiManager.HubCompany hubCompany = new HubCompany(App.Session);
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

        private async Task OnlineSyncStores()
        {

            int index = 0;
            int limit = 1000;
            DateTime dateIni = DateTime.Now.AddDays(-1000);

            ApiManager.HubStore hubStore = new HubStore(App.Session);
            ApiResponseOdooRpcT<res_store[]> dataList = await hubStore.GetByCreateDate(limit, index, dateIni.Year, dateIni.Month, dateIni.Day);

            if (dataList != null && dataList.result !=null && dataList.result.Length > 0)
            {
                //TODO: Temporal, eliminar cuando se haya corregido en ODOO
                foreach (var store in dataList.result)
                {
                    if (store.id < 50)
                    {
                        store.company_id = 1;
                    }
                    else
                    {
                        store.company_id = 2;
                    }
                }

                var database = new StoreDb();
                await database.InsertBatchAsync(dataList.result);
            }
        }

    }
}
