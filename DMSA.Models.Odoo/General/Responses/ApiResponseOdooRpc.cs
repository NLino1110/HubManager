using DMSA.Models.Odoo.Native;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMSA.Models.Odoo.General.Responses
{
    //public class ApiResponseOdooRpc
    //{
    //    public string jsonrpc { get; set; }
    //    public int id { get; set; }
    //    public int result { get; set; }
    //}

    [Obsolete("Debe ser eliminado")]
    public class ApiResponseOdooRpc_v2
    {
        public string jsonrpc { get; set; }
        public int id { get; set; }
        public bool result { get; set; }
        public Error? error { get; set; }
    }

    public class ApiResponseOdooRpc
    {
        public string jsonrpc { get; set; }
        public int id { get; set; }
        public int result { get; set; }
        public Error? error { get; set; }
    }

    [Obsolete("Debe ser eliminado")]
    public class ApiResponseOdooRpc_account_move___
    {
        public string jsonrpc { get; set; }
        public int id { get; set; }
        //public object? result { get; set; }
        public account_move_base[]? result { get; set; }
        public Error? error { get; set; }
    }

    [Obsolete("Debe ser eliminado")]
    public class ApiResponseOdooRpcDynamic___
    {
        public string jsonrpc { get; set; }
        public int id { get; set; }
        public dynamic result { get; set; }
        public Error? error { get; set; }
    }

    //public class __Error
    //{
    //    public int code { get; set; }
    //    public string message { get; set; }
    //    public Data data { get; set; }
    //}

    //public class __Data
    //{
    //    public string name { get; set; }
    //    public string debug { get; set; }
    //    public string message { get; set; }
    //    public string[] arguments { get; set; }
    //    public Context context { get; set; }
    //}

    //public class __Context
    //{
    //}
}
