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
    [Table("FACNIVELESPRECIOS")]
    public class FacNivelesPrecios
    {
        //[Key]
        //public int Id { get; set; }

        [Required]
        [Column("CODEMPRESA")]
        public int CodEmpresa { get; set; }

        [Required]
        [Column("CODNIVEL")]
        public int CodNivel { get; set; }

        [Required]
        [Column("DESCRIPCION")]
        [StringLength(20)]
        public string Descripcion { get; set; }

        [Required]
        [Column("NIVELBASE")]
        [StringLength(1)]
        public string NivelBase { get; set; }

        [Column("CANTIDADINI")]
        public decimal? CantidadIni { get; set; }

        [Column("CANTIDADFIN")]
        public decimal? CantidadFin { get; set; }

        [Required]
        [Column("USAALMACEN")]
        [StringLength(1)]
        public string UsaAlmacen { get; set; }

        [Column("CODTIPOLOCAL")]
        public decimal? CodTipoLocal { get; set; }

        [Column("BLOQUEO")]
        [StringLength(1)]
        public string Bloqueo { get; set; }


        public virtual ICollection<FacBonificadosXArticulo> FacBonificadosxArticuloFk { get; set; }
    }
}
