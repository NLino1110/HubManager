using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ResourceBuilder.Data.Structs.SyncTask
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
        [Key]
        public int Id { get; set; }
        public int SkuServiceTypeId { get; set; }
        public int SkuServiceValueId { get; set; }
        public int SkuId { get; set; }
        public string Name { get; set; }
        public string Text { get; set; }
        public bool IsActive { get; set; }
        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public DateTime aud_ins_date { get; set; }
    }
}
