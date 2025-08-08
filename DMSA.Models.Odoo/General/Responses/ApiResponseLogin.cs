using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace DMSA.Models.Odoo.General.Responses
{
    [Obsolete("Debe ser eliminado")]
    public class ApiResponseLogin
    {
        public string jsonrpc { get; set; }
        public object id { get; set; }
        public Result? result { get; set; }
        public Error? error { get; set; }
        public CookieCollection Cookies { get; set; }
    }

    [Obsolete("Debe ser eliminado")]
    public class Result
    {
        public string access_token { get; set; }
        public int expire_in { get; set; }
        public string token_type { get; set; }
        public string refresh_token { get; set; }
        public int uid { get; set; }
        public string api_key { get; set; }
    }
}
