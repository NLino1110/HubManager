using Blazored.Modal;
using Blazored.Modal.Services;
using Blazored.Toast.Services;
using DMSA.Models.Odoo.Tools;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR;
using ResourceBuilder.Handlers;
using ResourceBuilder.Shared;

namespace ResourceBuilder.Pages
{
    public partial class HubManager
    {
        private string Servidor;

        [Inject]
        IToastService toastService { get; set; }

        [Inject]
        IModalService modalService { get; set; }

        [Inject]
        ChatHub chatHub { get; set; }

        ConnectionManager ConnectedUsers { get; set; }

        List<string> listado {  get; set; }

        List<ConnectedDevice> devices { get; set; }

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

        private async Task SendMessageDevice(ConnectedDevice device)
        {
            string Title = "Enviar mensaje";
            var parameters = new ModalParameters();
            string Message = $@"Ingrese el texto:";
            var options = new ModalOptions
            {
                UseCustomLayout = true,
                DisableBackgroundCancel = true
            };

            parameters = new ModalParameters();
            parameters.Add(nameof(DisplayPrompt.Message), Message);
            parameters.Add(nameof(DisplayPrompt.PromptValue), "");
            var messageForm = modalService.Show<DisplayPrompt>(Title, parameters, options);
            var result = await messageForm.Result;

            if (result.Data != null)
            {
                string new_message = result.Data.ToString();

                if (new_message != null && new_message != string.Empty && new_message != "")
                {
                    await chatHub.SendMessageDevice(device.Id, "Servidor", new_message);
                    StateHasChanged();
                }
            }
        }        
    }
}
