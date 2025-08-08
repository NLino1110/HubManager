using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Models.DMSA.Mbw.Query
{   
    public class ClienteProfesional
    {
        [Key]
        [Column("ID")]
        public int Id { get; set; }

        [Column("TIPOTARJETA")]
        [MaxLength(1)]
        public string TipoTarjeta { get; set; } 

        [Column("FECHAAPROB")]
        public DateTime FechaAprob { get; set; } 

        [Column("NOMBRESCLIENTE")]
        [MaxLength(100)]
        public string NombresCliente { get; set; }

        [Column("APELLIDOSCLIENTE")]
        [MaxLength(100)]
        public string ApellidosCliente { get; set; } 

        [Column("EMAIL")]
        [MaxLength(200)]
        public string Email { get; set; } = null;

        [Column("FECHANACIMIENTO")]
        public DateTime FechaNacimiento { get; set; } 

        [Column("SEXO")]
        [MaxLength(1)]
        public string Sexo { get; set; } 

        [Column("IDENTIFICACION")]
        [MaxLength(15)]
        public string Identificacion { get; set; } 

        [Column("TELEFONO1")]
        [MaxLength(15)]
        public string Telefono1 { get; set; } 

        [Column("CELULAR")]
        [MaxLength(15)]
        public string Celular { get; set; } 

        [Column("DOMICILIO")]
        public string Domicilio { get; set; } 

        [Column("NOMCIUDAD")]
        public string NomCiudad { get; set; } 

        [Column("CODCIUDAD")]
        public int CodCiudad { get; set; }

        [Column("NOMPROVINCIA")]
        public string NomProvincia { get; set; }

        [Column("NOMBRECOMERCIAL")]
        public string NombreComercial { get; set; }

        [Column("DIRECCIONCOMERCIAL")]
        public string DireccionComercial { get; set; }

        [Column("REFERENCIACOMERCIAL")]
        public string ReferenciaComercial { get; set; }

        [Column("NOMBREINSTITUCION")]
        public string NombreInstitucion { get; set; }

        [Column("DIRECCIONINSTITUCION")]
        public string DireccionInstitucion { get; set; }

        [Column("TELEFONOINSTITUCION")]
        [MaxLength(15)]
        public string TelefonoInstitucion { get; set; } 

        [Column("TITULOACADEMICO")]
        public string TituloAcademico { get; set; }

        [Column("FECHAGRADUACION")]
        public DateTime? FechaGraduacion { get; set; }

        [Column("CODTIPOVENDEDOR")]
        public int CodTipoVendedor { get; set; }

        [Column("TIPOCOMPRA")]
        public string TipoCompra { get; set; }

        [Column("TIPOCLIENTE")]
        public string TipoCliente { get; set; }
    }

}
