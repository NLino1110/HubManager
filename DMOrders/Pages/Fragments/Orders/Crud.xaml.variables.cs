using System.Globalization;
using DMOrders.Models;
using DMOrders.Services.Promotions;
using DMSA.Models.Odoo.Abstract;
using DMSA.Models.Odoo.DMOrders.promotions;
using DMSA.Models.Odoo.DMOrders.promotions.abstractCustom;
using DMSA.Models.Odoo.Native;
using DMSA.Models.Odoo.Sales;
using DMSA.Sync.Core;
using DMSA.Sync.Core.Database.Sqlite;
using DMSA.Sync.Core.Database.Sqlite.Benefits;
using DMSA.Sync.Core.Database.Sqlite.Sales;
using Newtonsoft.Json;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows.Input;
using System.Text.RegularExpressions;

namespace DMOrders.Pages.Fragments.Orders
{
    public partial class Crud
    {
        private int ManualLinesAdded { get; set; }
        private int OriginalManualLinesAdded { get; set; }
    }
}
