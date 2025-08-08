using Blazored.Modal.Services;
using Blazored.Modal;
using Microsoft.AspNetCore.Components;
using ResourceBuilder.Services.Sales;
using ResourceBuilder.Shared;
using System.Diagnostics;
using Blazored.Toast.Services;
using ResourceBuilder.Shared.Modal;
using Blazored.Toast.Configuration;
using Blazored.Toast;

namespace ResourceBuilder.Pages
{
    public partial class OdooSync
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

        private async Task ClearDevices()
        {
            StateHasChanged();
        }

        private async Task SyncManager()
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
            
            var resHeaders = await invoiceService.GetInvoiceHeader(date_ini, date_end);

            int CountHour = 0;
            
            CountHour++;
                        
            if (date_end > date_end)
            {
                date_end = date_end;
            }

            // Llamar a la función
            Console.WriteLine($"Calling LaunchBatchSync with range: {date_ini} - {date_end}");
            totalHeaders += await invoiceService.LaunchBatchSync(date_ini, date_end);            

            // Mover al siguiente rango de 1 hora
            date_ini = date_ini.AddHours(1);

            Console.WriteLine("Total cabeceras: " + totalHeaders);
            Console.WriteLine($"Finished process:" + DateTime.Now);
            Console.WriteLine($"==========================================================================");
        }

        //private async Task SyncManager_old()
        //{
        //    DateTime date_ini = DateTime.Today.AddDays(-1).AddHours(16);
        //    DateTime date_end = DateTime.Today.AddDays(-1).AddHours(17);//.AddMinutes(30);

        //    date_ini = DateTime.Today.AddHours(10);
        //    date_end = DateTime.Today.AddHours(10).AddMinutes(59).AddSeconds(59);

        //    Debug.WriteLine("Iniciando importación MBW -> Odoo    " + DateTime.Now);
        //    Debug.WriteLine("date_ini    " + date_ini);
        //    Debug.WriteLine("date_end    " + date_end);

        //    Console.WriteLine("Iniciando importación MBW -> Odoo    " + DateTime.Now);
        //    Console.WriteLine("date_ini    " + date_ini);
        //    Console.WriteLine("date_end    " + date_end);

        //    InvoicesService invoiceService = new InvoicesService(_appDbContext, WebHostEnvironment);
        //    var resHeaders = await invoiceService.GetInvoiceHeader(date_ini, date_end);            
        //    var ids_headers = resHeaders.Select(doc => doc.NumCmprVenta).ToArray();
        //    var resDetails = await invoiceService.GetInvoiceDetails(ids_headers);
        //    var resPayments = await invoiceService.GetInvoicePayments(ids_headers);

        //    var resInsHeaders01 = await invoiceService.PutInvoiceHeader(resHeaders);
        //    var resInsDetails01 = await invoiceService.PutInvoiceDetails(resDetails);
        //    var resInsPayments01 = await invoiceService.PutInvoicePayments(resPayments);

        //    Debug.WriteLine(resInsHeaders01);

        //    Debug.WriteLine("Fin: SyncManager" + DateTime.Now);
        //}

        //private async Task<int> LaunchBatchSync(DateTime date_ini, DateTime date_end)
        //{
        //    int totalHeaders = 0;
        //    Debug.WriteLine("Iniciando importación MBW -> Odoo    " + DateTime.Now);
        //    Debug.WriteLine("date_ini    " + date_ini);
        //    Debug.WriteLine("date_end    " + date_end);

        //    Console.WriteLine("Iniciando importación MBW -> Odoo    " + DateTime.Now);
        //    Console.WriteLine("date_ini    " + date_ini);
        //    Console.WriteLine("date_end    " + date_end);

        //    InvoicesService invoiceService = new InvoicesService(_appDbContext, WebHostEnvironment);
        //    var resHeaders = await invoiceService.GetInvoiceHeader(date_ini, date_end);
        //    var ids_headers = resHeaders.Select(doc => doc.NumCmprVenta).ToArray();
        //    if (ids_headers.Length > 0)
        //    {
        //        totalHeaders += ids_headers.Length;
        //        Debug.WriteLine("Cabeceras:[" + ids_headers.Length + "]");
        //        Console.WriteLine("Cabeceras:[" + ids_headers.Length + "]");

        //        var resDetails = await invoiceService.GetInvoiceDetails(ids_headers);
        //        var resPayments = await invoiceService.GetInvoicePayments(ids_headers);

        //        var resInsHeaders01 = await invoiceService.PutInvoiceHeader(resHeaders);
        //        var resInsDetails01 = await invoiceService.PutInvoiceDetails(resDetails);
        //        var resInsPayments01 = await invoiceService.PutInvoicePayments(resPayments);                

        //        Debug.WriteLine("Detalles:[" + resDetails.Count + "]");
        //        Console.WriteLine("Detalles:[" + resDetails.Count + "]");

        //        Debug.WriteLine("Pagos:[" + resPayments.Count + "]");
        //        Console.WriteLine("Pagos:[" + resPayments.Count + "]");
        //    }
        //    else
        //    {
        //        Debug.WriteLine("Datos de cabecera no encontrados....");
        //        Console.WriteLine("Datos de cabecera no encontrados....");
        //    }

        //    Debug.WriteLine("Fin: SyncManager" + DateTime.Now);

        //    return totalHeaders;
        //}

        private async Task SmtpTest()
        {
            var parameters = new ModalParameters();

            EmailSettings emailSettings = new EmailSettings();
            emailSettings.id = 0;
            emailSettings.userName = "sistemas@macronegocios.ec";
            emailSettings.port = 587;
            emailSettings.password = "";
            emailSettings.EnableSsl = true;
            emailSettings.host = "smtp.google.com";

            parameters.Add(nameof(ModalCompanyTestSmtp.selectedItem), emailSettings);
            var options = new ModalOptions
            {
                UseCustomLayout = true,
                DisableBackgroundCancel = true
            };

            var messageForm = modalService.Show<ModalCompanyTestSmtp>("Smtp Test", parameters, options);

            var resultDialog = await messageForm.Result;

            if (resultDialog.Cancelled)
            {
                return;
            }
        }
    }
}
