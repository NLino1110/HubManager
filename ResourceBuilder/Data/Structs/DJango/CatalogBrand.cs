using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ResourceBuilder.Data.Structs.DJango
{
    [Table("Catalog_brand", Schema = "public")]
    public class CatalogBrand
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [Column("created_at")]
        public DateTime CreatedAt { get; set; }

        [Required]
        [Column("modified_at")]
        public DateTime ModifiedAt { get; set; }

        [Required]
        [Column("name")]
        [StringLength(50)]
        public string Name { get; set; }

        [Required]
        [Column("status")]
        public bool Status { get; set; }

        [Required]
        [Column("merchant_id")]
        public int MerchantId { get; set; }

        [Column("description")]
        public string Description { get; set; }

        [Column("keywords")]
        public string Keywords { get; set; }

        // 🔗 Relación con SettingsMerchant
        [ForeignKey("MerchantId")]
        public virtual SettingsMerchant Merchant { get; set; }

        // 🔗 Relación uno-a-muchos con CatalogProduct
        public virtual ICollection<CatalogProduct> Products { get; set; } = new List<CatalogProduct>();
    }
}
