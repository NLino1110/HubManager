using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VtexStrucsGen2.Strucs.Vtex
{
    //{{baseUrl}}/api/catalog/pvt/skuservicetype/:id
    /*
     {
    "Id": 1,
    "Name": "Ensamblaje2018",
    "IsActive": false,
    "ShowOnProductFront": false,
    "ShowOnCartFront": false,
    "ShowOnAttachmentFront": false,
    "ShowOnFileUpload": false,
    "IsGiftCard": false,
    "IsRequired": false
}
     */
    public class SkuServiceType
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public bool IsActive { get; set; }
        public bool ShowOnProductFront { get; set; }
        public bool ShowOnCartFront { get; set; }
        public bool ShowOnAttachmentFront { get; set; }
        public bool ShowOnFileUpload { get; set; }
        public bool IsGiftCard { get; set; }
        public bool IsRequired { get; set; }
    }
}
