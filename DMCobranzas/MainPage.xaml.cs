using CommunityToolkit.Maui.Alerts;
using DMCobranzas.Settings.helpers;
using DMSA.Models.Odoo.DebitCollection;
using DMSA.Sync.Core.Controls.Popups;
using DMSA.Sync.Core.Database.Sqlite.DebitCollection;
using DMSA.Sync.Core.Update.Pusher;

namespace DMCobranzas;
public partial class MainPage : ContentPage
{    
    readonly PopupSizeConstants popupSizeConstants;    

    int count = 0;

	public MainPage()
	{
		InitializeComponent();

        if (popupSizeConstants == null)
        {
            this.popupSizeConstants = new PopupSizeConstants(DeviceDisplay.Current);
        }
        else
        {
            this.popupSizeConstants = popupSizeConstants;
        }

        LoadSession();
        SetupPermissions();
    }

    private async void SetupPermissions()
    {
       
    }

    private async void LoadSession()
    {
        if (App.Session != null)
        {
            txtUser.Text = "Usuario: " + App.Session.CurrentUserFront.username;
            txtName.Text = "Nombre: " + App.Session.CurrentUserFront.nombres;
            txtUpdated.Text = "Ult. Actualización: " + App.Session.CurrentUserFront.log_fec_sincro.ToString("dd/MM/yyyy HH:mm:ss");
            txtUpdatedNC.Text = "Ult. Actualización NC: " + App.Session.CurrentUserFront.log_fec_sincro_nc.ToString("dd/MM/yyyy HH:mm:ss");
            txtLastAccess.Text = "Ult. Acceso: " + App.Session.CurrentUserFront.log_fec_acceso.ToString("dd/MM/yyyy HH:mm:ss");
        }

        var today = DateTime.Today;

        MultipleCobrosInvoiceDb _accountPaymentHeaderDb = new MultipleCobrosInvoiceDb(App.Session.odooConnection.DbNameSqlite);
        var resultItems = await _accountPaymentHeaderDb.GetItemsAsync(i => i.user_id == App.Session.CurrentUserFront.uid &&
            i.create_date < today &&
            i.payment_status == CobrosEstados.PENDIENTE);

        if(resultItems.Count > 0)
        {
            await UITools.ShowLoadingPopup(this);

            try
            {
                foreach (var item in resultItems)
                {
                    await DebitCollection.SendPayment(item, true);
                }

                await Toast.Make("Envíos automáticos realizados").Show();
            }
            catch(Exception ex)
            {
                await Toast.Make("Error:" + ex.InnerException.Message).Show();
            }

            await Task.Delay(500);
            await UITools.HideLoadingPopup();
        }        
    }
}

