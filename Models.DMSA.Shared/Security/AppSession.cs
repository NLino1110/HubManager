using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.DMSA.Shared.Security
{
    public class AppSession
    {
        public User CurrentUser { get; set; }
        public string urlServicioReportes = "https://www.dmujeres.com.ec:8443/MyBusinessWeb/servlet/ServicioTransporteReportes";
    }
}
