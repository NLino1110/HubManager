using System;
using System.Collections.Generic;
using System.Text;

namespace VtexStrucs
{
    public class vtexStockBalance
    {
        public string warehouseId { get; set; }
        public string warehouseName { get; set; }
        public int totalQuantity { get; set; }
        public int reservedQuantity { get; set; }
        public bool hasUnlimitedQuantity { get; set; }

    }
}
