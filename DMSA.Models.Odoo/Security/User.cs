//using SQLite;
//using CobranzasDMSA_Odoo.Models;
using DMSA.Models.Odoo.Native;
using DMSA.Models.Odoo.Tools;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMSA.Models.Security
{
    public class User
    {
        //public string exito { get; set; }
        //public string mensaje { get; set; }
        public int uid { get; set; }
        public int partner_id { get; set; }

        public string nombres { get; set; }
        public string nivel { get; set; }

        //TODO: Nombre de campo confuso
        // este campo es obtenido del api (VERIFICAR_USUARIO_WSJSON)
        // indica la fecha y hora actual
        public string fechasincronizado { get; set; }

        [NotMapped]
        public string access_token { get; set; }
        [NotMapped]
        public string refresh_token { get; set; }
        [NotMapped]
        public string api_key { get; set; }
        [NotMapped]
        public string token_type { get; set; }

        [NotMapped]
        public string? password { get; set; }
        [NotMapped]
        public string? username { get; set; }
        [NotMapped]
        public string? databasename { get; set; }
        //[NotMapped]
        //public string? fechatablet { get; set; }

        [NotMapped]
        //public string empresas { get; set; }
        public res_company[]? empresas { get; set; }

        [NotMapped]
        public bool Done { get; set; }

        [NotMapped]
        public DateTime log_fec_acceso { get; set; }

        [NotMapped]
        public DateTime log_fec_sincro { get; set; }

        [NotMapped]
        public DateTime log_fec_sincro_nc { get; set; }

        //[NotMapped]
        //[PrimaryKey, AutoIncrement]
        //public int uid { get; set; }

        public string GetPasswordDecrypt()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(password))
                    return string.Empty;

                return CryptoHelper.Decrypt(password);
            }
            catch (System.Security.Cryptography.CryptographicException)
            {                
                return string.Empty;
            }
            catch (Exception ex)
            {                
                Console.WriteLine($"Error al desencriptar: {ex.Message}");
                return string.Empty;
            }
        }
    }
}
