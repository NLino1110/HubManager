using DMSA.Models.Odoo.Tools;
using SQLite;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMSA.Models.Odoo.Abstract
{
    [Table("OdooConnections")]
    public class OdooConnection
    {
        [PrimaryKey]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [Column("name")]
        public string Name { get; set; } = string.Empty;  // nombre descriptivo de la conexión

        [Required]
        [Column("host")]
        public string Host { get; set; } = string.Empty;  // host de Odoo

        [Obsolete("")]
        [Required]
        [Column("host_dump")]
        public string HostDump { get; set; } = string.Empty;
        
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
        public string Username { get; set; } = string.Empty; // usuario de Odoo

        [Required]
        [Column("password")]
        public string Password { get; set; } = string.Empty; // contraseña de Odoo

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
                
        [Column("username_front")]
        public string UsernameFront { get; set; } = string.Empty;
                
        [Column("password_front")]
        public string PasswordFront { get; set; } = string.Empty;

        public List<OdooConnection> LoadDefault()
        {
            List<OdooConnection> defaultSettings = new List<OdooConnection>();
            
            defaultSettings.Add(new OdooConnection()
            {
                Id = 2,
                CompanyId = 1,
                Name = "Macronegocios (DEV)",
                Host = "https://dev-qa.macronegocios/",
                DbName = "macronegocios",
                Username = "admin",
                Password = CryptoHelper.Encrypt("demo"),
                Active = false,
                HostDump = "https://192.168.204.66:2443",
                DumpService = "/resources/tmp/android/sqlite/",
                HostPush = "https://admin.dmujeres.ec:1500/",
                DbLimitDefault = 300,
                IsProduction = false,
                IsTestMode = true,
                DataToleranceDays = 365,
                sale_channel_default = 8,
                res_center_default = 49,
                prefix_db = "dev_",
                email_domain = "macronegocios.ec",
                preload_email_domain = true,
                UsernameFront = "jchonillo",
                PasswordFront = "Y0.dM3c"
            }
            );

            defaultSettings.Add(new OdooConnection()
            {
                Id = 3,
                CompanyId = 1,
                Name = "DMujeres (DEV)",
                Host = "https://dev-qa.macronegocios/",
                DbName = "dmujeressa",
                Username = "admin",
                Password = CryptoHelper.Encrypt("demo"),
                Active = false,
                HostDump = "https://192.168.204.66:2443",
                DumpService = "/resources/tmp/android/sqlite/",
                DbLimitDefault = 300,
                IsProduction = false,
                IsTestMode = true,
                DataToleranceDays = 365,
                sale_channel_default = 8,
                res_center_default = 49,
                prefix_db = "dev_",
                email_domain = "macronegocios.ec",
                preload_email_domain = true,
                PasswordFront = "Y0.dM3c"
            }
            );
         
            defaultSettings.Add(new OdooConnection()
            {
                Id = 6,
                CompanyId = 1,
                Name = "DMujeres Prod",
                Host = "https://www.dmujeressa.ec/",
                DbName = "dmujeressa",
                Username = "admin",
                Password = CryptoHelper.Encrypt("demo"),
                Active = true,
                HostDump = "https://192.168.204.66:2443",
                DumpService = "/resources/tmp/android/sqlite/",
                DbLimitDefault = 300,
                IsProduction = true,
                IsTestMode = false,
                DataToleranceDays = 365,
                sale_channel_default = 3,
                res_center_default = 2,
                prefix_db = "prod1_",
                email_domain = "macronegocios.ec",
                preload_email_domain = true,
                PasswordFront = "Y0.dM3c"
            }
            );

            defaultSettings.Add(new OdooConnection()
            {
                Id = 7,
                CompanyId = 1,
                Name = "Macronegocios Prod",
                Host = "https://www.macronegocios.ec/",
                DbName = "macronegocios",
                Username = "admin",
                Password = CryptoHelper.Encrypt("demo"),
                Active = true,
                HostDump = "https://192.168.204.66:2443",
                DumpService = "/resources/tmp/android/sqlite/",
                DbLimitDefault = 300,
                IsProduction = true,
                IsTestMode = false,
                DataToleranceDays = 365,
                sale_channel_default = 8,
                res_center_default = 49,
                prefix_db = "prod1_",
                email_domain = "macronegocios.ec",
                preload_email_domain = true,
                PasswordFront = "Y0.dM3c"
            }
            );

            return defaultSettings;
        }
    }
}
