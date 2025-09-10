using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ResourceBuilder.Data.Structs.DJango
{
    [Table("Settings_identity", Schema = "public")]
    public class SettingsIdentity
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("app")]
        public string App { get; set; }

        [Column("object_id")]
        public int ObjectId { get; set; } // Este es el Id de CatalogSku

        [Column("external_id")]
        public string ExternalId { get; set; }

        [Column("content_type_id")]
        public int ContentTypeId { get; set; }
    }
}
