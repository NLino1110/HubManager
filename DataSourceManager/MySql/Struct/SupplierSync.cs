using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel.DataAnnotations;

namespace DataSourceManager.MySql.Struct
{
    public class SupplierSync
    {
        public int? id_vtex { get; set; }
        [Required(ErrorMessage = "Debe colocar un ruc válido.")]
        [Display(Name = "Ruc")]
        [StringLength(13, MinimumLength = 13, ErrorMessage = "RUC debe tener 13 dígitos")]
        [DataType(DataType.PhoneNumber)]
        //[RegularExpression(@"^\(?([0-9]{3})\)?[-. ]?([0-9]{3})[-. ]?([0-9]{4})$", ErrorMessage = "Not a valid phone number")]
        [RegularExpression(@"^(\d{13})$", ErrorMessage = "Ruc inválido")]
        public string ruc { get; set; }
        public string name_vtex { get; set; }
        public int? id_siscom { get; set; }
        public string password { get; set; }
        public string razon_social { get; set; }
        [EmailAddress(ErrorMessage = "Formato de email incorrecto.")]
        [Display(Name = "Email comercial")]
        public string email_com { get; set; }
        public string rep_legal { get; set; }
        [EmailAddress(ErrorMessage = "Formato de email incorrecto.")]
        [Display(Name = "Email representante legal")]
        public string email_rep_legal { get; set; }
        public decimal? margen { get; set; }
        public DateTime? aud_ins_date { get; set; }
        public DateTime? aud_mod_date { get; set; }
        public int? allowed_app { get; set; }
    }
}
