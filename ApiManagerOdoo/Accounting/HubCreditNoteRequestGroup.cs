using ApiManagerOdoo.Base;
using DMSA.Models.Odoo.Accounting;
using DMSA.Models.Odoo.General.Responses;
using DMSA.Models.Odoo.Tools;
using DMSA.Models.Security;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ApiManagerOdoo.Accounting
{
    public class HubCreditNoteRequestGroup : HubBase
    {
        string[] fields_array = new[] {
                "id",
                "name",
                "create_date",
                "write_date"
                };

        public HubCreditNoteRequestGroup(AppSession _setAppSession) : base(_setAppSession)
        {
            EndPointApi = "/web/dataset/call_kw";
            _modelname = "credit.note.request.group";
        }

        public async Task<ApiResponseOdooRpc?> GetHeaderCount(DateTime dateIni, DateTime dateEnd)
        {
            object[] args = new object[] { };
            object[] _custom_args = new object[] {
                new object[] {"create_date", ">=", $"{dateIni.Date.Year}-{dateIni.Date.Month:00}-{dateIni.Date.Day:00} 00:00:00" },
                new object[] {"create_date", "<=", $"{dateEnd.Date.Year} - {dateEnd.Date.Month:00} - {dateEnd.Date.Day:00} 23:59:59" },
            };

            return await GetCount(args, _custom_args);
        }
        
        public async Task<CreditNoteRequestGroup[]?> GetItemsFull(DateTime dateIni, DateTime dateEnd, int index)
        {
            int limit = 100;
            //int index = 0;

            var kwargs = new
            {
                limit,
                offset = index * limit,
                fields = fields_array //new[] { "id", "name", "uom_id", "default_code", "categ_id", "type", "product_brand_id", "active", "macro_product_available", "sale_ok", "purchase_ok", "trade_ok" }
            };

            object[] args = new object[] { };
            object[] _custom_args = new object[] {
                new object[] {"create_date", ">=", $"{dateIni.Date.Year}-{dateIni.Date.Month:00}-{dateIni.Date.Day:00} 00:00:00" },
                new object[] {"create_date", "<=", $"{dateEnd.Date.Year} - {dateEnd.Date.Month:00} - {dateEnd.Date.Day:00} 23:59:59" },
            };

            //TODO: TOMAR EN CUENTA EL SETTING DE JSON

            //JsonSerializerSettings settings = new JsonSerializerSettings();
            ////settings.DateFormatString = "yyyy-MM-dd HH:mm:ss";
            //settings.ContractResolver = new IncludeJsonIgnoreResolver();

            var result = await SearchRead<ApiResponseOdooRpcT<CreditNoteRequestGroup[]>>(args, _custom_args, kwargs);

            if (result != null && result.result != null & result.result?.Length > 0)
            {
                var resultApi = result.result;

                foreach (var headerItem in resultApi)
                {
                    var hubAmh = new HubCreditNoteRequest(_appSession);

                    var payments = await hubAmh.GetMovesByHeader(headerItem.id);

                    headerItem.account_moves = payments.result;

                    if (headerItem.account_moves != null)
                    {
                        foreach (var payment in headerItem.account_moves)
                        {
                            //Extraer de API
                            var hubLines = new HubCreditNoteRequestDetail(_appSession);
                            var lines = await hubLines.GetAccountMoveLineSend(payment.id);
                            payment.lines = lines.result;

                            //foreach (var lineItem in payment.lines)
                            //{
                            //    //Extraer de API

                            //}
                        }
                    }
                }

                return resultApi;
            }
            return null;
        }

        public async Task<ApiResponseOdooRpcT<int>?> SendHeader(CreditNoteRequestGroup SendObject)
        {
            var kwargs = new { };

            var settings = new JsonSerializerSettings
            {
                DateFormatString = "yyyy-MM-dd HH:mm:ss",
                ContractResolver = new IncludeJsonIgnoreResolver()
            };

            var serialized = JsonConvert.SerializeObject(SendObject, settings);

            var newJObject = JObject.Parse(serialized);
            
            JObjectExtensions.RemoveProperty(newJObject, "partner_display");
            JObjectExtensions.RemoveProperty(newJObject, "was_odoo_synced");
            JObjectExtensions.RemoveProperty(newJObject, "account_moves");

            object[] args = new object[] { newJObject };
            return await Create<ApiResponseOdooRpcT<int>>(args, kwargs, "credit.note.request.group");
        }
    }
}
