using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMSA.Models.Odoo.DMOrders.promotions.@abstract
{
    public sealed class RuleInfo
    {
        public long Id { get; set; }
        public double Discount { get; set; }
        public bool UnlimitedTime { get; set; }
        public DateTime? StartDate { get; set; }   // Date-only en tu modelo → usa DateTime? (UTC)
        public DateTime? EndDate { get; set; }
        public long? PaymentMethodId { get; set; }
        public long? SelectionTypeId { get; set; }
    }
}
