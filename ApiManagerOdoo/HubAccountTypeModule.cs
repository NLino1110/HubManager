using ApiManagerOdoo.Base;
using AppManagerOdoo.Tools;
using CobranzasDMSA_Odoo.Models;
using DMSA.Models.Odoo.General.Responses;
using DMSA.Models.Odoo.Native;
using DMSA.Models.Security;
using RestSharp;


namespace ApiManager
{
    public class HubAccountTypeModule : HubBase
    {
        string[] fields_array = new[] {
                "id",
                "name",
                "street",
                "stree2",
                "city"
        };
        public HubAccountTypeModule(AppSession _setAppSession) : base(_setAppSession)
        {
            EndPointApi = "/web/dataset/call_kw";
            _modelname = "account.type.module";
        }


        public async Task<ApiResponseOdooRpcT<AccountTypeModule[]>?> GetAll(string ids)
        {
            var kwargs = new
            {
                fields = fields_array // new[] { "id", "name", "uom_id", "default_code", "categ_id", "type", "product_brand_id", "active", "macro_product_available", "sale_ok", "purchase_ok", "trade_ok", "write_date" }
            };

            object[] args = new object[] { };
            object[] _custom_args = new object[] {
                new object[] { "id", "in", ids },
            };
            return await SearchRead<ApiResponseOdooRpcT<AccountTypeModule[]>>(args, _custom_args, kwargs);
        }

        public async Task<ApiResponseOdooRpcT<AccountTypeModule[]>?> GetAll()
        {
            var kwargs = new
            {
                fields = fields_array // new[] { "id", "name", "uom_id", "default_code", "categ_id", "type", "product_brand_id", "active", "macro_product_available", "sale_ok", "purchase_ok", "trade_ok", "write_date" }
            };

            object[] args = new object[] { };
            object[] _custom_args = new object[] {
                
            };
            return await SearchRead<ApiResponseOdooRpcT<AccountTypeModule[]>>(args, _custom_args, kwargs);
        }
    }
}
