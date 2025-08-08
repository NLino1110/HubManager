using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace Models.DMSA.Mbw.Query
{
    public class StockResult
    {
        public long CodBodegaAgencia { get; set; } // Cambia el tipo según corresponda (int, long, etc.)
        public string? NombreBodega { get; set; }
        public double CantDisponibleWms { get; set; } // Cambia el tipo si es diferente
        public double MinimoVtaWeb { get; set; } // Cambia el tipo si es diferente
        public double Cantidad { get; set; } // Cambia el tipo si es diferente
        public double CantidadReservada { get; set; } // Cambia el tipo si es diferente
        //[JsonProperty("fechaultingreso")]
        public DateTime? fechaultingreso { get; set; }
        //[JsonProperty("fechaultegreso")]
        public DateTime? fechaultegreso { get; set; }
    }
}
