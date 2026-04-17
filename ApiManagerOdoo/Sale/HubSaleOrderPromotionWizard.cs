using ApiManagerOdoo.Base;
using DMSA.Models.Odoo.General.Responses;
using DMSA.Models.Odoo.Native;
using DMSA.Models.Odoo.Promotions.Wizard;
using DMSA.Models.Odoo.Sales;
using DMSA.Models.Odoo.Tools;
using DMSA.Models.Security;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using System.Diagnostics;

namespace ApiManagerOdoo.Sale
{
    public class HubSaleOrderPromotionWizard : HubBase
    {
        string[] fields_array = new[] {
                "id",
                "name",
                "company_id",
                "state"
        };

        public HubSaleOrderPromotionWizard(AppSession _setAppSession) : base(_setAppSession)
        {
            EndPointApi = "/web/dataset/call_kw";
            _modelname = "sale.order.promotion.wizard";
        }

        public async Task<ApiResponseOdooRpc?> GetCount()
        {
            object[] args = new object[] { };
            object[] _custom_args = new object[] {
            };
            return await GetCount(args, _custom_args);
        }

        public async Task<ApiResponseOdooRpcT<sale_order[]>?> GetById(int id)
        {
            var kwargs = new
            {
                limit = 300,
                offset = 0,
                fields = fields_array
            };
            object[] args = new object[] { };
            object[] _custom_args = new object[] {
                new object[] {"id", "=", id },
            };
            return await SearchRead<ApiResponseOdooRpcT<sale_order[]>>(args, _custom_args, kwargs);
        }

        public async Task<ApiResponseOdooRpcT<sale_order[]>?> GetByIds(int[] ids)
        {
            var kwargs = new
            {
                limit = 300,
                offset = 0,
                fields = fields_array
            };
            object[] args = new object[] { };
            object[] _custom_args = new object[] {
                new object[] {"id", "in", ids },
            };
            return await SearchRead<ApiResponseOdooRpcT<sale_order[]>>(args, _custom_args, kwargs, true);
        }

        public async Task<ApiResponseOdooRpcT<SaleOrderPromotionWizardLineRead[]>?> GetWizardLines(int id)
        {
            var kwargs = new
            {
                limit = 300,
                offset = 0,
                fields = new[] {
                        "id",
                        "wizard_id",
                        "promotion_id",
                        "rule_id",
                        "promotions_type_id",
                        "discount",
                        "rule_value",
                        "qty_confirmation",
                        "lines_ids"
                }
            };
            object[] args = new object[] { };
            object[] _custom_args = new object[] {
                new object[] { "wizard_id", "=", id },
            };
            return await SearchRead<ApiResponseOdooRpcT<SaleOrderPromotionWizardLineRead[]>>("sale.order.promotion.wizard.line", args, _custom_args, kwargs, true);
        }

        public async Task<ApiResponseOdooRpcT<int>?> Create(SaleOrderPromotionWizard saleOrderPromotionWizard, bool requiredApproved)
        {
            var kwargs = new{};
            var settings = new JsonSerializerSettings
            {
                DateFormatString = "yyyy-MM-dd HH:mm:ss",
                ContractResolver = new IncludeJsonIgnoreResolver()
            };

            var serialized = JsonConvert.SerializeObject(saleOrderPromotionWizard, settings);

            var newJObject = JObject.Parse(serialized);

            JObjectExtensions.RemovePropertyFromOrderLineItems(newJObject, "gift_line_ids");
            JObjectExtensions.RemovePropertyFromOrderLineItems(newJObject, "all_gift_line_ids");

            //Crear
            object[] args = new object[] { newJObject };
            var created_data = await Create<ApiResponseOdooRpcT<int>>(args, kwargs);

            //Actualizar
            if(created_data.result > 0)
            {
                int new_id = created_data.result;

                //Obtener ids
                var wizardLines = await GetWizardLines(new_id);

                var all_gifts = saleOrderPromotionWizard.All_Gift_Line_Ids.ToArray();

                foreach (var gift in all_gifts)
                {
                    var lineData = (AllSaleOrderPromotionWizardGift) gift[2];
                    Debug.WriteLine($"Processing gift with Promotion_Line_Id: {lineData.Promotion_Line_Id}");
                    lineData.Promotion_Line_Id = wizardLines.result.FirstOrDefault(l => l.Lines_Ids.Intersect(lineData.Lines_Ids).Any())?.Id ?? 0;
                }

                var args_update = new object[]
                {
                    new object[] { new_id },
                    new
                    {
                        all_gift_line_ids = all_gifts
                    }
                };

                var kwargs_update = new {};

                var update_prices = await CallMethod<ApiResponseOdooRpcT<bool>>("/web/dataset/call_kw",
                    Method.Post,
                    args_update,
                    kwargs_update, "sale.order.promotion.wizard", "write");

                Debug.WriteLine($"Update result: {update_prices.result}");
            }

            return created_data;
        }
    }
}
