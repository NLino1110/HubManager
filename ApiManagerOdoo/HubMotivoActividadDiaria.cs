using ApiManagerOdoo.Base;
using AppManagerOdoo.Tools;
using CobranzasDMSA.Models;
using DMSA.Models.Clientes;
using DMSA.Models.General;
using DMSA.Models.General.Responses;
using DMSA.Models.Odoo.DMOrders.promotions;
using DMSA.Models.Odoo.DMOrders.tareas;
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
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Text.Json;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace ApiManager
{
    public class HubMotivoActividadDiaria : HubBase
    {
        string[] fields_array = {
            "id",
            "name",
            "description",            
        };

        public HubMotivoActividadDiaria(AppSession _setAppSession) : base(_setAppSession)
        {
            EndPointApi = "/web/dataset/call_kw";
            _modelname = "motivo.actividad.diaria";
        }

        public async Task<ApiResponseOdooRpc?> GetCount(int year, int month, int day)
        {            
            object[] args = new object[] { };

            object[] _custom_args = new object[] {
                new object[] { "write_date", ">=", $"{year}-{month:00}-{day:00} 00:00:00" },                
            };
            return await GetCount(args, _custom_args);
        }

        public async Task<ApiResponseOdooRpcT<MotivoActividadDiaria[]>?> GetItemsById(string ids)
        {   
            int limit = 300;
            int index = 0 ;

            var kwargs = new
            {
                limit = limit,
                offset = (index * limit),
                fields = fields_array
            };

            object[] args = new object[] { };
            object[] _custom_args = new object[] {
                new object[] { "id", "in", $"[{ids}]" }
            };
            return await SearchRead<ApiResponseOdooRpcT<MotivoActividadDiaria[]>>(args, _custom_args, kwargs, true);
        }

        public async Task<ApiResponseOdooRpcT<MotivoActividadDiaria[]>?> GetItems(DateTime dateIni, int limit, int index)
        {
            var kwargs = new
            {
                limit = limit,
                offset = (index * limit),
                fields = fields_array
            };

            object[] args = new object[] { };
            object[] _custom_args = new object[] {
                new object[] { "write_date", ">=", dateIni.ToString("yyyy-MM-dd") }
            };
            return await SearchRead<ApiResponseOdooRpcT<MotivoActividadDiaria[]>>(args, _custom_args, kwargs, true);
        }
    }
}
