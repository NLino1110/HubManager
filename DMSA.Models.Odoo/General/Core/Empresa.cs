using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace CobranzasDMSA.Models.General.Core
{
    public class Empresa
    {
        public string codempresa { get; set; }
        public string empresa { get; set; }
        //[JsonPropertyName("agencias")]
        public Agencia[] agencias { get; set; }
    }
}
