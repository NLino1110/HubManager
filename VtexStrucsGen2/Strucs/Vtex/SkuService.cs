using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VtexStrucsGen2.Strucs.Vtex
{
    //GET {{baseUrl}}/api/catalog/pvt/skuservicevalue/:IdSkuServicoValor
    public class SkuService
    {
        /*
        {
            "SkuServiceTypeId": 2,
            "SkuServiceValueId": 13,
            "SkuId": 1799,
            "Name": "Servicio de ensamblaje",
            "Text": "Servicio de ensamblaje",
            "IsActive": true
        }
        */
        public int Id { get; set; }
        public int SkuServiceTypeId { get; set; }
        public int SkuServiceValueId { get; set; }
        public int SkuId { get; set; }
        public string Name { get; set; }
        public string Text { get; set; }
        public bool IsActive { get; set; }
    }
}
