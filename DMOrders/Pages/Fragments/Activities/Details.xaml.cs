
using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Extensions;
using CommunityToolkit.Maui.Views;
using CommunityToolkit.Mvvm.Input;
using DMOrders.Controls;
using DMOrders.Services.Helpers;
using DMOrders.Shared;
using DMSA.Models.Odoo.DMOrders;
using DMSA.Models.Odoo.Native;
using System.Diagnostics;

namespace DMOrders.Pages.Fragments.Activities;

[XamlCompilation(XamlCompilationOptions.Compile)]
public partial class Details : ContentPage, IBackButtonHandler
{
    public ActivityHeader CurrentActivityHeader { get; set; }
	public Details()
	{
		InitializeComponent();        
        BindingContext = new DetailsViewModel();
    }

    public async Task<bool> OnBackButtonPressedAsync()
    {
        bool result = await DisplayAlert("Confirmación", "Minimizar la aplicación, ¿Desea continuar?", "Sí", "No");
        if (result)
        {
#if ANDROID
            Platform.CurrentActivity?.MoveTaskToBack(true);
#endif
        }
        return !result; // true => lo manejo yo y no cierro la app; false => dejar cerrar
    }

    protected override bool OnBackButtonPressed()
    {
        var tcs = new TaskCompletionSource<bool>();

        Dispatcher.Dispatch(async () =>
        {
            var leave = await DisplayAlert("Atención", "Los cambios que haya realizado no se guardarán. ¿Desea continuar?", "Si", "No");

            if (leave)
            {
                await Navigation.PopAsync();
            }
        });

        return true;
        //return base.OnBackButtonPressed();
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        //Debug.WriteLine("Details page is disappearing.");

        //if (true)
        //{
        //    // Si no se permite el cierre, evitar que la página se cierre
        //    Navigation.PopModalAsync(false);
        //}
    }

    private async void ButtonClose_Clicked(object sender, EventArgs e)
    {
        SendBackButtonPressed();
        //await Navigation.PopAsync();
    }

    private async void ButtonNew_Clicked(object sender, EventArgs e)
    {
        PopupSizeConstants popupSizeConstants = new PopupSizeConstants(DeviceDisplay.Current);

        var returnResultPopup = new PopupPlanningSlot(popupSizeConstants);
        returnResultPopup.CanBeDismissedByTappingOutsideOfPopup = false;
        returnResultPopup.activityHeader = CurrentActivityHeader;

        //if (!isWindows)
        //    returnResultPopup.Size = this.popupSizeConstants.Large;

        var result = await this.ShowPopupAsync(returnResultPopup);

        if (result != null)
        {
            //PartnerBankDb partnerBankDb = new PartnerBankDb();
            //var new_partnerBank = (res_partner_bank)result;

            //_res_partner_bank = new_partnerBank;

            //new_partnerBank.id = await partnerBankDb.getNewId();

            //await partnerBankDb.InsertAsync(new_partnerBank);

            //txtCuenta.Text = new_partnerBank.acc_number;
            //lblAccountHolder.Text = new_partnerBank.acc_holder_name;
            //lblAccountBank.Text = new_partnerBank.bank_name;
            //string type_account = "-";
            //switch (new_partnerBank.type_account)
            //{
            //    case "savings":
            //        {
            //            type_account = "AHORROS";
            //        }
            //        break;
            //    case "current":
            //        {
            //            type_account = "CORRIENTE";
            //        }
            //        break;
            //}

            //lblAccountType.Text = type_account;
            //stackAccountInfo.IsVisible = true;

        }
        else
        {
           // stackAccountInfo.IsVisible = false;
        }
    }

    private async void ButtonSave_Clicked(object sender, EventArgs e)
    {
        //Save data
        await Toast.Make("Datos almacenados").Show();
        await Navigation.PopAsync();
    }

    private void ButtonSync_Clicked(object sender, EventArgs e)
    {

    }

}