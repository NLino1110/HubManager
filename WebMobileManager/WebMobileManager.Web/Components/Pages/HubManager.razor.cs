using DMSA.Models.Odoo.Tools;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR;
using MudBlazor;
using WebMobileManager.Web.Handlers;
using WebMobileManager.Web.Views.Modals;
using static MudBlazor.CategoryTypes;
using static MudBlazor.Defaults.Classes;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace WebMobileManager.Web.Components.Pages
{
    public partial class HubManager
    {
        private string Servidor;

        [Inject] 
        private IDialogService DialogService { get; set; }

        [Inject]
        ISnackbar Snackbar { get; set; }

        [Inject]
        ChatHub chatHub { get; set; }

        ConnectionManager ConnectedUsers { get; set; }

        List<string> listado {  get; set; }

        List<ConnectedDevice> devices { get; set; }

        private IEnumerable<ConnectedDevice> Elements = new List<ConnectedDevice>();
        protected override Task OnInitializedAsync()
        {
            //listado = chatHub.ObtenerClientesConectados();

            var uri = new Uri(Navigation.Uri);
            Servidor = $"{uri.Scheme}://{uri.Host}:{uri.Port}";

            return base.OnInitializedAsync();
        }

        private async Task LaunchCacheBuilderOdoo(dynamic document)
        {
            devices = chatHub.GetDevices();
            StateHasChanged();
        }

        private async Task ClearDevices()
        {
            devices = null;
            chatHub.ClearDevices();
            StateHasChanged();
        }

        private async Task RequireInfoDevice(ConnectedDevice device)
        {
            //device = 
            await chatHub.RequireInfoDevice(device.Id);
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
            //Agregamos datos dummy a Elements
            Elements = new List<ConnectedDevice>
            {
                new ConnectedDevice { Id = "1", AppName = "Device 1" },
                new ConnectedDevice { Id = "2", AppName = "Device 2" },
                new ConnectedDevice { Id = "3", AppName = "Device 3" }
            };
        }

        //private async Task SendMessageDevice(ConnectedDevice device)
        //{
        //    string Title = "Enviar mensaje";
        //    var parameters = new ModalParameters();
        //    string Message = $@"Ingrese el texto:";
        //    var options = new ModalOptions
        //    {
        //        UseCustomLayout = true,
        //        DisableBackgroundCancel = true
        //    };

        //    parameters = new ModalParameters();
        //    parameters.Add(nameof(DisplayPrompt.Message), Message);
        //    parameters.Add(nameof(DisplayPrompt.PromptValue), "");
        //    var messageForm = modalService.Show<DisplayPrompt>(Title, parameters, options);
        //    var result = await messageForm.Result;

        //    if (result.Data != null)
        //    {
        //        string new_message = result.Data.ToString();

        //        if (new_message != null && new_message != string.Empty && new_message != "")
        //        {
        //            await chatHub.SendMessageDevice(device.Id, "Servidor", new_message);
        //            StateHasChanged();
        //        }
        //    }
        //}
        //        

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
    }
}
