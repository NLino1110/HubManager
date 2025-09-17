using DMSA.Models.Odoo.Tools;
using SQLite;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMSA.Models.Odoo
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

        [Required]
        [Column("database")]
        public string Database { get; set; } = string.Empty; // nombre de la base de datos

        [Required]
        [Column("username")]
        public string Username { get; set; } = string.Empty; // usuario de Odoo

        [Required]
        [Column("password")]
        public string Password { get; set; } = string.Empty; // contraseña de Odoo

        [Required]
        [Column("active")]
        public bool Active { get; set; } = true;

        [Column("create_date")]
        public long CreateDate { get; set; } = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        [Column("write_date")]
        public long WriteDate { get; set; } = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        public List<OdooConnection> LoadDefault()
        {
            List<OdooConnection> defaultSettings = new List<OdooConnection>();
            defaultSettings.Add(new OdooConnection()
                {
                    Id = 1,
                    Name = "Macronegocios",
                    Host = "http://localhost:8069/",
                    Database = "qamacronegocios",
                    Username = "admin",
                    Password = CryptoHelper.Encrypt("demo"),
                }
            );

            defaultSettings.Add(new OdooConnection()
                {
                    Id = 2,
                    Name = "DMujeres",
                    Host = "http://localhost:8069/",
                    Database = "qadmujeres",
                    Username = "admin",
                    Password = CryptoHelper.Encrypt("demo"),
                }
            );

            defaultSettings.Add(new OdooConnection()
                {
                    Id = 3,
                    Name = "Indeterminado",
                    Host = "http://localhost:8069/",
                    Database = "xxx",
                    Username = "admin",
                    Password = CryptoHelper.Encrypt("demo"),
                }
            );

            return defaultSettings;
        }
    }
}
