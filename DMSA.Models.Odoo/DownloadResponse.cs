using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CobranzasDMSA_Odoo.Models
{
    internal class DownloadResponse
    {
        public string downloadUrl { get; set; }
        public bool wasSuccess { get; set; }
        public List<string> filesList { get; set; }
    }
}
