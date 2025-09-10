using ResourceBuilder.Data.Structs.SyncTask;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using VtexStrucs.Strucs;

namespace ResourceBuilder.Data.Structs
{
    public class ItemForCompare
    {
        public string accountName { get; set; }
        //public string DetailUrl { get; set; }
        //public int ProductId { get; set; }
        //public int SkuId { get; set; }
        //public string RefId { get; set; }
        //public bool IsActive { get; set; }
        //public string ProductName { get; set; }
        //public string SkuImageUrl { get; set; }
        //public double price { get; set; }
        //public double listPrice { get; set; }
        //public double stock { get; set; }
        //public double reserved { get; set; }
        //public string brand { get; set; }        
        //public Dictionary<string, string> ProductCategories { get; set; } = new();
        //public List<Service> Services { get; set; }

        //Siscom Data
        public string siscomId { get; set; }
        public string siscomRefId { get; set; }
        public string siscomName { get; set; }
        public double siscomPvp { get; set; }
        //public double siscomPvpAnt { get; set; }
        public double siscomStock { get; set; }
        public string siscomEstado { get; set; }
        public bool siscomMktp { get; set; }
        public string siscomEnsambleId { get; set; }
        public string siscomEnsambleNombre { get; set; }
        public double siscomEnsambleCosto { get; set; }
        public double PROD_PESO_KG { get; set; }

        //Sync Data
        public ProductSync productSync { get; set; }
        //public string syncId { get; set; }
        //public string syncRefId { get; set; }
        //public string syncName { get; set; }
        //public double syncPvp { get; set; }
        //public double syncPvpAnt { get; set; }
        //public double syncStock { get; set; }
        //public int syncProvId { get; set; }

        public ItemForCompare_Promocion[] siscomPromociones { get; set; }

        public VtexStrucs.Strucs.ProductByFilter productByFilter { get; set; }
        public VtexStrucs.Strucs.ProductFull productFull { get; set; }
        //Contiene los datos del proveedor
        public VtexStrucs.Strucs.Product product { get; set; }
        public VtexStrucs.vTexPrice price { get; set; }
        public VtexStrucs.vtexStock stock { get; set; }
        public  VtexStrucs.Strucs.Supplier supplier { get; set; }
        
    }
}
