using ApiManagerOdoo.Base;
using DMSA.Models.Odoo.DMOrders.promotions.abstractCustom;
using DMSA.Models.Odoo.General.Requests;
using DMSA.Models.Odoo.General.Responses;
using DMSA.Models.Odoo.Native;
using DMSA.Models.Odoo.Promotions.Wizard;
using DMSA.Models.Odoo.Sales;
using DMSA.Models.Odoo.Tools;
using DMSA.Models.Security;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace ApiManager
{
    public class HubSaleOrder : HubBase
    {
        string[] fields_array = new[] {
                "id",
                "name",
                "company_id",
                "state"
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

        [Obsolete]
        public async Task<ApiResponseOdooRpc?> GetCountByCreateDate(int year, int month, int day)
        {
            object[] args = new object[] { };
            object[] _custom_args = new object[] {
                new object[] {"create_date", ">=", $"{year}-{month:00}-{day:00} 00:00:00" },
                new object[] {"create_date", "<=", $"{year}-{month:00}-{day:00} 23:59:59" },
            };
            return await GetCount(args, _custom_args);
        }

        [Obsolete]
        public async Task<ApiResponseOdooRpc?> GetCountByWriteDate(int year, int month, int day)
        {
            object[] args = new object[] { };            
            object[] _custom_args = new object[] {
                new object[] { "write_date", ">", $"{year}-{month:00}-{day:00} 23:59:59" },
            };
            return await GetCount(args, _custom_args);
        }

        [Obsolete]
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

        [Obsolete]
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

        [Obsolete]
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

        [Obsolete]
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

        public async Task<ApiResponseOdooRpcT<sale_order_line_read[]>?> GetLines(int sale_order_id)
        {
            var kwargs = new
            {
                fields = new[] {
                        "id",
                        "order_id",
                        "sequence",
                        "order_partner_id",
                        "state",
                        "product_id",
                        "product_template_id",
                        "price_unit",
                        "discount",
                        "price_subtotal",
                        "price_total",
                        "qty_available_today",
                        "qty_to_deliver",
                        "product_uom_qty",
                        "promotion_ids",
                        "rule_ids"
                }
            };

            object[] args = new object[] { };
            object[] _custom_args = new object[] {
                new object[] { "order_id", "=", sale_order_id },
            };
            return await SearchRead<ApiResponseOdooRpcT<sale_order_line_read[]>>("sale.order.line", args, _custom_args, kwargs, true);
        }


        int[] GetPromotionIds(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
                return Array.Empty<int>();

            try
            {
                var promos = JsonConvert.DeserializeObject<List<PromotionEvalItem>>(json);

                return promos?
                    .Where(p => p?.Promotion != null)
                    .Select(p => p.Promotion.id)
                    .Distinct()
                    .ToArray() ?? Array.Empty<int>();
            }
            catch
            {
                return Array.Empty<int>();
            }
        }

        public async Task<ApiResponseOdooRpcT<bool>?> WriteLines(List<sale_order_line> lines)
        {
            var kwargs = new { };

            foreach (var line in lines)
            {
                //var origin_gift_line_ids = GetPromotionIds(line.promotion_data);

                //if (!origin_gift_line_ids.Any())
                //    continue;

                int[] origin_gift_line_ids = line.origin_gift_line_ids;

                object[] args = new object[]
                    {
                        new int[]{ line.erp_id },
                        new {
                            sequence = line.sequence,
                            product_id = line.product_id,
                            price_unit = line.price_unit,
                            origin_gift_line_ids = origin_gift_line_ids
                        }
                    };

                await Write<ApiResponseOdooRpcT<bool>>(args, kwargs, "sale.order.line");
            }

            //return await Write<ApiResponseOdooRpcT<bool>>(args, kwargs, "sale.order.line");
            return new ApiResponseOdooRpcT<bool>();
        }

        public async Task<ApiResponseOdooRpcT<int>?> Create(sale_order sale_Order, bool requiredApproved)
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
            JObjectExtensions.RenameProperty(newJObject, "_pricelist_id", "pricelist_id");
            JObjectExtensions.RenameProperty(newJObject, "_partner_invoice_id", "partner_invoice_id");

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
            //JObjectExtensions.RemoveProperty(newJObject, "pricelist_id");
            JObjectExtensions.RemoveProperty(newJObject, "payment_term_id");
            JObjectExtensions.RemoveProperty(newJObject, "team_id");
            JObjectExtensions.RemoveProperty(newJObject, "user_id");
            JObjectExtensions.RemoveProperty(newJObject, "erp_id");
            JObjectExtensions.RemoveProperty(newJObject, "erp_name");
            JObjectExtensions.RemoveProperty(newJObject, "state_view");
            JObjectExtensions.RemoveProperty(newJObject, "promotion_ids_json");            

            JObjectExtensions.RemovePropertyFromOrderLineItems(newJObject, "_order_id");
            JObjectExtensions.RemovePropertyFromOrderLineItems(newJObject, "_product_uom_category_id");
            JObjectExtensions.RemovePropertyFromOrderLineItems(newJObject, "product_code");
            JObjectExtensions.RemovePropertyFromOrderLineItems(newJObject, "product_display");
            JObjectExtensions.RemovePropertyFromOrderLineItems(newJObject, "uom_category_display");
            //JObjectExtensions.RemovePropertyFromOrderLineItems(newJObject, "ordinal");

            JObjectExtensions.RemovePropertyFromOrderLineItems(newJObject, "_product_uom_qty");
            JObjectExtensions.RemovePropertyFromOrderLineItems(newJObject, "_qty_to_deliver");
            JObjectExtensions.RemovePropertyFromOrderLineItems(newJObject, "is_gift");
            JObjectExtensions.RemovePropertyFromOrderLineItems(newJObject, "promotion_data");
            JObjectExtensions.RemovePropertyFromOrderLineItems(newJObject, "_discount");
            JObjectExtensions.RemovePropertyFromOrderLineItems(newJObject, "_amount_discount");
            JObjectExtensions.RemovePropertyFromOrderLineItems(newJObject, "_price_tax");
            JObjectExtensions.RemovePropertyFromOrderLineItems(newJObject, "_price_subtotal");
            JObjectExtensions.RemovePropertyFromOrderLineItems(newJObject, "_price_total");
            JObjectExtensions.RemovePropertyFromOrderLineItems(newJObject, "_virtual_price_no_tax");
            JObjectExtensions.RemovePropertyFromOrderLineItems(newJObject, "virtual_price_no_tax");
            JObjectExtensions.RemovePropertyFromOrderLineItems(newJObject, "_virtual_iva_percentage");
            JObjectExtensions.RemovePropertyFromOrderLineItems(newJObject, "virtual_iva_percentage");
            JObjectExtensions.RemovePropertyFromOrderLineItems(newJObject, "total_times_allowed");
            JObjectExtensions.RemovePropertyFromOrderLineItems(newJObject, "_virtual_line_subtotal");
            JObjectExtensions.RemovePropertyFromOrderLineItems(newJObject, "virtual_line_subtotal");
            JObjectExtensions.RemovePropertyFromOrderLineItems(newJObject, "product_tmpl_id");
            JObjectExtensions.RemovePropertyFromOrderLineItems(newJObject, "max_gifts");
            JObjectExtensions.RemovePropertyFromOrderLineItems(newJObject, "product_tmpl_id");
            JObjectExtensions.RemovePropertyFromOrderLineItems(newJObject, "assigned_gifts");
            JObjectExtensions.RemovePropertyFromOrderLineItems(newJObject, "product_id_origin");
            JObjectExtensions.RemovePropertyFromOrderLineItems(newJObject, "show_delete_button");
            JObjectExtensions.RemovePropertyFromOrderLineItems(newJObject, "is_manual");

            JObjectExtensions.RemovePropertyFromOrderLineItems(newJObject, "promotion_ids_json");
            JObjectExtensions.RemovePropertyFromOrderLineItems(newJObject, "rule_ids_json");
            JObjectExtensions.RemovePropertyFromOrderLineItems(newJObject, "origin_gift_line_ids_json");
            JObjectExtensions.RemovePropertyFromOrderLineItems(newJObject, "origin_gift_line_ids");
            JObjectExtensions.RemovePropertyFromOrderLineItems(newJObject, "erp_id");
            JObjectExtensions.RemovePropertyFromOrderLineItems(newJObject, "origin_gift_line_ids_offline");
            JObjectExtensions.RemovePropertyFromOrderLineItems(newJObject, "promotionDataList"); 
            //FIX: Eliminados temporalmente
            //JObjectExtensions.RemovePropertyFromOrderLineItems(newJObject, "gift_id");            

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
            var created_data = await Create<ApiResponseOdooRpcT<int>>(args, kwargs);

            if(created_data.result > 0)
            {
                int new_order_id = created_data.result;

                var args_update_prices = new object[]
                {
                    new object[] { new_order_id }
                };

                var kwargs_update_prices = new Dictionary<string, object>
                {
                    ["context"] = new Dictionary<string, object>()
                };

                var update_prices = await CallMethod<ApiResponseOdooRpcT<bool>>("/web/dataset/call_kw",
                Method.Post,
                args_update_prices,
                kwargs_update_prices, "sale.order", "action_update_price_unit_without_vat");

                Debug.WriteLine($"Update prices result: {update_prices.result}");
            }

            if (created_data.result == 0 || !requiredApproved)
            {
                return created_data;
            }

            object[] args_s1 = new object[]
            {
                new object[] { },
                new Dictionary<string, object>
                {
                    { "free_order_state", "INGRESADO" },
                    { "name", false },
                    { "order_id", created_data.result },
                    { "note", "APLICACIÓN DE VENTA EXTERNA" }
                }
            };

            var kwargs_s1 = new Dictionary<string, object>
            {
                ["context"] = new Dictionary<string, object>(),

                ["specification"] = new Dictionary<string, object>
                {
                    ["free_order_state"] = new Dictionary<string, object>(),
                    ["name"] = new Dictionary<string, object>(),

                    ["order_id"] = new Dictionary<string, object>
                    {
                        ["fields"] = new Dictionary<string, object>()
                    },

                    ["note"] = new Dictionary<string, object>()
                }
            };

            var pre_aprobed_data = await CallMethod<ApiResponseOdooRpcT<List<wkf_state_order>>>("/web/dataset/call_kw",
                Method.Post,
                args_s1,
                kwargs_s1, "wkf.state.order", "web_save");

            int created_state_order = 0;

            if (pre_aprobed_data.result != null && pre_aprobed_data.result.Count > 0)
            {
                created_state_order = pre_aprobed_data.result[0].id;                             
            }

            var args_s2 = new object[]
            {
                new object[] { created_state_order }
            };

            var kwargs_s2 = new Dictionary<string, object>
            {
                ["context"] = new Dictionary<string, object>()
            };

            var aprobed_data = await CallMethod<ApiResponseOdooRpcV2>("/web/dataset/call_button",
                Method.Post,
                args_s2,
                kwargs_s2, "wkf.state.order", "confirm_free_order_state");

            return created_data;
        }


        public async Task<ApiResponseOdooRpcT<int>?> BuildPromoMemory(sale_order sale_Order, bool requiredApproved)
        {
            var kwargs = new { };
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
            JObjectExtensions.RenameProperty(newJObject, "_pricelist_id", "pricelist_id");
            JObjectExtensions.RenameProperty(newJObject, "_partner_invoice_id", "partner_invoice_id");

            var orderLines = sale_Order.order_line.Select(ol => (sale_order_line) ol[2]).ToList();

            //SaleOrderPromotionWizardDb saleOrderPromotionWizardDb = new SaleOrderPromotionWizardDb(App.Session.odooConnection.DbNameSqlite);
            var wizardData = new SaleOrderPromotionWizard
            {
                Order_Id = sale_Order.id,
                Line_Ids = orderLines.Where(p => p.is_gift).Select(p =>
                new SaleOrderPromotionWizardLineWrapper(new SaleOrderPromotionWizardLine
                {
                    Wizard_Id = p.id,
                    Promotion_Id = p.id,
                    Rule_Id = 1,
                    Discount = 100,
                    Rule_Value = 1,
                    Qty_Confirmation = true,
                    Lines_Ids = orderLines.Where(p => !p.is_gift).Select(p => p.id).ToArray()
                }
                )).ToList()
            };



            object[] args = new object[] { newJObject };
            var created_data = await Create<ApiResponseOdooRpcT<int>>(args, kwargs);

            if (created_data.result > 0)
            {
                int new_order_id = created_data.result;

                var args_update_prices = new object[]
                {
                    new object[] { new_order_id }
                };

                var kwargs_update_prices = new Dictionary<string, object>
                {
                    ["context"] = new Dictionary<string, object>()
                };

                var update_prices = await CallMethod<ApiResponseOdooRpcT<bool>>("/web/dataset/call_kw",
                Method.Post,
                args_update_prices,
                kwargs_update_prices, "sale.order", "action_update_price_unit_without_vat");

                Debug.WriteLine($"Update prices result: {update_prices.result}");
            }

            if (created_data.result == 0 || !requiredApproved)
            {
                return created_data;
            }

            object[] args_s1 = new object[]
            {
                new object[] { },
                new Dictionary<string, object>
                {
                    { "free_order_state", "INGRESADO" },
                    { "name", false },
                    { "order_id", created_data.result },
                    { "note", "APLICACIÓN DE VENTA EXTERNA" }
                }
            };

            var kwargs_s1 = new Dictionary<string, object>
            {
                ["context"] = new Dictionary<string, object>(),

                ["specification"] = new Dictionary<string, object>
                {
                    ["free_order_state"] = new Dictionary<string, object>(),
                    ["name"] = new Dictionary<string, object>(),

                    ["order_id"] = new Dictionary<string, object>
                    {
                        ["fields"] = new Dictionary<string, object>()
                    },

                    ["note"] = new Dictionary<string, object>()
                }
            };

            var pre_aprobed_data = await CallMethod<ApiResponseOdooRpcT<List<wkf_state_order>>>("/web/dataset/call_kw",
                Method.Post,
                args_s1,
                kwargs_s1, "wkf.state.order", "web_save");

            int created_state_order = 0;

            if (pre_aprobed_data.result != null && pre_aprobed_data.result.Count > 0)
            {
                created_state_order = pre_aprobed_data.result[0].id;
            }

            var args_s2 = new object[]
            {
                new object[] { created_state_order }
            };

            var kwargs_s2 = new Dictionary<string, object>
            {
                ["context"] = new Dictionary<string, object>()
            };

            var aprobed_data = await CallMethod<ApiResponseOdooRpcV2>("/web/dataset/call_button",
                Method.Post,
                args_s2,
                kwargs_s2, "wkf.state.order", "confirm_free_order_state");

            return created_data;
        }        
    }
}
