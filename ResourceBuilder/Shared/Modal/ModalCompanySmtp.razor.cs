using Blazored.Modal;
using Microsoft.AspNetCore.Components;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ResourceBuilder.Shared.Modal
{
    public partial class ModalCompanySmtp
    {
        [CascadingParameter]
        BlazoredModalInstance BlazoredModal { get; set; } = default!;

        //[Parameter]
        //public string Message { get; set; }

        async Task SubmitForm() => await BlazoredModal.CloseAsync();
        async Task Cancel() => await BlazoredModal.CancelAsync();

        public Dictionary<string, object> AdditionalAttributes = new Dictionary<string, object>();
        
        
        [Parameter]
        public EmailSettings selectedItem { get; set; }
        private async Task SaveChanges()
        {
            DataSourceManager.AppDbContext appDbContext = new DataSourceManager.AppDbContext();
            appDbContext.Update(selectedItem);
            await appDbContext.SaveChangesAsync();
            await SubmitForm();
        }
    }
}
