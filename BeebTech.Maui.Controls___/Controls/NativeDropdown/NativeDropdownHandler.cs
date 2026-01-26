using Microsoft.Maui.Handlers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BeebTech.Maui.Controls.Controls.NativeDropdown
{
    public partial class NativeDropdownHandler : ViewHandler<NativeDropdown, object>
    {
        protected override object CreatePlatformView()
            => CreatePlatformDropdown();

        partial object CreatePlatformDropdown();
    }
}
