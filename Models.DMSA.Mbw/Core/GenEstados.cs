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
    [Table("GENESTADOS")]
    public class GenEstados
    {
        [Key]
        [Column("CODESTADO")]
        public int CodEstado { get; set; }

        [Column("DESCRIPCION")]
        [Required]
        [StringLength(50)]
        public string Descripcion { get; set; }

        [Column("CODUSUARIO")]
        [StringLength(15)]
        public string CodigoUsuario { get; set; }

        [Column("FECHA")]
        public DateTime? Fecha { get; set; }
    }

    public static class GenEstados_ENUM
    {
        public const long ESTADO_ACTIVO = 1;
        public const long ESTADO_INACTIVO = 2;
        public const long ESTADO_ANULADO = 3;
        public const long ESTADO_CERRADO = 4;
        public const long ESTADO_FACTURADO = 5;
        public const long ESTADO_APROBADO = 6;
        public const long ESTADO_PROCESADO = 7;
        public const long ESTADO_NEGADO = 8;
        public const long ESTADO_MAYORIZADO = 9;
        public const long ESTADO_RESTRINGIDO = 10;
        public const long ESTADO_INGRESADO_CAJA = 11;
        public const long ESTADO_HISTORICO = 12;
        public const long ESTADO_CONFIRMADO = 13;
        public const long ESTADO_LIQUIDADO = 14;
        public const long ESTADO_PRE_LIQUIDADO = 15;
        public const long ESTADO_AÑO_CERRADO = 16;

        // Estados de Manejos de Requerimientos
        public const long ESTADO_PROCESO_CARGA = 17;
        public const long ESTADO_VERIFICANDO_STOCK = 18;
        public const long ESTADO_EN_TRANSITO = 20;
        public const long ESTADO_DESPACHO_TERMINADO = 21;
        public const long ESTADO_CONFIRMADO_MANUAL = 22;
        public const long ESTADO_CONFIRMADO_AUTOMATICO = 23;
        public const long ESTADO_INGRESO_VERIFICADO = 24;
        public const long ESTADO_PENDIENTE = 25;
        public const long ESTADO_EN_PROCESO_WMS = 26;

        public const long ESTADO_EN_PUERTO_DESTINO = 60;
        public const long ESTADO_RECIBIDO_PARCIAL = 64;

        public const long ESTADO_PROCESADO_MANUAL = 75;
        public const long ESTADO_EN_ESPERA_APROBACION = 78; // SCardenas 21/9/2020
        public const long ESTADO_PENDIENTE_POR_CERRAR = 80; // abravo 2021-10-20
        public const long ESTADO_EN_VERIFICACION = 81; // abravo 2021-10-25
        public const long ESTADO_PENDIENTE_WEB = 55; // Pdelgado 7/12/2021
        public const long ESTADO_PAGADO_WEB = 76; // Pdelgado 7/12/2021
        public const long ESTADO_CANCELADO_WEB = 56; // Pdelgado 7/12/2021
        public const long ESTADO_NO_ES_WEB = 0;
    }
}
