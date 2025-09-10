using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ResourceBuilder.Data.Structs.DJango
{
    [Table("Settings_merchant", Schema = "public")]
    public class SettingsMerchant
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [Column("name")]
        [StringLength(20)]
        public string Name { get; set; }

        [Column("erp")]
        [StringLength(20)]
        public string Erp { get; set; }

        [Column("facebook_id")]
        public long? FacebookId { get; set; }

        [Column("main_id")]
        public long? MainId { get; set; }

        [Column("email")]
        [StringLength(254)]
        public string Email { get; set; }

        // 🔗 Relaciones
        //[ForeignKey("FacebookId")]
        //public virtual OmsStore FacebookStore { get; set; }

        [ForeignKey("MainId")]
        public virtual OmsStore MainStore { get; set; }
        public ICollection<SettingsChannel> Channels { get; set; } = new List<SettingsChannel>();
        public ICollection<CatalogBrand> Brands { get; set; } = new List<CatalogBrand>();
        public ICollection<CatalogCategory> Categories { get; set; } = new List<CatalogCategory>();
        public ICollection<CatalogProduct> Products { get; set; } = new List<CatalogProduct>();
        public ICollection<OmsStore> Stores { get; set; } = new List<OmsStore>();
    }
}
