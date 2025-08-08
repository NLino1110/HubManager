using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Models.DMSA.Mbw.Inventario;

namespace Models.DMSA.Mbw.Sales
{
    [Table("GENVENDEDORES")]
    public class GenVendedores
    {
        [Required]
        [Column("CODEMPRESA")]
        public int CodEmpresa { get; set; }

        [Required]
        [Column("CODVENDEDOR")]
        public int CodVendedor { get; set; }

        [Required]
        [Column("NOMBRES")]
        [StringLength(100)]
        public string Nombres { get; set; }

        [Required]
        [Column("APELLIDOS")]
        [StringLength(100)]
        public string Apellidos { get; set; }

        [Required]
        [Column("DIRECCION")]
        [StringLength(80)]
        public string Direccion { get; set; }

        [Required]
        [Column("CODUSUARIO")]
        [StringLength(15)]
        public string CodUsuario { get; set; }

        [Required]
        [Column("CODESTADO")]
        public int CodEstado { get; set; }

        [Column("USUARIOCAMBIOESTADO")]
        [StringLength(15)]
        public string UsuarioCambioEstado { get; set; }

        [Required]
        [Column("FECHA")]
        public DateTime Fecha { get; set; }

        [Column("FECHACAMBIOESTADO")]
        public DateTime? FechaCambioEstado { get; set; }

        [Required]
        [Column("CODUSUARIOLOGIN")]
        [StringLength(15)]
        public string CodUsuarioLogin { get; set; }

        [Required]
        [Column("TIPOIDENTIFICACION")]
        [StringLength(1)]
        public string TipoIdentificacion { get; set; }

        [Required]
        [Column("IDENTIFICACION")]
        [StringLength(15)]
        public string Identificacion { get; set; }

        [Required]
        [Column("TIPO")]
        [StringLength(1)]
        public string Tipo { get; set; }

        [Column("CODEMPRESAJEFE")]
        public int? CodEmpresaJefe { get; set; }

        [Column("CODVENDEDORJEFE")]
        public int? CodVendedorJefe { get; set; }

        [Column("NUMDIASPLAZO")]
        public int? NumDiasPlazo { get; set; }

        [Column("CODTIPOVENDEDOR")]
        public int? CodTipoVendedor { get; set; }

        [Column("CODEMPRESAORIGEN")]
        public int? CodEmpresaOrigen { get; set; }

        [Column("ESIMPULSADOR")]
        [StringLength(1)]
        public string EsImpulsador { get; set; }

        [Column("CODREGION")]
        public int? CodRegion { get; set; }

        [Column("CORREO")]
        [StringLength(100)]
        public string Correo { get; set; }

        [Column("CODEMPRESAEMPL")]
        public int? CodEmpresaEmpl { get; set; }

        [Column("CODEMPLEADO")]
        public int? CodEmpleado { get; set; }

        [Column("CODTIPOCLIENTE")]
        public int? CodTipoCliente { get; set; }

        [Required]
        [Column("PROCESOKOM")]
        [StringLength(1)]
        public string ProcesoKom { get; set; }

        [Column("CONSIDERARCALC21")]
        [StringLength(1)]
        public string ConsiderarCalc21 { get; set; }

        [Column("TIPOIMPULSADOR")]
        [StringLength(1)]
        public string TipoImpulsador { get; set; }

        [Column("CODEMPRESAMARCA")]
        public int? CodEmpresaMarca { get; set; }

        [Column("CODMARCA")]
        public int? CodMarca { get; set; }
    }

}
