using BeebTech.Controls.Handlers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BeebTech.Controls.UI
{
    public static class MauiBuilderExtensions
    {
        public static MauiAppBuilder UseBeebTechControls(this MauiAppBuilder builder)
        {
            builder.ConfigureMauiHandlers(handlers =>
            {
                handlers.AddHandler(typeof(Dropdown), typeof(DropdownHandler));

            });

            return builder;
        }
    }
}
