using System.Threading.Tasks;
using Blazored.Modal;
using Blazored.Modal.Services;
using Blazored.Toast.Services;
using DMSA.Models.Odoo.Tools;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.QuickGrid;
using Microsoft.AspNetCore.SignalR;
using ResourceBuilder.Data.Sqlite;
using ResourceBuilder.Data.Structs;
using ResourceBuilder.Handlers;


namespace ResourceBuilder.Pages.manager
{
    public partial class AppsAdmin
    {
        [Inject]
        IToastService toastService { get; set; }

        [Inject]
        IModalService modalService { get; set; }

        IQueryable<AppDeploy> dataSource { get; set; }

        PaginationState pagination = new PaginationState { ItemsPerPage = 15 };

        string nameFilter = string.Empty;

        protected override async Task OnInitializedAsync()
        {
            AppDeployDb appDeployDb = new AppDeployDb();
            dataSource = (await appDeployDb.GetItemsAsync()).AsQueryable();
            //listado = chatHub.ObtenerClientesConectados();
            //return base.OnInitializedAsync();

            //await appDeployDb.InsertAsync(new AppDeploy()
            //{
            //    Id = 1,
            //    Name = "POS",
            //    Description = "EL POS PUES",
            //    IsDeployed = true,
            //    Version = "1.0.1"
            //});
        }
    }
}
