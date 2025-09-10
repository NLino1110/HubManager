using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace ResourceBuilder.Data.Structs.DJango
{
    [Table("Catalog_category", Schema = "public")]
    public class CatalogCategory
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

        [Column("main_id")]
        public int? MainId { get; set; }

        [Required]
        [Column("merchant_id")]
        public int MerchantId { get; set; }

        [Column("description")]
        public string Description { get; set; }

        [Column("keywords")]
        public string Keywords { get; set; }

        // 🔗 Relación con la categoría padre (self-reference)
        [ForeignKey("MainId")]
        public virtual CatalogCategory MainCategory { get; set; }

        // 🔗 Categorías hijas
        public virtual ICollection<CatalogCategory> SubCategories { get; set; } = new List<CatalogCategory>();

        // 🔗 Relación con SettingsMerchant
        [ForeignKey("MerchantId")]
        public virtual SettingsMerchant Merchant { get; set; }

        // 🔗 Productos relacionados a la categoría
        public virtual ICollection<CatalogProduct> Products { get; set; } = new List<CatalogProduct>();
    }
}
