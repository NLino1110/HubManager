using System;
using System.Collections.Generic;
using System.Text;

namespace VtexStrucs
{
    public class vtexStock
    {
        public string skyId { get; set; }
        public List<vtexStockBalance> balance { get; set; } = new List<vtexStockBalance>();
    }
}
