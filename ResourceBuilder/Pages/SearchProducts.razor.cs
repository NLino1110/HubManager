using Blazored.Modal;
using Blazored.Modal.Services;
using Blazored.Toast.Services;
using BlazorXTabs;
using Microsoft.AspNetCore.Components;
using Models.DMSA.Shared.Structs;
using ResourceBuilder.Data;

namespace ResourceBuilder.Pages
{
    public partial class SearchProducts
    {
        private XTabs _innerXTabs { get; set; }

        private async Task ShowSpinner()
        {
            _spinnerService.Show();
            await Task.Delay(500);
            _spinnerService.Hide();
        }

        protected override async Task OnInitializedAsync()
        {
            //_spinnerService.Show();
            //await Task.Delay(500);
            //_spinnerService.Hide();
        }
    }
}
