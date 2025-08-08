using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VtexStrucsGen2.Strucs.Vtex
{
    internal class SkuServiceTypeAttach
    {
        //POST {{baseUrl}}/api/catalog/pvt/skuservicetypeattachment
        /*
         {
            "AttachmentId": 1799,
            "SkuServiceTypeId": 2,
            "SkuServiceValueId": 13
        }
         */
        public int AttachmentId { get; set; }
        public int SkuServiceTypeId { get; set; }
        public int SkuServiceValueId { get; set; }
    }
}
