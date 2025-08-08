using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.DMSA.Mbw.Clientes
{
    public class OdooLopdp
    {
		[Key]
		public string? NUMIDENTIFICACION { get; set;}
        public string? TIPOIDENTIFICACION { get; set; }
        public string? NOMBRECOMPLETO {get; set;}
		public string? CELULAR {get; set;}
		public string? EMAIL { get; set;}
		public string? OTP { get; set;}
		public string? FINGERPRINT { get; set;}
        public DateTime FECHAREGISTRO {get; set;}		
		public DateTime? FECHAMODIFICACION {get; set;}
		public string? APLICACIONORIGEN {get; set;}
		public string? APLICACIONVERSION {get; set;}
		public string? PLATAFORMAORIGEN {get; set;}
		public string? PLATAFORMAMODIFICA {get; set;}
		public string? MARCAEQUIPO {get; set;}
		public string? MODELOEQUIPO {get; set;}
    }
}
