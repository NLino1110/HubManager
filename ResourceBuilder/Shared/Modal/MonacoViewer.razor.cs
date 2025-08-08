using Blazored.Modal;
using Blazored.Modal.Services;
using Blazored.Toast.Services;
using BlazorMonaco;
using Microsoft.AspNetCore.Components;
using System;
using System.Dynamic;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Linq;
using BlazorMonaco.Editor;
using Org.BouncyCastle.Asn1.Cmp;
using Microsoft.AspNetCore.Components.Web;
using Blazored.Toast.Configuration;

namespace ResourceBuilder.Shared.Modal
{
    public partial class MonacoViewer
    {
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
    }
}
