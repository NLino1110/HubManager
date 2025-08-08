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

    [Table("GENCLIENTES")]
    public class GenClientes
    {
        [Key]
        [Required]
        [Column("CODCLIENTE")]
        public int CodCliente { get; set; }

        [Required]
        [Column("NOMBRES")]
        [StringLength(150)]
        public string Nombres { get; set; }

        [Column("APELLIDOS")]
        [StringLength(150)]
        public string Apellidos { get; set; }

        [Required]
        [Column("TIPOIDENTIFICACION")]
        [StringLength(1)]
        public string TipoIdentificacion { get; set; }

        [Required]
        [Column("IDENTIFICACION")]
        [StringLength(15)]
        public string Identificacion { get; set; }

        [Required]
        [Column("DOMICILIO")]
        [StringLength(200)]
        public string Domicilio { get; set; }

        [Column("CODEMPRESA")]
        public int? CodEmpresa { get; set; }

        [Required]
        [Column("CODCIUDAD")]
        public int CodCiudad { get; set; }

        [Column("TELEFONO1")]
        [StringLength(20)]
        public string Telefono1 { get; set; }

        [Column("TELEFONO2")]
        [StringLength(20)]
        public string Telefono2 { get; set; }

        [Column("TELEFONO3")]
        [StringLength(20)]
        public string Telefono3 { get; set; }

        [Column("TELEFONO4")]
        [StringLength(20)]
        public string Telefono4 { get; set; }

        [Column("CELULAR")]
        [StringLength(25)]
        public string Celular { get; set; }

        [Column("FAX")]
        [StringLength(20)]
        public string Fax { get; set; }

        [Column("LUGARTRABAJO")]
        [StringLength(300)]
        public string LugarTrabajo { get; set; }

        [Column("TELEFONOTRABAJO")]
        [StringLength(15)]
        public string TelefonoTrabajo { get; set; }

        [Column("FAXTRAB")]
        [StringLength(15)]
        public string FaxTrab { get; set; }

        [Column("NOMBRECOMERCIAL")]
        [StringLength(80)]
        public string NombreComercial { get; set; }

        [Column("DIRECCIONLOCAL")]
        [StringLength(250)]
        public string DireccionLocal { get; set; }

        [Column("CODCIUDADLOCAL")]
        public int? CodCiudadLocal { get; set; }

        [Column("LOCALPROPIO")]
        [StringLength(1)]
        public string LocalPropio { get; set; }

        [Column("TIEMPOACTIVIDAD")]
        [StringLength(50)]
        public string TiempoActividad { get; set; }

        [Column("HORAATENCIONDESDE")]
        [StringLength(8)]
        public string HoraAtencionDesde { get; set; }

        [Column("HORAATENCIONHASTA")]
        [StringLength(8)]
        public string HoraAtencionHasta { get; set; }

        [Column("EMAIL")]
        [StringLength(200)]
        public string Email { get; set; }

        [Column("PERSONERIA")]
        [StringLength(1)]
        public string Personeria { get; set; }

        [Column("COMENTARIOS")]
        [StringLength(250)]
        public string Comentarios { get; set; }

        [Column("FOTO")]
        [StringLength(50)]
        public string Foto { get; set; }

        [Column("CALIFICACIONCREDITICIA")]
        [StringLength(100)]
        public string CalificacionCrediticia { get; set; }

        [Required]
        [Column("ESENTIDADFINANCIERA")]
        [StringLength(1)]
        public string EsEntidadFinanciera { get; set; }

        [Required]
        [Column("CODUSUARIO")]
        [StringLength(15)]
        public string CodUsuario { get; set; }

        [Required]
        [Column("FECHA")]
        public DateTime Fecha { get; set; }

        [Column("TELEFONOLOCAL1")]
        [StringLength(12)]
        public string TelefonoLocal1 { get; set; }

        [Column("TELEFONOLOCAL2")]
        [StringLength(12)]
        public string TelefonoLocal2 { get; set; }

        [Column("APELLIDOSNOMBRES")]
        [StringLength(105)]
        public string ApellidosNombres { get; set; }

        [Column("USUARIOMODIFICACION")]
        [StringLength(15)]
        public string UsuarioModificacion { get; set; }

        [Column("FECHAMODIFICACION")]
        public DateTime? FechaModificacion { get; set; }

        [Column("SEXO")]
        [StringLength(1)]
        public string Sexo { get; set; }

        [Column("CODESTADOCIVIL")]
        [StringLength(1)]
        public string CodEstadoCivil { get; set; }

        [Column("CODORIGENINGRESO")]
        [StringLength(1)]
        public string CodOrigenIngreso { get; set; }

        [Column("CODPARROQUIA")]
        public int? CodParroquia { get; set; }

        [Column("USUARIOCAMBIOPERSONERIA")]
        [StringLength(15)]
        public string UsuarioCambioPersoneria { get; set; }

        [Column("FECHACAMBIOPERSONERIA")]
        public DateTime? FechaCambioPersoneria { get; set; }

        [Column("BLOQUEAPROMOCIONES")]
        [StringLength(1)]
        public string BloqueaPromociones { get; set; }

        [Column("ESMODIFICABLEPEDIDOS")]
        [StringLength(1)]
        public string EsModificablePedidos { get; set; }

        [Column("ESAUTOSERVICIO")]
        [StringLength(1)]
        public string EsAutoservicio { get; set; }

        [Column("CODEMPRESATRABAJACLI")]
        public int? CodEmpresaTrabajaCli { get; set; }

        [Column("FECHANACIMIENTO")]
        public DateTime? FechaNacimiento { get; set; }

        [Column("ESPROFESIONAL")]
        [StringLength(1)]
        public string EsProfesional { get; set; }

        [Column("CODTIPOPROFESIONAL")]
        public int? CodTipoProfesional { get; set; }

        [Column("ORIGEN")]
        [StringLength(1)]
        public string Origen { get; set; }

        [Column("ESCLIENTEEVENTO")]
        [StringLength(1)]
        public string EsClienteEvento { get; set; }

        [Column("CODTIPOSTATUSDOCUMENTO")]
        public int? CodTipoStatusDocumento { get; set; }

        [Column("CODTIPOCALIFICACREDITICIA")]
        public int? CodTipoCalificaCrediticia { get; set; }

        [Column("CODCLIENTEPRINCIPAL")]
        public int? CodClientePrincipal { get; set; }

        [Column("LONGITUD")]
        [StringLength(50)]
        public string Longitud { get; set; }

        [Column("LATITUD")]
        [StringLength(50)]
        public string Latitud { get; set; }

        [Column("REDESSOCIALES")]
        [StringLength(200)]
        public string RedesSociales { get; set; }

        [Column("ESVALIDSRIMANUAL")]
        [StringLength(1)]
        public string EsValidSriManual { get; set; }
    }

}
