using ApiManagerOdoo.Base;
using ApiManagerOdoo.Tools;
using AppManagerOdoo.Tools;
using CobranzasDMSA.Models;
using CobranzasDMSA_Odoo.Models;
using DMSA.Models.Odoo.General.Responses;
using DMSA.Models.Odoo.Native;
using DMSA.Models.Odoo.Tools;
using DMSA.Models.Security;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ApiManager
{
    public class HubAccountMoveSendHeader : HubBase
    {
        string[] fields_array = new[] {
                "id",
                "name",
                "create_date",
                "write_date"
                };

        public HubAccountMoveSendHeader(AppSession _setAppSession) : base(_setAppSession)
        {
            EndPointApi = "/web/dataset/call_kw";
            _modelname = "account.move.send.header";
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
        
        public async Task<AccountMoveSendHeader[]?> GetItemsFull(DateTime dateIni, DateTime dateEnd, int index)
        {
            int limit = 100;
            //int index = 0;

            var kwargs = new
            {
                limit = limit,
                offset = (index * limit),
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

            var result = await SearchRead<ApiResponseOdooRpcT<AccountMoveSendHeader[]>>(args, _custom_args, kwargs);

            if (result != null && result.result != null & result.result?.Length > 0)
            {
                var resultApi = result.result;

                foreach (var headerItem in resultApi)
                {
                    var hubAmh = new HubAccountMoveSend(_appSession);

                    var payments = await hubAmh.GetMovesByHeader(headerItem.id);

                    headerItem.account_moves = payments.result;

                    if (headerItem.account_moves != null)
                    {
                        foreach (var payment in headerItem.account_moves)
                        {
                            //Extraer de API
                            var hubLines = new HubAccountMoveLineSend(_appSession);
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

        public async Task<ApiResponseOdooRpcT<int>?> SendHeader(AccountMoveSendHeader SendObject)
        {
            //Se requiere obtener las cookies para saber quien es el usuario que envía los pagos
            //HubUser hubUser = new HubUser(_appSession);
            //User user = new User();
            //user.username = _appSession.CurrentUser.username;
            //user.codclave = _appSession.CurrentUser.codclave;
            //DateTime currentDate = DateTime.Now;
            //var responseUser = await hubUser.TryLoginRpcWeb(user, currentDate);
            
            //if(responseUser == null)
            //{
            //    return new ApiResponseOdoo()
            //    {
            //        responseCode = 500,
            //        message = "Error de sesión",
            //    };
            //    //return new ResponseAuthenticate()
            //    //{
            //    //    jsonrpc = "2.0",
            //    //    error = new Error()
            //    //    {
            //    //        code = 500,
            //    //        message = "Error de sesión"
            //    //    },
            //    //};
            //}

            var kwargs = new { };

            var settings = new JsonSerializerSettings
            {
                DateFormatString = "yyyy-MM-dd HH:mm:ss",
                ContractResolver = new IncludeJsonIgnoreResolver()
            };

            var serialized = JsonConvert.SerializeObject(SendObject, settings);

            var newJObject = JObject.Parse(serialized);

            object[] args = new object[] { newJObject };
            return await Create<ApiResponseOdooRpcT<int>>(args, kwargs);
        }
    }
}
