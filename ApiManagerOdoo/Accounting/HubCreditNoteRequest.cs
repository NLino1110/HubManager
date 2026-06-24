using ApiManagerOdoo.Base;
using DMSA.Models.General.Requests;
using DMSA.Models.Odoo.Accounting;
using DMSA.Models.Odoo.DebitCollection;
using DMSA.Models.Odoo.General.Responses;
using DMSA.Models.Odoo.Tools;
using DMSA.Models.Security;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using System.Diagnostics;

namespace ApiManagerOdoo.Accounting
{
    public class HubCreditNoteRequest : HubBase
    {
        string[] fields_array = new[] {
                "id",
                "name",
                "street",
                "street2",
                "city"
                };

        public HubCreditNoteRequest(AppSession _setAppSession) : base(_setAppSession)
        {
            EndPointApi = "/web/dataset/call_kw";
            _modelname = "credit.note.request";
        }

        public async Task<ApiResponseOdooRpcT<List<OdooRpcResultInt>>?> Create(credit_note_request SendObject)
        {
            var kwargs = new { specification = new { } };

            var settings = new JsonSerializerSettings
            {
                DateFormatString = "yyyy-MM-dd HH:mm:ss",
            };

            var serialized = JsonConvert.SerializeObject(SendObject, settings);

            var newJObject = JObject.Parse(serialized);

            JObjectExtensions.RemoveProperty(newJObject, "reversed_entry_id");
            JObjectExtensions.RemoveProperty(newJObject, "payment_reference");
            JObjectExtensions.RemoveProperty(newJObject, "ref");
            JObjectExtensions.RemoveProperty(newJObject, "invoice_line_ids");
            JObjectExtensions.RemoveProperty(newJObject, "request_name");
            JObjectExtensions.RemoveProperty(newJObject, "module_id");
            JObjectExtensions.RemoveProperty(newJObject, "recipe_name");
            
            //JObjectExtensions.RemoveProperty(newJObject, "manufacturer");
            //JObjectExtensions.RemoveProperty(newJObject, "autosend");
            //JObjectExtensions.RemoveProperty(newJObject, "serial");

            object[] args = new object[] { new object[] { }, newJObject };

            var resultCreate = await CallMethod<ApiResponseOdooRpcT<List<OdooRpcResultInt>>>(EndPointApi, Method.Post, args, kwargs, "credit.note.request", "web_save");

            int new_record = 0;

            if (resultCreate != null && resultCreate.error == null && resultCreate.result.Any())
            {
                new_record = resultCreate.result[0].id;
            }
            else
            {
                return null;
            }

            //object[] args_step_2 = new object[] {  new_record };
            //var kwargs_step_2 = new { };
            //var resultButtonFill = await CallMethod<ApiResponseOdooRpcT<OdooRpcResultInt>>(EndPointApi, 
            //    Method.Post, 
            //    args_step_2,
            //    kwargs_step_2, 
            //    "credit.note.request", 
            //    "button_fill_details");

            //Process Step 3
            object[] args_step_2_5 = new object[] { new_record };
            var kwargs_step_2_5 = new { };
            var processSendNc = await CallMethod<ApiResponseOdooRpcT<OdooRpcResultInt>>(EndPointApi,
                Method.Post,
                args_step_2_5,
                kwargs_step_2_5,
                "credit.note.request",
                "process_send_nc");

            //if (resultButtonFill != null && resultButtonFill.error == null && resultButtonFill.id > 0)

            bool button_fill_success = true;
            if (button_fill_success)
            {
                object[] args_step_3 = new object[] {
                    new object[] { new object[] { "parent_id", "=", new_record } }
                };

                string[] fields_array = {
                    "id",
                    "line_id",
                    "product_id",                    
                };

                var kwargs_step_3 = new {
                    fields = fields_array,
                };

                var getDetails = await CallMethod<ApiResponseOdooRpcT<List<credit_note_request_detail_get>>>(EndPointApi, 
                    Method.Post, 
                    args_step_3, 
                    kwargs_step_3, 
                    "credit.note.request.detail", 
                    "search_read");

                if (getDetails.result.Any())
                {
                    foreach (var item in getDetails.result)
                    {
                        int qty_item = 0;

                        //var line_prod = SendObject.lines.Where(x => x.product_id == item.product_id_ && x.line_id == item.line_id).FirstOrDefault();
                        var line_prod = SendObject.lines.Where(x => x.line_id == item.line_id_).FirstOrDefault();

                        if (line_prod != null)
                        {
                            qty_item = (int)line_prod.quantity;

                            var kwargs_item = new { };
                                object[] args_item = new object[] {
                                new object[] { item.id },
                                new { quantity = qty_item }
                            };

                            var writeDetails = await CallMethod<ApiResponseOdooRpcT<bool>>(EndPointApi,
                                Method.Post,
                                args_item,
                                kwargs_item,
                                "credit.note.request.detail",
                                "write");
                            Debug.WriteLine(writeDetails);
                        }
                        else
                        {
                            Debug.WriteLine($"No se encontró el producto {item.product_id_} en las líneas del request");
                        }
                    }
                }

                //ETAPA BARBARA
                object[] args_step_3_0 = new object[] { new_record };
                var kwargs_step_3_0 = new { };
                var process_review_ventas = await CallMethod<ApiResponseOdooRpcT<OdooRpcResultInt>>(EndPointApi,
                    Method.Post,
                    args_step_3_0,
                    kwargs_step_3_0,
                    "credit.note.request",
                    "process_review_ventas");
            }
            else
            { 
                return null;
            }

            return resultCreate;
        }

        public async Task<ApiResponseOdooRpcT<int>?> SendHeader(credit_note_request SendObject)
        {
            var kwargs = new { };

            var settings = new JsonSerializerSettings
            {
                DateFormatString = "yyyy-MM-dd HH:mm:ss",
                ContractResolver = new IncludeJsonIgnoreResolver(new string[] { "was_odoo_synced", "lines" })
            };

            var serialized = JsonConvert.SerializeObject(SendObject, settings);

            var newJObject = JObject.Parse(serialized);

            object[] args = new object[] { newJObject };

            return await Create<ApiResponseOdooRpcT<int>>(args, kwargs, "account.move.send");
        }

        public async Task<ApiResponseOdooRpcT<int>?> SendLine(credit_note_request_detail SendObject)
        {
            var kwargs = new { };

            var settings = new JsonSerializerSettings
            {
                DateFormatString = "yyyy-MM-dd HH:mm:ss",
                ContractResolver = new IncludeJsonIgnoreResolver(new string[] { "was_odoo_synced", "lines" })
            };

            var serialized = JsonConvert.SerializeObject(SendObject, settings);

            var newJObject = JObject.Parse(serialized);

            object[] args = new object[] { newJObject };

            return await Create<ApiResponseOdooRpcT<int>>(args, kwargs, "account.move.line");
        }


        public async Task<ApiResponseOdooRpcT<string[]>?> check_credit_note_overdraft(ApiRequestCheckCn SendObject)
        {
            string EndPointApiLine = "/connect/check_credit_note_overdraft";
         
            var kwargs = new { };

            var settings = new JsonSerializerSettings
            {
                DateFormatString = "yyyy-MM-dd HH:mm:ss",
                //ContractResolver = new IncludeJsonIgnoreResolver(new string[] { "was_odoo_synced", "lines" })
            };

            var serialized = JsonConvert.SerializeObject(SendObject, settings);

            var newJObject = JObject.Parse(serialized);

            object[] args = new object[] { newJObject };

            return await Call<ApiRequestCheckCn, ApiResponseOdooRpcT<string[]>>(EndPointApiLine, 
                Method.Post, SendObject, true);
        }

        public async Task<ApiResponseOdooRpcT<credit_note_request[]>?> GetMovesByHeader(int parent_id)
        {
            int limit = 100;
            int index = 0;
            int year = 0;
            int month = 0;
            int day = 0;

            var kwargs = new
            {
                limit,
                offset = index * limit,
                fields = fields_array
            };

            object[] args = new object[] { };
            object[] _custom_args = new object[] {
                new object[] {
                    "parent_id", "=", parent_id,
                    //"create_date", ">=", $"{year}-{month:00}-{day:00} 00:00:00",                
                    },
            };
            return await SearchRead<ApiResponseOdooRpcT<credit_note_request[]>>(args, _custom_args, kwargs);
        }
    }
}
