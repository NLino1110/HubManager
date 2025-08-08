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

namespace Models.DMSA.Mbw.Inventario
{    
    [Table("GENARTICULOSWEB")]
    public class GenArticulosWeb
    {
        [Key]
        [Column("CODARTICULO")]
        public int CodArticulo { get; set; }

        //[ForeignKey("ArticuloPadre")]
        [Column("CODARTICULOPADRE")]
        public int? CodArticuloPadre { get; set; }

        //public virtual ArticulosXEmpresa ArticuloPadre { get; set; }

        [Column("VAR_COLOR")]
        [StringLength(250)]
        public string? VarColor { get; set; }

        [Column("VAR_TAMANO")]
        [StringLength(250)]
        public string? VarTamano { get; set; }

        [Column("VAR_OTROS")]
        [StringLength(250)]
        public string? VarOtros { get; set; }

        [Column("FILTRO_1_CARACTERISTICA")]
        [StringLength(250)]
        public string? Filtro1Caracteristica { get; set; }

        [Column("FILTRO_2_TIPOCOLOR")]
        [StringLength(250)]
        public string? Filtro2TipoColor { get; set; }

        [Column("FILTRO_3_COMPONENTES")]
        [StringLength(250)]
        public string? Filtro3Componentes { get; set; }

        [Column("FILTRO_4_TIPOUSO")]
        [StringLength(250)]
        public string? Filtro4TipoUso { get; set; }

        [Column("ARTICULOVARIABLE")]
        [StringLength(1)]
        public string? ArticuloVariable { get; set; }
    }
}
