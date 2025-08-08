//using SQLite;

using CobranzasDMSA.Models.General.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace DMSA.Models.Security.Proformas
{
    public class User
    {
        public bool succes { get; set; }
        public bool exito { get; set; }
        public string nombreUsuario { get; set; }
        public string habilitadoMovil { get; set; }
        public string mensaje { get; set; }
        public string codigoUsuario { get; set; }
        //[JsonPropertyName("accesos")]
        public Empresa[] accesos { get; set; }
        
        //[NotMapped]
        //public string? codclave { get; set; }
        //[NotMapped]
        //public string? fechatablet { get; set; }

        //[NotMapped]
        ////public string empresas { get; set; }
        //public Empresa[]? empresas { get; set; }

        //[NotMapped]
        //public bool Done { get; set; }

        ////[NotMapped]
        ////[PrimaryKey, AutoIncrement]
        //public int ID { get; set; }
    }
}
