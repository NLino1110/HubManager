using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Models.DMSA.Mbw.Core;
using System.Runtime.Serialization;

namespace Models.DMSA.Mbw.Sales
{    

    [Table("FACBONIFICADOSXLINEA")]
    public class FacBonificadosXLinea
    {
        [Key]
        [Required]
        [Column("CODBONIFICADOLINEA")]
        public decimal CodBonificadoLinea { get; set; }

        [Required]
        [Column("CODEMPRESA")]
        public decimal CodEmpresa { get; set; }

        [Required]
        [Column("CODAGENCIA")]
        public decimal CodAgencia { get; set; }

        [Column("CODTIPOCLIENTE")]
        public decimal? CodTipoCliente { get; set; }

        [Column("CODEMPRESANIVEL")]
        public decimal? CodEmpresaNivel { get; set; }

        [Column("CODNIVEL")]
        public decimal? CodNivel { get; set; }

        [Required]
        [Column("CODEMPRESAMARCA")]
        public decimal CodEmpresaMarca { get; set; }

        [Required]
        [Column("CODMARCA")]
        public decimal CodMarca { get; set; }

        [Required]
        [Column("CODEMPRESALINEA")]
        public decimal CodEmpresaLinea { get; set; }

        [Required]
        [Column("CODLINEA")]
        public decimal CodLinea { get; set; }

        [Required]
        [Column("FECHAINICIO")]
        public DateTime FechaInicio { get; set; }

        [Required]
        [Column("FECHAFIN")]
        public DateTime FechaFin { get; set; }

        [Required]
        [Column("MINIMOAPLICADSC")]
        public decimal MinimoAplicadsc { get; set; }

        [Required]
        [Column("PORCDESCUENTO")]
        public decimal PorcDescuento { get; set; }

        [Required]
        [Column("CODUSUARIO")]
        [StringLength(15)]
        public string CodUsuario { get; set; }

        [Required]
        [Column("FECHAINGRESO")]
        public DateTime FechaIngreso { get; set; }

        [Column("USUARIOMODIFICACION")]
        [StringLength(15)]
        public string UsuarioModificacion { get; set; }

        [Column("FECHAMODIFICACION")]
        public DateTime? FechaModificacion { get; set; }

        [Required]
        [Column("CODESTADO")]
        public decimal CodEstado { get; set; }
    }

}
