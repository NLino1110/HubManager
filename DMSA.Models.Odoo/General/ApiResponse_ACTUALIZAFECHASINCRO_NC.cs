using DMSA.Models.Clientes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMSA.Models.General
{

    //public class ApiResponse_ACTUALIZAFECHASINCRO_NC
    //{
    //    public bool exito { get; set; }
    //    public bool habilitado { get; set; }
    //    public DateTime fecha { get; set; }
    //}


    public class ApiResponse_ACTUALIZAFECHASINCRO_NC
    {
        public int responseCode { get; set; }
        public bool success { get; set; }
        public string message { get; set; }
        public DateTime current_datetime { get; set; }
    }

}
