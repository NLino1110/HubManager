using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UraniumUI.Dialogs;

namespace DMOrdersUI.Services.Helpers
{
    static internal class ServicesExposer
    {
        static public IDialogService DialogService { get; set; }
    }
}
