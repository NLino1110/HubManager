using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace ResourceBuilder.Data.Structs.SyncTask
{
    public class ProductSync
    {        
        public string prod_vtex_id { get; set; }        
        public int prod_vtex_sku { get; set; }        
        public string account_name { get; set; }
        public string prod_name { get; set; }
        public int prod_po { get; set; }
        public decimal? prod_pvp { get; set; }
        public decimal? prod_pvp_up { get; set; }
        public int? prod_stock { get; set; }
        public int? prod_stock_up { get; set; }
        public decimal? prod_costo { get; set; }
        public decimal? prod_costo_up { get; set; }
        public decimal? prod_disc { get; set; }
        public decimal? prod_disc_up { get; set; }
        public int? prod_prov { get; set; }
        public int? prod_status { get; set; }        
        public DateTime? aud_mod_date { get; set; }
        public int productid { get; set; }
        public DateTime? aud_ins_date { get; set; }
        public int? prod_active { get; set; }
        public int? prod_active_up { get; set; }
        public decimal? prod_pvp_ant { get; set; }
        public decimal? prod_pvp_ant_up { get; set; }
        public string prod_url_img_01 { get; set; }
        public bool prod_mkplace { get; set; }
        public string prod_ensamble { get; set; }

        //[JsonIgnore]
        //[NotMapped]
        public int? prod_reserved { get; set; }
    }
}
