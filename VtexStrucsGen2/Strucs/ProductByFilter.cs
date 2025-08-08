using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace VtexStrucs.Strucs
{
    public class ProductByFilter
    {
        public ProductByFilterItem[] items { get; set; }
        public Paging paging { get; set; }
    }

    public class ProductByFilterItem
    {
        public stockKeepingUnitBasicDtoCollectionT[] stockKeepingUnitBasicDtoCollection { get; set; }
        public int? productId { get; set; }
        public string? refId { get; set; }
        public string? productName { get; set; }
        public string? imageUrl { get; set; }
        public string? detailUrl { get; set; }
        public bool? isActive { get; set; }
        public int[] productClusterIds { get; set; }
        public string brand { get; set; }
        //public int? SkuId { get; set; }        
    }

    public class Paging
    {
        public int page { get; set; }
        public int perPage { get; set; }
        public int total { get; set; }
        public int pages { get; set; }
        public int limit { get; set; }
    }

    public class stockKeepingUnitBasicDtoCollectionT
    {
        public int? id { get; set; }
        public string? skuName { get; set; }
        public bool? isKit { get; set; }
        public string? refId { get; set; }
        public string[] skuKitItems { get; set; }
        public int[] productClusterIds { get; set; }
        public string? imageUrl { get; set; }        
        public bool? isActive { get; set; }

    }
}
