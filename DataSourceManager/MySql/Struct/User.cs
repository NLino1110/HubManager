using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace DataSourceManager.MySql.Struct
{
    [Table("SecurityUser")]
    public partial class User
    {        
        public User()
        {
            refreshTokens = new HashSet<RefreshToken>();
        }

        //[Required, Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
        [Key]
        public int userId { get; set; }
        [EmailAddress(ErrorMessage = "Formato de email incorrecto.")]
        public string emailAddress { get; set; }

        //[Required(ErrorMessage = "Password es requerido")]
        //[StringLength(255, ErrorMessage = "Largo de password entre 5 y 255 caracteres", MinimumLength = 5)]
        //[DataType(DataType.Password)]
        public string? password { get; set; }
        public string? source { get; set; }
        public string firstName { get; set; }
        public string middleName { get; set; }
        public string lastName { get; set; }
        public short? roleId { get; set; }
        public int pubId { get; set; }
        public DateTime? hireDate { get; set; }

        
        [NotMapped]
        public string accessToken { get; set; }
        [NotMapped]
        public string refreshToken { get; set; }

        [NotMapped]
        public virtual ICollection<RefreshToken> refreshTokens { get; set; }
        public int? supplier_id { get; set; }
    }
}
