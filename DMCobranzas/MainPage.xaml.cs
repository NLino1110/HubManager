using Microsoft.Maui.ApplicationModel.Communication;
using RestSharp;
using static DMCobranzas.DetailModal;
using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;
using DMCobranzas.Settings;
using System.Diagnostics;
using DMCobranzas.Models;
using DMCobranzas.Services.ApiHub;
using CommunityToolkit.Maui.Sample.Models;
using CommunityToolkit.Maui.Sample.ViewModels.Views;
using DMCobranzas.Settings.helpers;
using CommunityToolkit.Maui.Sample;
using CommunityToolkit.Maui.Views;
using DMCobranzas.Services.Database.Sqlite;

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

            AccountPaymentHeaderDb _accountPaymentHeaderDb = new AccountPaymentHeaderDb();
            var resultItems = await _accountPaymentHeaderDb.GetItemsPendingAsync(App.Session.CurrentUser.uid);
            if(resultItems.Count > 0)
            {
                await UITools.ShowLoadingPopup(this);

                try
                {
                    foreach (var item in resultItems)
                    {
                        await SendController.SendPayment(item, true);
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

