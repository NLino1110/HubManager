//using SQLite;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.DMSA.Shared.Security
{
    public class User
    {
        public string exito { get; set; }
        public string mensaje { get; set; }
        public string codusuario { get; set; }
        public string nombres { get; set; }
        public string nivel { get; set; }
        public string fechasincronizado { get; set; }
        
        [NotMapped]
        public string clave { get; set; }
        [NotMapped]
        public string empresas { get; set; }

        [NotMapped]
        public bool Done { get; set; }

        //[NotMapped]
        //[PrimaryKey, AutoIncrement]
        public int ID { get; set; }
    }
}
