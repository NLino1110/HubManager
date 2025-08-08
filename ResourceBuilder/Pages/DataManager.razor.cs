

using Blazored.Modal;
using Blazored.Modal.Services;
using Blazored.Toast.Services;
using BlazorXTabs;
using Microsoft.AspNetCore.Components;
using Models.DMSA.Shared.Structs;
using ResourceBuilder.Data;

namespace ResourceBuilder.Pages
{
    public partial class DataManager
    {
        [Inject]
        BuilderService builderService { get; set; }

        [Inject]
        IModalService modalService { get; set; }

        [Inject]
        IToastService toastService { get; set; }

        ItemBuild[] itemBuilds { get; set; }

        public Dictionary<string, object> AdditionalAttributesBusy =
            new Dictionary<string, object>();

        private XTabs _innerXTabs { get; set; }

        [CascadingParameter]
        BlazoredModalInstance BlazoredModal { get; set; } = default!;

    }
}
