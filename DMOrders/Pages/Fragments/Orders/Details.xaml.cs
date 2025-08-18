
using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Extensions;
using CommunityToolkit.Maui.Views;
using CommunityToolkit.Mvvm.Input;
using DMOrders.Controls;
using DMOrders.Pages.Fragments.Orders.modals;
using DMOrders.Services.Database.Sqlite;
using DMOrders.Services.Helpers;
using DMOrders.Services.Update.Pusher;
using DMOrders.Shared;
using DMSA.Models.Odoo.DMOrders;
using DMSA.Models.Odoo.Native;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace DMOrders.Pages.Fragments.Orders;

[XamlCompilation(XamlCompilationOptions.Compile)]
public partial class Details : ContentPage, IBackButtonHandler
{
    public res_company CurrentCompany { get; set; }
    public res_partner _CurrentPartner { get; set; }
    public sale_order CurrentSaleOrder { get; set; }

    PopupSelectProduct returnResultPopup = new PopupSelectProduct();
    
    public ICommand EditCommand { get; set; }
    public ICommand DeleteCommand { get; set; }

    public res_partner CurrentPartner
    {
        get => _CurrentPartner;
        set
        {
            if (_CurrentPartner != value)
            {
                _CurrentPartner = value;
                OnPropertyChanged(nameof(CurrentPartner));
            }
        }
    }

    public Details()
	{
		InitializeComponent();        
        BindingContext = new DetailsViewModel();

        returnResultPopup.CanBeDismissedByTappingOutsideOfPopup = false;

        EditCommand = new Command(EditItem);
        DeleteCommand = new Command(DeleteItem);
    }

    protected override void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        base.OnPropertyChanged(propertyName);
        if(propertyName.Contains("CurrentPartner"))
        {
            //PrepareForm();
        }
        Debug.WriteLine($"Property changed: {propertyName}");
    }

    public async Task PrepareForm()
    {
        if( CurrentPartner != null )
        {
            //DESDE LISTA DE CLIENTES PARA AGREGAR NUEVA ORDEN
            Title = CurrentPartner.name;
            if(CurrentSaleOrder == null)
            {
                Title += " [*]";
            }
            else
            {
                Title += " [nuevo]";
            }

            ((DetailsViewModel)this.BindingContext)._CurrentPartner = CurrentPartner;
            ((DetailsViewModel)this.BindingContext).CurrentCompany = CurrentCompany;
            ((DetailsViewModel)this.BindingContext).CurrentSaleOrder = CurrentSaleOrder;
            ((DetailsViewModel)this.BindingContext).LoadData();
        }
        else
        {
            ResPartnerDb resPartnerDb = new ResPartnerDb();
            var CurrentPartner = await resPartnerDb.GetItemsAsync(CurrentCompany.id , CurrentSaleOrder._partner_id);
            if(CurrentPartner != null)
            {
                Title = CurrentPartner.name;
            }

            if (CurrentSaleOrder != null)
            {
                Title += " [edición]";
            }
            ((DetailsViewModel)this.BindingContext).CurrentCompany = CurrentCompany;
            ((DetailsViewModel)this.BindingContext).CurrentSaleOrder = CurrentSaleOrder;
            ((DetailsViewModel)this.BindingContext).LoadData();
        }
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
                await Navigation.PopModalAsync();
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

    private async void ButtonAddNew_Clicked_old(object sender, EventArgs e)
    {
        CatalogViewerModel previousCatalogViewerModel;

        if (returnResultPopup.BindingContext != null)
            previousCatalogViewerModel = (CatalogViewerModel)returnResultPopup.BindingContext;
        else
            previousCatalogViewerModel = new CatalogViewerModel();

        returnResultPopup = new PopupSelectProduct();
        returnResultPopup.BindingContext = previousCatalogViewerModel;

        //TODO: Replicar Reset
        //returnResultPopup.Reset();
        var result = await PopupExtensions.ShowPopupAsync(this, returnResultPopup);

        if (result != null)
        {
            var selected_product = (product_product) result;

            sale_order_line NewOrderLine = new sale_order_line
            {
                id = 0,
                product_code = selected_product.code,
                product_id = selected_product.id,
                product_display = selected_product.display_name
            };
            ((DetailsViewModel)this.BindingContext).AddOrderLine(NewOrderLine);
        }
        else
        {
           // stackAccountInfo.IsVisible = false;
        }
    }


    private async void ButtonAddNew_Clicked(object sender, EventArgs e)
    {
        SearchProductView.IsVisible = true;

        //var selected_product = (product_product)result;

        //sale_order_line NewOrderLine = new sale_order_line
        //{
        //    id = 0,
        //    product_code = selected_product.code,
        //    product_id = selected_product.id,
        //    product_display = selected_product.display_name
        //};
        //((DetailsViewModel)this.BindingContext).AddOrderLine(NewOrderLine);

    }

    private async void ButtonSave_Clicked(object sender, EventArgs e)
    {
        var viewModel = (DetailsViewModel)this.BindingContext;
        var orderLines = viewModel.OrderLines;
        var saleOrderDb = new SaleOrderDb();
        var saleOrderLineDb = new SaleOrderLineDb();

        sale_order targetOrder;

        bool isNew = CurrentSaleOrder == null;

        if (isNew)
        {
            targetOrder = new sale_order
            {
                _partner_id = _CurrentPartner.id,
                _company_id = CurrentCompany.id,
                date_order = DateTime.Now,
                //note = fieldNote.Text
            };

            if (await saleOrderDb.InsertAsync(targetOrder) <= 0)
            {
                await Toast.Make("Error al crear la orden").Show();
                return;
            }
        }
        else
        {
            targetOrder = CurrentSaleOrder;
            targetOrder.write_date = DateTime.Now;
            //targetOrder.note = fieldNote.Text;

            if (await saleOrderDb.UpdateAsync(targetOrder) <= 0)
            {
                await Toast.Make("Error al actualizar la orden").Show();
                return;
            }

            // Eliminar líneas anteriores antes de insertar las nuevas
            await saleOrderLineDb.DeleteItemOfParent(targetOrder);
        }

        // Asignar el ID de la orden a las líneas y guardar
        foreach (var orderLine in orderLines)
        {
            orderLine._order_id = targetOrder.id;
            await saleOrderLineDb.InsertAsync(orderLine);
        }

        await Toast.Make(isNew ? "Orden creada" : "Orden actualizada").Show();

        //if (await saleOrderLineDb.InsertBatchAsync(orderLines.ToArray()) > 0)
        //{
        //    await Toast.Make(isNew ? "Orden creada" : "Orden actualizada").Show();
        //}
        //else
        //{
        //    await Toast.Make("Error al guardar líneas").Show();
        //}

        await Navigation.PopAsync();
    }


    private async void ButtonSync_Clicked(object sender, EventArgs e)
    {
        ServerPusher serverPusher = new ServerPusher();

        var orderLinesList = ((DetailsViewModel)this.BindingContext).OrderLines.ToList();
        CurrentSaleOrder.order_line = new List<OrderLineWrapper>();        
        foreach (var orderLine in orderLinesList)
        {
            orderLine.price_subtotal = 1;
            orderLine.price_unit = 1;
            orderLine.product_uom_qty = 1;

            CurrentSaleOrder.order_line.Add(new OrderLineWrapper(orderLine));
        }

        await serverPusher.SendSaleOrder(CurrentSaleOrder);
    }

    private async void EditItem(object obj)
    {
        Debug.WriteLine(obj);
        Debug.WriteLine("EditItem");
    }

    private async void DeleteItem(object obj)
    {        
        Debug.WriteLine("DeleteItem");
        ((DetailsViewModel)this.BindingContext).OrderLines.Remove((sale_order_line) obj);
    }
}