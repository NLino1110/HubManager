using Blazored.Modal.Services;
using Blazored.Toast.Services;
using Microsoft.AspNetCore.Components;
using ResourceBuilder.Services.Sales;

namespace ResourceBuilder.Pages
{
    public partial class SyncManager
    {
        DateTimeOffset? StartDate { get; set; } = DateTime.Today;
        DateTimeOffset? EndDate { get; set; } = DateTime.Today.AddDays(1).AddTicks(-1);

        [Inject]
        public IModalService modalService { get; set; }

        [Inject]
        IToastService toastService { get; set; }

        [Inject]
        private IWebHostEnvironment _webHostEnvironment { get; set; }

        //[Inject] DataSourceManager.AppDbContext _appDbContext { get; set; }

        [Inject]
        private InvoicesService invoiceService { get; set; }

        private async Task SyncInit()
        {
            //void ConfigureToast(ToastSettings settings)
            //{
            //    settings.Position = ToastPosition.BottomCenter;
            //    settings.IconType = IconType.Blazored;
            //    settings.DisableTimeout = true;
            //    settings.ExtendedTimeout = 0;
            //    settings.PauseProgressOnHover = false;
            //    settings.ShowCloseButton = false;
            //    settings.ShowProgressBar = true;                
            //}

            //toastService.ShowInfo("No puede continuar, no hay productos modificados.", ConfigureToast);

            //if (true) return;

            int totalHeaders = 0;
            //int year = 2024;
            //int month = 11;
            //int day = 26;

            //DateTime date_ini = StartDate.Value.Date;
            //DateTime date_end = EndDate.Value.Date.AddDays(1).AddSeconds(-1);

            DateTime date_ini = StartDate.Value.DateTime;
            DateTime date_end = EndDate.Value.DateTime;

            //var resHeaders = await invoiceService.GetInvoiceHeader(date_ini, date_end);

            int CountHour = 0;

            CountHour++;

            if (date_end > date_end)
            {
                date_end = date_end;
            }

            // Llamar a la función
            Console.WriteLine($"Calling LaunchBatchSync with range: {date_ini} - {date_end}");
            totalHeaders += await invoiceService.SyncCustomers(date_ini, date_end);

            // Mover al siguiente rango de 1 hora
            date_ini = date_ini.AddHours(1);

            Console.WriteLine("Total cabeceras: " + totalHeaders);
            Console.WriteLine($"Finished process:" + DateTime.Now);
            Console.WriteLine($"==========================================================================");
        }
    }
}
