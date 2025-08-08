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
    [Table("GENMARCAS")]
    public class GenMarca
    {
        //[Key]
        [Column("CODMARCA")]
        public int CodMarca { get; set; }

        //[Required]
        [ForeignKey("Empresa")]
        [Column("CODEMPRESAMARCA")]
        public int CodEmpresaMarca { get; set; }

        //[NotMapped]                
        public virtual GenEmpresa Empresa { get; set; }

        [Required]
        [Column("DESCRIPCION")]
        [MaxLength(100)]
        public string Descripcion { get; set; }

        [Required]
        [Column("CODUSUARIO")]
        [MaxLength(15)]
        public string CodUsuario { get; set; }

        [Required]
        [Column("FECHA")]
        public DateTime Fecha { get; set; }

        [Column("CODCLASIFICACION")]
        public int? CodClasificacion { get; set; }        

        [Required]
        [Column("VENDIBLEOTRASCIAS")]
        [MaxLength(1)]
        public string VendibleOtrasCias { get; set; }

        [Column("ORDENPRESENTACION")]
        public int? OrdenPresentacion { get; set; }

        [Column("PORCCOSTOINTERCOMPANIA", TypeName = "decimal(16,4)")]
        public decimal? PorcCostoInterCompania { get; set; }

        [Column("PROCESOKOM")]
        [MaxLength(1)]
        public string? ProcesoKom { get; set; }

        [Column("ORIGEN")]
        [MaxLength(1)]
        public string? Origen { get; set; }

        [Column("CODEMPRESACATEGORIA")]
        public int? CodEmpresaCategoria { get; set; }

        [Column("CODCATEGORIAMARCA")]
        public int? CodCategoriaMarca { get; set; }

        [Column("DPADREPROTAURUS")]
        [MaxLength(1)]
        public string? DPadreProTaurus { get; set; }

        [Column("DPADREPROPLAY")]
        [MaxLength(1)]
        public string? DPadreProPlay { get; set; }        
    }
}
