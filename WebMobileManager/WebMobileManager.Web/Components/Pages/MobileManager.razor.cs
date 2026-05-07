using DMSA.Models.Odoo.Tools;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR;
using MudBlazor;
using WebMobileManager.Web.Handlers;
using WebMobileManager.Web.Handlers.Models;
using WebMobileManager.Web.Views.Modals;
using static MudBlazor.CategoryTypes;
using static MudBlazor.Defaults.Classes;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace WebMobileManager.Web.Components.Pages
{
    public partial class MobileManager
    {
        private string Servidor { get; set; }

        [Inject] 
        private IDialogService DialogService { get; set; }

        [Inject]
        ISnackbar Snackbar { get; set; }

        [Inject]
        ChatHub chatHub { get; set; }

        ConnectionManager ConnectedUsers { get; set; }

        List<string> listado {  get; set; }

        private IEnumerable<ConnectedDevice> devices = new List<ConnectedDevice>();
        protected override Task OnInitializedAsync()
        {
            //listado = chatHub.ObtenerClientesConectados();

            var uri = new Uri(Navigation.Uri);
            Servidor = $"{uri.Scheme}://{uri.Host}:{uri.Port}";

            return base.OnInitializedAsync();
        }

        private async Task LoadDevices()
        {
            devices = chatHub.GetDevices();
            StateHasChanged();
        }

        private async Task ClearDevices()
        {
            devices = new List<ConnectedDevice>();
            chatHub.ClearDevices();
            StateHasChanged();
        }

        private async Task RequireInfoDevice(ConnectedDevice device)
        {            
            await chatHub.RequireInfoDevice(device.Id);
            StateHasChanged();
        }

        private async Task RequireFullInfoDevice(ConnectedDevice device)
        {            
            await chatHub.RequireFullInfoDevice(device.Id);
            StateHasChanged();
        }

        private async Task SendMessage(ConnectedDevice device)
        {
            //device = 
            await chatHub.SendMessage("FILLLL","MESSSAGEEEEE");
            StateHasChanged();
        }

        private async Task TestDialog()
        {
            await DialogService.ShowMessageBoxAsync(
                "Test",
                "Esto es una prueba",
                yesText: "OK"
            );

            Snackbar.Clear();
            Snackbar.Configuration.PositionClass = Defaults.Classes.Position.BottomRight;            
            Snackbar.Add("message", Severity.Success);
        }

        private async Task LoadDummy()
        {            
            devices = new List<ConnectedDevice>
            {
                new ConnectedDevice { Id = "1", AppName = "DMCobranzas", VersionString="1.0.1", DeviceName="Device 1", CurrentLocation="{\"Latitude\":40.7128,\"Longitude\":-74.0060}" },
                new ConnectedDevice { Id = "2", AppName = "DMOrders", VersionString="1.0.1", DeviceName = "Device 2" },
                new ConnectedDevice { Id = "3", AppName = "DMOrders", VersionString="1.0.1", DeviceName = "Device 3" },
                new ConnectedDevice { Id = "4", AppName = "DMOrders", VersionString="1.0.1", DeviceName = "Device 3" },
                new ConnectedDevice { Id = "5", AppName = "DMCobranzas", VersionString="1.0.1", DeviceName = "Device 3" },
                new ConnectedDevice { Id = "6", AppName = "DMCobranzas", VersionString="1.0.1", DeviceName = "Device 3" },
                new ConnectedDevice { Id = "7", AppName = "DMOrders", VersionString="1.0.1", DeviceName = "Device 3" },
                new ConnectedDevice { Id = "8", AppName = "DMCobranzas", VersionString="1.0.1", DeviceName = "Device 3" },
                new ConnectedDevice { Id = "9", AppName = "DMCobranzas", VersionString="1.0.1", DeviceName = "Device 3" },
                new ConnectedDevice { Id = "10", AppName = "DMCobranzas", VersionString="1.0.1", DeviceName = "Device 3" },
                new ConnectedDevice { Id = "11", AppName = "DMOrders", VersionString="1.0.1", DeviceName = "Device 3" },
                new ConnectedDevice { Id = "12", AppName = "DMCobranzas", VersionString="1.0.1", DeviceName = "Device 3" },
                new ConnectedDevice { Id = "13", AppName = "DMCobranzas", VersionString="1.0.1", DeviceName = "Device 3" },
                new ConnectedDevice { Id = "14", AppName = "DMOrders", VersionString="1.0.1", DeviceName = "Device 3" },
                new ConnectedDevice { Id = "15", AppName = "DMCobranzas", VersionString="1.0.1", DeviceName = "Device 3" },
                new ConnectedDevice { Id = "16", AppName = "DMCobranzas", VersionString="1.0.1", DeviceName = "Device 3" },
                new ConnectedDevice { Id = "17", AppName = "DMDataSafe", VersionString="1.0.1", DeviceName = "Device 3" },
            };
        }

        private async Task SendMessageDevice(ConnectedDevice device)
        {
            var parameters = new DialogParameters
                {
                    { nameof(DisplayPrompt.Title), "Enviar mensaje" },
                    { nameof(DisplayPrompt.Message), "Ingrese el texto:" },
                    { nameof(DisplayPrompt.PromptValue), "" }
                };

            var options = new DialogOptions
            {
                CloseOnEscapeKey = true,
                BackdropClick = false
            };

            var dialogReference = await DialogService.ShowAsync<DisplayPrompt>("", parameters, options);

            var result = await dialogReference.Result;

            if (!result.Canceled && result.Data is string message && !string.IsNullOrWhiteSpace(message))
            {
                await chatHub.SendMessageDevice(device.Id, "Servidor", message);
                StateHasChanged();
            }
        }

        private async Task SendUpdateNotify(ConnectedDevice device)
        {
            //List<MobileAppDto> apps = new();

            //apps = new List<MobileAppDto>
            //{
            //    new() { Name = "App Ventas", PackageId = "com.dm.ventas", Version = "1.2.0", IsActive = true },
            //    new() { Name = "App Inventario", PackageId = "com.dm.inventario", Version = "2.0.1", IsActive = true },
            //    new() { Name = "App Delivery", PackageId = "com.dm.delivery", Version = "3.1.4", IsActive = false },
            //    new() { Name = "App Reportes", PackageId = "com.dm.reportes", Version = "1.0.5", IsActive = true },
            //    new() { Name = "App RRHH", PackageId = "com.dm.rrhh", Version = "4.2.0", IsActive = false }
            //};

            //var parameters = new DialogParameters
            //{
            //    { "Apps", apps }
            //};

            //var options = new DialogOptions
            //{
            //    MaxWidth = MaxWidth.Medium,
            //    FullWidth = true
            //};

            //var dialog = await DialogService.ShowAsync<ConfirmNotify>(
            //    "Notificar apps",
            //    parameters,
            //    options
            //);

            //var result = await dialog.Result;

            //if (!result.Canceled)
            //{                
            //    await chatHub.SendNotifyDevice(device.Id, "Servidor", "message");
            //    StateHasChanged();
            //}

            var parameters = new DialogParameters
            {
                { "ContentText", $"¿Notificar actualización de {device.AppName} para dispositivo?" },
                { "ButtonText", "Confirmar" },
                { "Color", Color.Primary }
            };

            var options = new DialogOptions
            {
                CloseOnEscapeKey = true
            };

            var confirm = await DialogService.ShowAsync<ConfirmDialog>("Confirmación", parameters, options);
            var result = await confirm.Result;

            if (!result.Canceled)
            {
                
            }
        }
    }
}
