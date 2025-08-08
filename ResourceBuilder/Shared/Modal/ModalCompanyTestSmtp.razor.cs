using Blazored.Modal;
using Microsoft.AspNetCore.Components;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ResourceBuilder.Shared.Modal
{
    public partial class ModalCompanyTestSmtp
    {
        [CascadingParameter]
        BlazoredModalInstance BlazoredModal { get; set; } = default!;

        //[Parameter]
        //public string Message { get; set; }

        async Task SubmitForm() => await BlazoredModal.CloseAsync();
        async Task Cancel() => await BlazoredModal.CancelAsync();

        public Dictionary<string, object> AdditionalAttributes = new Dictionary<string, object>();
        private string emailToTest = "";
        private bool SucessTest { get; set; }
        private bool DoneTest { get; set; }
        private bool RunningTest { get; set; }
        [Parameter]
        public EmailSettings selectedItem { get; set; }
        private async Task InitTest()
        {
            RunningTest = true;

            await InvokeAsync(() =>
            {
                StateHasChanged();
            });

            //selectedItemData = selectedItem;
            ControllerManager.EmailManager.Sender sender = new ControllerManager.EmailManager.Sender();

            SucessTest = sender.TestSmtpV2(selectedItem, emailToTest);
            DoneTest = true;

            RunningTest = false;
            await InvokeAsync(() =>
            {
                StateHasChanged();
            });

            //await new Task(() => { });
        }
    }
}
