using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMSA.Models.Clientes
{
    public class ClienteAprobacion
	{
		[Key]
		public string? IDENTIFICACION {get; set;}
        public int CODAGENCIA {get; set;}
        public int NUMPEDIDO {get; set;}
        public int CODCLIENTE {get; set;}
		public string? NOMBRESCLIENTE {get; set;}
		public string? APELLIDOSCLIENTE {get; set;}
		public string? TIPOIDENTIFICACION {get; set;}
		public string? DIRECCIONCLIENTE {get; set;}
		public string? TELEFONOCLIENTE{get; set;}
		public string? EMAILCLIENTE{get; set;}
		public string? CODEMPRESA{get; set;}
		public string? CODVENDEDOR{get; set;}
        public string? CODUSUARIO { get; set; }
        public DateTime FECHAREGISTRO{get; set;} 
		public DateTime? FECHACAMBIOESTADO{get; set;} 
		public DateTime? FECHAMODIFICACION{get; set;} 
		public string? APLICACIONORIGEN{get; set;} 
		public string? APLICACIONVERSION{get; set;}        
		public string? PLATAFORMAORIGEN{get; set;} 
		public string? PLATAFORMAMODIFICA{get; set;} 
		public string? MARCAEQUIPO{get; set;} 
		public string? MODELOEQUIPO{get; set;}
        public int CODESTADO { get; set; }
    }
}
