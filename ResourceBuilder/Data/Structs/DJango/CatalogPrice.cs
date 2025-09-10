using Org.BouncyCastle.Security;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ResourceBuilder.Data.Structs.DJango
{
    [Table("Catalog_price", Schema = "public")]
    public class CatalogPrice
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
        [Column("type")]
        [StringLength(10)]
        public string Type { get; set; }

        [Required]
        [Column("minimum")]
        public int Minimum { get; set; }

        [Required]
        [Column("value")]
        public double Value { get; set; }

        [Column("start")]
        public DateTime? Start { get; set; }

        [Column("end")]
        public DateTime? End { get; set; }

        [Required]
        [Column("status")]
        public bool Status { get; set; }

        [Required]
        [Column("sku_id")]
        public int SkuId { get; set; }

        [Column("store_id")]
        public long? StoreId { get; set; }

        // 🔗 Relaciones (FKs)
        [ForeignKey("SkuId")]
        public virtual CatalogSku Sku { get; set; }

        [ForeignKey("StoreId")]
        public virtual OmsStore Store { get; set; }
    }
}
