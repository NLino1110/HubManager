using ApiManagerOdoo.Base;
using DMSA.Models.Odoo.General.Requests;
using DMSA.Models.Odoo.General.Responses;
using DMSA.Models.Odoo.Native;
using DMSA.Models.Odoo.Tools;
using DMSA.Models.Security;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

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
            JObjectExtensions.RenameProperty(newJObject, "_pricelist_id", "pricelist_id");

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

            JObjectExtensions.RemovePropertyFromOrderLineItems(newJObject, "_order_id");
            JObjectExtensions.RemovePropertyFromOrderLineItems(newJObject, "_product_uom_category_id");
            JObjectExtensions.RemovePropertyFromOrderLineItems(newJObject, "product_code");
            JObjectExtensions.RemovePropertyFromOrderLineItems(newJObject, "product_display");
            JObjectExtensions.RemovePropertyFromOrderLineItems(newJObject, "uom_category_display");
            JObjectExtensions.RemovePropertyFromOrderLineItems(newJObject, "ordinal");

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
