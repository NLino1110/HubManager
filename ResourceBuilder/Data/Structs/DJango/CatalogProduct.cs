using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ResourceBuilder.Data.Structs.DJango
{
    [Table("Catalog_product", Schema = "public")]
    public class CatalogProduct
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
        [Column("reference")]
        [StringLength(50)]
        public string Reference { get; set; }

        [Required]
        [Column("name")]
        [StringLength(200)]
        public string Name { get; set; }

        [Column("short_description")]
        public string ShortDescription { get; set; }

        [Column("description")]
        public string Description { get; set; }

        [Column("brand_id")]
        public int? BrandId { get; set; }

        [Column("category_id")]
        public int? CategoryId { get; set; }

        [Required]
        [Column("merchant_id")]
        public int MerchantId { get; set; }

        [Column("keywords")]
        public string Keywords { get; set; }

        [Required]
        [Column("promotional")]
        public bool Promotional { get; set; }

        // 🔗 Relaciones
        [ForeignKey("BrandId")]
        public virtual CatalogBrand Brand { get; set; }

        [ForeignKey("CategoryId")]
        public virtual CatalogCategory Category { get; set; }

        [ForeignKey("MerchantId")]
        public virtual SettingsMerchant Merchant { get; set; }

        public virtual ICollection<CatalogSku> Skus { get; set; } = new List<CatalogSku>();
    }
}
