using ApiManagerOdoo.Base;
using DMSA.Models.Odoo.General.Responses;
using DMSA.Models.Odoo.Native;
using DMSA.Models.Odoo.Tools;
using DMSA.Models.Security;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ApiManager
{
    public class HubSaleOrder : HubBase
    {
        string[] fields_array = new[] {
                "id",
                "name",
                "complete_name",
                "active",
                "usage",
                "comment",
                "posx",
                "posy",
                "posz",
                "parent_path",
                "scrap_location",
                "return_location",
                "replenish_location",
                "cyclic_inventory_frequency",
                "barcode",
                "net_weight",
                "forecast_weight",
                "display_name",
                "create_date",
                "location_id",
                "child_ids",
                "child_internal_location_ids",
                "company_id",
                "removal_strategy_id",
                "putaway_rule_ids",
                "quant_ids",
                "last_inventory_date",
                "next_inventory_date",
                "warehouse_view_ids",
                "warehouse_id",
                "storage_category_id",
                "outgoing_move_line_ids",
                "incoming_move_line_ids",
                "create_uid",
                "write_uid",
                "write_date",
                "valuation_in_account_id",
                "valuation_out_account_id",
        };

        public HubSaleOrder(AppSession _setAppSession) : base(_setAppSession)
        {
            EndPointApi = "/web/dataset/call_kw";
            _modelname = "sale.order";
        }

        public async Task<ApiResponseOdooRpc?> GetCount()
        {
            object[] args = new object[] { };
            object[] _custom_args = new object[] {
            };
            return await GetCount(args, _custom_args);
        }

        public async Task<ApiResponseOdooRpc?> GetCountByCreateDate(int year, int month, int day)
        {
            object[] args = new object[] { };
            object[] _custom_args = new object[] {
                new object[] {"create_date", ">=", $"{year}-{month:00}-{day:00} 00:00:00" },
                new object[] {"create_date", "<=", $"{year}-{month:00}-{day:00} 23:59:59" },
            };
            return await GetCount(args, _custom_args);
        }

        public async Task<ApiResponseOdooRpc?> GetCountByWriteDate(int year, int month, int day)
        {
            object[] args = new object[] { };            
            object[] _custom_args = new object[] {
                new object[] { "write_date", ">", $"{year}-{month:00}-{day:00} 23:59:59" },
            };
            return await GetCount(args, _custom_args);
        }
        
        public async Task<ApiResponseOdooRpcT<sale_order[]>?> GetByCreateDate(int limit, int index, int year, int month, int day)
        {            
            var kwargs = new
            {
                limit = limit,
                offset = (index * limit),
                fields = fields_array
            };

            object[] args = new object[] { };
            object[] _custom_args = new object[] {
                new object[] {"create_date", ">=", $"{year}-{month:00}-{day:00} 00:00:00" },
                //new object[] {"create_date", "<=", $"{year}-{month:00}-{day:00} 23:59:59" },
            };
            return await SearchRead<ApiResponseOdooRpcT<sale_order[]>>(args, _custom_args, kwargs);
        }

        public async Task<ApiResponseOdooRpcT<sale_order[]>?> GetByWriteDate_dl(int year, int month, int day)
        {
            var kwargs = new
            {
                fields = new[] { "id", "write_date" }
            };

            object[] args = new object[] { };
            object[] _custom_args = new object[] {
                new object[] { "write_date", ">", $"{year}-{month:00}-{day:00} 23:59:59" },
            };
            return await SearchRead<ApiResponseOdooRpcT<sale_order[]>>(args, _custom_args, kwargs);
        }

        public async Task<ApiResponseOdooRpcT<sale_order[]>?> GetByCreateDate_dl(int year, int month, int day)
        {
            var kwargs = new
            {
                fields = new[] { "id", "create_date", "write_date" }
            };

            object[] args = new object[] { };
            object[] _custom_args = new object[] {
                new object[] { "create_date", ">", $"{year}-{month:00}-{day:00} 23:59:59" },
            };
            return await SearchRead<ApiResponseOdooRpcT<sale_order[]>>(args, _custom_args, kwargs);
        }
        
        public async Task<ApiResponseOdooRpcT<sale_order[]>?> GetByWriteDate(int year, int month, int day)
        {
            var kwargs = new
            {
                fields = fields_array
            };

            object[] args = new object[] { };
            object[] _custom_args = new object[] {                
                new object[] { "write_date", ">", $"{year}-{month:00}-{day:00} 23:59:59" },
            };
            return await SearchRead<ApiResponseOdooRpcT<sale_order[]>>(args, _custom_args, kwargs);
        }

        public async Task<ApiResponseOdooRpcT<int>?> Create(sale_order sale_Order)
        {
            var kwargs = new{};
            var settings = new JsonSerializerSettings
            {
                DateFormatString = "yyyy-MM-dd HH:mm:ss",
                ContractResolver = new IncludeJsonIgnoreResolver()
            };

            var serialized = JsonConvert.SerializeObject(sale_Order, settings);

            var newJObject = JObject.Parse(serialized);

            JObjectExtensions.RenameProperty(newJObject, "_partner_id", "partner_id");
            JObjectExtensions.RenameProperty(newJObject, "_company_id", "company_id");
            JObjectExtensions.RenameProperty(newJObject, "_warehouse_id", "warehouse_id");
            JObjectExtensions.RenameProperty(newJObject, "_currency_id", "currency_id");
            JObjectExtensions.RenameProperty(newJObject, "_center_id", "center_id");

            JObjectExtensions.RemoveProperty(newJObject, "is_synchronized");
            JObjectExtensions.RemoveProperty(newJObject, "date_synchronized");
            JObjectExtensions.RemoveProperty(newJObject, "is_imported");
            JObjectExtensions.RemoveProperty(newJObject, "date_imported");
            JObjectExtensions.RemoveProperty(newJObject, "partner_display");
            JObjectExtensions.RemoveProperty(newJObject, "partner_display_name");
            JObjectExtensions.RemoveProperty(newJObject, "partner_display_address");
            JObjectExtensions.RemoveProperty(newJObject, "partner_display_status");
            JObjectExtensions.RemoveProperty(newJObject, "partner_invoice_id");
            JObjectExtensions.RemoveProperty(newJObject, "partner_shipping_id");
            JObjectExtensions.RemoveProperty(newJObject, "pricelist_id");
            JObjectExtensions.RemoveProperty(newJObject, "payment_term_id");
            JObjectExtensions.RemoveProperty(newJObject, "team_id");
            JObjectExtensions.RemoveProperty(newJObject, "user_id");
            JObjectExtensions.RemoveProperty(newJObject, "erp_id");

            JObjectExtensions.RemovePropertyFromOrderLineItems(newJObject, "_order_id");
            JObjectExtensions.RemovePropertyFromOrderLineItems(newJObject, "_product_uom_category_id");
            JObjectExtensions.RemovePropertyFromOrderLineItems(newJObject, "product_code");
            JObjectExtensions.RemovePropertyFromOrderLineItems(newJObject, "product_display");
            JObjectExtensions.RemovePropertyFromOrderLineItems(newJObject, "uom_category_display");
            JObjectExtensions.RemovePropertyFromOrderLineItems(newJObject, "ordinal");

            JObjectExtensions.RemovePropertyFromOrderLineItems(newJObject, "_product_uom_qty");
            JObjectExtensions.RemovePropertyFromOrderLineItems(newJObject, "_qty_to_deliver");
            //JObjectExtensions.RenamePropertyFromOrderLineItems(newJObject, "_product_uom_qty", "product_uom_qty");

            //if (newJObject["partner_id"] != null)
            //{
            //    newJObject.Remove("partner_id");
            //}

            //if (newJObject["_partner_id"] != null)
            //{
            //    newJObject["partner_id"] = newJObject["_partner_id"];
            //    newJObject.Remove("_partner_id");
            //}

            object[] args = new object[] { newJObject };
            return await Create<ApiResponseOdooRpcT<int>>(args, kwargs);
        }


        
    }
}
