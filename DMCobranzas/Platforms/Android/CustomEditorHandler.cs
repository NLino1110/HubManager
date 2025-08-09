using Android.Content;
using Android.Widget;
using Microsoft.Maui.Controls.Compatibility;
using Microsoft.Maui.Controls.Compatibility.Platform.Android;
using Microsoft.Maui.Controls.Platform;
using Microsoft.Maui.Handlers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMCobranzas.Platforms
{
    public class CustomEditorHandler : EditorHandler
    {
        public CustomEditorHandler() : base(Mapper)
        {
        }

        public static IPropertyMapper<Microsoft.Maui.IEditor, CustomEditorHandler> Mapper =
            new PropertyMapper<Microsoft.Maui.IEditor, CustomEditorHandler>(EditorHandler.Mapper)
            {
                [nameof(IEditor.IsReadOnly)] = MapIsReadOnly
            };

        public static void MapIsReadOnly(CustomEditorHandler handler, IEditor editor)
        {
            if (handler.PlatformView is EditText editText)
            {
                // Permite la selección de texto en modo de solo lectura
                editText.SetTextIsSelectable(true);
                editText.Focusable = false;
                editText.FocusableInTouchMode = false;
                editText.Clickable = false;
                editText.LongClickable = true; // Permite copia de texto
            }
        }
    }
}
