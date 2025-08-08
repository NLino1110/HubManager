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
    [Table("CAJCAJAS")]
    public class CajCajas
    {
        [Column("CODAGENCIA")]
        public int CodAgencia { get; set; }

        [Column("NUMCAJA")]
        public int NumCaja { get; set; }

        [Column("NOMBRE")]
        [StringLength(40)]
        public string? Nombre { get; set; }

        [Column("TIPOCAJA")]
        [StringLength(1)]
        public string? TipoCaja { get; set; }

        [Column("PORCMAXUSO")]
        public decimal PorcMaxUso { get; set; }

        [Column("CUPOCAJA")]
        public decimal CupoCaja { get; set; }

        [Column("VALORMAXEGR")]
        public decimal ValorMaxEgr { get; set; }

        [Column("USUARIOCAJA")]
        [StringLength(15)]
        public string? UsuarioCaja { get; set; }

        [Column("CODEMPRESA")]
        public int CodEmpresa { get; set; }

        [Column("NUMCUENTA")]
        [StringLength(15)]
        public string? NumCuenta { get; set; }

        [Column("CODESTADO")]
        public int CodEstado { get; set; }

        [Column("USUARIOCAMBIOESTADO")]
        [StringLength(15)]
        public string? UsuarioCambioEstado { get; set; }

        [Column("FECHACAMBIOESTADO")]
        public DateTime? FechaCambioEstado { get; set; }

        [Column("CODUSUARIO")]
        [StringLength(15)]
        public string? CodUsuario { get; set; }

        [Column("FECHA")]
        public DateTime Fecha { get; set; }

        [Column("USOCAJA")]
        [StringLength(2)]
        public string? UsoCaja { get; set; }

        [Column("REPORTACORREO")]
        [StringLength(300)]
        public string? ReportaCorreo { get; set; }

        [Column("REPORTACORREOEGRESOS")]
        [StringLength(300)]
        public string? ReportaCorreoEgresos { get; set; }

        [Column("REPORTACORREOVALDIFEST")]
        [StringLength(300)]
        public string? ReportaCorreoValDiFest { get; set; }

        [Column("REPORTACORREOTIEMESTDEP")]
        [StringLength(300)]
        public string? ReportaCorreoTiemEstDep { get; set; }
    }

}
