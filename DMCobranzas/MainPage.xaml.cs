using CommunityToolkit.Maui.Alerts;
using DMCobranzas.Services;
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

        Appearing += MainPage_Appearing;
        SetupPermissions();
    }

    private async void MainPage_Appearing(object sender, EventArgs e)
    {
        await RefreshStatusLabelsAsync();
    }

    private async Task RefreshStatusLabelsAsync()
    {
        if (App.Session?.CurrentUserFront == null)
            return;

        txtUser.Text = "Usuario: " + App.Session.CurrentUserFront.username;
        txtName.Text = "Nombre: " + App.Session.CurrentUserFront.nombres;

        var syncDate = await SyncStatusLabels.ResolveLastSyncDateAsync();
        if (syncDate.Year > 2000)
        {
            txtUpdated.Text = SyncStatusLabels.FormatHomeLastSyncText(syncDate);
            App.Session.CurrentUserFront.log_fec_sincro = syncDate;
        }
        else if (App.Session.CurrentUserFront.log_fec_sincro.Year > 2000)
        {
            txtUpdated.Text = SyncStatusLabels.FormatHomeLastSyncText(App.Session.CurrentUserFront.log_fec_sincro);
        }

        txtUpdatedNC.Text = "Ult. Actualización NC: " + App.Session.CurrentUserFront.log_fec_sincro_nc.ToString("dd/MM/yyyy HH:mm:ss");
        txtLastAccess.Text = "Ult. Acceso: " + App.Session.CurrentUserFront.log_fec_acceso.ToString("dd/MM/yyyy HH:mm:ss");
        txtAppUpdateDate.Text = SyncStatusLabels.FormatAppUpdateText();
    }

    private async void SetupPermissions()
    {
       
    }

    private async void LoadSession()
    {
        await RefreshStatusLabelsAsync();

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

