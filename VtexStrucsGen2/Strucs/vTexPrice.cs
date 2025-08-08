using System;
using System.Collections.Generic;
using System.Text;

namespace VtexStrucs
{
    public class vTexPrice
    {
        public string itemId { get; set; }
        public decimal ?listPrice { get; set; }
        public decimal costPrice { get; set; }
        public decimal markup { get; set; }
        public decimal basePrice { get; set; }
    }
}
