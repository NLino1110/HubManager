using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VtexStrucsGen2.Strucs.Vtex
{
    using System;
    using System.Collections.Generic;

    public partial class OrderItem
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public string ProductId { get; set; }
        public int Quantity { get; set; }
        public string ReferenceId { get; set; }
        public double Price { get; set; }
        public double ListPrice { get; set; }
        public int SkuId { get; set; }
        public double Discount { get; set; }
        public double Tax { get; set; }
        public string VtexData { get; set; }
        public virtual Order Order { get; set; }
    }
}
