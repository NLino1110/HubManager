using BeebTech.Controls.Controls;
using BeebTech.Controls.Handlers;
using BeebTech.Controls.Views;
using Plainer.Maui;
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
                //handlers.AddInputKitHandlers();
                handlers.AddHandler(typeof(Dropdown), typeof(DropdownHandler));
                handlers.AddHandler(typeof(StatefulContentView), typeof(StatefulContentViewHandler));
                handlers.AddHandler(typeof(SelectableLabel), typeof(SelectableLabelHandler));
                handlers.AddPlainer();
            });

            return builder;
        }
    }
}
