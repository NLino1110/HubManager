using DMSA.Models.Odoo.Tools;
using SQLite;
using System.ComponentModel.DataAnnotations;

namespace DMSA.Models.Odoo.Abstract
{
    [Table("OdooConnections")]
    public partial class OdooConnection
    {
        [PrimaryKey]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [Column("name")]
        public string Name { get; set; } = string.Empty;

        [Required]
        [Column("host")]
        public string Host { get; set; } = string.Empty;
                
        [Required]
        [Column("host_dump")]
        public string HostDump { get; set; } = string.Empty;
                
        [Column("host_dump_api_key")]
        public string HostDumpApiKey { get; set; } = string.Empty;

        [Required]
        [Column("host_push")]
        public string HostPush { get; set; } = string.Empty;

        [Obsolete("")]
        [Required]
        [Column("dump_service")]
        public string DumpService { get; set; } = string.Empty;

        [Required]
        [Column("db_name")]
        public string DbName { get; set; } = string.Empty; // nombre de la base de datos

        [Ignore]
        public string DbNameSqlite
        {
            get
            {
                return string.Concat(prefix_db ?? string.Empty, DbName ?? string.Empty);
            }
        }

        [Ignore]
        public string DbNameSqliteStatic
        {
            get
            {
                return string.Concat(prefix_db ?? string.Empty, DbName ?? string.Empty, "_static");
            }
        }

        [Required]
        [Column("db_limit_default")]
        public int DbLimitDefault { get; set; } = 300;

        [Required]
        [Column("company_id")]
        public int CompanyId { get; set; }

        [Required]
        [Column("res_center_default")]
        public int res_center_default { get; set; }

        [Required]
        [Column("username")]
        public string Username { get; set; } = string.Empty;

        [Required]
        [Column("password")]
        public string Password { get; set; } = string.Empty;

        [Required]
        [Column("active")]
        public bool Active { get; set; }

        [Required]
        [Column("is_prod")]
        public bool IsProduction { get; set; }

        [Required]
        [Column("is_test")]
        public bool IsTestMode { get; set; }

        [Column("create_date")]
        public long CreateDate { get; set; } = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        [Column("write_date")]
        public long WriteDate { get; set; } = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        [Required]
        [Column("data_tolerance_days")]
        public int DataToleranceDays { get; set; }

        [Required]
        [Column("sale_channel_default")]
        public int sale_channel_default { get; set; }

        [Column("prefix_db")]
        public string prefix_db { get; set; }

        [Column("email_domain")]
        public string? email_domain { get; set; } = "macronegocios.ec";

        [Column("preload_email_domain")]
        public bool preload_email_domain { get; set; } = true;

        [Column("stage_id")]
        public int stage_id { get; set; }
        [Column("project_id")]
        public int project_id { get; set; }
        [Column("parent_id")]
        public int parent_id { get; set; }

        [Column("subclasificacion_gasto_default")]
        public int subclasificacion_gasto_default { get; set; }

        [Column("username_front")]
        public string UsernameFront { get; set; } = string.Empty;
                
        [Column("password_front")]
        public string PasswordFront { get; set; } = string.Empty;
    }
}
