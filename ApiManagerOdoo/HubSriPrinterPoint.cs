using ApiManagerOdoo.Base;
using AppManagerOdoo.Tools;
using CobranzasDMSA.Models;
using DMSA.Models.Clientes;
using DMSA.Models.General;
using DMSA.Models.General.Responses;
using DMSA.Models.Odoo.General.Responses;
using DMSA.Models.Odoo.Native;
using DMSA.Models.Security;
//using Microsoft.AspNetCore.Components;
//using Microsoft.Extensions.Configuration;
//using Microsoft.Extensions.Logging;
//using Microsoft.Extensions.Options;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text.Json;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace ApiManager
{
    public class HubSriPrinterPoint : HubBase
    {
        string[] fields_array = new[] {
                "id",
                "name",                
        };

        public HubSriPrinterPoint(AppSession _setAppSession) : base(_setAppSession)
        {
            //EndPointApi = "/connect/get_res_partner";
            EndPointApi = "/web/dataset/call_kw";
            _modelname = "res.partner";
        }

        public async Task<ApiResponseOdooRpcT<sri_printer_point[]>?> GetItemsById(string ids)
        {
            var kwargs = new
            {
                fields = fields_array
            };

            object[] args = new object[] { };
            object[] _custom_args = new object[] {
                new object[] {"id", "in", ids },
            };
            return await SearchRead<ApiResponseOdooRpcT<sri_printer_point[]>>(args, _custom_args, kwargs);
        }

        public async Task<ApiResponseOdooRpcT<sri_printer_point[]>?> GetItems()
        {
            var kwargs = new
            {
                fields = fields_array
            };

            object[] args = new object[] { };
            object[] _custom_args = new object[] {
                
            };
            return await SearchRead<ApiResponseOdooRpcT<sri_printer_point[]>>(args, _custom_args, kwargs);
        }
    }
}
