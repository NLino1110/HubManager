
using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Extensions;
using CommunityToolkit.Maui.Views;
using CommunityToolkit.Mvvm.Input;
using DMOrders.Controls;
using DMOrders.Services.Helpers;
using DMOrders.Services.Update.Pusher;
using DMOrders.Shared;
using DMSA.Models.Odoo.DMOrders;
using DMSA.Models.Odoo.DMOrders.tareas;
using DMSA.Models.Odoo.Native;
using System.Diagnostics;
using System.Windows.Input;

namespace DMOrders.Pages.Fragments.Activities;

[XamlCompilation(XamlCompilationOptions.Compile)]
public partial class Details : ContentPage, IBackButtonHandler
{
    public ProjectTask CurrentProjectTask { get; set; }
    public ICommand EditCommand { get; set; }

    private async void EditItem(object obj)
    {
        Debug.WriteLine("EditItem");
        var ItemForEdit = (AccountAnalyticLine)obj;
        PopupSizeConstants popupSizeConstants = new PopupSizeConstants(DeviceDisplay.Current);
        
        var returnResultPopup = new PopupAccountAnalyticLine(popupSizeConstants, CurrentProjectTask, ItemForEdit);
        returnResultPopup.CanBeDismissedByTappingOutsideOfPopup = false;        
        var result = await this.ShowPopupAsync(returnResultPopup);

        if (result != null)
        {

        }
        else
        {
            // stackAccountInfo.IsVisible = false;
        }

        await ((DetailsViewModel) BindingContext).PublicLoadActivities();
    }

    private void ViewObj_Disappearing(object? sender, EventArgs e)
    {
        Debug.WriteLine("ViewObj_Disappearing");
    }

    public Details(ProjectTask _CurrentActivityHeader)
	{
		InitializeComponent();

        CurrentProjectTask = _CurrentActivityHeader;

        if(CurrentProjectTask != null)
        {
            lblMainTitle.Text = $"Actividades Diarias No. {CurrentProjectTask.id}   Fecha {CurrentProjectTask.name}";
        }

        BindingContext = new DetailsViewModel(CurrentProjectTask);
        EditCommand = new Command(EditItem);
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
        //var tcs = new TaskCompletionSource<bool>();

        Dispatcher.Dispatch(async () =>
        {
            //var leave = await DisplayAlert("Atención", "Los cambios que haya realizado no se guardarán. ¿Desea continuar?", "Si", "No");

            //if (leave)
            //{
                await Navigation.PopModalAsync();
            //}
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
        
        var returnResultPopup = new PopupAccountAnalyticLine(popupSizeConstants, CurrentProjectTask, null);
        returnResultPopup.CanBeDismissedByTappingOutsideOfPopup = false;
        
        var result = await this.ShowPopupAsync(returnResultPopup);

        if (result != null)
        {

        }
        else
        {
           // stackAccountInfo.IsVisible = false;
        }

        await ((DetailsViewModel)BindingContext).PublicLoadActivities();
    }

    private async void ButtonSave_Clicked(object sender, EventArgs e)
    {
        //Save data
        await Toast.Make("Datos almacenados").Show();
        await Navigation.PopModalAsync();
    }

    private async void ButtonSync_Clicked(object sender, EventArgs e)
    {
        ServerPusher serverPusher = new ServerPusher();

        //var orderLinesList = ((CrudViewModel)this.BindingContext).OrderLines.ToList();
        //CurrentSaleOrder.order_line = new List<OrderLineWrapper>();
        //CurrentSaleOrder._center_id = App.Session.odooConnection.res_center_default;

        //foreach (var orderLine in orderLinesList)
        //{
        //    //orderLine.price_subtotal = 1;
        //    orderLine.price_unit = 1;
        //    orderLine.product_uom_qty = 1;

        //    CurrentSaleOrder.order_line.Add(new OrderLineWrapper(orderLine));
        //}

        await serverPusher.SendProjectTask(CurrentProjectTask);
    }
}