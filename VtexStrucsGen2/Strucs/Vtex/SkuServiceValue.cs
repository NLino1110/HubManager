using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VtexStrucsGen2.Strucs.Vtex
{
    //GET {{baseUrl}}/api/catalog/pvt/skuservicevalue/:IdSkuServicoValor
    public class SkuServiceValue
    {
        /*
        {
            "Id": 4,
            "SkuServiceTypeId": 1,
            "Name": "Ensamblaje  $8 (Incluido IVA)",
            "Value": 8.0000,
            "Cost": 8.0000
        }
         */        
        public int Id { get; set; }
        public int SkuServiceTypeId { get; set; }
        public string Name { get; set; }
        public decimal Value { get; set; }
        public decimal Cost { get; set; }

    }
}
