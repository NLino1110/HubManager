using Android.Content;
using DMOrders.Platforms.Android;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Compatibility;
using Microsoft.Maui.Controls.Handlers.Compatibility;
using Microsoft.Maui.Controls.Platform.Compatibility;
using Microsoft.Maui.Controls.Platform;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

[assembly: ExportRenderer(typeof(Shell), typeof(NoAnimationShellRenderer))]
namespace DMOrders.Platforms.Android
{
    public class NoAnimationShellRenderer : ShellRenderer
    {
        public NoAnimationShellRenderer(Context context) : base(context)
        {
            Debug.WriteLine("IShellSectionRenderer");
        }

        protected override IShellSectionRenderer CreateShellSectionRenderer(ShellSection shellSection)
        {
            var renderer = base.CreateShellSectionRenderer(shellSection);
            if (renderer is ShellSectionRenderer sectionRenderer)
            {
                //sectionRenderer.NavAnimation = null; // Quita las animaciones
                Debug.WriteLine("IShellSectionRenderer");
            }
            return renderer;
        }
    }
}
