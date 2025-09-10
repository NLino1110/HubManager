using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace ResourceBuilder.Data.Structs.DJango
{
    [Table("Settings_channel", Schema = "public")]
    public class SettingsChannel
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [Column("name")]
        [StringLength(50)]
        public string Name { get; set; }

        [Required]
        [Column("merchant_id")]
        public int MerchantId { get; set; }

        // 🔗 Relación con SettingsMerchant
        [ForeignKey("MerchantId")]
        public virtual SettingsMerchant Merchant { get; set; }

        // 🔗 Colección de tiendas asociadas a este canal
        public virtual ICollection<OmsStore> Stores { get; set; } = new List<OmsStore>();
    }
}
