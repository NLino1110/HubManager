using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ResourceBuilder.Data.Structs.DJango
{
    [Table("OMS_store", Schema = "public")]
    public class OmsStore
    {
        [Key]
        [Column("id")]
        public long Id { get; set; }   // bigserial → long (Int64)

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
        [Column("merchant_id")]
        public int MerchantId { get; set; }

        [Required]
        [Column("ecommerce")]
        public bool Ecommerce { get; set; }

        [Column("main_id")]
        public long? MainId { get; set; }   // FK a la misma tabla

        [Column("channel_id")]
        public int? ChannelId { get; set; }

        // 🔗 Relaciones
        [ForeignKey("MainId")]
        public virtual OmsStore? MainStore { get; set; }

        public virtual ICollection<OmsStore> SubStores { get; set; } = new List<OmsStore>();

        [ForeignKey("ChannelId")]
        public virtual SettingsChannel? Channel { get; set; }

        [ForeignKey("MerchantId")]
        public virtual SettingsMerchant Merchant { get; set; }

        public virtual ICollection<CatalogPrice> Prices { get; set; } = new List<CatalogPrice>();
    }
}
