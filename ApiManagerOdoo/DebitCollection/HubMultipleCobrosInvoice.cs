using ApiManagerOdoo.Base;
using AppManagerOdoo.Tools;
using CobranzasDMSA.Models;
using DMSA.Models.Clientes;
using DMSA.Models.General;
using DMSA.Models.General.Requests;
using DMSA.Models.General.Responses;
using DMSA.Models.Odoo.Accounting;
using DMSA.Models.Odoo.DebitCollection;
using DMSA.Models.Odoo.General.Responses;
using DMSA.Models.Odoo.Tools;
using DMSA.Models.Security;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using System;
using System.Collections.Generic;

namespace ApiManager
{
    public class HubMultipleCobrosInvoice : HubBase
    {
        string[] fields_array = { };

        public HubMultipleCobrosInvoice(AppSession _setAppSession) : base(_setAppSession)
        {
            EndPointApi = "/web/dataset/call_kw";
            _modelname = "multiple.cobros.invoice";
        }

        public async Task<ApiResponseOdooRpcT<List<OdooRpcResultInt>>?> Create(MultipleCobrosInvoice SendObject)
        {
            var kwargs = new { specification = new {} };

            var settings = new JsonSerializerSettings
            {
                DateFormatString = "yyyy-MM-dd HH:mm:ss",
            };

            var serialized = JsonConvert.SerializeObject(SendObject, settings);

            var newJObject = JObject.Parse(serialized);

            //JObjectExtensions.RenameProperty(newJObject, "_partner_id", "partner_id");
            JObjectExtensions.RemoveProperty(newJObject, "name");
            JObjectExtensions.RemoveProperty(newJObject, "recipe_name");
            JObjectExtensions.RemoveProperty(newJObject, "guid");
            JObjectExtensions.RemoveProperty(newJObject, "payment_status");
            JObjectExtensions.RemoveProperty(newJObject, "model");
            JObjectExtensions.RemoveProperty(newJObject, "manufacturer");
            JObjectExtensions.RemoveProperty(newJObject, "autosend");
            JObjectExtensions.RemoveProperty(newJObject, "serial");

            //JObjectExtensions.RemoveProperty(newJObject, "partner_retail_id");
            //SendObject.partner_retail_id = 0;

            object[] args = new object[] { new object[] {}, newJObject };

            return await CallMethod<ApiResponseOdooRpcT<List<OdooRpcResultInt>>>(EndPointApi, Method.Post, args, kwargs, "multiple.cobros.invoice", "web_save");
        }

        public async Task<ApiResponseOdooRpc?> GetCount(DateTime dateIni, DateTime dateEnd)
        {
            object[] args = new object[] { };            
            object[] _custom_args = new object[] {
                new object[] {"create_date", ">=", dateIni.ToString("yyyy-MM-dd") },
                new object[] {"create_date", "<=", dateEnd.ToString("yyyy-MM-dd") },
            };
            return await GetCount(args, _custom_args);
        }

        public async Task<ApiResponseOdooRpcT<MultipleCobrosInvoice[]>?> GetPaymentItems(int parent_id)
        { 
            var kwargs = new
            {
                //limit = limit,
                //offset = (index * limit),
                fields = fields_array
            };

            object[] args = new object[] { };
            object[] _custom_args = new object[] {
                new object[] { "parent_id", "=", parent_id },
            };
            return await SearchRead<ApiResponseOdooRpcT<MultipleCobrosInvoice[]>>(args, _custom_args, kwargs, true);
        }

        public async Task<ApiResponseOdooRpcT<MultipleCobrosInvoiceLine[]>?> GetInvoiceLineSend(int parent_payment_id)
        {
            var kwargs = new
            {
                //limit = limit,
                //offset = (index * limit),
                fields = fields_array
            };

            object[] args = new object[] { };
            object[] _custom_args = new object[] {
                new object[] { "parent_payment_id", "=", parent_payment_id },                
            };
            return await SearchRead<ApiResponseOdooRpcT<MultipleCobrosInvoiceLine[]>>("account.payment.invoice.line.send", args, _custom_args, kwargs, true);
        }

        public async Task<ApiResponseOdooRpcT<MultipleCobrosInvoice[]>?> GetItemsFull(int uid, DateTime dateIni, DateTime dateEnd, int limit, int index)
        {
            //string domain = $"domain=[('uid','=',{apiRequestOdoo_V1.uid}),('create_date','>=','{apiRequestOdoo_V1.dateIni.ToString("yyyy-MM-dd")}'),";
            //domain += $"('create_date','<=','{apiRequestOdoo_V1.dateEnd.ToString("yyyy-MM-dd")}')]";

            var kwargs = new
            {
                limit = limit,
                offset = (index * limit),
                fields = fields_array
            };

            object[] args = new object[] { };
            object[] _custom_args = new object[] {                
                new object[] { "uid", "=", uid },
                new object[] { "create_date", ">=", dateIni.ToString("yyyy-MM-dd") },
                new object[] { "create_date", "<=", dateEnd.ToString("yyyy-MM-dd") },
            };
            return await SearchRead<ApiResponseOdooRpcT<MultipleCobrosInvoice[]>>(args, _custom_args, kwargs);
        }

        public async Task<ApiResponseOdooRpcT<int>?> SendHeader(MultipleCobrosInvoice SendObject)
        {            
            var kwargs = new { };

            var settings = new JsonSerializerSettings
            {
                DateFormatString = "yyyy-MM-dd HH:mm:ss",
            };

            var serialized = JsonConvert.SerializeObject(SendObject, settings);

            var newJObject = JObject.Parse(serialized);

            object[] args = new object[] { newJObject };

            return await Create<ApiResponseOdooRpcT<int>>(args, kwargs);
        }

        public async Task<ApiResponseOdooRpcT<List<OdooRpcResultInt>>?> SendPayments(MultipleCobrosInvoiceLine SendObject)
        {
            var settings = new JsonSerializerSettings
            {
                DateFormatString = "yyyy-MM-dd HH:mm:ss",
            };
            
            //settings.ContractResolver = new IgnorePropertyResolver("payment_invoice_line_ids");
            var kwargs = new { specification = new { } };

            var serialized = JsonConvert.SerializeObject(SendObject, settings);

            var newJObject = JObject.Parse(serialized);

            object[] args = new object[] { new object[] { }, newJObject };

            //return await Create<ApiResponseOdooRpcT<int>>(args, kwargs, "multiple.cobros.invoice.line");
            return await CallMethod<ApiResponseOdooRpcT<List<OdooRpcResultInt>>>(EndPointApi, Method.Post, args, kwargs, "multiple.cobros.invoice.line", "web_save");
        }

        public async Task<ApiResponseOdooRpcT<int>?> SendPaymentsInvoiceLine(MultipleCobrosInvoiceLineAi SendObject)
        {            
            var kwargs = new { };

            var settings = new JsonSerializerSettings
            {
                DateFormatString = "yyyy-MM-dd HH:mm:ss",
            };

            var serialized = JsonConvert.SerializeObject(SendObject, settings);

            var newJObject = JObject.Parse(serialized);

            object[] args = new object[] { newJObject };

            return await Create<ApiResponseOdooRpcT<int>>(args, kwargs, "account.payment.invoice.line.send");
        }
    }
}
