using Blazored.Modal;
using Blazored.Modal.Services;
using Blazored.Toast.Services;
//using Blazorise;
using BlazorMonaco;
using BlazorMonaco.Editor;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using ResourceBuilder.Services.Sales;
using RestSharp;
using System.Diagnostics;

namespace ResourceBuilder.Shared.Modal
{
    public partial class MonacoViewer
    {
        [Inject]
        DataSourceManager.AppDbContext appDbContext { get; set; }

        [Inject]
        IToastService toastService { get; set; }
        [CascadingParameter]
        BlazoredModalInstance BlazoredModal { get; set; } = default!;
        async Task Close() => await BlazoredModal.CloseAsync(ModalResult.Ok(true));
        async Task Cancel() => await BlazoredModal.CancelAsync();

        [Inject]
        public IModalService ModalService { get; set; }

        private StandaloneCodeEditor _editor { get; set; }
        private string[] decorationIds;
        private BlazorXTabs.XTabs _xtabs { get; set; }        
        [Inject] public BlazorDownloadFile.IBlazorDownloadFileService BlazorDownloadFileService { get; set; }
                
        [Parameter]
        public string ValueToSet { get; set; }


        [Parameter]
        public int CodArticulo { get; set; }

        protected override void OnAfterRender(bool firstRender)
        {
            _ = SetValue();
            base.OnAfterRender(firstRender);
        }

        private async Task SetValue()
        {
            //Console.WriteLine($"setting value to: {documentTemplate.doc_source}");
            await _editor.SetValue(ValueToSet);
        }

        private async Task GetValue()
        {
            var val = await _editor.GetValue();
            Console.WriteLine($"value is: {val}");
        }

        private void OnContextMenu(EditorMouseEvent eventArg)
        {
            Console.WriteLine("OnContextMenu : " + System.Text.Json.JsonSerializer.Serialize(eventArg));
        }

        private async Task ChangeTheme(ChangeEventArgs e)
        {
            Console.WriteLine($"setting theme to: {e.Value.ToString()}");
            await Global.SetTheme(e.Value.ToString());
        }

        private StandaloneEditorConstructionOptions EditorConstructionOptions(StandaloneCodeEditor editor)
        {
            return new StandaloneEditorConstructionOptions
            {
                AutomaticLayout = true,
                //Language = "javascript",
                //Language = "html",
                Language = "json",
                GlyphMargin = true,
                Value = "",
                //FixedOverflowWidgets = false,
                //ScrollBeyondLastColumn = 100000,
                //MaxTokenizationLineLength = 100000,
                WordWrap = "on",
                WordWrapColumn = 0,
            };
        }

        private async Task EditorOnDidInit()
        {
            await _editor.AddCommand((int)KeyMod.CtrlCmd | (int)KeyCode.KeyH, (editor, keyCode) =>
            {
                Console.WriteLine("Ctrl+H : Initial editor command is triggered.");
            });

            var newDecorations = new ModelDeltaDecoration[]
            {
                new ModelDeltaDecoration
                {
                    Range = new BlazorMonaco.Range(3,1,3,1),
                    Options = new ModelDecorationOptions
                    {
                        IsWholeLine = true,
                        ClassName = "decorationContentClass",
                        GlyphMarginClassName = "decorationGlyphMarginClass"
                    }
                }
            };

            decorationIds = await _editor.DeltaDecorations(null, newDecorations);
            // You can now use 'decorationIds' to change or remove the decorations
        }

        async Task CopyContent()
        {
            //await _editor.
            await ClipboardService.CopyTextToClipboardAsync(ValueToSet);
            toastService.ShowInfo("Resultado copiado al portapapeles.");
        }

        async Task LaunchGet()
        {
            bool with_prices = true;
            bool with_stock = true;
            bool with_full_stock = true;
            
            var articulos = new List<object>
            {
                CodArticulo
            };

            var responseData = await LaunchItemLocal(articulos, new List<object>(), true, true, new long[] { }, false, with_full_stock, appDbContext);

            if (responseData == null)
            {
                toastService.ShowError("Sin contenido.");
                return;
            }

            var parameters = new ModalParameters();
            ValueToSet = responseData;
            Debug.WriteLine(responseData);
        }

        async Task<string> LaunchItemLocal(List<object> CodeListProducts,
            List<object> CodeListBrands,
            bool with_discount,
            bool with_stock,
            long[] stores,
            bool sendData,
            bool with_full_stock,
            DataSourceManager.AppDbContext appDbContext)
        {

            //await InvokeAsync(async () =>
            //{
            var parametros = new Models.DMSA.Mbw.Abstract.ParametersMode1();

            parametros.brands = CodeListBrands;
            parametros.with_prices = true;
            parametros.with_stock = true;
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

            return jsonResult;
        }


        async Task<string> SendData()
        {
            ValueToSet = await _editor.GetValue();
            string jsonResult = ValueToSet;

            //await _editor.SetValue(ValueToSet)

            EcommerceService ecommerceService = new EcommerceService(appDbContext);
            
            Debug.WriteLine(jsonResult);
            
            string urlMiddleware = "http://api.dmujeres.ec/dmujeres/sku/bulk/";//ConfigurationHelper.GetAppSettings().middleware_url;

            urlMiddleware = "http://api.dmujeres-dev.ec:8000/dmujeres/v2/sku/bulk/";

            var response = await ecommerceService.SendDataToMiddleware(
                    jsonResult,
                    Method.Put,
                    urlMiddleware,
                    appDbContext);

            jsonResult = JsonConvert.SerializeObject(response, Formatting.Indented);
            
            Debug.WriteLine(jsonResult);

            return jsonResult;
        }
    }
}
