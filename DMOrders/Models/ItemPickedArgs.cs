using DMSA.Models.Odoo.Native;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMOrders.Models
{
    public class ItemPickedArgs
    {
        public product_product product { get; set; }
        public decimal qty_real { get; set; }
        public decimal qty_sol { get; set; }
        public int CurrentPriceList { get; set; }
    }

}
