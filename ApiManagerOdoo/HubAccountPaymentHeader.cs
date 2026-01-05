using ApiManagerOdoo.Base;
using AppManagerOdoo.Tools;
using CobranzasDMSA.Models;
using DMSA.Models.Clientes;
using DMSA.Models.General;
using DMSA.Models.General.Requests;
using DMSA.Models.General.Responses;
using DMSA.Models.Odoo.Accounting;
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
    public class HubAccountPaymentHeader : HubBase
    {
        string[] fields_array = { };

        public HubAccountPaymentHeader(AppSession _setAppSession) : base(_setAppSession)
        {
            EndPointApi = "/web/dataset/call_kw";
            _modelname = "account.payment.header";
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

        public async Task<ApiResponseOdooRpcT<AccountPaymentSend[]>?> GetPaymentItems(int parent_id)
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
            return await SearchRead<ApiResponseOdooRpcT<AccountPaymentSend[]>>("account.payment.send", args, _custom_args, kwargs, true);
        }

        public async Task<ApiResponseOdooRpcT<AccountPaymentInvoiceLineSend[]>?> GetInvoiceLineSend(int parent_payment_id)
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
            return await SearchRead<ApiResponseOdooRpcT<AccountPaymentInvoiceLineSend[]>>("account.payment.invoice.line.send", args, _custom_args, kwargs, true);
        }

        public async Task<ApiResponseOdooRpcT<AccountPaymentHeaderSend[]>?> GetItemsFull(int uid, DateTime dateIni, DateTime dateEnd, int limit, int index)
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
            return await SearchRead<ApiResponseOdooRpcT<AccountPaymentHeaderSend[]>>(args, _custom_args, kwargs);
        }

        public async Task<ApiResponseOdooRpcT<int>?> SendHeader(AccountPaymentHeaderSend SendObject)
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

        public async Task<ApiResponseOdooRpcT<int>?> SendPayments(AccountPaymentSend SendObject)
        {
            JsonSerializerSettings settings = new JsonSerializerSettings();
            settings.DateFormatString = "yyyy-MM-dd HH:mm:ss";
            settings.ContractResolver = new IgnorePropertyResolver("payment_invoice_line_ids");
            var kwargs = new { };

            var serialized = JsonConvert.SerializeObject(SendObject, settings);

            var newJObject = JObject.Parse(serialized);

            object[] args = new object[] { newJObject };

            return await Create<ApiResponseOdooRpcT<int>>(args, kwargs, "account.payment.send");
        }

        public async Task<ApiResponseOdooRpcT<int>?> SendPaymentsInvoiceLine(AccountPaymentInvoiceLineSend SendObject)
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
