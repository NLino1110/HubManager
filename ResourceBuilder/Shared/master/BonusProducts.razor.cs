using Blazored.Modal;
using Blazored.Modal.Services;
using Blazored.Toast;
using Blazored.Toast.Services;
using BlazorSpinner;
using BlazorTable;
using BlazorTable.Components.ServerSide;
using BlazorTable.Interfaces;
using CobranzasDMSA.Models.General.Core;
using DataSourceManager;
using Excubo.Generators.Blazor.ExperimentalDoNotUseYet;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.QuickGrid;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.EntityFrameworkCore;
using Models.DMSA.Mbw.Core;
using Models.DMSA.Mbw.Inventario;
using Models.DMSA.Mbw.Sales;
using Models.DMSA.Shared.Tools;
using Newtonsoft.Json;
using ResourceBuilder.ControllerManager.Ecommerce;
using ResourceBuilder.Services.Inventory;
using ResourceBuilder.Services.Sales;
using ResourceBuilder.Shared.Modal;
using RestSharp;
using System;
using System.Diagnostics;
using System.Diagnostics.PerformanceData;
using System.Linq;

namespace ResourceBuilder.Shared.master
{
    public class GroupedMarca
    {
        public int CodMarca { get; set; }
        public string Marca { get; set; }
        public int TotalArticulos { get; set; }
        public List<int> DistinctArticulos { get; set; }
        public List<RangoFecha> RangosFechas { get; set; }
    }

    public class RangoFecha
    {
        public DateTime FechaInicio { get; set; }
        public DateTime FechaFin { get; set; }
        public int TotalArticulosPorRango { get; set; }
    }

    public partial class BonusProducts
    {
        //DataSourceManager.AppDbContext appDbContext = new DataSourceManager.AppDbContext();

        [CascadingParameter]
        BlazorSpinner.SpinnerService _spinnerService { get; set; } = default!;

        IQueryable<GroupedMarca> groupedBrandTmp { get; set; }

        IQueryable<FacBonificadosXArticulo> dataSource { get; set; }
        IQueryable<GenMarca> dataSourceBrands { get; set; }

        PaginationState pagination = new PaginationState { ItemsPerPage = 15 };
        PaginationState pagination_brands = new PaginationState { ItemsPerPage = 15 };

        [Inject]
        IConfiguration configuration { get; set; }

        DateTimeOffset? StartDate { get; set; } = DateTime.Today.AddMonths(-1);
        DateTimeOffset? EndDate { get; set; } = DateTime.Today.AddDays(1).AddTicks(-1);

        private GenEmpresa SelectedCompany = null;
        public GenMarca SelectedBrand;
        //[Inject]
        //public BrandService brandService { get; set; }
        //private List<FacBonificadosXArticulo> dataSource = null;
        private List<GenEmpresa> empresas = null;

        //public Dictionary<string, object> AdditionalAttributes = new Dictionary<string, object>();
        public Dictionary<string, object> AdditionalAttributesBusy = new Dictionary<string, object>();

        [Inject] public BlazorDownloadFile.IBlazorDownloadFileService BlazorDownloadFileService { get; set; }

        [Inject]
        DataSourceManager.AppDbContext appDbContext { get; set; }

        [Inject]
        public IModalService modalService { get; set; }

        [Inject]
        IToastService toastService { get; set; }

        [Inject]
        private HttpClient httpClient { get; set; }

        private string input_search_param_var = string.Empty;

        private int clienteWeb = 15;


        List<string> multipleSelectionData;
        List<string> multipleSelectionTexts;
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

        private async Task LaunchEdition(FacBonificadosXArticulo selectedItem)
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

        protected async Task SearchData()
        {
            try
            {
                int[] codArticulos = new int[] { };

                List<GenArticulos> genArticulo = null;
                List<ArticulosXEmpresa> artXEmpresa = null;
                if (input_search_param_var != String.Empty)
                {
                    //SE BUSCAN LOS CODIGOS DE LOS ARTICULOS QUE SE HAN ENVIADO COMO PARAMETROS
                    if (int.TryParse(input_search_param_var, out int numero))
                    {
                        Console.WriteLine("El valor ingresado es un número: " + numero);

                        genArticulo = await appDbContext.GENARTICULOS.Where(a =>
                        a.CodArticulo == numero || a.CodAlterno.Contains(input_search_param_var)).ToListAsync();
                    }
                    else // if (input_search_param_var.All(char.IsLetter))
                    {
                        Console.WriteLine("El valor ingresado contiene solo caracteres.");
                        genArticulo = await appDbContext.GENARTICULOS
                            .Where(a => a.CodAlterno == input_search_param_var.Trim()
                            || a.Descripcion.Contains(input_search_param_var.Trim())).ToListAsync();
                    }

                    if (genArticulo == null)
                    {
                        toastService.ShowError("Datos de artículo no encontrados!");
                        return;
                    }

                    codArticulos = genArticulo.Select(articulo => articulo.CodArticulo).ToArray();
                    artXEmpresa = await appDbContext.ARTICULOSXEMPRESA
                            .Include(z => z.Articulo)
                            .Include(y => y.Marca)
                            .Where(Data =>
                        Data.CodEmpresa == SelectedCompany.CodEmpresa
                        && codArticulos.Contains(Data.CodArticulo)
                        && Data.CodEstado == 1)?
                        .ToListAsync();
                }
                else
                {
                    //SE BUSCAN TODOS LOS ARTICULOS RELACIONADOS CON LA EMPRESA Y MARCA SELECCIONADAS
                    if (SelectedBrand != null)
                    {           
                            artXEmpresa = await appDbContext.ARTICULOSXEMPRESA
                                .Include(z => z.Articulo)
                                .Include(y => y.Marca)
                                .Where(Data =>
                            Data.CodEmpresa == SelectedCompany.CodEmpresa
                            && Data.CodMarca == SelectedBrand.CodMarca
                            && Data.CodEstado == 1
                            &&
                            (
                            Data.ActivaWeb == "N" ||
                            Data.VentaAlmacenes == "S" ||
                            Data.VentaAlmacenes == "N"
                            )
                            )?.ToListAsync();                            
                    }
                    else
                    {
                        //SE BUSCAN TODOS LOS ARTICULOS RELACIONADOS CON LA EMPRESA SELECCIONADA
                        artXEmpresa = await appDbContext.ARTICULOSXEMPRESA
                            .Include(z => z.Articulo)
                            .Include(y => y.Marca)
                            .Where(Data =>
                        Data.CodEmpresa == SelectedCompany.CodEmpresa
                        && Data.CodEstado == 1)?
                        .ToListAsync();
                    }

                    codArticulos = artXEmpresa.Select(articulo => articulo.CodArticulo).ToArray();
                    genArticulo = artXEmpresa.Select(articulo => articulo.Articulo).ToList();
                }

                List<GenMarca> brandTemp = new List<GenMarca>();

                List<FacBonificadosXArticulo> dataSource_tmp = null;
                
                dataSource_tmp = await appDbContext.FACBONIFICADOSXARTICULO
                    .Where(c => c.CodEmpresa == SelectedCompany.CodEmpresa
                        && c.CodTipoCliente == clienteWeb
                        && c.FechaInicio >= StartDate
                        && c.FechaFin <= EndDate
                        && c.CodEstado == 1
                        && codArticulos.Contains(c.CodArticulo))
                    //.Take(2000)
                    .ToListAsync();
                
                if (artXEmpresa != null && dataSource_tmp.Count > 0)
                {
                    //foreach (var itemSource in artXEmpresa)
                    for (int i = 0; i < dataSource_tmp.Count; i++)
                    {
                        var itemSource = dataSource_tmp[i];

                        var ArticulosXEmpresaFound = artXEmpresa.
                            Where(x =>
                            x.CodArticulo == dataSource_tmp[i].CodArticulo 
                            && x.CodEmpresa == dataSource_tmp[i].CodEmpresa)
                            .FirstOrDefault();

                        dataSource_tmp[i].ArticulosXEmpresa = ArticulosXEmpresaFound;

                        //    var brandAlreadyAdded = brandTemp.Where(x => x.CodMarca == itemSource.CodMarca).FirstOrDefault();
                        //    if (brandAlreadyAdded == null)
                        //    {
                        //        var itemFound = await appDbContext.FACBONIFICADOSXARTICULO
                        //        .Where(c => c.CodEmpresa == SelectedCompany.CodEmpresa
                        //            && c.CodArticulo == itemSource.CodArticulo
                        //            && c.FechaInicio >= StartDate
                        //            && c.FechaFin <= EndDate
                        //            && c.CodEstado == 1)
                        //        .Take(1)
                        //        .FirstOrDefaultAsync();
                        //    }
                    }

                    dataSource = dataSource_tmp.AsQueryable();
                }
                else
                {

                    dataSource_tmp = new List<FacBonificadosXArticulo>();

                    foreach(var articulo in genArticulo)
                    {
                        var ArticulosXEmpresaFound = artXEmpresa.
                            Where(x =>
                            x.CodArticulo == articulo.CodArticulo
                            && x.CodEmpresa == SelectedCompany.CodEmpresa)
                            .FirstOrDefault();

                        if(ArticulosXEmpresaFound == null)
                        {
                            ArticulosXEmpresaFound = new ArticulosXEmpresa();
                            ArticulosXEmpresaFound.Articulo = articulo;
                            ArticulosXEmpresaFound.Marca = new GenMarca();
                            ArticulosXEmpresaFound.Marca.CodMarca = 0;
                            ArticulosXEmpresaFound.Marca.Descripcion = "-";
                        }

                        FacBonificadosXArticulo facBonificadosXArticulo = new FacBonificadosXArticulo();
                        facBonificadosXArticulo.Articulo = articulo;
                        facBonificadosXArticulo.CodBonificadoArticulo = 0;
                        facBonificadosXArticulo.CodArticulo = articulo.CodArticulo;
                        facBonificadosXArticulo.CodAgencia = 0;
                        facBonificadosXArticulo.ArticulosXEmpresa = ArticulosXEmpresaFound;
                        facBonificadosXArticulo.FechaInicio = DateTime.Now.AddYears(-1000);
                        facBonificadosXArticulo.FechaFin = DateTime.Now.AddYears(-1000);
                        facBonificadosXArticulo.MinimoAplicaDscto = 0;
                        facBonificadosXArticulo.PorcDescuento = 0;
                        facBonificadosXArticulo.Precio = 0;
                        facBonificadosXArticulo.ValorDescuento = 0;

                        dataSource_tmp.Add(facBonificadosXArticulo);                        
                    }

                    dataSource = dataSource_tmp.AsQueryable();

                    //toastService.ShowError("No se encontró en bonificados: " + 
                    //    genArticulo[0].CodArticulo + " - " +
                    //    genArticulo[0].CodAlterno + " - " +
                    //    genArticulo[0].Descripcion);

                    toastService.ShowError("No se encontró en bonificados");

                    return;                    
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error: {e.Message}");
                Console.WriteLine($"Error: {e}");
            }
        }


        protected async Task SearchDataCurrent()
        {
            try
            {
                int[] codArticulos = new int[] { };

                List<GenArticulos> genArticulo = null;
                List<ArticulosXEmpresa> artXEmpresa = null;
                if (input_search_param_var != String.Empty)
                {
                    //SE BUSCAN LOS CODIGOS DE LOS ARTICULOS QUE SE HAN ENVIADO COMO PARAMETROS
                    if (int.TryParse(input_search_param_var, out int numero))
                    {
                        Console.WriteLine("El valor ingresado es un número: " + numero);

                        genArticulo = await appDbContext.GENARTICULOS.Where(a =>
                        a.CodArticulo == numero || a.CodAlterno.Contains(input_search_param_var)).ToListAsync();
                    }
                    else // if (input_search_param_var.All(char.IsLetter))
                    {
                        Console.WriteLine("El valor ingresado contiene solo caracteres.");
                        genArticulo = await appDbContext.GENARTICULOS
                            .Where(a => a.CodAlterno == input_search_param_var.Trim()
                            || a.Descripcion.Contains(input_search_param_var.Trim())).ToListAsync();
                    }

                    if (genArticulo == null)
                    {
                        toastService.ShowError("Datos de artículo no encontrados!");
                        return;
                    }

                    codArticulos = genArticulo.Select(articulo => articulo.CodArticulo).ToArray();
                    artXEmpresa = await appDbContext.ARTICULOSXEMPRESA
                            .Include(z => z.Articulo)
                            .Include(y => y.Marca)
                            .Where(Data =>
                        Data.CodEmpresa == SelectedCompany.CodEmpresa
                        && codArticulos.Contains(Data.CodArticulo)
                        && Data.CodEstado == 1)?
                        .ToListAsync();
                }
                else
                {
                    //SE BUSCAN TODOS LOS ARTICULOS RELACIONADOS CON LA EMPRESA Y MARCA SELECCIONADAS
                    if (SelectedBrand != null)
                    {
                        artXEmpresa = await appDbContext.ARTICULOSXEMPRESA
                            .Include(z => z.Articulo)
                            .Include(y => y.Marca)
                            .Where(Data =>
                        Data.CodEmpresa == SelectedCompany.CodEmpresa
                        && Data.CodMarca == SelectedBrand.CodMarca
                        && Data.CodEstado == 1
                        &&
                        (
                        Data.ActivaWeb == "N" ||
                        Data.VentaAlmacenes == "S" ||
                        Data.VentaAlmacenes == "N"
                        )
                        )?.ToListAsync();
                    }
                    else
                    {
                        //SE BUSCAN TODOS LOS ARTICULOS RELACIONADOS CON LA EMPRESA SELECCIONADA
                        artXEmpresa = await appDbContext.ARTICULOSXEMPRESA
                            .Include(z => z.Articulo)
                            .Include(y => y.Marca)
                            .Where(Data =>
                        Data.CodEmpresa == SelectedCompany.CodEmpresa
                        && Data.CodEstado == 1)?
                        .ToListAsync();
                    }

                    codArticulos = artXEmpresa.Select(articulo => articulo.CodArticulo).ToArray();
                    genArticulo = artXEmpresa.Select(articulo => articulo.Articulo).ToList();
                }

                List<GenMarca> brandTemp = new List<GenMarca>();

                List<FacBonificadosXArticulo> dataSource_tmp = null;

                DateTime today = DateTime.Today;                
                DateTime fechaExclusion = new DateTime(today.Year, 12, 31);

                dataSource_tmp = await appDbContext.FACBONIFICADOSXARTICULO
                    .Where(c => c.CodEmpresa == SelectedCompany.CodEmpresa
                        && c.CodEstado == 1
                        && c.CodTipoCliente == clienteWeb
                        && codArticulos.Contains(c.CodArticulo)
                        && (today <= c.FechaFin && c.FechaFin != fechaExclusion)
                        ).ToListAsync();

                if (artXEmpresa != null && dataSource_tmp.Count > 0)
                {
                    //foreach (var itemSource in artXEmpresa)
                    for (int i = 0; i < dataSource_tmp.Count; i++)
                    {
                        var itemSource = dataSource_tmp[i];

                        var ArticulosXEmpresaFound = artXEmpresa.
                            Where(x =>
                            x.CodArticulo == dataSource_tmp[i].CodArticulo
                            && x.CodEmpresa == dataSource_tmp[i].CodEmpresa)
                            .FirstOrDefault();

                        dataSource_tmp[i].ArticulosXEmpresa = ArticulosXEmpresaFound;
                    }

                    dataSource = dataSource_tmp.AsQueryable();
                }
                else
                {

                    dataSource_tmp = new List<FacBonificadosXArticulo>();

                    foreach (var articulo in genArticulo)
                    {
                        var ArticulosXEmpresaFound = artXEmpresa.
                            Where(x =>
                            x.CodArticulo == articulo.CodArticulo
                            && x.CodEmpresa == SelectedCompany.CodEmpresa)
                            .FirstOrDefault();

                        if (ArticulosXEmpresaFound == null)
                        {
                            ArticulosXEmpresaFound = new ArticulosXEmpresa();
                            ArticulosXEmpresaFound.Articulo = articulo;
                            ArticulosXEmpresaFound.Marca = new GenMarca();
                            ArticulosXEmpresaFound.Marca.CodMarca = 0;
                            ArticulosXEmpresaFound.Marca.Descripcion = "-";
                        }

                        FacBonificadosXArticulo facBonificadosXArticulo = new FacBonificadosXArticulo();
                        facBonificadosXArticulo.Articulo = articulo;
                        facBonificadosXArticulo.CodBonificadoArticulo = 0;
                        facBonificadosXArticulo.CodArticulo = articulo.CodArticulo;
                        facBonificadosXArticulo.CodAgencia = 0;
                        facBonificadosXArticulo.ArticulosXEmpresa = ArticulosXEmpresaFound;
                        facBonificadosXArticulo.FechaInicio = DateTime.Now.AddYears(-1000);
                        facBonificadosXArticulo.FechaFin = DateTime.Now.AddYears(-1000);
                        facBonificadosXArticulo.MinimoAplicaDscto = 0;
                        facBonificadosXArticulo.PorcDescuento = 0;
                        facBonificadosXArticulo.Precio = 0;
                        facBonificadosXArticulo.ValorDescuento = 0;

                        dataSource_tmp.Add(facBonificadosXArticulo);
                    }

                    dataSource = dataSource_tmp.AsQueryable();

                    toastService.ShowError("No se encontró en bonificados");

                    return;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error: {e.Message}");
                Console.WriteLine($"Error: {e}");
            }
        }

        protected async Task FillData()
        {
            if (SelectedCompany == null)
            {
                toastService.ShowError("Compañia requerida!");
                return;
            }

            //dynamic dataParams = new ExpandoObject();

            //Company<object> dynamicEntity = new Company<object>();
            //CoreResponse response = await dynamicEntity.GetAsync(dataParams);
            //companies = response.data;

            //dataSource = await appDbContext.GENMARCAS.Where(c => c.Descripcion.Contains("PALLADIO")).ToListAsync();
            //dataSource = await appDbContext.GENMARCAS.ToListAsync();
            //await FillData(id_param_01.ToString());

            InvokeAsync(async () =>
            {
                //StateHasChanged();
                //await Task.Delay(100);
                _spinnerService.Show();
            });

            try
            {
                List<GenArticulos> genArticulo = null;
                if (input_search_param_var != String.Empty)
                {
                    if (int.TryParse(input_search_param_var, out int numero))
                    {
                        Console.WriteLine("El valor ingresado es un número: " + numero);

                        genArticulo = await appDbContext.GENARTICULOS.Where(a =>
                        a.CodArticulo == numero || a.CodAlterno.Contains(input_search_param_var)).ToListAsync();
                    }
                    else // if (input_search_param_var.All(char.IsLetter))
                    {
                        Console.WriteLine("El valor ingresado contiene solo caracteres.");
                        genArticulo = await appDbContext.GENARTICULOS
                            .Where(a => a.CodAlterno == input_search_param_var.Trim()
                            || a.Descripcion.Contains(input_search_param_var.Trim())).ToListAsync();

                        //genArticulo = await appDbContext.GENARTICULOS
                        //    .Where(a => a.CodAlterno == input_search_param_var.Trim()).ToListAsync();
                    }
                    //else
                    //{
                    //Console.WriteLine("El valor ingresado contiene una combinación de números y letras.");
                    //}

                    if (genArticulo == null)
                    {
                        toastService.ShowError("Datos de artículo no encontrados!");
                        return;
                    }
                }

                List<FacBonificadosXArticulo> dataSource_tmp = null;

                if (genArticulo == null)
                {
                    dataSource_tmp = await appDbContext.FACBONIFICADOSXARTICULO
                        .Where(c => c.CodEmpresa == SelectedCompany.CodEmpresa
                            && c.FechaInicio >= StartDate
                            && c.FechaFin <= EndDate
                            && c.CodEstado == 1
                            )
                        //.Take(2000)
                        .ToListAsync();
                }
                else
                {
                    int[] codArticulos = genArticulo.Select(articulo => articulo.CodArticulo).ToArray();
                    dataSource_tmp = await appDbContext.FACBONIFICADOSXARTICULO
                        .Where(c => c.CodEmpresa == SelectedCompany.CodEmpresa
                            && c.FechaInicio >= StartDate
                            && c.FechaFin <= EndDate
                            && c.CodEstado == 1
                            && codArticulos.Contains(c.CodArticulo))
                        .Take(2000)
                        .ToListAsync();
                }

                List<FacBonificadosXArticulo> dataSource_tmp_2 = new List<FacBonificadosXArticulo>();

                if (SelectedBrand != null)
                {
                    if (dataSource_tmp != null)
                        foreach (var itemSource in dataSource_tmp)
                        {
                            //    itemSource.Empresa = appDbContext.GENEMPRESAS.Where(Data => Data.CodEmpresa == itemSource.CodEmpresaMarca)?.FirstOrDefault();                    
                            var artXEmpresa = appDbContext.ARTICULOSXEMPRESA
                                .Include(z => z.Articulo)
                                .Include(y => y.Marca)
                                .Where(Data =>
                            Data.CodEmpresa == itemSource.CodEmpresa
                            && Data.CodArticulo == itemSource.CodArticulo
                            && Data.CodMarca == SelectedBrand.CodMarca)?.FirstOrDefault();

                            if (artXEmpresa != null)
                            {
                                itemSource.ArticulosXEmpresa = artXEmpresa;
                                dataSource_tmp_2.Add(itemSource);
                            }
                            else
                            {
                                //dataSource_tmp.Remove(itemSource);
                                Debug.WriteLine("No encontrado con marca seleccionada " + itemSource.CodArticulo + " ");
                            }
                        }

                    dataSource = dataSource_tmp_2.AsQueryable();
                }
                else
                {
                    if (dataSource_tmp != null)
                        foreach (var itemSource in dataSource_tmp)
                        {
                            var artXEmpresa = appDbContext.ARTICULOSXEMPRESA
                                .Include(z => z.Articulo)
                                .Include(y => y.Marca)
                                .Where(Data =>
                            Data.CodEmpresa == itemSource.CodEmpresa
                            && Data.CodArticulo == itemSource.CodArticulo)?.FirstOrDefault();

                            if (artXEmpresa != null)
                            {
                                itemSource.ArticulosXEmpresa = artXEmpresa;
                            }
                        }

                    dataSource = dataSource_tmp.AsQueryable();
                }

                await pagination.SetCurrentPageIndexAsync(0);
                //MakeHtml(data, HtmlForPreview);
                Console.WriteLine("Consulta terminada");
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error: {e.Message}");
                Console.WriteLine($"Error: {e}");
            }

            InvokeAsync(async () =>
            {
                //StateHasChanged();
                await Task.Delay(100);
                _spinnerService.Hide();
            });

        }

        async public Task OnClickSearchNonBon()
        {
            if (SelectedCompany == null)
            {
                toastService.ShowError("Compañia requerida!");
                return;
            }

            _spinnerService.Show();

            InvokeAsync(async () =>
            {
                await Task.Delay(100);
                //await FillData();
                await SearchDataNonBon();
                StateHasChanged();
                _spinnerService.Hide();
            });
        }

        async public Task OnClickSearchGeneral()
        {
            if (SelectedCompany == null)
            {
                toastService.ShowError("Compañia requerida!");
                return;
            }

            _spinnerService.Show();
            
            InvokeAsync(async () =>
            {            
                await Task.Delay(100);
                //await FillData();
                await SearchData();
                StateHasChanged();
                _spinnerService.Hide();
            });
            
        }

        async public Task OnClickSearchCurrent()
        {
            if (SelectedCompany == null)
            {
                toastService.ShowError("Compañia requerida!");
                return;
            }

            _spinnerService.Show();

            await InvokeAsync(async () =>
            {
                await Task.Delay(100);
                //await FillData();
                await SearchDataCurrent();
                StateHasChanged();
                _spinnerService.Hide();
            });

        }

        async public Task OnClickSearchBrands()
        {            
            _spinnerService.Show();
            //await SearchBrands();
            //_spinnerService.Show();
            InvokeAsync(async () =>
            {
                //StateHasChanged();
                await Task.Delay(100);
                //StateHasChanged();
                await SearchBrandsByResult();
                //await SearchBrands();
                StateHasChanged();
                _spinnerService.Hide();
            });
            //_ = InvokeAsync(SearchBrands);
            //await Task.Delay(500);
            //_spinnerService.Hide();
        }

        async public Task OnClickSearchBrandsCurrent()
        {
            _spinnerService.Show();
            //await SearchBrands();
            //_spinnerService.Show();
            InvokeAsync(async () =>
            {
                //StateHasChanged();
                await Task.Delay(100);
                //StateHasChanged();
                await SearchBrandsCurrent();
                StateHasChanged();
                _spinnerService.Hide();
            });
            //_ = InvokeAsync(SearchBrands);
            //await Task.Delay(500);
            //_spinnerService.Hide();
        }

        protected async Task SearchDataNonBon()
        {
            try
            {
                //int[] codArticulos = new int[] { };

                //List<GenArticulos> genArticulo = null;
                List<ArticulosXEmpresa> artXEmpresa = null;

                //if (input_search_param_var != String.Empty)
                //{
                //    //SE BUSCAN LOS CODIGOS DE LOS ARTICULOS QUE SE HAN ENVIADO COMO PARAMETROS
                //    if (int.TryParse(input_search_param_var, out int numero))
                //    {
                //        Console.WriteLine("El valor ingresado es un número: " + numero);

                //        genArticulo = await appDbContext.GENARTICULOS.Where(a =>
                //        a.CodArticulo == numero || a.CodAlterno.Contains(input_search_param_var)).ToListAsync();
                //    }
                //    else // if (input_search_param_var.All(char.IsLetter))
                //    {
                //        Console.WriteLine("El valor ingresado contiene solo caracteres.");
                //        genArticulo = await appDbContext.GENARTICULOS
                //            .Where(a => a.CodAlterno == input_search_param_var.Trim()
                //            || a.Descripcion.Contains(input_search_param_var.Trim())).ToListAsync();
                //    }

                //    if (genArticulo == null)
                //    {
                //        toastService.ShowError("Datos de artículo no encontrados!");
                //        return;
                //    }

                //    codArticulos = genArticulo.Select(articulo => articulo.CodArticulo).ToArray();
                //}
                //else
                {
                    //SE BUSCAN TODOS LOS ARTICULOS RELACIONADOS CON LA EMPRESA Y MARCA SELECCIONADAS
                    if (SelectedBrand != null)
                    {
                        artXEmpresa = await appDbContext.ARTICULOSXEMPRESA
                            .Include(z => z.Articulo)
                            .Include(y => y.Marca)
                            .Where(Data =>
                        Data.CodEmpresa == SelectedCompany.CodEmpresa
                        && Data.CodMarca == SelectedBrand.CodMarca
                        && Data.CodEstado == 1                        
                        &&
                        (
                        Data.ActivaWeb == "N" ||
                        Data.VentaAlmacenes == "S" ||
                        Data.VentaAlmacenes == "N"
                        )
                        )?.ToListAsync();
                    }
                    else
                    {

                        ///SELECT RTRIM(XMLAGG(XMLELEMENT(e, codarticulo || ',')).EXTRACT('//text()').GETCLOBVAL(), ',') AS codarticulos
                        ///FROM articulosxempresa
                        ///WHERE codempresa = 2
                        ///AND codestado = 1
                        ///AND activa_web = 'S';

                        //SE BUSCAN TODOS LOS ARTICULOS RELACIONADOS CON LA EMPRESA SELECCIONADA
                        artXEmpresa = await appDbContext.ARTICULOSXEMPRESA
                            .Include(z => z.Articulo)
                            .Include(y => y.Marca)
                            .Where(Data =>
                        Data.CodEmpresa == SelectedCompany.CodEmpresa
                        && Data.CodArticulo > 26325
                        && Data.CodEstado == 1                        
                        && Data.ActivaWeb == "S")?
                        .ToListAsync();
                    }

                    //codArticulos = artXEmpresa.Select(articulo => articulo.CodArticulo).ToArray();
                    //genArticulo = artXEmpresa.Select(articulo=>articulo.Articulo).ToList();
                }

                List<GenMarca> brandTemp = new List<GenMarca>();

                List<FacBonificadosXArticulo> dataSource_tmp = null;               
                
                dataSource_tmp = new List<FacBonificadosXArticulo>();

                foreach (var articuloXEmp in artXEmpresa)
                {
                    FacBonificadosXArticulo facBonificadosXArticulo = new FacBonificadosXArticulo();
                    facBonificadosXArticulo.Articulo = articuloXEmp.Articulo;
                    facBonificadosXArticulo.CodBonificadoArticulo = 0;
                    facBonificadosXArticulo.CodArticulo = articuloXEmp.CodArticulo;
                    facBonificadosXArticulo.CodAgencia = 0;
                    facBonificadosXArticulo.ArticulosXEmpresa = articuloXEmp;
                    facBonificadosXArticulo.FechaInicio = DateTime.Now.AddYears(-1000);
                    facBonificadosXArticulo.FechaFin = DateTime.Now.AddYears(-1000);
                    facBonificadosXArticulo.MinimoAplicaDscto = 0;
                    facBonificadosXArticulo.PorcDescuento = 0;
                    facBonificadosXArticulo.Precio = 0;
                    facBonificadosXArticulo.ValorDescuento = 0;

                    dataSource_tmp.Add(facBonificadosXArticulo);
                }

                dataSource = dataSource_tmp.AsQueryable();

                //toastService.ShowError("No se encontró en bonificados: " + 
                //    genArticulo[0].CodArticulo + " - " +
                //    genArticulo[0].CodAlterno + " - " +
                //    genArticulo[0].Descripcion);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error: {e.Message}");
                Console.WriteLine($"Error: {e}");
            }
        }

       


        async public Task SearchBrandsByResult()
        {            
            try
            {
                //AGRUPAR SIN TOTAL EN RANGOS DE FECHA
                //var groupedDataTmp = dataSource
                //    .AsEnumerable()
                //    .Where(x => x.ArticulosXEmpresa?.Marca != null)
                //    .GroupBy(x => (x.ArticulosXEmpresa.Marca.Descripcion ?? string.Empty).Trim(),
                //             StringComparer.OrdinalIgnoreCase)
                //    .Select(g => new
                //    {
                //        Marca = g.Key,
                //        TotalArticulos = g
                //            .Select(x => x.CodArticulo)
                //            .Distinct()
                //            .Count(),
                //        DistinctArticulos = g
                //            .Select(x => x.CodArticulo)
                //            .Distinct()
                //            .ToList(),
                //        RangosFechas = g
                //            .Select(x => new { x.FechaInicio, x.FechaFin })
                //            .Distinct()
                //            .ToList()
                //    })
                //    .ToList();

                //AGRUPAR CON TOTAL EN RANGOS DE FECHA
                var groupedBrandDataTmp = dataSource
                    .AsEnumerable()
                    .Where(x => x.ArticulosXEmpresa?.Marca != null)
                    .GroupBy(x => new
                    {
                        CodMarca = x.ArticulosXEmpresa.Marca.CodMarca,
                        Descripcion = (x.ArticulosXEmpresa.Marca.Descripcion ?? string.Empty).Trim()
                    })
                    .Select(g => new GroupedMarca
                    {
                        CodMarca = g.Key.CodMarca,
                        Marca = g.Key.Descripcion,
                        TotalArticulos = g.Select(x => x.CodArticulo).Distinct().Count(),
                        DistinctArticulos = g.Select(x => x.CodArticulo).Distinct().ToList(),
                        RangosFechas = g
                            .GroupBy(x => new { x.FechaInicio, x.FechaFin })  // Agrupamos por rango de fecha
                            .Select(r => new RangoFecha
                            {
                                FechaInicio = r.Key.FechaInicio,
                                FechaFin = r.Key.FechaFin,
                                TotalArticulosPorRango = r.Select(x => x.CodArticulo).Distinct().Count()
                            })
                            .ToList()
                    })
                    .ToList();

                groupedBrandTmp = groupedBrandDataTmp.AsQueryable();

                var groupedData = dataSource
                    .AsEnumerable()
                    .DistinctBy(x => (x.ArticulosXEmpresa?.Marca.Descripcion ?? string.Empty).Trim(), StringComparer.OrdinalIgnoreCase)
                    .Select(x => new FacBonificadosXArticulo
                    {
                        CodArticulo = x.CodArticulo,
                        ArticulosXEmpresa = x.ArticulosXEmpresa
                    })
                    .ToList();

                List<GenMarca> brandTemp = new List<GenMarca>();
                                
                for (int i = 0; i < groupedData.Count; i++)
                {
                    var itemSource = groupedData[i];

                    var brandAlreadyAdded = brandTemp.Where(x => x.CodMarca == itemSource.ArticulosXEmpresa.CodMarca).FirstOrDefault();

                    if (brandAlreadyAdded == null)
                    {                       
                        Debug.WriteLine("Nueva Marca agregada " + itemSource.ArticulosXEmpresa.Marca.Descripcion);                        
                        brandTemp.Add(itemSource.ArticulosXEmpresa.Marca);                        
                    }
                    else
                    {
                        Debug.WriteLine("Marca ya agregada " + itemSource.ArticulosXEmpresa.Marca.Descripcion);
                    }
                }

                if (brandTemp.Count > 0)
                {                    
                    dataSourceBrands = brandTemp.Distinct().AsQueryable();
                    Console.WriteLine(dataSourceBrands.Count());
                }                

                await pagination_brands.SetCurrentPageIndexAsync(0);                
                Console.WriteLine("Consulta terminada");
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error: {e.Message}");
                Console.WriteLine($"Error: {e}");
            }

            _spinnerService.Hide();
        }

        async public Task SearchBrands()
        {
            if (SelectedCompany == null)
            {
                toastService.ShowError("Compañia requerida!");
                return;
            }

            //_ = InvokeAsync(StateHasChanged);            
            //_spinnerService.Show();            

            try
            {
                var artXEmpresa = await appDbContext.ARTICULOSXEMPRESA
                        .Include(z => z.Articulo)
                        .Include(y => y.Marca)
                        .Where(Data =>
                    Data.CodEmpresa == SelectedCompany.CodEmpresa
                    && Data.CodEstado == 1 && 
                    (
                    Data.ActivaWeb == "N" ||
                    Data.VentaAlmacenes == "S" ||
                    Data.VentaAlmacenes == "N"
                    ))
                    //.Take(10)
                    .ToListAsync();

                List<GenMarca> brandTemp = new List<GenMarca>();

                //foreach (var itemSource in artXEmpresa)
                for (int i = 0; i < artXEmpresa.Count; i++)
                {
                    var itemSource = artXEmpresa[i];

                    var brandAlreadyAdded = brandTemp.Where(x => x.CodMarca == itemSource.CodMarca).FirstOrDefault();
                    if (brandAlreadyAdded == null)
                    {
                        var itemFound = await appDbContext.FACBONIFICADOSXARTICULO
                        .Where(c => c.CodEmpresa == SelectedCompany.CodEmpresa
                            && c.CodArticulo == itemSource.CodArticulo
                            && c.FechaInicio >= StartDate
                            && c.FechaFin <= EndDate
                            && c.CodEstado == 1)
                        .Take(1)
                        .FirstOrDefaultAsync();
                        //if (artXEmpresa != null)
                        //{
                        //    itemSource.ArticulosXEmpresa = artXEmpresa;
                        //}

                        if (itemFound != null)
                        {
                            Debug.WriteLine("Nueva Marca agregada " + itemSource.Marca.Descripcion);
                            //artXEmpresa.Marca.Descripcion
                            brandTemp.Add(itemSource.Marca);
                            artXEmpresa.RemoveAll(x => x.CodMarca == itemSource.CodMarca);
                        }
                    }
                    else
                    {
                        Debug.WriteLine("Marca ya agregada " + itemSource.Marca.Descripcion);
                    }
                }

                if (brandTemp.Count > 0)
                {
                    //brandTemp = brandTemp.GroupBy(br => br.Descripcion).Distinct().ToList();
                    //brandTemp = brandTemp.Distinct().ToList();
                    dataSourceBrands = brandTemp.Distinct().AsQueryable();
                    Console.WriteLine(dataSourceBrands.Count());
                }
                //dataSource = dataSource_tmp.AsQueryable();

                await pagination_brands.SetCurrentPageIndexAsync(0);
                //MakeHtml(data, HtmlForPreview);
                Console.WriteLine("Consulta terminada");
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error: {e.Message}");
                Console.WriteLine($"Error: {e}");
            }

            _spinnerService.Hide();
        }

        async public Task SearchBrandsCurrent()
        {
            if (SelectedCompany == null)
            {
                toastService.ShowError("Compañia requerida!");
                return;
            }

            //_ = InvokeAsync(StateHasChanged);            
            //_spinnerService.Show();

            try
            {
                //var artXEmpresa = await appDbContext.ARTICULOSXEMPRESA
                //        .Include(z => z.Articulo)
                //        .Include(y => y.Marca)
                //        .Where(Data =>
                //    Data.CodEmpresa == SelectedCompany.CodEmpresa
                //    && Data.CodEstado == 1 
                //    //&&
                //    //(
                //    //Data.ActivaWeb == "N" ||
                //    //Data.VentaAlmacenes == "S" ||
                //    //Data.VentaAlmacenes == "N"
                //    //)
                //    )                    
                //    .ToListAsync();

                var artXEmpresa = await appDbContext.ARTICULOSXEMPRESA
                      .Include(z => z.Articulo)
                      .Include(y => y.Marca)
                      .Where(Data =>
                  Data.CodEmpresa == SelectedCompany.CodEmpresa
                  && Data.CodEstado == 1)?
                  .ToListAsync();

                List<GenMarca> brandTemp = new List<GenMarca>();

                DateTime today = DateTime.Today;                
                DateTime fechaExclusion = new DateTime(today.Year, 12, 31);

                var itemsFound = await appDbContext.FACBONIFICADOSXARTICULO
                       .Where(c => c.CodEmpresa == SelectedCompany.CodEmpresa                           
                           && c.CodEstado == 1
                            && (today <= c.FechaFin && c.FechaFin != fechaExclusion)
                        )
                       .ToListAsync();

                for (int i = 0; i < artXEmpresa.Count; i++)
                {
                    var itemSource = artXEmpresa[i];

                    var brandAlreadyAdded = brandTemp.Where(x => x.CodMarca == itemSource.CodMarca).FirstOrDefault();
                    if (brandAlreadyAdded == null)
                    {
                        var itemFound = itemsFound.Where(c => c.CodArticulo == itemSource.CodArticulo)
                            .FirstOrDefault();

                        if (itemFound != null)
                        {
                            Debug.WriteLine("Nueva Marca agregada " + itemSource.Marca.Descripcion);
                            //artXEmpresa.Marca.Descripcion
                            brandTemp.Add(itemSource.Marca);
                            artXEmpresa.RemoveAll(x => x.CodMarca == itemSource.CodMarca);
                        }
                    }
                    else
                    {
                        Debug.WriteLine("Marca ya agregada " + itemSource.Marca.Descripcion);
                    }
                }

                if (brandTemp.Count > 0)
                {                    
                    dataSourceBrands = brandTemp.Distinct().AsQueryable();
                    Console.WriteLine(dataSourceBrands.Count());
                }                

                await pagination_brands.SetCurrentPageIndexAsync(0);                
                Console.WriteLine("Consulta terminada");
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error: {e.Message}");
                Console.WriteLine($"Error: {e}");
            }

            _spinnerService.Hide();
        }

        protected async Task SearchBrands_old()
        {
            if (SelectedCompany == null)
            {
                toastService.ShowError("Compañia requerida!");
                return;
            }

            try
            {
                var dataSource_tmp = await appDbContext.FACBONIFICADOSXARTICULO
                    .Where(c => c.CodEmpresa == SelectedCompany.CodEmpresa
                        && c.FechaInicio >= StartDate
                        && c.FechaFin <= EndDate
                        && c.CodEstado == 1)
                    //.Take(2000)
                    .ToListAsync();

                List<FacBonificadosXArticulo> dataSource_tmp_2 = new List<FacBonificadosXArticulo>();

                List<GenMarca> brandTemp = new List<GenMarca>();
                foreach (var itemSource in dataSource_tmp)
                {
                    var artXEmpresa = appDbContext.ARTICULOSXEMPRESA
                        .Include(z => z.Articulo)
                        .Include(y => y.Marca)
                        .Where(Data =>
                    Data.CodEmpresa == itemSource.CodEmpresa
                    && Data.CodArticulo == itemSource.CodArticulo)?.FirstOrDefault();

                    //if (artXEmpresa != null)
                    //{
                    //    itemSource.ArticulosXEmpresa = artXEmpresa;
                    //}

                    //artXEmpresa.Marca.Descripcion
                    brandTemp.Add(artXEmpresa.Marca);
                }

                if (brandTemp.Count > 0)
                {
                    //brandTemp = brandTemp.GroupBy(br => br.Descripcion).Distinct().ToList();
                    //brandTemp = brandTemp.Distinct().ToList();
                    dataSourceBrands = brandTemp.Distinct().AsQueryable();
                    Console.WriteLine(dataSourceBrands.Count());
                }
                //dataSource = dataSource_tmp.AsQueryable();

                await pagination.SetCurrentPageIndexAsync(0);
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
            //dataSourceIE = await brandService.GetDataAsync();
            //Debug.WriteLine("Spinner Service" + _spinnerService);
            await LoadCompanies();
            //await FillData();
            //await base.GetLoggedUser();

            //_loader = new _DataLoader(httpClient);

            //data = (await _loader.LoadDataAsync(filterData)).Records;

            StartDate = DateTime.Now.Date; //new DateTimeOffset(2024, 12, 1, 0, 0, 0, 0, new TimeSpan());
            EndDate = DateTime.Now.Date.AddDays(4).AddSeconds(-1);//new DateTimeOffset(2024, 12, 31, 0, 0, 0, 0, new TimeSpan());
        }

        private async Task<IEnumerable<GenMarca>> SearchBrand(string searchText)
        {
            var search = appDbContext.GENMARCAS.Where(x => x.Descripcion.Contains(searchText) && x.CodEmpresaMarca == SelectedCompany.CodEmpresa).AsEnumerable();
            return search;
        }

        private void SelectedResultChanged(GenMarca result)
        {
            SelectedBrand = result;
            //movieCredits = await client.GetPersonMovieCredits(result.Id);
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
            int CodEmpresa = int.Parse(e.Value.ToString());
            SelectedCompany = await appDbContext.GENEMPRESAS.Where(x => x.CodEmpresa == CodEmpresa).FirstOrDefaultAsync();
            //await FillData(e.Value.ToString());
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

        async Task LaunchSendPrices(FacBonificadosXArticulo p)
        {
            var parameters = new ModalParameters();

            string Message = "Desea sincronizar el productos seleccionado (precios)? " + 
                p.CodBonificadoArticulo + "-" + 
                p.CodArticulo + "-" + 
                p.ArticulosXEmpresa.Articulo.Descripcion;
            parameters.Add(nameof(DisplayMessageCustom.Message), Message);
            var options = new ModalOptions
            {
                UseCustomLayout = true,
                DisableBackgroundCancel = true
            };
            
            var messageForm = modalService.Show<DisplayMessageCustom>("Sincronización", parameters, options);

            var resultDialog = await messageForm.Result;

            if (resultDialog.Cancelled)
            {
                return;
            }

            //=> message = $"You want to fire {p.FirstName} {p.LastName}";
            //toastService.ShowSuccess("Enviando " + p.CodBonificadoArticulo);

            _spinnerService.Show();

            //InvokeAsync(async () =>
            //{
                
            //});
            
            var articulos = new List<object>
            {
                p.CodArticulo
            };

            var responseData = await LaunchItemLocal(articulos, new List<object> (), true, false, new long[] { }, true, false);

            _spinnerService.Hide();

            toastService.ShowSuccess("Respuesta " + responseData);
            Debug.WriteLine(responseData);
        }

        async Task LaunchSendStock(FacBonificadosXArticulo p)
        {
            var parameters = new ModalParameters();

            string Message = "Desea sincronizar el productos seleccionado (stock)? " +
                p.CodBonificadoArticulo + "-" +
                p.CodArticulo + "-" +
                p.ArticulosXEmpresa.Articulo.Descripcion;
            parameters.Add(nameof(DisplayMessageCustom.Message), Message);
            var options = new ModalOptions
            {
                UseCustomLayout = true,
                DisableBackgroundCancel = true
            };
            var messageForm = modalService.Show<DisplayMessageCustom>("Sincronización", parameters, options);

            var resultDialog = await messageForm.Result;

            if (resultDialog.Cancelled)
            {
                return;
            }

            //=> message = $"You want to fire {p.FirstName} {p.LastName}";
            //toastService.ShowSuccess("Enviando " + p.CodBonificadoArticulo);

            _spinnerService.Show();

            //InvokeAsync(async () =>
            //{

            //});

            var articulos = new List<object>
            {
                p.CodArticulo
            };

            var responseData = await LaunchItemLocal(articulos, new List<object>(), false, true, new long[] { }, true, false);

            _spinnerService.Hide();

            toastService.ShowSuccess("Respuesta " + responseData);
            Debug.WriteLine(responseData);
        }

        async Task LaunchGet(FacBonificadosXArticulo p, bool with_prices, bool with_stock, bool with_full_stock)
        {            
            var articulos = new List<object>
            {
                p.CodArticulo
            };

            var responseData = await LaunchItemLocal(articulos, new List<object>(), true, false, new long[] {}, false, with_full_stock);

            if(responseData == null)
            {
                toastService.ShowError("Sin contenido.");
                return;
            }

            //_spinnerService.Hide();

            //toastService.ShowSuccess("Respuesta " + responseData);

            var parameters = new ModalParameters();

            //string Message = "JSON? " +
            //    p.CodBonificadoArticulo + "-" +
            //    p.CodArticulo + "-" +
            //    p.Articulo.Descripcion;

            parameters.Add(nameof(MonacoViewer.ValueToSet), responseData);
            parameters.Add(nameof(MonacoViewer.CodArticulo), p.CodArticulo);
            
            var options = new ModalOptions
            {
                UseCustomLayout = true,
                DisableBackgroundCancel = true
            };
            var messageForm = modalService.Show<MonacoViewer>("JSON", parameters, options);

            var resultDialog = await messageForm.Result;

            if (resultDialog.Cancelled)
            {
                return;
            }

            Debug.WriteLine(responseData);
        }

        [Obsolete("Debe ser eliminado")]
        async Task<string> LaunchGetItem(List<object> CodeListProducts, List<object> CodeListBrands, 
            DateTime date_start, DateTime date_end, bool with_prices, bool with_stock, long[] stores)
        {
            HubSyncWebItems hubSyncWebItems = new HubSyncWebItems(ConfigurationHelper.GetAppSettings().profile);
            Models.DMSA.Mbw.Abstract.ParametersMode1 parameters_post = new Models.DMSA.Mbw.Abstract.ParametersMode1();
            parameters_post.ids = CodeListProducts;
            parameters_post.brands = CodeListBrands;
            parameters_post.date_start = date_start;
            parameters_post.date_end = date_end;
            parameters_post.with_prices = with_prices;
            parameters_post.with_stock = with_stock;
            parameters_post.stores = stores;

            var responseData = await hubSyncWebItems.GetData(parameters_post);
            return responseData;
        }

        [Obsolete("Debe ser eliminado")]
        async Task<string> LaunchItem(List<object> CodeListProducts, List<object> CodeListBrands, bool with_discount, bool with_stock, long[] stores)
        {
            HubSyncWebItems hubSyncWebItems = new HubSyncWebItems(ConfigurationHelper.GetAppSettings().profile);
            Models.DMSA.Mbw.Abstract.ParametersMode1 parameters_post = new Models.DMSA.Mbw.Abstract.ParametersMode1();
            parameters_post.ids = CodeListProducts;
            parameters_post.brands = CodeListBrands;
            parameters_post.with_prices = with_discount;
            parameters_post.with_stock = with_stock;
            parameters_post.stores = stores;
            var responseData = await hubSyncWebItems.SendForUpdate(parameters_post);
            return responseData;
        }

        async Task<string> LaunchItemLocal(List<object> CodeListProducts, 
            List<object> CodeListBrands, 
            bool with_discount, 
            bool with_stock, 
            long[] stores,
            bool sendData,
            bool with_full_stock)
        {

            //await InvokeAsync(async () =>
            //{
                var parametros = new Models.DMSA.Mbw.Abstract.ParametersMode1();    

                parametros.brands = CodeListBrands;

                parametros.with_prices = false;
                parametros.with_discount = with_discount;
                parametros.with_stock = with_stock;
                parametros.with_full_stock = with_full_stock;

                parametros.ids = new List<object>
                {
                    CodeListProducts
                };

                parametros.stores = stores;
                parametros.ids = CodeListProducts;

                parametros.date_start = DateTime.Now.Date.AddDays(-30);
                parametros.date_end = DateTime.Now.Date.AddDays(30);

                EcommerceService ecommerceService = new EcommerceService(appDbContext);
                var articulosEnvio = await ecommerceService.MakeProducts(parametros);
                var jsonResult = JsonConvert.SerializeObject(articulosEnvio,
                    Formatting.Indented,
                    new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore });

            Debug.WriteLine(jsonResult);

            if (sendData)
            {
                string urlMiddleware = "http://api.dmujeres.ec/dmujeres/sku/bulk/";   //ConfigurationHelper.GetAppSettings().middleware_url;

                //urlMiddleware = "http://api.dmujeres-dev.ec:8000/dmujeres/v2/sku/bulk/";

                var response = await ecommerceService.SendDataToMiddleware(
                        jsonResult,
                        Method.Put,
                        urlMiddleware,
                        appDbContext);

                jsonResult = JsonConvert.SerializeObject(response,
                    Formatting.Indented);

            }
            //});

            return jsonResult;
        }

        async Task LaunchSendByBrand(GenMarca p)
        {
            var parameters = new ModalParameters();

            string Message = "Desea sincronizar la marca seleccionada? " +
                p.CodMarca + "-" +
                p.Descripcion;
            parameters.Add(nameof(DisplayMessageCustom.Message), Message);
            var options = new ModalOptions
            {
                UseCustomLayout = true,
                DisableBackgroundCancel = true
            };
            var messageForm = modalService.Show<DisplayMessageCustom>("Sincronización Marca", parameters, options);

            var resultDialog = await messageForm.Result;

            if (resultDialog.Cancelled)
            {
                return;
            }

            //=> message = $"You want to fire {p.FirstName} {p.LastName}";
            //toastService.ShowSuccess("Enviando " + p.CodMarca + " " + p.Descripcion);

            var marcas = new List<object>
            {
                p.CodMarca
            };

            var responseData = await LaunchItem(new List<object>(), marcas, false, true, new long[] { });           

            Debug.WriteLine(responseData);
        }

        async Task LaunchSendResults()
        {
            var groupedData = dataSource
                .AsEnumerable()
                .DistinctBy(x => x.CodArticulo)
                .Select(x => new FacBonificadosXArticulo
                {
                    CodArticulo = x.CodArticulo
                })
                .ToList();

            var parameters = new ModalParameters();

            string Message = "Desea sincronizar los productos listados? " + groupedData.Count();
            parameters.Add(nameof(DisplayMessageCustom.Message), Message);
            var options = new ModalOptions
            {
                UseCustomLayout = true,
                DisableBackgroundCancel = true
            };

            var messageForm = modalService.Show<DisplayMessageCustom>("Sincronización", parameters, options);

            var resultDialog = await messageForm.Result;

            if (resultDialog.Cancelled)
            {
                return;
            }

            //=> message = $"You want to fire {p.FirstName} {p.LastName}";
            toastService.ShowSuccess("Enviando productos de la lista");

            //for (int i = 0; i < dataSource.Count(); i += 10) // Avanza en bloques de 10
            //{
            //    int long_take = Math.Min(10, dataSource.Count() - i); // Evita exceder el límite de dataSource
            //    var range_items = dataSource.Skip(i).Take(long_take); // Toma los elementos correctos

            //    var articulos = range_items.Select(item => (object) item.CodArticulo).ToList(); // Extrae CodArticulo
            //    var responseData = await LaunchItemLocal(articulos, new List<object>(), true, false, new long[] { }, true, false); // Lanza la operación para el lote actual

            //    Debug.WriteLine(responseData);
            //    Debug.WriteLine("Enviados " + i + " de " + dataSource.Count());
            //    Console.WriteLine("Enviados " + i + " de " + dataSource.Count());
            //}

            //Envio 1x1
            //for (int i = 0; i < dataSource.Count(); i++)
            //{
            //    var item = dataSource.ElementAt(i);
            //    var articulos = new List<object> { item.CodArticulo.ToString() };

            //    int retryCount = 0;
            //    string responseData = "";

            //    Debug.WriteLine("Procesando " + i.ToString() + " de " + dataSource.Count().ToString());

            //    do
            //    {
            //        responseData = await LaunchItemLocal(
            //            articulos,
            //            new List<object>(),
            //            true,
            //            false,
            //            new long[] { },
            //            true,
            //            false
            //        );

            //        //Debug.WriteLine($"Intento {retryCount + 1}: {responseData}");
            //        retryCount++;

            //        if (responseData == "[]")
            //        {
            //            Debug.WriteLine("Envío correcto " + item.CodArticulo.ToString());
            //            Debug.WriteLine("Intento número " + retryCount.ToString());
            //            Debug.WriteLine("===============================================");
            //            break; // Éxito, salimos del bucle interno
            //        }
            //        else
            //        {
            //            //Debug.WriteLine(responseData);
            //            Debug.WriteLine("Envío icorrecto " + item.CodArticulo.ToString());
            //            Debug.WriteLine("Se va a reintentar " + retryCount.ToString());
            //            Debug.WriteLine("===============================================");
            //        }
            //    } while (retryCount < 3);
            //}


            //Envio 10 en 10
            int maxParallel = 10;
            int maxRetries = 3;

            Console.WriteLine($"=================================================");
            Console.WriteLine($"Iniciando nuevo proceso de sincronización...");
            Console.WriteLine(DateTime.Now.ToString());
            
            for (int i = 0; i < groupedData.Count(); i += maxParallel)
            {
                // Tomamos bloques de 10
                var batch = groupedData.Skip(i).Take(maxParallel).ToList();

                var tasks = batch.Select(async item =>
                {
                    var articulos = new List<object> { item.CodArticulo.ToString() };
                    string responseData = "[]";
                    int attempts = 0;

                    // Reintento hasta 3 veces si no es "[]"
                    while (attempts < maxRetries)
                    {
                        responseData = await LaunchItemLocal(
                            articulos,
                            new List<object>(),
                            true, 
                            false,
                            new long[] { },
                            false, //true, 
                            false
                        );

                        if (responseData == "[]")
                            break;

                        attempts++;
                        Debug.WriteLine($"Reintentando {item.CodArticulo}, intento {attempts}");
                        Debug.WriteLine("=====================================================");
                    }

                    return new { item.CodArticulo, responseData };
                });

                // Esperamos a que todos los del batch terminen
                var results = await Task.WhenAll(tasks);

                foreach (var result in results)
                {
                    Debug.WriteLine($"Articulo {result.CodArticulo} -> {result.responseData}");
                }

                Console.WriteLine($"Procesados {Math.Min(i + maxParallel, groupedData.Count())} de {groupedData.Count()}");
                Debug.WriteLine("=====================================================");
            }

            Console.WriteLine($"Terminado proceso de sincronización...");
            Console.WriteLine(DateTime.Now.ToString());
        }

        public async Task<List<InvStock>> LoadItemsWithStockAsync()
        {
            var parameters = new ModalParameters();

            //string Message = "Seleccionar agencia";
            parameters.Add(nameof(SelectorAgenciaBodega.appDbContext), appDbContext);
            var options = new ModalOptions
            {
                UseCustomLayout = true,
                DisableBackgroundCancel = true
            };
            var messageForm = modalService.Show<SelectorAgenciaBodega>("Seleccionar agencia", parameters, options);

            var resultDialog = await messageForm.Result;

            if (resultDialog.Cancelled)
            {
                return null;
            }

            long[] agencias = new long[] { 95, 96 };

            var listStock = appDbContext.INVSTOCKS.Where(s => agencias.Contains(s.CodBodegaAgencia) && s.Cantidad > 0)                
                .GroupBy(i => i.CodArticulo)
                .Select(g => g.First())
                .ToList();
            
            var articulos = new List<object>();
                        
            int total = listStock.Count;
            int size = 10;

            Console.WriteLine("Total items: " + total);

            for (int i = 0; i < total; i += size)
            {
                articulos.Clear();
                var grupo = listStock
                    .Skip(i)
                    .Take(size)
                    .Select(s => s.CodArticulo.ToString())
                    .ToList();

                var listaCodigos = string.Join(",", grupo);
                                
                Console.WriteLine("Grupo: " + i);
                Console.WriteLine(listaCodigos);

                articulos.AddRange(grupo);
                //var responseData = await LaunchGetItem(articulos, new List<object>(),
                //    DateTime.Now.Date,
                //    DateTime.Now.AddDays(1).AddSeconds(-1),
                //    true,
                //    true,
                //    agencias);

                var responseData_stock = await LaunchItem(articulos, new List<object>(), false, true, agencias);
                Console.WriteLine(responseData_stock);
                Console.WriteLine("Fin grupo...");
                //break;
            }

            Console.WriteLine("Terminado LoadItemsWithStockAsync");

            //foreach (var item in listStock)
            //{
            //    articulos.Add(item.CodArticulo);

            //    var responseData = await LaunchGetItem(articulos, new List<object>(), 
            //        DateTime.Now.Date, 
            //        DateTime.Now.AddDays(1).AddSeconds(-1), 
            //        true, 
            //        true);

            //    var responseData_stock = await LaunchItem(articulos, new List<object>(), true, true);
            //    Console.WriteLine(responseData_stock);
            //    //var responseData_disc = await LaunchItem(articulos, new List<object>(), true, false);
            //    //Console.WriteLine(responseData_disc);
            //    break;
            //}

            return listStock;
        }

        public async Task SendBrandByMBW(int CodMarca)
        {
            string url_soap = "http://192.168.204.43:8081";

            var options = new RestClientOptions(url_soap)
            {
                Timeout = TimeSpan.FromMilliseconds(-1)
            };

            var client = new RestClient(options);
            var request = new RestRequest("/MyBusiness-MyBusinessEJB/WSIntegracionEcommerce", Method.Post);
            request.AddHeader("Content-Type", "text/xml");
            request.AddHeader("Cookie", "frontend_lang=es_EC");
            var body = @"<soapenv:Envelope xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/"" xmlns:web=""http://webservices.etech.com/"">" + "\n" +
            @"   <soapenv:Header/>" + "\n" +
            @"   <soapenv:Body>" + "\n" +
            @"      <web:syncWebEcommerce>" + "\n" +
            @"         <codigoArticulo></codigoArticulo>" + "\n" +
            @"         <codmarca>" + CodMarca.ToString() + "</codmarca>" + "\n" +
            @"      </web:syncWebEcommerce>" + "\n" +
            @"   </soapenv:Body>" + "\n" +
            @"</soapenv:Envelope>";
            request.AddParameter("text/xml", body, ParameterType.RequestBody);
            RestResponse response = await client.ExecuteAsync(request);
            Console.WriteLine(response);
        }

        public async Task SendItemsByMBW(string ItemsCode)
        {
            var options = new RestClientOptions("http://localhost:8081")
            {
                Timeout = TimeSpan.FromMilliseconds(-1)
            };

            var client = new RestClient(options);
            var request = new RestRequest("//MyBusiness-MyBusinessEJB/WSIntegracionEcommerce", Method.Post);
            request.AddHeader("Content-Type", "text/xml");
            request.AddHeader("Cookie", "frontend_lang=es_EC");
            var body =
            @$"<soapenv:Envelope xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/"" xmlns:web=""http://webservices.etech.com/"">
               <soapenv:Header/>
               <soapenv:Body>
                  <web:enviomasivo>
                     <jsonDatos>{ItemsCode}</jsonDatos>
                  </web:enviomasivo>
               </soapenv:Body>
            </soapenv:Envelope>";
            request.AddParameter("text/xml", body, ParameterType.RequestBody);
            RestResponse response = await client.ExecuteAsync(request);
            //Console.WriteLine(response.Content);
        }

        [Obsolete]
        public async Task SendAllBrandsByMbw()
        {
            int totalCount = 0;
            foreach(var brand in dataSourceBrands)
            {
                totalCount++;
                Console.WriteLine("Enviando:" + brand.Descripcion + " " + brand.CodMarca);
                Debug.WriteLine("Enviando:" + brand.Descripcion + " " + brand.CodMarca);
                await SendBrandByMBW(brand.CodMarca);
                Console.WriteLine(totalCount + "/" + dataSourceBrands.Count());
                Debug.WriteLine(totalCount + "/" + dataSourceBrands.Count());
            }
        }

        public async Task SendAllBrandsByMbwBatch_grup_awaited()
        {
            int totalCount = dataSourceBrands.Count();
            int batchSize = 10;

            for (int i = 0; i < totalCount; i += batchSize)
            {
                var batch = dataSourceBrands.Skip(i).Take(batchSize).ToList();
                Console.WriteLine($"Enviando lote {i / batchSize + 1} de {Math.Ceiling((double)totalCount / batchSize)}");
                foreach (var brand in batch)
                {
                    Console.WriteLine($" -> {brand.CodMarca}: {brand.Descripcion}");
                }
                var tasks = batch.Select(brand => SendBrandByMBW(brand.CodMarca));
                await Task.WhenAll(tasks);
                Console.WriteLine($"{Math.Min(i + batchSize, totalCount)}/{totalCount} marcas enviadas.");
            }
        }

        public async Task SendAllBrandsByMbwBatch()
        {
            int[] excludedBrands = []; // 306, 175, 12, 247, 144, 126, 258, 157, 190, 252, 310, 333, 328, 90, 350, 351, 334, ];
            var dataSourceBrandsList = dataSourceBrands.Where(i=> !excludedBrands.Contains( i.CodMarca )).ToList();

            int totalCount = dataSourceBrandsList.Count();
            int batchSize = 10;
            var activeTasks = new List<Task>();
            var taskInfo = new Dictionary<Task, string>();

            int i = 0;

            while (i < totalCount || activeTasks.Count > 0)
            {
                while (activeTasks.Count < batchSize && i < totalCount)
                {
                    var brand = dataSourceBrandsList[i];                    
                    Console.WriteLine($" -> {brand.CodMarca}: {brand.Descripcion}");                    
                    var task = SendBrandByMBW(brand.CodMarca);
                    activeTasks.Add(task);
                    taskInfo[task] = brand.CodMarca.ToString() + "-" + brand.Descripcion;

                    task.ContinueWith(t =>
                    {
                        lock (activeTasks)
                        {                            
                            Console.WriteLine($"Terminada la marca: {taskInfo[t]}");
                            activeTasks.Remove(t);
                        }
                    });

                    i++;
                }

                await Task.WhenAny(activeTasks);
                Console.WriteLine($"{i}/{totalCount} marcas enviadas.");
            }

            await Task.WhenAll(activeTasks);
        }
    }
}
