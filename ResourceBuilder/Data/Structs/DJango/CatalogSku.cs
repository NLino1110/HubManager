using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ResourceBuilder.Data.Structs.DJango
{
   
    [Table("Catalog_sku", Schema = "public")]
    public class CatalogSku
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

        [Required]
        [Column("weight")]
        public double Weight { get; set; }

        [Required]
        [Column("width")]
        public double Width { get; set; }

        [Required]
        [Column("height")]
        public double Height { get; set; }

        [Required]
        [Column("length")]
        public double Length { get; set; }

        [Required]
        [Column("status")]
        public bool Status { get; set; }

        [Required]
        [Column("product_id")]
        public int ProductId { get; set; }

        [Required]
        [Column("show_store")]
        public bool ShowStore { get; set; }

        [Required]
        [Column("show_web")]
        public bool ShowWeb { get; set; }

        // 🔗 Relaciones
        [ForeignKey("ProductId")]
        public virtual CatalogProduct Product { get; set; }

        public virtual ICollection<CatalogPrice> Prices { get; set; } = new List<CatalogPrice>();
    }


}
