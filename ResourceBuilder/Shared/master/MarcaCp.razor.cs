using Blazored.Modal;
using Blazored.Modal.Services;
using BlazorTable;
using BlazorTable.Components.ServerSide;
using BlazorTable.Interfaces;
using DataSourceManager;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.EntityFrameworkCore;
using Models.DMSA.Mbw.Core;
using Models.DMSA.Mbw.Inventario;
using System.Linq;

namespace ResourceBuilder.Shared.master
{
    public partial class MarcaCp
    {
        [Inject]
        IConfiguration configuration { get; set; }
        
        private ITable<GenMarca> Table;

        private List<GenMarca> dataSource = null;
        private List<GenEmpresa> empresas = null;

        public Dictionary<string, object> AdditionalAttributes = new Dictionary<string, object>();

        [Inject] public BlazorDownloadFile.IBlazorDownloadFileService BlazorDownloadFileService { get; set; }

        private int ItemForRemove;        

        DataSourceManager.AppDbContext appDbContext = new DataSourceManager.AppDbContext();

        [Inject]
        public IModalService modalService { get; set; }

        private int id_param_01 { get; set; }
        private string input_search_param_var = string.Empty;

        private IDataLoader<GenMarca> _loader;

        private IEnumerable<GenMarca> data;

        [Inject]
        private HttpClient httpClient { get; set; }

       
        private async Task Create()
        {
            //var parameters = new ModalParameters();

            //parameters.Add(nameof(Shared.Company.ModalCompanyCRUD.selectedItem), new Models.Custom.Company());
            ////parameters.Add(nameof(ModalDocData.documentItemXml), documentItemXml);
            //var options = new ModalOptions
            //{
            //    UseCustomLayout = true,
            //    DisableBackgroundCancel = true
            //};
            //var messageForm = modalService.Show<Shared.Company.ModalCompanyCRUD>("Nueva compañia ", parameters, options);

            //var result = await messageForm.Result;

            //if (!result.Cancelled)
            //{
            //    //toastService.ShowInfo("No puede continuar, no hay productos modificados.");
            //    //StateHasChanged();
            //}
        }

        private async Task SaveChanges(master.Marca selectedItem)
        {
            var parameters = new ModalParameters();

            //parameters.Add(nameof(Shared.Company.ModalCompanyCRUD.selectedItem), selectedItem);
            ////parameters.Add(nameof(ModalDocData.documentItemXml), documentItemXml);
            //var options = new ModalOptions
            //{
            //    UseCustomLayout = true,
            //    DisableBackgroundCancel = true
            //};
            //var messageForm = modalService.Show<Shared.Company.ModalCompanyCRUD>("Modificar datos de compañia ", parameters, options);

            //var result = await messageForm.Result;

            //if (!result.Cancelled)
            //{
            //    //toastService.ShowInfo("No puede continuar, no hay productos modificados.");
            //    //StateHasChanged();
            //}
        }

        private async Task LaunchEdition(GenMarca selectedItem)
        {
            var parameters = new ModalParameters();

            //parameters.Add(nameof(Shared.Company.ModalCompany.companyItem), selectedItem);
            ////parameters.Add(nameof(ModalDocData.documentItemXml), documentItemXml);
            //var options = new ModalOptions
            //{
            //    //UseCustomLayout = true,
            //    DisableBackgroundCancel = true,
            //    Size = ModalSize.Large
            //};

            //var messageForm = modalService.Show<Shared.Company.ModalCompany>("Datos de Compañia " + selectedItem.name, parameters, options);

            //var result = await messageForm.Result;

            //if (!result.Cancelled)
            //{
            //    //toastService.ShowInfo("No puede continuar, no hay productos modificados.");
            //    //StateHasChanged();
            //}

        }

        private async Task<bool> Delete(int id)
        {
            

            return true;
        }

        protected async Task FillData()
        {
            //dynamic dataParams = new ExpandoObject();

            //Company<object> dynamicEntity = new Company<object>();
            //CoreResponse response = await dynamicEntity.GetAsync(dataParams);
            //companies = response.data;
            
            //dataSource = await appDbContext.GENMARCAS.Where(c => c.Descripcion.Contains("PALLADIO")).ToListAsync();
            //dataSource = await appDbContext.GENMARCAS.ToListAsync();
            await FillData(id_param_01.ToString());
        }

        private async Task FillData(string id)
        {
            try
            {                
                if (appDbContext.GENMARCAS == null)
                {
                    Console.WriteLine($"Error LoadData : EF document_tpl null");
                    return;
                }

                id_param_01 = int.Parse(id);

                dataSource = await appDbContext.GENMARCAS.Where(c => c.Descripcion.Contains(input_search_param_var) && c.CodEmpresaMarca == id_param_01).ToListAsync();

                foreach (var itemSource in dataSource)
                {
                    itemSource.Empresa = appDbContext.GENEMPRESAS.Where(Data => Data.CodEmpresa == itemSource.CodEmpresaMarca)?.FirstOrDefault();
                }
                //MakeHtml(data, HtmlForPreview);
                Console.WriteLine("Consulta terminada");
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error: {e.Message}");
                Console.WriteLine($"Error: {e}");
            }
        }

        protected override async Task OnInitializedAsync()
        {
            await LoadCompanies();
            await FillData();
            //await base.GetLoggedUser();

            _loader = new _DataLoader(httpClient);            

            //data = (await _loader.LoadDataAsync(filterData)).Records;
        }

        private void OnChangeTable(ChangeEventArgs __e)
        {
            Console.WriteLine("Tabla modificada");
        }

        private EventCallback<MouseEventArgs> __LaunchEdition { get; set; }

        protected async Task LoadCompanies()
        {
            empresas = await appDbContext.GENEMPRESAS.ToListAsync();
        }

        private async void OnUpdated(ChangeEventArgs e)
        {
            await FillData(e.Value.ToString());
        }

        public class _DataLoader : IDataLoader<GenMarca>
        {            
            private readonly HttpClient _client;
            public _DataLoader(HttpClient client)
            {
                _client = client;                
            }

            public async Task<PaginationResult<GenMarca>> LoadDataAsync(FilterData parameters)
            {
                //var data = await _client.GetFromJsonAsync<GenMarca[]>("sample-data/MOCK_DATA.json");
                //IQueryable<GenMarca> query = data.AsQueryable();

                DataSourceManager.AppDbContext appDbContext = new DataSourceManager.AppDbContext();
                //IQueryable<GenMarca> query = appDbContext.GENEMPRESAS.AsQueryable();
                var query = appDbContext.GENMARCAS.AsEnumerable();

                if (parameters?.Query != null)
                {
                    //query = query.Where(
                    //    x => x.email.ToLowerInvariant().Contains(parameters.Query.ToLowerInvariant()) ||
                    //         x.full_name.ToLowerInvariant().Contains(parameters.Query.ToLowerInvariant()));
                    //
                    query = query.Where(x => x.Descripcion.ToLowerInvariant().Contains(parameters.Query.ToLowerInvariant())).AsEnumerable();
                }

                if (parameters?.OrderBy != null)
                {
                    var orderBy = parameters.OrderBy.Split(" ");
                    if (orderBy.Length == 2)
                    {
                        var isSortDescending = orderBy[1] == "desc";
                        var prop = typeof(GenMarca).GetProperty(orderBy[0]);
                        query = isSortDescending ? query.OrderByDescending(x => prop.GetValue(x, null))
                            : query.OrderBy(x => prop.GetValue(x, null));
                    }
                }

                var results = parameters?.Top.HasValue ?? false ?
                    query.Skip(parameters.Skip.GetValueOrDefault())
                    .Take(parameters.Top.Value).ToList() :
                    query.ToList();

                return new PaginationResult<GenMarca>
                {
                    Records = results,
                    Skip = parameters?.Skip ?? 0,
                    Total = query.ToList().Count,
                    Top = parameters?.Top ?? 0
                };
            }
        }

    }
}
