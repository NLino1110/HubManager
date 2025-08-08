using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Models.DMSA.Mbw.Core
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("GENAGENCIAS")]
    public class GenAgencias
    {
        [Key]
        [Column("CODAGENCIA")]
        public int CodAgencia { get; set; }

        [Column("CODEMPRESA")]
        public int CodEmpresa { get; set; }

        [Required]
        [Column("NOMBRE")]
        [StringLength(100)]
        public string? Nombre { get; set; }

        [Required]
        [Column("DIRECCION")]
        [StringLength(200)]
        public string? Direccion { get; set; }

        [Required]
        [Column("TIPOAGENCIA")]
        [StringLength(1)]
        public string? TipoAgencia { get; set; }

        [Column("NUMERORUC")]
        [StringLength(20)]
        public string? NumeroRuc { get; set; }

        [Required]
        [Column("TELEFONO1")]
        [StringLength(15)]
        public string? Telefono1 { get; set; }

        [Column("TELEFONO2")]
        [StringLength(15)]
        public string? Telefono2 { get; set; }

        [Required]
        [Column("FAX")]
        [StringLength(15)]
        public string? Fax { get; set; }

        [Column("CELULAR")]
        [StringLength(15)]
        public string? Celular { get; set; }

        [Column("EMAIL")]
        [StringLength(30)]
        public string? Email { get; set; }

        [Required]
        [Column("CODCIUDAD")]
        public int CodCiudad { get; set; }

        [Required]
        [Column("CODESTADO")]
        public int CodEstado { get; set; }

        [Column("USUARIOCAMBIOESTADO")]
        [StringLength(15)]
        public string? UsuarioCambioEstado { get; set; }

        [Column("FECHACAMBIOESTADO")]
        public DateTime? FechaCambioEstado { get; set; }

        [Required]
        [Column("CODUSUARIO")]
        [StringLength(15)]
        public string? CodUsuario { get; set; }

        [Required]
        [Column("FECHA")]
        public DateTime Fecha { get; set; }

        [Column("CODTIPOLOCAL")]
        public int? CodTipoLocal { get; set; }

        [Column("NOMBRECOMERCIAL")]
        [StringLength(50)]
        public string? NombreComercial { get; set; }

        [Column("DIRECCIONMATRIZ")]
        [StringLength(200)]
        public string? DireccionMatriz { get; set; }

        [Column("WEB")]
        [StringLength(1)]
        public string? Web { get; set; }

        [Required]
        [Column("UBICACIONREPORTE")]
        [StringLength(1)]
        public string? UbicacionReporte { get; set; }

        [Required]
        [Column("ENVIOECOMMERCE")]
        [StringLength(1)]
        public string? EnvioEcommerce { get; set; }

        [Column("HABILITADA")]
        [StringLength(1)]
        public string? Habilitada { get; set; }

        [Column("USANIVELMATRIZ")]
        [StringLength(1)]
        public string? UsaNivelMatriz { get; set; }

        [Column("TIEMPOLIMITEDEP")]
        [StringLength(5)]
        public string? TiempoLimiteDep { get; set; }

        [Column("FECHAENVIO")]
        public DateTime? FechaEnvio { get; set; }

        [Column("DPADREPROTAURUS")]
        [StringLength(1)]
        public string? DpadreProtaurus { get; set; }

        [Column("DPADRECONTTAURUS")]
        public int? DpadreContTaurus { get; set; }

        [Column("DPADRECONTPLAY")]
        public int? DpadreContPlay { get; set; }
    }

}
