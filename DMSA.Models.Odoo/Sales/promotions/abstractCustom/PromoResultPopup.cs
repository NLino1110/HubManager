using DMSA.Models.Odoo.DMOrders.promotions;
using DMSA.Models.Odoo.DMOrders.promotions.abstractCustom;
using DMSA.Models.Odoo.Native;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMSA.Models.Odoo.Sales.promotions.abstractCustom
{
    public class PromoResultPopup
    {
        public List<PromotionEvalResult> benefits { get; set; }
        public int ActionResult { get; set; } = 0;
        public List<sale_order_line> manualGifts { get; set; }
    }
}
