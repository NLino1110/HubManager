using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;
using CommunityToolkit.Maui.Sample;
using CommunityToolkit.Maui.Sample.Models;
using CommunityToolkit.Maui.Sample.ViewModels.Views;
using CommunityToolkit.Maui.Views;
using DMCobranzas.Models;
using DMCobranzas.Services.ApiHub;
using DMCobranzas.Settings;
using DMCobranzas.Settings.helpers;
using DMSA.Models.Odoo.DebitCollection;
using DMSA.Sync.Core.Database.Sqlite.DebitCollection;
using DMSA.Sync.Core.Database.Sqlite.Payments;
using DMSA.Sync.Core.Update.Pusher;
using Microsoft.Maui.ApplicationModel.Communication;
using RestSharp;
using System.Diagnostics;
using static DMCobranzas.DetailModal;

namespace DMCobranzas;
public partial class MainPage : ContentPage
{    
    readonly PopupSizeConstants popupSizeConstants;
    readonly CsharpBindingPopupViewModel csharpBindingPopupViewModel;

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

        this.csharpBindingPopupViewModel = csharpBindingPopupViewModel;

        LoadSession();

        SetupPermissions();
    }

    private async void SetupPermissions()
    {
        //TODO: No implementado, se ha encontrado que es algo complejo hasta ahora.
        // permisos de la aplicación por medio de MAUI, no se han encontrado ejemplos funcionales.

        //bool hasPermission = await PermissionHelper.AskForBluetoothAdminPermission();
        
        //if (hasPermission)
        //{
        //    // Aquí colocas el código para acceder a la función que requiere el permiso BluetoothAdmin
        //    // Por ejemplo, activar el Bluetooth, realizar operaciones con dispositivos Bluetooth, etc.
        //}
        //else
        //{
        //    // Aquí puedes mostrar un mensaje al usuario indicando que el permiso es necesario para cierta funcionalidad.
        //}

        //await PermissionHelper.CheckBluetoothAccess();
    }

    private void LoadSession()
    {
        if (App.Session != null)
        {
            txtUser.Text = "Usuario: " + App.Session.CurrentUserFront.username;
            txtName.Text = "Nombre: " + App.Session.CurrentUserFront.nombres;
            txtUpdated.Text = "Ult. Actualización: " + App.Session.CurrentUserFront.log_fec_sincro.ToString("dd/MM/yyyy HH:mm:ss");
            txtUpdatedNC.Text = "Ult. Actualización NC: " + App.Session.CurrentUserFront.log_fec_sincro_nc.ToString("dd/MM/yyyy HH:mm:ss");
            txtLastAccess.Text = "Ult. Acceso: " + App.Session.CurrentUserFront.log_fec_acceso.ToString("dd/MM/yyyy HH:mm:ss");
        }

        //Aquí evaluar estado actual para saber si se deben
        //  enviar automaticamente los datos para la sincronización
        //envioAutomaticoCobros
        
        Task.Run(async () =>
        {
            if(App.Session.odooConnection.IsTestMode)
            {
                return;
            }

            MultipleCobrosInvoiceDb _accountPaymentHeaderDb = new MultipleCobrosInvoiceDb(App.Session.odooConnection.DbNameSqlite);
            var resultItems = await _accountPaymentHeaderDb.GetItemsAsync(i => i.user_id == App.Session.CurrentUser.uid &&
                Convert.ToDateTime(i.create_date).Date != DateTime.Today &&
                (i.payment_status == DMSA.Models.CobrosEstados.ENVIANDO ||
                i.payment_status == DMSA.Models.CobrosEstados.ERROR ||
                i.payment_status == DMSA.Models.CobrosEstados.PENDIENTE));

            if(resultItems.Count > 0)
            {
                await UITools.ShowLoadingPopup(this);

                try
                {
                    foreach (var item in resultItems)
                    {
                        await DebitCollection.SendPayment(item, true);
                    }
                }
                catch(Exception ex)
                {
                    await Toast.Make("Error:" + ex.InnerException.Message).Show();
                }

                await Task.Delay(500);
                await UITools.HideLoadingPopup();
            }
        });
    }
}

