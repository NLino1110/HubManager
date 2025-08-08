using Models.DMSA.Shared.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.DMSA.Shared.General
{
    public class ApiResponse_v2
    {        
        public bool exito { get; set; }        
        public UserSingle[]? data { get; set; }        
    }
}
