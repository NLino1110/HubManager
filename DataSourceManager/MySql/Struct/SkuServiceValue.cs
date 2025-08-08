using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataSourceManager.MySql.Struct
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

        [Key]
        public int Id { get; set; }
        public int SkuServiceTypeId { get; set; }
        public string Name { get; set; }
        public decimal Value { get; set; }
        public decimal Cost { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public DateTime aud_ins_date { get; set; }
    }
}
