using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace DMSA.Models.MovilCobranzas.Api
{   
    public class st_formaspago
    {
        //public int id { get; set; }
        //public int name { get; set; }
        public string codigo { get; set; }
        //[JsonProperty("name")]
        public string descripcion { get; set; }
        public string tipo { get; set; }
        public string porcentaje { get; set; }
    }
}
