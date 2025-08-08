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
    [Table("FACCMPRVENTANOREALIZADA")]
    public class FacCmprVentaNoRealizada
    {
        [Key]
        [Required]
        [Column("SECUENCIAL")]
        public decimal Secuencial { get; set; }

        [Required]
        [Column("CODAGENCIA")]
        public decimal CodAgencia { get; set; }

        [Required]
        [Column("FECHAREGISTRO")]
        public DateTime FechaRegistro { get; set; }

        [Required]
        [Column("CODARTICULO")]
        public decimal CodArticulo { get; set; }

        [Column("CODUNIDADMEDIDABASE")]
        [StringLength(15)]
        public string CodUnidadMedidaBase { get; set; }

        [Required]
        [Column("CODUNIDADMEDIDA")]
        [StringLength(15)]
        public string CodUnidadMedida { get; set; }

        [Column("CODBODEGAAGENCIA")]
        public decimal? CodBodegaAgencia { get; set; }

        [Required]
        [Column("CANTIDAD")]
        public decimal Cantidad { get; set; }

        [Column("CODUSUARIO")]
        [StringLength(15)]
        public string CodUsuario { get; set; }
    }

}
