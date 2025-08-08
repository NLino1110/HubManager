using Blazored.Modal;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.QuickGrid;
using Models.DMSA.Mbw.Inventario;
using Models.DMSA.Mbw.Sales;

namespace ResourceBuilder.Shared.Modal
{
    class BodegasXAgenciaExt: BodegasXAgencia
    {
        public virtual bool selected {  get; set; } = false;
    }

    public partial class SelectorAgenciaBodega
    {
        [Parameter]
        public DataSourceManager.AppDbContext appDbContext { get; set; }

        [CascadingParameter]
        BlazoredModalInstance BlazoredModal { get; set; } = default!;

        async Task SubmitForm() => await BlazoredModal.CloseAsync();
        async Task Cancel() => await BlazoredModal.CancelAsync();
        public Dictionary<string, object> AdditionalAttributes = new Dictionary<string, object>();

        IQueryable<BodegasXAgenciaExt> dataSource { get; set; }
        PaginationState pagination = new PaginationState { ItemsPerPage = 15 };
        PaginationState pagination_brands = new PaginationState { ItemsPerPage = 15 };
        protected override async Task OnInitializedAsync()
        {
            var agencias = appDbContext.GENAGENCIAS.Where(x => x.CodEmpresa == 2).ToList();
            var agencias_ids = agencias.Select(a => a.CodAgencia).ToArray();

            var dataSource_tmp = appDbContext.BODEGASXAGENCIA
                            .Where(Data => Data.CodEstado == 1 &&
                            agencias_ids.Contains(Data.CodAgencia) ).ToList();

            dataSource = dataSource_tmp.AsQueryable().Select(b => new BodegasXAgenciaExt
            {
                CodAgencia = b.CodAgencia,
                CodBodega = b.CodBodega,
                CodBodegaAgencia = b.CodBodegaAgencia,
                CodEstado = b.CodEstado,
                ControlDisponibleWMS = b.ControlDisponibleWMS,
                ControlManual = b.ControlManual,
                CorreoContacto = b.CorreoContacto,
                EnvioEcommerce = b.EnvioEcommerce,
                EsDevolucion = b.EsDevolucion,
                FechaCambioEstado = b.FechaCambioEstado,
                Observaciones = b.Observaciones,
                Principal = b.Principal,
                ProcesaWMS = b.ProcesaWMS,
                UsuarioCambioEstado = b.UsuarioCambioEstado,
                Web = b.Web,
                selected = false
            });
        }

        async Task SelectItem(BodegasXAgencia p, bool with_prices, bool with_stock)
        {

        }
    }
}
