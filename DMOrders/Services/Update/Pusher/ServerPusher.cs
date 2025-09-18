using ApiManager;
using CommunityToolkit.Maui.Alerts;
using DMOrders.Services.Database.Sqlite;
using DMSA.Models.General.Requests;
using DMSA.Models.Odoo.General.Responses;
using DMSA.Models.Odoo.Native;
using DMSA.Models.Security;
using Microsoft.Maui;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using static System.Net.Mime.MediaTypeNames;

namespace DMOrders.Services.Update.Pusher
{
    public partial class ServerPusher
    {
        int year { get; set; }
        int month { get; set; }
        int day { get; set; }
        int limit { get; set; }

        public AppSession appSession => App.Session;
        public ServerPusher() 
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

            limit = App.Session.odooConnection.DbLimitDefault;
        }

        public async Task<bool> Push()
        {
            //await OnlineSyncCompany();
            //await OnlineSyncStores();
            return true;
        }
                
        public async Task SendSaleOrder(sale_order sale_Order)
        {   
            //object[] _args_ = new object[] {
            //    sale_Order
            //};

            //object kargs = new object[] {};

            ApiManager.HubSaleOrder hubStore = new HubSaleOrder(App.Session);
            ApiResponseOdooRpcT<int> resultTask = await hubStore.Create(sale_Order);

            if (resultTask != null && resultTask.error != null)
            {
                Debug.WriteLine(resultTask.error.data.message);
                Debug.WriteLine(resultTask.error.data.debug);
                await Toast.Make("Error:" + resultTask.error.data.message).Show();
                return;
            }

            if (resultTask != null && resultTask.result !=null)
            {
                await Toast.Make("Datos enviados correctamente").Show();

                sale_Order.is_synchronized = true;
                sale_Order.date_synchronized = DateTime.Now;
                SaleOrderDb saleOrderDb = new SaleOrderDb();
                await saleOrderDb.UpdateAsync(sale_Order);
            }            
        }
    }
}
