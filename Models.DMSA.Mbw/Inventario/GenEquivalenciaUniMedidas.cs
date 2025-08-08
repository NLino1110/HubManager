using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Models.DMSA.Mbw.Inventario;

namespace Models.DMSA.Mbw.Inventario
{
    [Table("GENEQUIVALENCIAUNIMEDIDAS")]
    public class GenEquivalenciaUniMedidas
    {
        [Key]
        [Column("CODEQUIVUNIMEDIDA")]
        public decimal Codequivunimedida { get; set; }

        [Column("CODUNIDADORIGEN")]
        [MaxLength(15)]
        public string Codunidadorigen { get; set; }

        [Column("CODUNIDADDESTINO")]
        [MaxLength(15)]
        public string Codunidaddestino { get; set; }

        [Column("FACTORCONVERSION")]
        public decimal Factorconversion { get; set; }

        [Column("FECHA")]
        public DateTime? Fecha { get; set; }

        [Column("CODUSUARIO")]
        [MaxLength(20)]
        public string Codusuario { get; set; }

        [Column("FECHAMODIFICACION")]
        public DateTime? Fechamodificacion { get; set; }

        [Column("USUARIOMODIFICACION")]
        [MaxLength(20)]
        public string Usuariomodificacion { get; set; }
    }
}
